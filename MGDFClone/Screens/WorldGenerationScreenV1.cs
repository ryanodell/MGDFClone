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
        public WorldGenerationScreenV1(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 3.5f;
            _camera.LookAt(Vector2.Zero);
            _worldGenerator = new WorldGeneratorV1(new WorldGenerationParameters {
                WorldTemperatureParameters = WorldTemperatureParameters.Default,
                ElevationParameters = ElevationParameters.Default
            });

            //_worldGenerator = new WorldGeneratorV1(eWorldSize.Small, eSeason.Winter);
        }

        public override void LoadContent() {
            if (_worldGenerator != null && _worldGenerator.WorldMap != null) {
                _worldGenerator.GenerateWorld();
            }
        }

        public override void UnloadContent() {

        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
        }
        public override void Draw(GameTime gameTime) {
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
            }
            _spriteBatch.End();
            MainGame.ImGui.BeginLayout(gameTime);
            ImGui.Begin("World Generation Parameters");
            ElevationParameters elevationParameters = _worldGenerator.WorldGenerationParameters.ElevationParameters;
            WorldTemperatureParameters worldTemperatureParameters = _worldGenerator.WorldGenerationParameters.WorldTemperatureParameters;
            #region BackingFields
            float imgui_WaterElevation = elevationParameters.WaterElevation;
            float imgui_MaxElevationInMeters = elevationParameters.MaxElevationInMeters;
            int imgui_ElevationOctaves = elevationParameters.PerlinOctaves;

            float imgui_minimumTemperature = worldTemperatureParameters.MinimumTemperature;
            float imgui_maximumTemperature = worldTemperatureParameters.MaximumTemperature;
            float imgui_waterCoolingFactor = worldTemperatureParameters.WaterCoolingFactor;
            float imgui_waterTemperature = worldTemperatureParameters.WaterTemperature;
            float imgui_lapseRate = worldTemperatureParameters.LapseRate;
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
                    ImGui.InputFloat("Min Temp", ref imgui_minimumTemperature);
                    ImGui.InputFloat("Max Temp", ref imgui_maximumTemperature);
                    ImGui.InputFloat("Water Cooling", ref imgui_waterCoolingFactor);
                    ImGui.InputFloat("Water Temp", ref imgui_waterTemperature);
                    ImGui.InputFloat("Lapse Rate", ref imgui_lapseRate);
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
            #region BackingFields
            elevationParameters.WaterElevation = imgui_WaterElevation;
            elevationParameters.MaxElevationInMeters = imgui_MaxElevationInMeters;
            elevationParameters.PerlinOctaves = imgui_ElevationOctaves;

            worldTemperatureParameters.MinimumTemperature = imgui_minimumTemperature;
            worldTemperatureParameters.MaximumTemperature = imgui_maximumTemperature;
            worldTemperatureParameters.WaterCoolingFactor = imgui_waterCoolingFactor;
            worldTemperatureParameters.WaterTemperature = imgui_waterTemperature;
            worldTemperatureParameters.LapseRate = imgui_lapseRate;
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
            if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract)) {
                _camera.Zoom -= 0.3f;
            }
            if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add)) {
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
