using MGDFClone.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MGDFClone.Screens;
public class CosineWaveScreen : ScreenBase {
    private float _camSpeed = 8.0f;
    private Camera2D _camera;
    private Texture2D pixelTexture;
    public CosineWaveScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 3.5f;
        _camera.LookAt(Vector2.Zero);
    }

    public override void LoadContent() {
        pixelTexture = new Texture2D(_graphics.GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });
    }

    public override void UnloadContent() {

    }

    public override void Update(GameTime gameTime) {
        _handleCameraMovement();
    }

    public override void Draw(GameTime gameTime) {
        

        _spriteBatch.Begin();

        // Draw cosine curves
        DrawCosineCurve(_spriteBatch, pixelTexture, Color.Green, 10f, 0.1f, 300);
        DrawCosineCurve(_spriteBatch, pixelTexture, Color.Red, 50f, 0.5f, 300);
        DrawCosineCurve(_spriteBatch, pixelTexture, Color.Blue, 30f, 0.2f, 300, (float)Math.PI / 2);

        _spriteBatch.End();
    }

    private void DrawCosineCurve(SpriteBatch spriteBatch, Texture2D pixel, Color color, float amplitude, float frequency, float baseY, float phaseShift = 0f) {
        int previousX = 0;
        float previousY = baseY + amplitude * (float)Math.Cos(frequency * previousX + phaseShift);

        for (int x = 1; x < 800; x++) // Assuming you're drawing across 800 pixels width
        {
            float y = baseY + amplitude * (float)Math.Cos(frequency * x + phaseShift);

            // Draw a line between (previousX, previousY) and (x, y)
            DrawLine(spriteBatch, pixel, color, previousX, previousY, x, y);

            // Update the previous point
            previousX = x;
            previousY = y;
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Color color, float x1, float y1, float x2, float y2, int thickness = 1) {
        // Calculate the distance between the two points
        float distance = Vector2.Distance(new Vector2(x1, y1), new Vector2(x2, y2));

        // Calculate the angle between the two points
        float angle = (float)Math.Atan2(y2 - y1, x2 - x1);

        // Draw the pixel texture stretched between the two points
        spriteBatch.Draw(pixel,
            new Vector2(x1, y1),
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(distance, thickness),
            SpriteEffects.None,
            0);
    }

    private void _handleCameraMovement() {
        if (_inputManager.IsHeld(Keys.D) || _inputManager.IsHeld(Keys.NumPad6)) {
            _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.A) || _inputManager.IsHeld(Keys.NumPad4)) {
            _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y);
        }
        if (_inputManager.IsHeld(Keys.NumPad7)) {
            _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad9)) {
            _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.W) || _inputManager.IsHeld(Keys.NumPad8)) {
            _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y - _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.S) || _inputManager.IsHeld(Keys.NumPad2)) {
            _camera.Position = new Vector2(_camera.Position.X, _camera.Position.Y + _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad1)) {
            _camera.Position = new Vector2(_camera.Position.X - _camSpeed, _camera.Position.Y + _camSpeed);
        }
        if (_inputManager.IsHeld(Keys.NumPad3)) {
            _camera.Position = new Vector2(_camera.Position.X + _camSpeed, _camera.Position.Y + _camSpeed);
        }
        if (_inputManager.JustPressed(Keys.OemMinus) || _inputManager.JustPressed(Keys.Subtract) || _inputManager.GetMouseScroll() < 0) {
            _camera.Zoom -= 0.3f;
        }
        if (_inputManager.JustPressed(Keys.OemPlus) || _inputManager.JustPressed(Keys.Add) || _inputManager.GetMouseScroll() > 0) {
            _camera.Zoom += 0.3f;
        }
        if (_inputManager.IsHeld(Keys.Space)) {
            _camSpeed = 16.0f;
        } else {
            _camSpeed = 8.0f;
        }
    }

}
