using MGDFClone.Screens;
using Microsoft.Xna.Framework;

namespace MGDFClone.Managers; 
public class ScreenManager {
    private static ScreenManager _instance;
    private MainGame _game;
    public ScreenManager() { }
    public ScreenBase CurrentScreen { get; private set; }
    public static ScreenManager Instance {
        get {
            if (_instance == null) {
                _instance = new ScreenManager();
            }
            return _instance;
        }
    }
    public void Init(MainGame game) {
        _game = game;
    }

    public void ChangeScreen<T>() where T : ScreenBase {
        if (_game == null) {
            throw new InvalidOperationException("ScreenManager has not been initialized. Call Init first");
        }
        var newScreen = Activator.CreateInstance(typeof(T), _game.GraphicsDeviceManager, _game.SpriteBatch, _game.InputManager) as T;
        CurrentScreen?.UnloadContent();
        CurrentScreen = newScreen;
        CurrentScreen?.LoadContent();
    }
    public void LoadContent() {
        CurrentScreen?.LoadContent();
    }

    public void Update(GameTime gameTime) {
        CurrentScreen?.Update(gameTime);
    }

    public void Draw(GameTime gameTime) {
        CurrentScreen?.Draw(gameTime);
    }

    public void UnloadContent() {
        CurrentScreen?.UnloadContent();
    }
}
