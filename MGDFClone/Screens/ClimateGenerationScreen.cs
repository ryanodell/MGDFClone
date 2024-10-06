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

namespace MGDFClone.Screens {
    public class ClimateGenerationScreen : ScreenBase {
        private float _camSpeed = 8.0f;
        private readonly int mapWidth = 50, mapHeight = 50;
        private World _world;
        private Camera2D _camera;
        private readonly RenderSystem _renderSystem;
        private float[] _heightMap;
        //In F
        private float _minTemp = -40.0f;
        private float _maxTemp = 110.0f;
        private float _waterCoolingFactor = 10.0f;
        private float _waterTemperature = 50.0f;
        private float[] _temperatureMap;
        private float _waterElevation = 0.30f;

        // Maximum elevation in meters
        float maxElevationInMeters = 3000.0f;
        // Lapse rate in °C per 1000 meters (standard value).
        float lapseRate = 6.5f;
        // Convert lapse rate to Fahrenheit if using Fahrenheit scale: 6.5°C ≈ 11.7°F
        //float lapseRateF = 13.7f;
        float lapseRateF = 25.7f;
        Season currentSeason = Season.Spring;

        // Define the temperature modifiers for each season.
        // These can be positive or negative depending on how you want the temperature to change.
        Dictionary<Season, float> SeasonalTemperatureOffsets = new Dictionary<Season, float>() {
            { Season.Winter, -20.0f },   // Winter is 20°F colder.
            { Season.Spring, 5.0f },     // Spring is 5°F warmer.
            { Season.Summer, 20.0f },    // Summer is 20°F warmer.
            { Season.Autumn, 0.0f }      // Autumn has no change.
        };

        List<Entity> temperatureTiles = new List<Entity>();
        private bool _showTemp = true;

        public ClimateGenerationScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 3.5f;
            _camera.LookAt(Vector2.Zero);
            _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
            _temperatureMap = new float[mapWidth * mapHeight];
        }

        public override void LoadContent() {
            _heightMap = PerlinNoiseV4.GeneratePerlinNoise(mapWidth, mapHeight, 3);
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
            _calculateTemperatures();
            _addTemperateSprites();
            _logTemperatureDetails();
        }

        private void _logTemperatureDetails() {
            Log.Logger.Information($"Season: {currentSeason.ToString()}");
            Log.Logger.Information($"Max Temp: {_temperatureMap.Max()}");
            Log.Logger.Information($"Min Temp: {_temperatureMap.Min()}");
            Log.Logger.Information($"Avg Temp: {_temperatureMap.Average()}");
            Log.Logger.Information($"____________________________________");
        }

        private void _clearTemperatureSprites() {
            foreach (var tempTile in temperatureTiles) {
                tempTile.Disable();
            }
            temperatureTiles.Clear();
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
                        Alpha = 1.0f
                    });
                    temperatureTiles.Add(temperatureTile);
                }
            }
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


        public override void UnloadContent() {

        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
            if(_inputManager.JustReleased(Keys.OemTilde)) {
                _showTemp = !_showTemp;
                if (!_showTemp) {
                    _clearTemperatureSprites();
                } else {
                    _addTemperateSprites();
                }                
            }
            if (_inputManager.JustReleased(Keys.D1)) {
                _clearTemperatureSprites();
                currentSeason = Season.Winter;
                _calculateTemperatures();
                _addTemperateSprites();
            }
            if (_inputManager.JustReleased(Keys.D2)) {
                _clearTemperatureSprites();
                currentSeason = Season.Spring;
                _calculateTemperatures();
                _addTemperateSprites();
            }
            if (_inputManager.JustReleased(Keys.D3)) {
                _clearTemperatureSprites();
                currentSeason = Season.Summer;
                _calculateTemperatures();
                _addTemperateSprites();
            }
            if (_inputManager.JustReleased(Keys.D4)) {
                _clearTemperatureSprites();
                currentSeason = Season.Autumn;
                _calculateTemperatures();
                _addTemperateSprites();
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
    }
    // Seasonal temperature adjustments in °F.
    public enum Season {
        Winter,
        Spring,
        Summer,
        Autumn
    }

}
