using DefaultEcs;
using MGDFClone.Core;
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
            _worldGenerator.GenerateWorld();
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
