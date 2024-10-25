using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.Managers;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens;
public class RegionTileGenerationScreen : ScreenBase {
    private WorldGeneratorV1 m_WorldGenerator;
    private Camera2D m_Camera;
    private float m_CamSpeed = 8.0f;
    private (int Column, int Row) m_SelectedTile = (0, 0);
    private int m_SelectedColSize = 1;
    private int m_SelectedRowSize = 1;
    private const float m_BlinkTime = 500;
    private float m_CurrentBlinkTimer = m_BlinkTime;
    private bool m_SelectorVisible = true;
    private Vector2 m_SelectorPosition => new Vector2(m_SelectedTile.Column * Globals.TILE_SIZE, m_SelectedTile.Row* Globals.TILE_SIZE);
    public RegionTileGenerationScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        m_Camera = new Camera2D(_graphics.GraphicsDevice);
        m_Camera.Zoom = 1.0f;
        m_Camera.LookAt(Vector2.Zero);
        m_WorldGenerator = new WorldGeneratorV1(new WorldGenerationParameters {
            WorldTemperatureParameters = WorldTemperatureParameters.Default,
            ElevationParameters = ElevationParameters.Default,
            ClimateParameters = ClimateParameters.Default,
            WorldTemperatureParametersV2 = WorldTemperatureParametersV2.Default
        });
    }

    public override void LoadContent() {
        m_WorldGenerator.GenerateWorld();
    }

    public override void UnloadContent() {

    }

    public override void Update(GameTime gameTime) {
        //_handleCameraMovement();
        _handleSelector();
        m_Camera.LookAt(m_SelectorPosition);
        m_WorldGenerator.ApplyTemperature();
        m_WorldGenerator.ApplyHumidity();
        m_WorldGenerator.ApplyVegitation();
        m_CurrentBlinkTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (m_CurrentBlinkTimer < 0) {
            m_CurrentBlinkTimer = m_BlinkTime;
            m_SelectorVisible = !m_SelectorVisible;
        }
    }

    public override void Draw(GameTime gameTime) {
        _graphics.GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, m_Camera.GetViewMatrix());
        var data = m_WorldGenerator.WorldMap;
        for (int i = 0; i < data.RegionTiles.Length; i++) {
            int row = i / data.Width;
            int column = i % data.Width;
            RegionTile1 regionTile = data.RegionTiles[i];
            eBiome biome = regionTile.Biome;
            float vegitation = regionTile.Vegitation;
            if (vegitation > 0.50f && regionTile.Elevation > m_WorldGenerator.WorldGenerationParameters.ElevationParameters.WaterElevation
                    && regionTile.Elevation < m_WorldGenerator.WorldGenerationParameters.ElevationParameters.WaterElevation
                        + m_WorldGenerator.WorldGenerationParameters.ElevationParameters.WaterToSandOffset + m_WorldGenerator.WorldGenerationParameters.ElevationParameters.SandToGrassOffet
                            + m_WorldGenerator.WorldGenerationParameters.ElevationParameters.GrassToHillOffset) {
                eSprite sprite = BiomeManagerV1.GetSpriteForBiome(biome);
                Color color = Color.Green;
                if (regionTile.Temperature < 10.0f) {
                    color = Color.White;
                }
                _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
            } else {
                eSprite sprite = eSprite.None;
                Color color = Color.White;
                var tileType = m_WorldGenerator.DetermineTerrainTile(data.RegionTiles[i].Elevation);
                TileTypeHelper.SetSpriteData(ref sprite, ref color, tileType);
                if (regionTile.Temperature < 0.0f) {
                    color = Color.White;
                }
                _spriteBatch.Draw(Globals.TEXTURE, new Vector2(column * Globals.TILE_SIZE, row * Globals.TILE_SIZE), SpriteSheetManager.GetSourceRectForSprite(sprite), color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);
            }
        }
        if (m_SelectorVisible) {
            _spriteBatch.Draw(Globals.TEXTURE, m_SelectorPosition, SpriteSheetManager.GetSourceRectForSprite(eSprite.CapitalX), Color.Yellow);
        }
        _spriteBatch.End();
    }

    private void _handleSelector() {
        if (_inputManager.JustReleased(Keys.D) || _inputManager.JustReleased(Keys.NumPad6)) {
            m_SelectedTile.Column++;
        }
        if (_inputManager.JustReleased(Keys.A) || _inputManager.JustReleased(Keys.NumPad4)) {
            m_SelectedTile.Column--;
        }
        if (_inputManager.JustReleased(Keys.NumPad7)) {
            m_SelectedTile.Column--;
            m_SelectedTile.Row--;
        }
        if (_inputManager.JustReleased(Keys.NumPad9)) {
            m_SelectedTile.Column++;
            m_SelectedTile.Row--;
        }
        if (_inputManager.JustReleased(Keys.W) || _inputManager.JustReleased(Keys.NumPad8)) {
            m_SelectedTile.Row--;
        }
        if (_inputManager.JustReleased(Keys.S) || _inputManager.JustReleased(Keys.NumPad2)) {
            m_SelectedTile.Row++;
        }
        if (_inputManager.JustReleased(Keys.NumPad1)) {
            m_SelectedTile.Column--;
            m_SelectedTile.Row++;
        }
        if (_inputManager.JustReleased(Keys.NumPad3)) {
            m_SelectedTile.Column++;
            m_SelectedTile.Row++;
        }
        if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
            m_Camera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
            m_Camera.Zoom += 0.3f;
        }
    }

    private void _handleCameraMovement() {
        if (_inputManager.IsHeld(Keys.D) || _inputManager.IsHeld(Keys.NumPad6)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X + m_CamSpeed, m_Camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.A) || _inputManager.IsHeld(Keys.NumPad4)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X - m_CamSpeed, m_Camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.NumPad7)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X - m_CamSpeed, m_Camera.Position.Y - m_CamSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad9)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X + m_CamSpeed, m_Camera.Position.Y - m_CamSpeed);
        }
        if (_inputManager.IsHeld(Keys.W) || _inputManager.IsHeld(Keys.NumPad8)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X, m_Camera.Position.Y - m_CamSpeed);
        }
        if (_inputManager.IsHeld(Keys.S) || _inputManager.IsHeld(Keys.NumPad2)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X, m_Camera.Position.Y + m_CamSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad1)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X - m_CamSpeed, m_Camera.Position.Y + m_CamSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad3)) {
            m_Camera.Position = new Vector2(m_Camera.Position.X + m_CamSpeed, m_Camera.Position.Y + m_CamSpeed);
        }
        if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
            m_Camera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
            m_Camera.Zoom += 0.3f;
        }
        if (_inputManager.IsHeld(Keys.Space)) {
            m_CamSpeed = 16.0f;
        } else {
            m_CamSpeed = 8.0f;
        }
    }
}
