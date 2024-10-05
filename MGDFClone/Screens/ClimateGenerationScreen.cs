using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.PerlinNoise;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MGDFClone.Screens {
    public class ClimateGenerationScreen : ScreenBase {
        private float _camSpeed = 8.0f;
        private int mapWidth = 75, mapHeight = 75;
        private World _world;
        private Camera2D _camera;
        private readonly RenderSystem _renderSystem;
        private float[] _heightMap;
        private float[] _climateMap;
        public ClimateGenerationScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 3.5f;
            _camera.LookAt(Vector2.Zero);
            _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);

        }

        public override void LoadContent() {
            _heightMap = PerlinNoiseV4.GeneratePerlinNoise(mapWidth, mapHeight, 3);
            _climateMap = PerlinNoiseV4.GeneratePerlinNoise(mapWidth, mapHeight, 4);
            for (int i = 0; i < _heightMap.Length; i++) {
                Entity tile = _world.CreateEntity();
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                int row = i / mapWidth;
                int column = i % mapWidth;
                var tileType = TileTypeHelper.DetermineBaseTerrain(_heightMap[i]);
                TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                tile.Set(new DrawInfoComponent {
                    Sprite = sprite,
                    Color = color,
                    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                    Alpha = 1.0f
                });

                if (_climateMap[i] < 0.5f) {
                    Entity climateTile = _world.CreateEntity();
                    eSprite climateSprite = eSprite.Number5;
                    tile.Set(new DrawInfoComponent {
                        Sprite = climateSprite,
                        Color = Color.Red,
                        Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE),
                        Alpha = 1.0f
                    });

                }
            }
            for (int i = 0; i < _climateMap.Length; i++) {
                int row = i / mapHeight;
                int column = i % mapHeight;
                if (_climateMap[i] < 0.5f) {

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
