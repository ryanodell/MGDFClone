using DefaultEcs;
using DefaultEcs.System;
using MGDFClone.Components;
using MGDFClone.Core;
using MGDFClone.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGDFClone.System;

[With(typeof(DrawInfoComponent))]
public sealed class RenderSystem : AEntitySetSystem<float> {
    private readonly SpriteBatch _spriteBatch;
    private readonly Camera2D _camera;
    public RenderSystem(World world, SpriteBatch spriteBatch, Camera2D camera = null) : base(world) {
        _spriteBatch = spriteBatch;
        _camera = camera;
    }
    protected override void PreUpdate(float state) {
        if (_camera == null) {
            _spriteBatch.Begin();
        } else {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.GetViewMatrix());
        }
    }

    protected override void Update(float state, in Entity entity) {
        ref var drawInfo = ref entity.Get<DrawInfoComponent>();
        _spriteBatch.Draw(Globals.TEXTURE, drawInfo.Position, SpriteSheetManager.GetSourceRectForSprite(drawInfo.Sprite), drawInfo.Color, 0.0f, 
            Vector2.Zero, Vector2.One, SpriteEffects.None, 1.0f);

    }

    protected override void PostUpdate(float state) {
        _spriteBatch.End();
    }
}
