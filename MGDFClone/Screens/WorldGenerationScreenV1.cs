using DefaultEcs;
using ImGuiNET;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.Managers;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;
using Vec2 = System.Numerics.Vector2;
using Vec3 = System.Numerics.Vector3;
using Vec4 = System.Numerics.Vector4;

namespace MGDFClone.Screens {
    public class WorldGenerationScreenV1 : ScreenBase {
        private float _camSpeed = 8.0f;
        private World _world;
        private Camera2D _camera;
        WorldGeneratorV1 _worldGenerator;
        private ImGuiRenderer m_ImGui = MainGame.ImGui;
        private RenderTarget2D m_OverworldRenderTarget;
        private bool m_ShowTemperaturemap = false;
        private bool m_ShowHumidity = true;
        private float m_TemperatureAlpha = 1.0f;
        public WorldGenerationScreenV1(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 3.5f;
            _camera.LookAt(Vector2.Zero);
            _worldGenerator = new WorldGeneratorV1(new WorldGenerationParameters {
                WorldTemperatureParameters = WorldTemperatureParameters.Default,
                ElevationParameters = ElevationParameters.Default,
                ClimateParameters = ClimateParameters.Default
            });
        }

        public override void LoadContent() {
            m_OverworldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            if (_worldGenerator != null && _worldGenerator.WorldMap != null) {
                _worldGenerator.GenerateWorld();
            }
        }

        public override void UnloadContent() {

        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
            _worldGenerator.ApplyTemperature();
        }
        public override void Draw(GameTime gameTime) {
            _graphics.GraphicsDevice.SetRenderTarget(m_OverworldRenderTarget);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.GetViewMatrix());
            if (_worldGenerator != null && _worldGenerator.WorldMap != null) {
                var data = _worldGenerator.WorldMap;
                for (int i = 0; i < data.RegionTiles.Length; i++) {
                    int row = i / data.Width;
                    int column = i % data.Width;
                    Entity tile = _world.CreateEntity();
                    eSprite sprite = eSprite.None;
                    Color color = Color.White;
                    //var tileType = TileTypeHelper.DetermineBaseTerrain(data.RegionTiles[i].Elevation);
                    var tileType = _worldGenerator.DetermineTerrainTile(data.RegionTiles[i].Elevation);
                    TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                    _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
                };
                if (m_ShowTemperaturemap) {
                    for (int i = 0; i < data.RegionTiles.Length; i++) {
                        int row = i / data.Width;
                        int column = i % data.Height;
                        float temperature = data.RegionTiles[i].Temperature;
                        eSprite sprite = TileTypeHelper.DetermineTemperatureTile(temperature);
                        Color color = TileTypeHelper.DetermineTemperatureColor(temperature);
                        _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color * m_TemperatureAlpha, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
                    }
                }
                if (m_ShowHumidity) {
                    for (int i = 0; i < data.RegionTiles.Length; i++) {
                        int row = i / data.Width;
                        int column = i % data.Height;
                        float humidity = data.RegionTiles[i].Humidity;
                        eSprite sprite = eSprite.CapitalO;
                        Color color = TileTypeHelper.DetermineHumidityColor(humidity * 100.0f);
                        _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color * m_TemperatureAlpha, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
                        //Entity humidity = _world.CreateEntity();
                        //humidity.Set(new DrawInfoComponent {
                        //    Sprite = eSprite.CapitalO,
                        //    Color = TileTypeHelper.DetermineHumidityColor(_finalHumidityMap[i] * 100.0f),
                        //    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                        //    Alpha = 0.450f
                        //});
                        //humidityTiles.Add(humidity);
                        //eSprite sprite = TileTypeHelper.DetermineTemperatureTile(humidity);
                        //Color color = TileTypeHelper.DetermineTemperatureColor(humidity);
                        //_spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color * m_TemperatureAlpha, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
                    }
                }
            }
            _spriteBatch.End();
            _graphics.GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();
            _spriteBatch.Draw(m_OverworldRenderTarget, new Vector2(250.0f, 0.0f), Color.White);
            _spriteBatch.End();

