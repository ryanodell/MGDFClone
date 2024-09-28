using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens; 
public class OverworldScreen : ScreenBase {
    private World _world;
    private Camera2D _camera;
    private readonly RenderSystem _renderSystem;
    float[][] perlinNoise;
    int[][] octaveNoise;
    float _camSpeed = 8.0f;
    int _octaves = 5;
    public OverworldScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _world = new World();
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 3.5f;
        _camera.LookAt(Vector2.Zero);
        _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
        perlinNoise = PerlinNoiseV2.GeneratePerlinNoise(300, 500, _octaves);
        octaveNoise = PerlinNoiseV2.GetOctaveIndices(perlinNoise, _octaves);
    }

    public override void LoadContent() {
        eTileMapType[][] elevationMap = MapGenerator.GenerateMap(500, 300, _octaves);
        for(int i = 0; i < elevationMap.Length; i++) {
            for (int j = 0; j < elevationMap[i].Length; j++) {
                Entity tileEntity = _world.CreateEntity();
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                eTileMapType tile = elevationMap[i][j];
                switch (tile) {
                    case eTileMapType.DeepWater:
                        sprite = eSprite.Tilde;
                        color = Color.DarkBlue;
                        break;
                    case eTileMapType.Water:
                        sprite = eSprite.Tilde;
                        color = Color.Blue;
                        break;
                    case eTileMapType.Sand:
                        sprite = eSprite.CurhsedRocks2;
                        color = Color.Yellow;
                        break;
                    case eTileMapType.Grass:
                        sprite = eSprite.TallGrass;
                        color = Color.DarkGreen;
                        break;
                    case eTileMapType.Hill:
                        sprite = eSprite.Mountain;
                        color = Color.SaddleBrown;
                        break;
                    case eTileMapType.Mountain:
                        sprite = eSprite.TriangleUp;
                        color = Color.Gray;
                        break;
                    case eTileMapType.Snow:
                        sprite = eSprite.Tilde;
                        color = Color.White;
                        break;
                    default:
                        break;
                }
                tileEntity.Set(new DrawInfoComponent {
                    Sprite = sprite,
                    Color = color,
                    Position = new Vector2(j * Globals.TILE_SIZE, i * Globals.TILE_SIZE)
                });
            }

        }
    }

    public override void Update(GameTime gameTime) {
        _handleCameraMovement();

    }

    public override void Draw(GameTime gameTime) {
        _renderSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
    public override void UnloadContent() {

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
