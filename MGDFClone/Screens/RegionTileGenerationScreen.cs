using MGDFClone.Core;
using MGDFClone.Features.MapGen;
using MGDFClone.Features.WorldGen;
using MGDFClone.Managers;
using MGDFClone.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Serilog;
using Serilog.Core;

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
    private int m_ShiftModifier = 8;
    private Vector2 m_SelectorPosition => new Vector2(m_SelectedTile.Column * Globals.TILE_SIZE, m_SelectedTile.Row * Globals.TILE_SIZE);
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
        bool isSheftHeld = _inputManager.IsPressed(Keys.LeftShift) || _inputManager.IsPressed(Keys.RightShift);
        if (_inputManager.JustReleased(Keys.D) || _inputManager.JustReleased(Keys.NumPad6)) {
            if (isSheftHeld) {
                m_SelectedTile.Column += m_ShiftModifier;
            } else {
                m_SelectedTile.Column++;
            }
        }
        if (_inputManager.JustReleased(Keys.A) || _inputManager.JustReleased(Keys.NumPad4)) {
            if (isSheftHeld) {
                m_SelectedTile.Column -= m_ShiftModifier;
            } else {
                m_SelectedTile.Column--;
            }
        }
        if (_inputManager.JustReleased(Keys.NumPad7)) {
            if (isSheftHeld) {
                m_SelectedTile.Column -= m_ShiftModifier;
                m_SelectedTile.Row -= m_ShiftModifier;
            } else {
                m_SelectedTile.Column--;
                m_SelectedTile.Row--;
            }
        }
        if (_inputManager.JustReleased(Keys.NumPad9)) {
            if (isSheftHeld) {
                m_SelectedTile.Column += m_ShiftModifier;
                m_SelectedTile.Row -= m_ShiftModifier;
            } else {
                m_SelectedTile.Column++;
                m_SelectedTile.Row--;
            }
        }
        if (_inputManager.JustReleased(Keys.W) || _inputManager.JustReleased(Keys.NumPad8)) {
            if (isSheftHeld) {
                m_SelectedTile.Row -= m_ShiftModifier;
            } else {
                m_SelectedTile.Row--;
            }
        }
        if (_inputManager.JustReleased(Keys.S) || _inputManager.JustReleased(Keys.NumPad2)) {
            if (isSheftHeld) {
                m_SelectedTile.Row += m_ShiftModifier;
            } else {
                m_SelectedTile.Row++;
            }
        }
        if (_inputManager.JustReleased(Keys.NumPad1)) {
            if (isSheftHeld) {
                m_SelectedTile.Column -= m_ShiftModifier;
                m_SelectedTile.Row += m_ShiftModifier;
            } else {
                m_SelectedTile.Column--;
                m_SelectedTile.Row++;
            }
        }
        if (_inputManager.JustReleased(Keys.NumPad3)) {
            if (isSheftHeld) {
                m_SelectedTile.Column += m_ShiftModifier;
                m_SelectedTile.Row += m_ShiftModifier;
            } else {
                m_SelectedTile.Column++;
                m_SelectedTile.Row++;
            }
        }
        if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
            m_Camera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
            m_Camera.Zoom += 0.3f;
        }
    }
}