            MainGame.ImGui.BeginLayout(gameTime);
            ImGui.Begin("World Generation Parameters");
            WorldGenerationParameters worldGenerationParameters = _worldGenerator.WorldGenerationParameters;
            ElevationParameters elevationParameters = _worldGenerator.WorldGenerationParameters.ElevationParameters;
            WorldTemperatureParameters worldTemperatureParameters = _worldGenerator.WorldGenerationParameters.WorldTemperatureParameters;
            ClimateParameters climateParameters = _worldGenerator.WorldGenerationParameters.ClimateParameters;
            #region BackingFields
            eWorldSize imgui_worldSize = worldGenerationParameters.WorldSize;
            float imgui_WaterElevation = elevationParameters.WaterElevation;
            float imgui_MaxElevationInMeters = elevationParameters.MaxElevationInMeters;
            int imgui_ElevationOctaves = elevationParameters.PerlinOctaves;

            float imgui_minimumTemperature = worldTemperatureParameters.MinimumTemperature;
            float imgui_maximumTemperature = worldTemperatureParameters.MaximumTemperature;
            float imgui_waterCoolingFactor = worldTemperatureParameters.WaterCoolingFactor;
            float imgui_waterTemperature = worldTemperatureParameters.WaterTemperature;
            float imgui_lapseRate = worldTemperatureParameters.LapseRate;

            float imgui_mountainThreshold = climateParameters.MountainThreshold;
            float imgui_percipitationFactor = climateParameters.PercipitationFactor;
            float imgui_rainShadowEffect = climateParameters.RainShadowEffect;
            float imgui_eastwardDissipation = climateParameters.EastwardDissipation;
            float imgui_baseMoisture = climateParameters.BaseMoisture;
            float imgui_minimumHumidity = climateParameters.MinimumHumidity;
            float imgui_maximumHunidty = climateParameters.MaximumHunidty;
            int imgui_climatePerlinOctaves = climateParameters.PerlinOctaves;
            float imgui_climateWaterFactor = climateParameters.WaterFactor;

