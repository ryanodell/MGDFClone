using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.Managers;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MGDFClone.Screens;
public class WorldInspectorScreen : ScreenBase {
    private Camera2D m_WorldCamera, m_RegionCamera, m_InformationCamera;
    private RenderTarget2D m_WorldRenderTarget, m_RegionRenderTarget, m_InformationTarget;
    Viewport m_WorldViewport, m_RegionViewport, m_InformationViewport;
    private WorldGeneratorV1 _worldGenerator;
    private RegionTile1[] m_RegionTiles;
    int sectionWidth;
    public WorldInspectorScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        m_WorldCamera = new Camera2D(_graphics.GraphicsDevice);
        m_RegionCamera = new Camera2D(_graphics.GraphicsDevice);
        m_InformationCamera = new Camera2D(graphics.GraphicsDevice);
        m_WorldCamera.Zoom = 1.0f;
        m_RegionCamera.Zoom = 1.0f;
        m_InformationCamera.Zoom = 1.0f;

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

    private void _drawWorldTile(RegionTile1[] tiles) {

    }

    public override void LoadContent() {
        sectionWidth = _graphics.PreferredBackBufferWidth / 3;
        //m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        m_WorldRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, sectionWidth, _graphics.PreferredBackBufferHeight);
        m_RegionRenderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        m_InformationTarget = new RenderTarget2D(_graphics.GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        
        m_WorldViewport = new Viewport(0, 0, sectionWidth, _graphics.PreferredBackBufferHeight);
        m_RegionViewport = new Viewport(0, sectionWidth, sectionWidth, _graphics.PreferredBackBufferHeight);
        m_InformationViewport = new Viewport(0, sectionWidth * 2, sectionWidth, _graphics.PreferredBackBufferHeight);
        _worldGenerator.GenerateWorld();
        m_RegionTiles = _worldGenerator.WorldMap.RegionTiles;
    }

    public override void UnloadContent() {

    }

    public override void Update(GameTime gameTime) {
        
    }

    public override void Draw(GameTime gameTime) {
        _graphics.GraphicsDevice.Clear(Color.Black);
        
        _graphics.GraphicsDevice.SetRenderTarget(m_WorldRenderTarget);
        //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_WorldCamera.GetViewMatrix());
        _spriteBatch.Begin();
        //_graphics.GraphicsDevice.Viewport = m_WorldViewport;
        _spriteBatch.Draw(Globals.TEXTURE, Vector2.Zero, new Rectangle(0, 0, Globals.TILE_SIZE * 20, Globals.TILE_SIZE * 20), Color.White);
        _graphics.GraphicsDevice.Clear(Color.Black);
        _spriteBatch.End();


        _graphics.GraphicsDevice.SetRenderTarget(m_RegionRenderTarget);
        //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_RegionCamera.GetViewMatrix());        
        _spriteBatch.Begin();
        //_graphics.GraphicsDevice.Viewport = m_RegionViewport;
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
        _spriteBatch.Draw(m_RegionRenderTarget, new Vector2(sectionWidth, 0), Color.White);
        //_spriteBatch.Draw(m_InformationTarget, new Rectangle(500, 0, 250, _graphics.PreferredBackBufferHeight), Color.White);
        _spriteBatch.End();

        //spriteBatch.Begin();
        //spriteBatch.Draw(renderTarget1, new Rectangle(0, 0, screenWidth, sectionHeight), Color.White);
        //spriteBatch.Draw(renderTarget2, new Rectangle(0, sectionHeight, screenWidth, sectionHeight), Color.White);
        //spriteBatch.Draw(renderTarget3, new Rectangle(0, sectionHeight * 2, screenWidth, sectionHeight), Color.White);
        //spriteBatch.End();
    }
}
