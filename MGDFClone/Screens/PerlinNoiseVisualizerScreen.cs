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

namespace MGDFClone.Screens
{
    public class PerlinNoiseVisualizerScreen : ScreenBase {
        private float _basePersistance = 0.7f;
        private float _baseAplitude = 1.0f;
        private float _baseTotalAplitude = 0.0f;

        private Camera2D _camera;
        private World _world;
        float _camSpeed = 8.0f;
        private readonly RenderSystem _renderSystem;
        private bool _step = false;
        private int _width = 150, _height = 150;
        private float[] _baseNoiseBorked;
        private float[] _perlinNoiseBorked;

        private float _persistanceBorked = 0.7f;
        private float _amplitudeBorked = 1.0f;
        private float _totalAmplitudeBorked = 0.0f;
        private int _currentOcataveBorked;
        public PerlinNoiseVisualizerScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
            _world = new World();
            _camera = new Camera2D(_graphics.GraphicsDevice);
            _camera.Zoom = 0.5f;
            _camera.LookAt(new Vector2((_width * 16) / (_height * 16) / 2));
            _camera.Position = new Vector2((_width * 16) / (_height * 16) / 2);
            _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
        }       

        public override void LoadContent() {
            _generateWhiteNoise(_width, _height);
            _perlinNoiseBorked = new float[_width * _height];
        }

        public override void UnloadContent() {

        }

        private void _runBorked() {
            float[] smoothNoise = _generateSmoothNoise(_width, _height, _currentOcataveBorked);
            _amplitudeBorked *= _persistanceBorked;
            _totalAmplitudeBorked += _amplitudeBorked;
            for (int i = 0; i < _width * _height; i++) {
                _perlinNoiseBorked[i] += smoothNoise[i] * _amplitudeBorked;
            }
            for (int i = 0; i < _width * _height; i++) {
                _perlinNoiseBorked[i] /= _totalAmplitudeBorked;
            }            
            for (int i = 0; i < _perlinNoiseBorked.Length; i++) {
                Entity tile = _world.CreateEntity();
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                int row = i / _width;
                int column = i % _height;
                
                float elevation = _perlinNoiseBorked[i];
                switch (elevation) {
                    case <= 0.0f:
                        sprite = eSprite.Orb;
                        break;
                    case <= 0.1f:
                        sprite = eSprite.Number1;
                        color = Color.DarkBlue;
                        break;
                    case <= 0.2f:
                        sprite = eSprite.Number2;
                        color = Color.Blue;
                        break;
                    case <= 0.3f:
                        sprite = eSprite.Number3;
                        color = Color.LightBlue;
                        break;
                    case <= 0.4f:
                        sprite = eSprite.Number4;
                        color = Color.DarkGreen;
                        break;
                    case <= 0.5f:
                        sprite = eSprite.Number5;
                        color = Color.Green;
                        break;
                    case <= 0.6f:
                        sprite = eSprite.Number6;
                        color = Color.DarkGreen;
                        break;
                    case <= 0.7f:
                        sprite = eSprite.Number7;
                        color = Color.DarkRed;
                        break;
                    case <= 0.8f:
                        sprite = eSprite.Number8;
                        color = Color.Red;
                        break;
                    case <= 0.9f:
                        sprite = eSprite.Number9;
                        color = Color.OrangeRed;
                        break;
                    default:
                        sprite = eSprite.Anvil;
                        color = Color.OrangeRed;
                        break;
                }
                tile.Set(new DrawInfoComponent {
                    Sprite = sprite,
                    Color = color,
                    Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE)
                });
            }
            Log.Logger.Debug("Drew Ocave: " + _currentOcataveBorked);
            _currentOcataveBorked++;
        }

        public override void Update(GameTime gameTime) {
            _handleCameraMovement();
            if (_inputManager.JustReleased(Keys.Space)) {
                _step = true;
            }
            if (_step) {
                //Remove all entities
                _reset();
                _runBorked();
                _step = false;
            }
        }

        private void _reset() {
            foreach (var entity in _world.GetEntities().AsEnumerable()) {
                entity.Dispose();
            }
        }

        public override void Draw(GameTime gameTime) {
            _renderSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private float[] _generateSmoothNoiseSuboptimally(int width, int height, int octave) {
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
                    float top = PerlinNoiseV2.Interpolate(_baseNoiseBorked[index0], _baseNoiseBorked[index1], horizontal_blend);

                    // Blend the bottom two corners
                    float bottom = PerlinNoiseV2.Interpolate(_baseNoiseBorked[index2], _baseNoiseBorked[index3], horizontal_blend);

                    // Final blend
                    smoothNoise[i + j * width] = PerlinNoiseV2.Interpolate(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
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
                    float top = PerlinNoiseV2.Interpolate(_baseNoiseBorked[index0], _baseNoiseBorked[index1], horizontal_blend);

                    // Blend the bottom two corners
                    float bottom = PerlinNoiseV2.Interpolate(_baseNoiseBorked[index2], _baseNoiseBorked[index3], horizontal_blend);

                    // Final blend
                    smoothNoise[i + j * width] = PerlinNoiseV2.Interpolate(top, bottom, vertical_blend);
                }
            }

            return smoothNoise;
        }

        private eTileMapType _determineBaseTerrain(float elevationValue) {
            if (elevationValue < 0.20f)
                return eTileMapType.DeepWater; // Low elevation = Water
            if (elevationValue < 0.30f)
                return eTileMapType.Water; // Low elevation = Water
            else if (elevationValue < 0.35f)
                return eTileMapType.Sand; // Slightly higher = Sand (beach)
            else if (elevationValue < 0.50f)
                return eTileMapType.Grass; // Middle = Grasslands
            else if (elevationValue < 0.70f)
                return eTileMapType.Hill; // Middle = Grasslands
            else if (elevationValue < 0.80f)
                return eTileMapType.Mountain; // Higher = Mountain
            else
                return eTileMapType.Snow; // Highest = Snow
        }



        private void _generateWhiteNoise(int width, int height) {
            _baseNoiseBorked = new float[width * height];
            Random random = new Random(); // Instantiate the random number generator

            // Generate random noise values
            for (int i = 0; i < width * height; i++) {
                _baseNoiseBorked[i] = (float)random.NextDouble();
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