            #endregion

            ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
            if (ImGui.BeginTabBar("tabBar", tab_bar_flags)) {
                if (ImGui.BeginTabItem("Elevation")) {
                    ImGui.InputInt("Octaves", ref imgui_ElevationOctaves);
                    ImGui.SliderFloat("Water Elevation", ref imgui_WaterElevation, 0.0f, 1.0f);
                    ImGui.InputFloat("Max Elevation", ref imgui_MaxElevationInMeters);
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("World Temperature")) {
                    ImGui.Checkbox("Show", ref m_ShowTemperaturemap);
                    ImGui.SameLine();
                    ImGui.SliderFloat("Alpha", ref m_TemperatureAlpha, 0.0f, 1.0f);
                    ImGui.InputFloat("Min Temp", ref imgui_minimumTemperature);
                    ImGui.InputFloat("Max Temp", ref imgui_maximumTemperature);
                    ImGui.InputFloat("Water Cooling", ref imgui_waterCoolingFactor);
                    ImGui.InputFloat("Water Temp", ref imgui_waterTemperature);
                    ImGui.InputFloat("Lapse Rate", ref imgui_lapseRate);
                    string seasonLabel = worldTemperatureParameters.Season.ToString();
                    string[] seasonComboOptions = Enum.GetNames(typeof(eSeason));
                    int seasonSelectedIndex = 0;
                    for (int i = 0; i < seasonComboOptions.Length; i++) {
                        if (seasonComboOptions[i] == seasonLabel) {
                            seasonSelectedIndex = i;
                        }
                    }
                    if (ImGui.BeginCombo("Season", seasonLabel, ImGuiComboFlags.None)) {
                        for (int i = 0; i < seasonComboOptions.Length; i++) {
                            bool isSelected = (seasonSelectedIndex == i);
                            if (ImGui.Selectable(seasonComboOptions[i], isSelected)) {
                                //imgui_season = (eSeason)Enum.Parse(typeof(eSeason), seasonComboOptions[i]);
                                worldTemperatureParameters.Season = (eSeason)Enum.Parse(typeof(eSeason), seasonComboOptions[i]);
                            }
                            if (isSelected) {
                                ImGui.SetItemDefaultFocus();
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.EndTabItem();
                }                
                if (ImGui.BeginTabItem("Climate")) {
                    ImGui.Checkbox("Show", ref m_ShowHumidity);
                    ImGui.InputFloat("Mtn Threshold", ref imgui_mountainThreshold);
                    ImGui.InputFloat("Percip Factor", ref imgui_percipitationFactor);
                    ImGui.InputFloat("Rain Shadow", ref imgui_rainShadowEffect);
                    ImGui.InputFloat("Eastward Dis", ref imgui_eastwardDissipation);
                    ImGui.InputFloat("Base Moisture", ref imgui_baseMoisture);
                    ImGui.InputFloat("Min Humidity", ref imgui_minimumHumidity);
                    ImGui.InputFloat("Max Humidity", ref imgui_maximumHunidty);
                    ImGui.InputInt("Octaves", ref imgui_climatePerlinOctaves);
                    ImGui.InputFloat("Water Factor", ref imgui_climateWaterFactor);
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            ImGui.SeparatorText("World Parameters");
            string worldsizeLabel = worldGenerationParameters.WorldSize.ToString();
            string[] worlSizeComboOptions = Enum.GetNames(typeof(eWorldSize));
            int worlSizeSelectedIndex = 0;
            for (int i = 0; i < worlSizeComboOptions.Length; i++) {
                if (worlSizeComboOptions[i] == worldsizeLabel) {
                    worlSizeSelectedIndex = i;
                }
            }
            if (ImGui.BeginCombo("WorldSize", worldsizeLabel, ImGuiComboFlags.None)) {
                for (int i = 0; i < worlSizeComboOptions.Length; i++) {
                    bool isSelected = (worlSizeSelectedIndex == i);
                    if (ImGui.Selectable(worlSizeComboOptions[i], isSelected)) {
                        imgui_worldSize = (eWorldSize)Enum.Parse(typeof(eWorldSize), worlSizeComboOptions[i]);
                    }
                    if (isSelected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }


            #region BackingFields
            worldGenerationParameters.WorldSize = imgui_worldSize;

            elevationParameters.WaterElevation = imgui_WaterElevation;
            elevationParameters.MaxElevationInMeters = imgui_MaxElevationInMeters;
            elevationParameters.PerlinOctaves = imgui_ElevationOctaves;

            worldTemperatureParameters.MinimumTemperature = imgui_minimumTemperature;
            worldTemperatureParameters.MaximumTemperature = imgui_maximumTemperature;
            worldTemperatureParameters.WaterCoolingFactor = imgui_waterCoolingFactor;
            worldTemperatureParameters.WaterTemperature = imgui_waterTemperature;
            worldTemperatureParameters.LapseRate = imgui_lapseRate;

            climateParameters.MountainThreshold = imgui_mountainThreshold;
            climateParameters.PercipitationFactor = imgui_percipitationFactor;
            climateParameters.RainShadowEffect = imgui_rainShadowEffect;
            climateParameters.EastwardDissipation = imgui_eastwardDissipation;
            climateParameters.BaseMoisture = imgui_baseMoisture;
            climateParameters.MinimumHumidity = imgui_minimumHumidity;
            climateParameters.MaximumHunidty = imgui_maximumHunidty;
            climateParameters.PerlinOctaves = imgui_climatePerlinOctaves;
            climateParameters.WaterFactor = imgui_climateWaterFactor;
            #endregion
            if (ImGui.Button("Re-Generate World")) {
                _worldGenerator.GenerateWorld();
            }
            ImGui.End();

            MainGame.ImGui.EndLayout();
        }
        private void _handleCameraMovement() {
            if (_inputManager.IsHeld(Keys.D) || _inputManager.IsHeld(Keys.NumPad6)) {
                _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y);
            }
            if (_inputManager.IsHeld(Keys.A) || _inputManager.IsHeld(Keys.NumPad4)) {
                _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y);
            }
            if (_inputManager.IsHeld(Keys.NumPad7)) {
                _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y - _camSpeed);
            }
            if (_inputManager.IsHeld(Keys.NumPad9)) {
                _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y - _camSpeed);
            }
            if (_inputManager.IsHeld(Keys.W) || _inputManager.IsHeld(Keys.NumPad8)) {
                _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y - _camSpeed);
            }
            if (_inputManager.IsHeld(Keys.S) || _inputManager.IsHeld(Keys.NumPad2)) {
                _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y + _camSpeed);
            }
            if (_inputManager.IsHeld(Keys.NumPad1)) {
                _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y + _camSpeed);
            }
            if (_inputManager.IsHeld(Keys.NumPad3)) {
                _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y + _camSpeed);
            }
            if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
                _camera.Zoom -= 0.3f;
            }
            if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
                _camera.Zoom += 0.3f;
            }
            if (_inputManager.IsHeld(Keys.Space)) {
                _camSpeed = 16.0f;
            } else {
                _camSpeed = 8.0f;
            }
        }

    }
}
