using MGDFClone.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGDFClone.Screens; 
public abstract class ScreenBase {
    protected GraphicsDeviceManager _graphics;
    protected SpriteBatch _spriteBatch;
    protected InputManager _inputManager;
    public ScreenBase(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) { 
        _graphics = graphics;
        _spriteBatch = spriteBatch;
        _inputManager = inputManager;
    }
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
    public abstract void LoadContent();
    public abstract void UnloadContent();
}
