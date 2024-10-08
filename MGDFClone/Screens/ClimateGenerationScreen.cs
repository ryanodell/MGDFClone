using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.PerlinNoise;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Serilog;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MGDFClone.Screens; 
public class ClimateGenerationScreen : ScreenBase {
    private float _camSpeed = 8.0f;
    private readonly int mapWidth = 50, mapHeight = 50;
    private World _world;
    private Camera2D _camera;
    private readonly RenderSystem _renderSystem;
    private float[] _heightMap;

    ///////////////////////////////////////////////////Temperature related/////////////////////////////////////////////
    private float _minTemp = -20.0f;
    private float _maxTemp = 125.0f;
    private float _waterCoolingFactor = 10.0f;
    private float _waterTemperature = 50.0f;
    private float[] _temperatureMap;
    //Offset to avoid overly hot beaches
    private float _waterElevation = 0.60f;
    // Maximum elevation in meters
    float maxElevationInMeters = 7000.0f;
    // Convert lapse rate to Fahrenheit if using Fahrenheit scale: 6.5°C ≈ 11.7°F | 1000 meters (standard value).
    float lapseRateF = 11.7f;
    Season currentSeason = Season.Summer;
    Dictionary<Season, float> SeasonalTemperatureOffsets = new Dictionary<Season, float>() {
        { Season.Winter, -20.0f },   // Winter is 20°F colder.
        { Season.Spring, 5.0f },     // Spring is 5°F warmer.
        { Season.Summer, 20.0f },    // Summer is 20°F warmer.
        { Season.Autumn, 0.0f }      // Autumn has no change.
    };
    List<Entity> temperatureTiles = new List<Entity>();
    private bool _showTemp = false;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////Atmosphere related/////////////////////////////////////////////
    private float[] _initialHumidityMap;
    private float[] _finalHumidityMap;
    private float _mountainThreshold = 0.8f;      // Elevation threshold for mountains
    private float _percipitationFactor = 0.9f;    // How much moisture is deposited by mountains
    private float _rainShadowEffect = 0.2f;       // Percentage of remaining moisture after crossing mountains
    private float _eastwardDissipation = 0.85f;   // Eastward dissipation of moisture

    private float _baseMoisture = 0.25f;

    List<Entity> humidityTiles = new List<Entity>();
    private bool _showHumidityMap = false;
    private float _minimumHumidity = 0.0f;
    private float _maximumHunidty = 100.0f;

