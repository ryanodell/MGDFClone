using MGDFClone.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGDFClone.Screens {
    public class OverworldScreen : ScreenBase {
        public OverworldScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) { }
                
        public override void LoadContent() {
            
        }        
        public override void Update(GameTime gameTime) {

        }
        public override void Draw(GameTime gameTime) {
            _spriteBatch.Begin();

            _spriteBatch.End();
        }
        public override void UnloadContent() {

        }

    }
}
