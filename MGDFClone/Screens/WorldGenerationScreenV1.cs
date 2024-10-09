using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens {
    public class WorldGenerationScreenV1 : ScreenBase {
        private float _camSpeed = 8.0f;
        private World _world;
        private Camera2D _camera;
        private readonly RenderSystem _renderSystem;
        WorldGeneratorV1 _worldGenerator;
        public WorldGenerationScreenV1(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 3.5f;
            _camera.LookAt(Vector2.Zero);
            _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
            _worldGenerator = new WorldGeneratorV1(eWorldSize.Small, eSeason.Winter);
        }       

        public override void LoadContent() {
            if (_worldGenerator != null && _worldGenerator.WorldMap != null) {
                _worldGenerator.GenerateWorld();
                var data = _worldGenerator.WorldMap;
                for (int i = 0; i < data.RegionTiles.Length; i++) {
                    int row = i / data.Width;
                    int column = i % data.Width;
                    Entity tile = _world.CreateEntity();
                    eSprite sprite = eSprite.None;
                    Color color = Color.White;
                    var tileType = TileTypeHelper.DetermineBaseTerrain(data.RegionTiles[i].Elevation);
                    TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                    tile.Set(new DrawInfoComponent {
                        Sprite = sprite,
                        Color = color,
                        Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                        Alpha = 1.0f
                    });
                }
            }
        }

        public override void UnloadContent() {

        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
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
}
