using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MGDFClone.Core; 
public class Camera2D(GraphicsDevice graphicsDevice) {

    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Origin { get; set; }
    public float Zoom { get; set; }
    public Vector2 Center => Position + Origin;

    public void Move(Vector2 direction) {
        Position += Vector2.Transform(direction, Matrix.CreateRotationZ(-Rotation));
    }

    public Vector2 WorldToScreen(Vector2 position) {
        var viewPort = _graphicsDevice.Viewport;
        return Vector2.Transform(position + new Vector2(viewPort.X, viewPort.Y), GetViewMatrix());
    }

    public void LookAt(Vector2 position) {
        Position = position - new Vector2((_graphicsDevice.Viewport.Width / 2.0f) / Zoom, (_graphicsDevice.Viewport.Height / 2.0f) / Zoom);
    }

    public Matrix GetViewMatrix() {
        var viewPort = _graphicsDevice.Viewport;
        float aspectRatio = (float)viewPort.Width / viewPort.Height;
        float targetAspectRatio = 16.0f / 9.0f;
        float scaleX = aspectRatio >= targetAspectRatio ? 1.0f : targetAspectRatio / aspectRatio;
        float scaleY = aspectRatio >= targetAspectRatio ? aspectRatio / targetAspectRatio : 1.0f;
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
            Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom * scaleX, Zoom * scaleY, 1) *
            Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
    }


    /// <summary>
    /// Keeping this around for now, still reading up on aspect ratios and whatnot
    /// </summary>
    /// <returns></returns>
    public Matrix GetViewMatrixNoAspectRatio() {
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
            Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom, Zoom, 1) *
            Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
    }

    public Matrix GetInverseViewMatrix() {
        return Matrix.Invert(GetViewMatrix());
    }
}
