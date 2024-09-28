using DefaultEcs;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGDFClone.Screens; 
public class OverworldScreen : ScreenBase {
    private World _world;
    private Camera2D _camera;
    private readonly RenderSystem _renderSystem;
    public OverworldScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _world = new World();
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 3.5f;
        _camera.LookAt(Vector2.Zero);
        _renderSystem = new RenderSystem(_world, _spriteBatch, _camera);
    }

    public override void LoadContent() {
        Entity test = _world.CreateEntity();
        test.Set(new DrawInfoComponent {
            Color = Color.Red,
            Position = Vector2.Zero,
            Sprite = eSprite.Block3D
        });
    }        
    public override void Update(GameTime gameTime) {

    }
    public override void Draw(GameTime gameTime) {
        _renderSystem.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }
    public override void UnloadContent() {

    }

}
