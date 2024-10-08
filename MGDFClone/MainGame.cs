using MGDFClone.Core;
using MGDFClone.Managers;
using MGDFClone.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGDFClone;
public class MainGame : Game {
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private InputManager _inputManager;
    public GraphicsDeviceManager GraphicsDeviceManager { get { return _graphics; } }
    public SpriteBatch SpriteBatch { get { return _spriteBatch; } }
    public InputManager InputManager { get { return _inputManager; } }
    public MainGame() {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferMultiSampling = true;
        _graphics.SynchronizeWithVerticalRetrace = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.HardwareModeSwitch = false;
        _graphics.IsFullScreen = false;
        Content.RootDirectory = "Assets";
        IsMouseVisible = true;
        _inputManager = new InputManager(this) { UpdateOrder = 0 };
        this.Components.Add(_inputManager);
    }   

    protected override void Initialize() {
        ScreenManager.Instance.Init(this);
        base.Initialize();
            
    }

    protected override void LoadContent() {
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();
        _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
        //ScreenManager.Instance.ChangeScreen<OverworldScreen>();
        //ScreenManager.Instance.ChangeScreen<MapGenerationScreen>();
        //ScreenManager.Instance.ChangeScreen<ClimateGenerationScreen>();
        ScreenManager.Instance.ChangeScreen<WorldGenerationScreenV1>();
        Globals.TEXTURE = Content.Load<Texture2D>("kruggsmash");
        Globals.FONT = Content.Load<SpriteFont>("SDS_8x8");
        base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
        base.Update(gameTime);
        ScreenManager.Instance.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        ScreenManager.Instance.Draw(gameTime);
    }
    protected override void UnloadContent() {
        base.UnloadContent();
    }
}