    private List<Entity> _initHumidities = new List<Entity>();
    private bool _showInitHumidity = false;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public ClimateGenerationScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _world = new World();
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 3.5f;
        _camera.LookAt(Vector2.Zero);
        _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
        _temperatureMap = new float[mapWidth * mapHeight];
        _initialHumidityMap = new float[mapWidth * mapHeight];
        _finalHumidityMap = new float[mapWidth * mapHeight];
    }

    public override void LoadContent() {
        _heightMap = PerlinNoiseV4.GeneratePerlinNoise(mapWidth, mapHeight, 3);
        _initialHumidityMap = PerlinNoiseV4.GeneratePerlinNoise(mapWidth, mapHeight, 3);
        _logInitHumidityDetails();
        for (int i = 0; i < _heightMap.Length; i++) {
            int row = i / mapWidth;
            int column = i % mapWidth;
            Entity tile = _world.CreateEntity();
            eSprite sprite = eSprite.None;
            Color color = Color.White;
            var tileType = TileTypeHelper.DetermineBaseTerrain(_heightMap[i]);
            TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
            tile.Set(new DrawInfoComponent {
                Sprite = sprite,
                Color = color,
                Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                Alpha = 1.0f
            });
        }
        _changeSeason(Season.Winter);
    }

    private void _calculateHumidity() {
        // 1. Left to right pass
        for (int y = 0; y < mapHeight; y++) {
            float moisture = _initialHumidityMap[y];  // Start with initial moisture level for each row

            for (int x = 0; x < mapWidth; x++) {
                int index = y * mapWidth + x;  // Calculate the index for the current tile

                float temperature = _temperatureMap[index];
                float moistureCapacity = _calculateMoistureCapacity(temperature);
                float height = _heightMap[index];

                // If the tile is water, add extra moisture to simulate evaporation
                if (height < 0.30f) {
                    moisture += 20.0f;  // Increase moisture for water tiles (adjust this value as needed)
                }

                // Handle mountains
                if (height > _mountainThreshold) {
                    _finalHumidityMap[index] = Math.Min(moisture * _percipitationFactor, moistureCapacity);
                    moisture *= _rainShadowEffect;  // Rain shadow effect
                } else {
                    moisture = Math.Min(moisture, moistureCapacity);
                    _finalHumidityMap[index] = moisture;
                }

                moisture *= _eastwardDissipation;
            }
        }

        // 2. Right to left pass - undecided if we do this or not
        for (int y = 0; y < mapHeight; y++) {
            float moisture = _initialHumidityMap[y];  // Reset initial moisture for each row

            for (int x = mapWidth - 1; x >= 0; x--) {
                int index = y * mapWidth + x;  // Calculate the index for the current tile

                float temperature = _temperatureMap[index];
                float moistureCapacity = _calculateMoistureCapacity(temperature);
                float height = _heightMap[index];

                if (height < 0.30f) {
                    moisture += 20.0f;  // Increase moisture for water tiles
                }

                if (height > _mountainThreshold) {
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], Math.Min(moisture * _percipitationFactor, moistureCapacity));
                    moisture *= _rainShadowEffect;
                } else {
                    moisture = Math.Min(moisture, moistureCapacity);
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], moisture);
                }

                moisture *= _eastwardDissipation;
            }
        }

        // 3. Top to bottom pass
        for (int x = 0; x < mapWidth; x++) {
            float moisture = _initialHumidityMap[x];  // Initial moisture for each column

            for (int y = 0; y < mapHeight; y++) {
                int index = y * mapWidth + x;  // Calculate the index for the current tile

                float temperature = _temperatureMap[index];
                float moistureCapacity = _calculateMoistureCapacity(temperature);
                float height = _heightMap[index];

                if (height < 0.30f) {
                    moisture += 20.0f;  // Increase moisture for water tiles
                }

                if (height > _mountainThreshold) {
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], Math.Min(moisture * _percipitationFactor, moistureCapacity));
                    moisture *= _rainShadowEffect;
                } else {
                    moisture = Math.Min(moisture, moistureCapacity);
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], moisture);
                }

                moisture *= _eastwardDissipation;  // Change this to a vertical dissipation factor if needed
            }
        }

        // 4. Bottom to top pass
        for (int x = 0; x < mapWidth; x++) {
            float moisture = _initialHumidityMap[x];  // Initial moisture for each column

            for (int y = mapHeight - 1; y >= 0; y--) {
                int index = y * mapWidth + x;  // Calculate the index for the current tile

                float temperature = _temperatureMap[index];
                float moistureCapacity = _calculateMoistureCapacity(temperature);
                float height = _heightMap[index];

                if (height < 0.30f) {
                    moisture += 20.0f;  // Increase moisture for water tiles
                }

                if (height > _mountainThreshold) {
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], Math.Min(moisture * _percipitationFactor, moistureCapacity));
                    moisture *= _rainShadowEffect;
                } else {
                    moisture = Math.Min(moisture, moistureCapacity);
                    _finalHumidityMap[index] = Math.Max(_finalHumidityMap[index], moisture);
                }

                moisture *= _eastwardDissipation;  // Consider using a vertical dissipation factor here
            }
        }

        // Optional: Normalize or scale the humidity values between 0 and 100
        for (int i = 0; i < _finalHumidityMap.Length; i++) {
            _finalHumidityMap[i] = Math.Clamp(_finalHumidityMap[i], _minimumHumidity, _maximumHunidty);
        }
    }

    private void _addHumiditySprites() {
        if (_showHumidityMap) {
            for (int i = 0; i < _finalHumidityMap.Length; i++) {
                int row = i / mapWidth;
                int column = i % mapWidth;
                float elevation = _finalHumidityMap[i];
                Entity humidity = _world.CreateEntity();
                humidity.Set(new DrawInfoComponent {
                    Sprite = eSprite.CapitalO,
                    Color = TileTypeHelper.DetermineHumidityColor(_finalHumidityMap[i] * 100.0f),
                    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                    Alpha = 0.450f
                });
                humidityTiles.Add(humidity);
            }
        }
    }

    private void _clearHumiditySprites() {
        foreach (var humTile in humidityTiles) {
            humTile.Dispose();
        }
        humidityTiles.Clear();
    }

    private void _addInitHumiditySprites() {
        if (_showInitHumidity) {
            for (int i = 0; i < _initialHumidityMap.Length; i++) {
                int row = i / mapWidth;
                int column = i % mapWidth;
                Entity humidity = _world.CreateEntity();
                humidity.Set(new DrawInfoComponent {
                    Sprite = eSprite.CapitalQ,
                    Color = TileTypeHelper.DetermineHumidityColor(_initialHumidityMap[i] * 100.0f),
                    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                    Alpha = 0.650f
                });
                _initHumidities.Add(humidity);
            }
        }
    }

    private void _clearInitHumiditySprites() {
        foreach (var humTile in _initHumidities) {
            humTile.Dispose();
        }
        _initHumidities.Clear();
    }

    private float _calculateMoistureCapacity(float temperature) {
        //return (float)Math.Exp(temperature / 10.0f) - 1.0f;  // Adjust parameters to suit your map's temperature scale
        return (float)Math.Exp(temperature / 200.5f) - 1.0f + _baseMoisture;
    }   

    private void _addTemperateSprites() {
        if (_showTemp) {
            for (int i = 0; i < _heightMap.Length; i++) {
                int row = i / mapWidth;
                int column = i % mapWidth;
                float elevation = _heightMap[i];
                Entity temperatureTile = _world.CreateEntity();
                temperatureTile.Set(new DrawInfoComponent {
                    Sprite = TileTypeHelper.DetermineTemperatureTile(_temperatureMap[i]),
                    Color = TileTypeHelper.DetermineTemperatureColor(_temperatureMap[i]),
                    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                    Alpha = 0.650f
                });
                temperatureTiles.Add(temperatureTile);
            }
        }
    }

    private void _clearTemperatureSprites() {
        foreach (var tempTile in temperatureTiles) {
            tempTile.Dispose();
        }
        temperatureTiles.Clear();
    }

    private void _calculateTemperatures() {
        float seasonalOffset = SeasonalTemperatureOffsets[currentSeason];
        for (int i = 0; i < _heightMap.Length; i++) {
            int row = i / mapWidth;
            int column = i % mapWidth;
            float elevation = _heightMap[i];
            float latitudeFactor = row / (float)(mapHeight - 1);
            latitudeFactor = latitudeFactor * latitudeFactor;
            latitudeFactor = 1.0f / (1.0f + MathF.Exp(-10.0f * (latitudeFactor - 0.5f)));
            float baseTemperature = _maxTemp - latitudeFactor * (_maxTemp - _minTemp);
            // Calculate the temperature drop due to elevation.
            float elevationInMeters = elevation * maxElevationInMeters;
            // Calculate the cooling effect based on the elevation.
            float elevationCooling = (elevationInMeters / 1000.0f) * lapseRateF;
            //float adjustedTemperature = baseTemperature - (elevation * _waterCoolingFactor);
            float adjustedTemperature = baseTemperature - elevationCooling;
            if (elevation < _waterElevation) {
                float waterBlendFactor = (elevation / _waterElevation);
                adjustedTemperature = MathHelper.Lerp(_waterTemperature, adjustedTemperature, waterBlendFactor);
            }
            adjustedTemperature += seasonalOffset;
            _temperatureMap[i] = adjustedTemperature;
        }
        _logTemperatureDetails();
    }

    public override void UnloadContent() { }

    private void _changeSeason(Season season) {
        if(currentSeason == season) return;
        currentSeason = season;
        _clearTemperatureSprites();
        _calculateTemperatures();
        _addTemperateSprites();

        _clearHumiditySprites();
        _calculateHumidity();
        _addHumiditySprites();
    }

    public override void Update(GameTime gameTime) {
        _handleCameraMovement();
        if (_inputManager.JustReleased(Keys.Z)) {
            _showTemp = !_showTemp;
            if (!_showTemp) {
                _clearTemperatureSprites();
            } else {
                _addTemperateSprites();
            }
        }
        if (_inputManager.JustReleased(Keys.X)) {
            _showHumidityMap = !_showHumidityMap;
            if (!_showHumidityMap) {
                _clearHumiditySprites();
            } else {
                _addHumiditySprites();
            }
        }
        if (_inputManager.JustReleased(Keys.C)) {
            _showInitHumidity = !_showInitHumidity;
            if (!_showInitHumidity) {
                _clearInitHumiditySprites();
            } else {
                _addInitHumiditySprites();
            }
        }
        if (_inputManager.JustReleased(Keys.D1)) {
            _changeSeason(Season.Winter);
        }
        if (_inputManager.JustReleased(Keys.D2)) {
            _changeSeason(Season.Spring);
        }
        if (_inputManager.JustReleased(Keys.D3)) {
            _changeSeason(Season.Summer);
        }
        if (_inputManager.JustReleased(Keys.D4)) {
            _changeSeason(Season.Autumn);
        }
    }

    public override void Draw(GameTime gameTime) {
        _renderSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    private void _handleCameraMovement() {
        if (_inputManager.IsHeld(Keys.D)) {
            _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.A)) {
            _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.W)) {
            _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.S)) {
            _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y + _camSpeed);
        }
        if (_inputManager.JustPressed(Keys.OemMinus)) {
            _camera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus)) {
            _camera.Zoom += 0.3f;
        }
        if (_inputManager.IsHeld(Keys.Space)) {
            _camSpeed = 16.0f;
        } else {
            _camSpeed = 8.0f;
        }
    }

    private void _logHumidityDetails() {
        Log.Logger.Information($"Season: {currentSeason.ToString()}");
        Log.Logger.Information($"Max Humidity: {_finalHumidityMap.Max()}");
        Log.Logger.Information($"Min Humidity: {_finalHumidityMap.Min()}");
        Log.Logger.Information($"Avg Humidity: {_finalHumidityMap.Average()}");
        Log.Logger.Information($"____________________________________");
    }

    private void _logTemperatureDetails() {
        Log.Logger.Information($"Season: {currentSeason.ToString()}");
        Log.Logger.Information($"Max Temp: {_temperatureMap.Max()}");
        Log.Logger.Information($"Min Temp: {_temperatureMap.Min()}");
        Log.Logger.Information($"Avg Temp: {_temperatureMap.Average()}");
        Log.Logger.Information($"____________________________________");
    }

    private void _logInitHumidityDetails() {
        Log.Logger.Information($"Season: {currentSeason.ToString()}");
        Log.Logger.Information($"Max Init Humidity: {_initialHumidityMap.Max()}");
        Log.Logger.Information($"Min Init Humidity: {_initialHumidityMap.Min()}");
        Log.Logger.Information($"Avg Init Humidity: {_initialHumidityMap.Average()}");
        Log.Logger.Information($"____________________________________");
    }

}
// Seasonal temperature adjustments in °F.
public enum Season {
    Winter,
    Spring,
    Summer,
    Autumn
}
