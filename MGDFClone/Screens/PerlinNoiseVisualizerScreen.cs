using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens {
    public class PerlinNoiseVisualizerScreen : ScreenBase {
        private Camera2D _camera;
        private World _world;
        float _camSpeed = 8.0f;
        private readonly RenderSystem _renderSystem;
        private bool _step = false;
        private int _width = 50, _height = 50;
        private float[] _baseNoise;
        private float[] _perlinNoise;

        private float _persistance = 0.7f;
        private float _amplitude = 1.0f;
        private float _totalAmplitude = 0.0f;
        private int _currentOcatave;
        public PerlinNoiseVisualizerScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 2.5f;
            _camera.LookAt(Vector2.Zero);
            _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
        }       

        public override void LoadContent() {
            _generateWhiteNoise(_width, _height);
            _perlinNoise = new float[_width * _height];
        }

        public override void UnloadContent() {

        }

        private void _run() {
            float[] smoothNoise = _generateSmoothNoise(_width, _height, _currentOcatave);
            _amplitude *= _persistance;
            _totalAmplitude += _amplitude;
            for (int i = 0; i < _width * _height; i++) {
                _perlinNoise[i] += smoothNoise[i] * _amplitude;
            }
            for (int i = 0; i < _width * _height; i++) {
                _perlinNoise[i] /= _totalAmplitude;
            }
            _currentOcatave++;
        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
            if (_inputManager.JustReleased(Keys.Space)) {
                _step = true;
            }
            if (_step) {
                //Remove all entities
                foreach (var entity in _world.GetEntities().AsEnumerable()) {
                    entity.Dispose();
                }



                _step = false;
            }
        }

        public override void Draw(GameTime gameTime) {
            _renderSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private float[] _generateSmoothNoise(int width, int height, int octave) {
            float[] smoothNoise = new float[width * height];
            int samplePeriod = 1 << octave; // 2 ^ octave
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < width; i++) {
                // Calculate horizontal sampling indices
                int sample_i0 = (i / samplePeriod) * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; // Wrap around
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++) {
                    // Calculate vertical sampling indices
                    int sample_j0 = (j / samplePeriod) * samplePeriod;
                    int sample_j1 = (sample_j0 + samplePeriod) % height; // Wrap around
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    // Calculate indices for baseNoise and smoothNoise
                    int index0 = sample_i0 + sample_j0 * width; // Top-left corner
                    int index1 = sample_i1 + sample_j0 * width; // Top-right corner
                    int index2 = sample_i0 + sample_j1 * width; // Bottom-left corner
                    int index3 = sample_i1 + sample_j1 * width; // Bottom-right corner

                    // Blend the top two corners
                    float top = PerlinNoiseV2.Interpolate(_baseNoise[index0], _baseNoise[index1], horizontal_blend);

                    // Blend the bottom two corners
                    float bottom = PerlinNoiseV2.Interpolate(_baseNoise[index2], _baseNoise[index3], horizontal_blend);

                    // Final blend
                    smoothNoise[i + j * width] = PerlinNoiseV2.Interpolate(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
        }

        public void _generateWhiteNoise(int width, int height) {
            _baseNoise = new float[width * height];
            Random random = new Random(); // Instantiate the random number generator

            // Generate random noise values
            for (int i = 0; i < width * height; i++) {
                _baseNoise[i] = (float)random.NextDouble();
            }
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
