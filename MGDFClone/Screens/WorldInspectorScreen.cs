using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.Managers;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens;
public class WorldInspectorScreen : ScreenBase {
    private Camera2D m_WorldCamera, m_RegionCamera, m_InformationCamera, m_FullCamera;
    private RenderTarget2D m_WorldRenderTarget, m_RegionRenderTarget, m_InformationTarget;
    Viewport m_WorldViewport, m_RegionViewport, m_InformationViewport;
    private WorldGeneratorV1 _worldGenerator;
    private RegionTile1[] m_RegionTiles;
    int sectionWidth;
    private WorldTile1 m_SelectedWorldTile;
    private int m_SelectedRow = 0;
    private int m_SelectedColumn = 0;
    private bool m_ShowFull = false;
    private float _camSpeed = 8.0f;
    public WorldInspectorScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        m_WorldCamera = new Camera2D(_graphics.GraphicsDevice);
        m_RegionCamera = new Camera2D(_graphics.GraphicsDevice);
        m_InformationCamera = new Camera2D(graphics.GraphicsDevice);
        m_FullCamera = new Camera2D(graphics.GraphicsDevice);
        m_WorldCamera.Zoom = 1.0f;
        m_RegionCamera.Zoom = 1.0f;
        m_InformationCamera.Zoom = 1.0f;
        m_FullCamera.Zoom = 1.0f;

