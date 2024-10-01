using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Features;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens;
public class MapGenerationScreen : ScreenBase {
    private Camera2D _camera;
    private World _world;
    private readonly RenderSystem _renderSystem;
    float _camSpeed = 8.0f;
    private float[][] m_elevationMap;
    //private int mapWidth = 100, mapHeight = 75;
    private int mapWidth = 3, mapHeight = 3;
    private const int regionSize = 16;

    private float[] m_heightMap;
    public MapGenerationScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _world = new World();
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 3.5f;
        _camera.LookAt(Vector2.Zero);
        _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
    }

    public override void LoadContent() {
        //width,height overworld tiles, which consist of 16x16 regions
        //m_heightMap = PerlinNoiseV2.GeneratePerlinNoise(mapWidth, mapHeight, 3);        
        //for (int i = 0; i < m_heightMap.Length; i++) {
        //    Entity tile = _world.CreateEntity();
        //    eSprite sprite = eSprite.None;
        //    Color color = Color.White;
        //    int row = i / mapWidth;
        //    int column = i % mapWidth;
        //    var tileType = _determineBaseTerrain(m_heightMap[i]);
        //    switch (tileType) {
        //        case eTileMapType.DeepWater:
        //            sprite = eSprite.Water2;
        //            color = Color.DarkBlue;
        //            break;
        //        case eTileMapType.Water:
        //            sprite = eSprite.Water2;
        //            color = Color.Blue;
        //            break;
        //        case eTileMapType.Sand:
        //            sprite = eSprite.CurhsedRocks2;
        //            color = Color.Yellow;
        //            break;
        //        case eTileMapType.Grass:
        //            sprite = eSprite.TallGrass;
        //            color = Color.DarkGreen;
        //            break;
        //        case eTileMapType.SmallTree:
        //            sprite = eSprite.SmallTree;
        //            color = Color.DarkOliveGreen;
        //            break;
        //        case eTileMapType.Forest:
        //            sprite = eSprite.BigTree;
        //            color = Color.Green;
        //            break;
        //        case eTileMapType.Hill:
        //            sprite = eSprite.Mountain;
        //            color = Color.SaddleBrown;
        //            break;
        //        case eTileMapType.Mountain:
        //            sprite = eSprite.TriangleUp;
        //            color = Color.Gray;
        //            break;
        //        case eTileMapType.Snow:
        //            sprite = eSprite.Tilde;
        //            color = Color.White;
        //            break;
        //        default:
        //            break;
        //    }
        //    tile.Set(new DrawInfoComponent {
        //        Sprite = sprite,
        //        Color = color,
        //        Position = new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE)
        //    });
        //}
        m_elevationMap = MapGeneratorV2.GenerateIsland(mapWidth, mapHeight);
        MapGeneratorV2.ApplyMapFalloff(m_elevationMap, 3);
        for (int i = 0; i < mapWidth * regionSize; i++) {
            for (int j = 0; j < mapHeight * regionSize; j++) {
                _addTileToWorld(i, j);
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

    private void _addTileToWorld(int i, int j) {
        Entity tile = _world.CreateEntity();
        eSprite sprite = eSprite.None;
        Color color = Color.White;
        eTileMapType eTileMapType = eTileMapType.None;
        float value = m_elevationMap[i][j];
        eTileMapType = _determineBaseTerrain(value);
        switch (eTileMapType) {
            case eTileMapType.DeepWater:
                sprite = eSprite.Water2;
                color = Color.DarkBlue;
                break;
            case eTileMapType.Water:
                sprite = eSprite.Water2;
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
            case eTileMapType.SmallTree:
                sprite = eSprite.SmallTree;
                color = Color.DarkOliveGreen;
                break;
            case eTileMapType.Forest:
                sprite = eSprite.BigTree;
                color = Color.Green;
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
        tile.Set(new DrawInfoComponent {
            Sprite = sprite,
            Color = color,
            Position = new Vector2(j * Globals.TILE_SIZE, i * Globals.TILE_SIZE)
        });
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