        m_WorldCamera.LookAt(Vector2.Zero);
        m_RegionCamera.LookAt(Vector2.Zero);
        m_InformationCamera.LookAt(Vector2.Zero);
        _worldGenerator = new WorldGeneratorV1(new WorldGenerationParameters {
            WorldTemperatureParameters = WorldTemperatureParameters.Default,
            ElevationParameters = ElevationParameters.Default,
            ClimateParameters = ClimateParameters.Default,
            WorldTemperatureParametersV2 = WorldTemperatureParametersV2.Default
        });
    }

    private int _getIndex(int column, int row) {
        return (row * _worldGenerator.WorldMap.WorldWidth) + column;
    }

    private void _drawWorldTile(RegionTile1[] tiles) {

    }

    public override void LoadContent() {
        sectionWidth = _graphics.PreferredBackBufferWidth / 3;
        //m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        //m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, sectionWidth, _graphics.PreferredBackBufferHeight);
        //m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, Globals.TILE_SIZE * Globals.REGION_CHUNK_SIZE, _graphics.PreferredBackBufferHeight);
        m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        m_RegionRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        m_InformationTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

        //m_WorldViewport = new Viewport(0, 0, sectionWidth, _graphics.PreferredBackBufferHeight);
        m_WorldViewport = new Viewport(0, 0, Globals.TILE_SIZE * Globals.REGION_CHUNK_SIZE + 1, _graphics.PreferredBackBufferHeight);
        m_RegionViewport = new Viewport(0, 0, sectionWidth, _graphics.PreferredBackBufferHeight / 2);
        m_InformationViewport = new Viewport(0, sectionWidth * 2, sectionWidth, _graphics.PreferredBackBufferHeight);
        _worldGenerator.GenerateWorld();
        m_RegionTiles = _worldGenerator.WorldMap.RegionTiles;
        int index = _getIndex(m_SelectedColumn, m_SelectedRow);
        m_SelectedWorldTile = _worldGenerator.WorldMap.WorldTiles[index];
    }

    public override void UnloadContent() {

    }

    public override void Update(GameTime gameTime) {
        if (m_ShowFull) {
            _handleCameraMovement();
        } else {
            if (_inputManager.JustReleased(Keys.D)) {
                m_SelectedColumn++;
                int index = _getIndex(m_SelectedColumn, m_SelectedRow);
                if (index > _worldGenerator.WorldMap.WorldTiles.Length) {
                    m_SelectedColumn--;
                    return;
                }
                m_SelectedWorldTile = _worldGenerator.WorldMap.WorldTiles[index];
            }
            if (_inputManager.JustReleased(Keys.A)) {
                m_SelectedColumn--;
                int index = _getIndex(m_SelectedColumn, m_SelectedRow);
                if (index < 0) {
                    m_SelectedColumn++;
                    return;
                }
                m_SelectedWorldTile = _worldGenerator.WorldMap.WorldTiles[index];
            }

            if (_inputManager.JustReleased(Keys.W)) {
                m_SelectedRow--;
                int index = _getIndex(m_SelectedColumn, m_SelectedRow);
                if (index < 0) {
                    m_SelectedRow++;
                    return;
                }
                m_SelectedWorldTile = _worldGenerator.WorldMap.WorldTiles[index];
            }
            if (_inputManager.JustReleased(Keys.S)) {
                m_SelectedRow++;
                int index = _getIndex(m_SelectedColumn, m_SelectedRow);
                if (index > _worldGenerator.WorldMap.WorldTiles.Length) {
                    m_SelectedRow--;
                    return;
                }
                m_SelectedWorldTile = _worldGenerator.WorldMap.WorldTiles[index];
            }
        }
        if (_inputManager.JustReleased(Keys.OemTilde)) {
            m_ShowFull = !m_ShowFull;
        }
    }

    public override void Draw(GameTime gameTime) {
        _graphics.GraphicsDevice.Clear(Color.Black);

        if (m_ShowFull) {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_FullCamera.GetViewMatrix());
            var data = _worldGenerator.WorldMap;
            for (int i = 0; i < data.RegionTiles.Length; i++) {
                int row = i / data.Width;
                int column = i % data.Width;
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                var tileType = _worldGenerator.DetermineTerrainTile(data.RegionTiles[i].Elevation);
                TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
            }            
            _spriteBatch.End();

        } else {
            _graphics.GraphicsDevice.SetRenderTarget(m_WorldRenderTarget);
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_WorldCamera.GetViewMatrix());
            _spriteBatch.Begin();
            _graphics.GraphicsDevice.Viewport = m_WorldViewport;
            for (int i = 0; i < m_SelectedWorldTile.RegionTiles.Length; i++) {
                RegionTile1 worldTile = m_SelectedWorldTile.RegionTiles[i];
                int col = i % Globals.REGION_CHUNK_SIZE;
                int row = i / Globals.REGION_CHUNK_SIZE;
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                var tileType = _worldGenerator.DetermineTerrainTile(worldTile.Elevation);
                TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                _spriteBatch.Draw(Globals.TEXTURE, new Vector2(col * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);

            }
            _graphics.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.End();


            _graphics.GraphicsDevice.SetRenderTarget(m_RegionRenderTarget);
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_RegionCamera.GetViewMatrix());        
            _spriteBatch.Begin();
            _graphics.GraphicsDevice.Viewport = m_RegionViewport;
            for (int i = 0; i < _worldGenerator.WorldMap.WorldTiles.Length; i++) {
                WorldTile1 worldTile = _worldGenerator.WorldMap.WorldTiles[i];
                int col = i % _worldGenerator.WorldMap.WorldWidth;
                int row = i / _worldGenerator.WorldMap.WorldWidth;
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                var tileType = _worldGenerator.DetermineTerrainTile(worldTile.AverageElevation);
                TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                _spriteBatch.Draw(Globals.TEXTURE, new Vector2(col * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
            }
            _spriteBatch.Draw(Globals.TEXTURE, new Vector2(m_SelectedColumn * Globals.TILE_SIZE, m_SelectedRow * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(eSprite.CapitalX), Color.Yellow, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
            _graphics.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.End();

            //_graphics.GraphicsDevice.SetRenderTarget(m_InformationTarget);
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_InformationCamera.GetViewMatrix());
            ////_graphics.GraphicsDevice.Viewport = m_InformationViewport;
            //_spriteBatch.Draw(Globals.TEXTURE, Vector2.Zero, new Rectangle(0, 0, Globals.TILE_SIZE, Globals.TILE_SIZE), Color.White);
            //_graphics.GraphicsDevice.Clear(Color.Black);
            //_spriteBatch.End();
            _graphics.GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin();
            _spriteBatch.Draw(m_WorldRenderTarget, new Vector2(0, 0), Color.White);
            _spriteBatch.Draw(m_RegionRenderTarget, new Vector2(sectionWidth + Globals.TILE_SIZE * 4, 0), Color.White);
            //_spriteBatch.Draw(m_InformationTarget, new Rectangle(500, 0, 250, _graphics.PreferredBackBufferHeight), Color.White);
            _spriteBatch.End();

            //spriteBatch.Begin();
            //spriteBatch.Draw(renderTarget1, new Rectangle(0, 0, screenWidth, sectionHeight), Color.White);
            //spriteBatch.Draw(renderTarget2, new Rectangle(0, sectionHeight, screenWidth, sectionHeight), Color.White);
            //spriteBatch.Draw(renderTarget3, new Rectangle(0, sectionHeight * 2, screenWidth, sectionHeight), Color.White);
            //spriteBatch.End();
        }
    }

    private void _handleCameraMovement() {
        if (_inputManager.IsHeld(Keys.D) || _inputManager.IsHeld(Keys.NumPad6)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X + _camSpeed, m_FullCamera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.A) || _inputManager.IsHeld(Keys.NumPad4)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X - _camSpeed, m_FullCamera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.NumPad7)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X - _camSpeed, m_FullCamera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad9)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X + _camSpeed, m_FullCamera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.W) || _inputManager.IsHeld(Keys.NumPad8)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X, m_FullCamera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.S) || _inputManager.IsHeld(Keys.NumPad2)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X, m_FullCamera.Position.Y + _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad1)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X - _camSpeed, m_FullCamera.Position.Y + _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad3)) {
            m_FullCamera.Position = new Vector2(m_FullCamera.Position.X + _camSpeed, m_FullCamera.Position.Y + _camSpeed);
        }
        if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
            m_FullCamera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
            m_FullCamera.Zoom += 0.3f;
        }
        if (_inputManager.IsHeld(Keys.Space)) {
            _camSpeed = 16.0f;
        } else {
            _camSpeed = 8.0f;
        }
    }
}
