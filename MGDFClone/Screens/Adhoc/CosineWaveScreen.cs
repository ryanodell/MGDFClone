using ImGuiNET;
using MGDFClone.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGuiNet;

namespace MGDFClone.Screens;
public class CosineWaveScreen : ScreenBase {
    private float _camSpeed = 8.0f;
    private Camera2D _camera;
    private Texture2D pixelTexture;
    private ImGuiRenderer m_ImGui = MainGame.ImGui;
    public CosineWaveScreen(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, InputManager inputManager) : base(graphics, spriteBatch, inputManager) {
        _camera = new Camera2D(_graphics.GraphicsDevice);
        _camera.Zoom = 1.0f;
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
        int mapHeight = 48;
        float minModerateTemperature = 60.0f;  // Minimum temperature in moderate region
        float maxModerateTemperature = 90.0f;  // Maximum temperature in moderate region
        float minExtremeTemperature = -20.0f;  // Minimum temperature at the extreme (bottom)
        float maxExtremeTemperature = 110.0f;  // Maximum temperature at the extreme (top)
        float moderateRegionHeightFraction = 0.75f;  // 50% of the map is moderate

        float[] baseTemperatures = CalculateBaseTemperature(
            mapHeight,
            minModerateTemperature,
            maxModerateTemperature,
            minExtremeTemperature,
            maxExtremeTemperature,
            moderateRegionHeightFraction
        );
        var test = baseTemperatures;
    }

    private float m_Amplitude = 50.0f;
    private float m_Width = 600.0f;
    private float m_BaseY = 300.0f;
    private float m_StartX = 100.0f;
    float m_EndX = 700.0f;

    public override void Draw(GameTime gameTime) {


        //_spriteBatch.Begin();
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, _camera.GetViewMatrix());

        // Draw cosine curves
        //DrawCosineCurve(_spriteBatch, pixelTexture, Color.Green, 10f, 0.1f, 300);
        //DrawCosineCurve(_spriteBatch, pixelTexture, Color.Red, 50f, 0.5f, 300);
        //DrawCosineCurve(_spriteBatch, pixelTexture, Color.Blue, 30f, 0.2f, 300, (float)Math.PI / 2);

        //DrawCosineArc(_spriteBatch, pixelTexture, Color.Green, 50f, 600f, 300f);
        DrawCosineArc(_spriteBatch, pixelTexture, Color.Green, m_Amplitude);
        float[] results = CalculateBaseTemperaturePerRow(48, m_Amplitude, - 20.0f, 110.0f);
        _spriteBatch.End();
        MainGame.ImGui.BeginLayout(gameTime);
        ImGui.Begin("Cosine Arc Settings");
        ImGui.SliderFloat("Amplitude", ref m_Amplitude, 0.0f, 500.0f);
        ImGui.End();
        MainGame.ImGui.EndLayout();
    }

    float[] CalculateTemperatureWithAmplitude(int mapHeight, float minTemperature, float maxTemperature, float amplitude) {
        float[] rowTemperatureMap = new float[mapHeight];  // Array to store base temperature for each row
        float midTemperature = (minTemperature + maxTemperature) / 2.0f;  // Midpoint of temperature range
        float temperatureRange = (maxTemperature - minTemperature) / 2.0f;  // Half the range

        // Loop through each row (Y-coordinate in the map)
        for (int row = 0; row < mapHeight; row++) {
            // Normalize the row position to a value between 0 and 1 (range [0, pi])
            float normalizedRow = (float)row / (mapHeight - 1);  // Range [0, 1]
            float cosineValue = (float)Math.Cos(normalizedRow * Math.PI);  // Cosine between [1, -1]

            // Apply the amplitude to control the steepness of the curve (1 = normal steepness)
            float scaledCosine = cosineValue * amplitude;

            // Map the cosine result into the desired temperature range
            float temperature = midTemperature + scaledCosine * temperatureRange;

            // Store the result
            rowTemperatureMap[row] = temperature;
        }

        return rowTemperatureMap;
    }

    float[] CalculateBaseTemperature(int mapHeight, float minModerateTemperature, float maxModerateTemperature, float minExtremeTemperature, float maxExtremeTemperature, float moderateRegionHeightFraction) {
        float[] rowTemperatureMap = new float[mapHeight];  // Array to store base temperature for each row

        int moderateRegionStart = (int)(mapHeight * (1.0f - moderateRegionHeightFraction) / 2.0f);  // Start of the moderate region
        int moderateRegionEnd = mapHeight - moderateRegionStart;  // End of the moderate region

        // Loop through each row (Y-coordinate in the map)
        for (int row = 0; row < mapHeight; row++) {
            if (row >= moderateRegionStart && row <= moderateRegionEnd) {
                // Moderate region (in the middle)
                rowTemperatureMap[row] = MathHelper.Lerp(minModerateTemperature, maxModerateTemperature, (float)(row - moderateRegionStart) / (moderateRegionEnd - moderateRegionStart));
            } else if (row < moderateRegionStart) {
                // Bottom extreme region - moving towards colder temperatures
                float distanceFromEdge = (float)row / moderateRegionStart;
                rowTemperatureMap[row] = MathHelper.Lerp(minExtremeTemperature, minModerateTemperature, distanceFromEdge);
            } else {
                // Top extreme region - moving towards hotter temperatures
                float distanceFromEdge = (float)(row - moderateRegionEnd) / (mapHeight - moderateRegionEnd);
                rowTemperatureMap[row] = MathHelper.Lerp(maxModerateTemperature, maxExtremeTemperature, distanceFromEdge);
            }
        }

        return rowTemperatureMap;
    }


    //float[] CalculateBaseTemperature(int mapHeight, float minModerateTemperature, float maxModerateTemperature, float minExtremeTemperature, float maxExtremeTemperature, float moderateRegionHeightFraction) {
    //    float[] rowTemperatureMap = new float[mapHeight];  // Array to store base temperature for each row

    //    int moderateRegionStart = (int)(mapHeight * (1.0f - moderateRegionHeightFraction) / 2.0f);  // Start of the moderate region
    //    int moderateRegionEnd = mapHeight - moderateRegionStart;  // End of the moderate region

    //    // Loop through each row (Y-coordinate in the map)
    //    for (int row = 0; row < mapHeight; row++) {
    //        if (row >= moderateRegionStart && row <= moderateRegionEnd) {
    //            // Moderate region (in the middle)
    //            rowTemperatureMap[row] = MathHelper.Lerp(minModerateTemperature, maxModerateTemperature, (float)(row - moderateRegionStart) / (moderateRegionEnd - moderateRegionStart));
    //        } else if (row < moderateRegionStart) {
    //            // Bottom extreme region
    //            float distanceFromEdge = (float)(moderateRegionStart - row) / moderateRegionStart;
    //            rowTemperatureMap[row] = MathHelper.Lerp(minExtremeTemperature, minModerateTemperature, distanceFromEdge);
    //        } else {
    //            // Top extreme region
    //            float distanceFromEdge = (float)(row - moderateRegionEnd) / (mapHeight - moderateRegionEnd);
    //            rowTemperatureMap[row] = MathHelper.Lerp(maxModerateTemperature, maxExtremeTemperature, distanceFromEdge);
    //        }
    //    }

    //    return rowTemperatureMap;
    //}



    float[] CalculateModerateTemperaturePerRow(int mapHeight, float minTemperature, float maxTemperature) {
        float[] rowTemperatureMap = new float[mapHeight];  // Array to store base temperature for each row
        float temperatureRange = maxTemperature - minTemperature;  // Total temperature range
        float baseY = (minTemperature + maxTemperature) / 2.0f;  // Center of the temperature range
        float startX = 0;  // Starting row
        float endX = mapHeight - 1;  // Ending row
        float arcLength = endX - startX;  // Total vertical distance of the arc (number of rows)

        // Loop through each row (Y-coordinate in the map)
        for (int row = 0; row < mapHeight; row++) {
            // Map the row to the cosine function's input range (0 to pi)
            float normalizedX = (row - startX) / arcLength * (float)Math.PI;  // Map row to range [0, pi]

            // Calculate the corresponding y-value using cosine (Cosine will range from -1 to 1)
            float cosineValue = (float)Math.Cos(normalizedX);

            // The cosine curve will naturally create a “moderate” effect in the middle and extreme values near the edges

            // Normalize cosine to the temperature range [minTemperature, maxTemperature]
            float baseTemperature = baseY + (temperatureRange / 2.0f) * cosineValue;

            // Store the base temperature for this row
            rowTemperatureMap[row] = baseTemperature;
        }

        return rowTemperatureMap;  // Return the array of base temperatures per row
    }


    private float[] CalculateBaseTemperaturePerRow(int mapHeight, float amplitude, float minTemperature, float maxTemperature) {
        float[] rowTemperatureMap = new float[mapHeight];  // Array to store base temperature for each row
        float baseY = (minTemperature + maxTemperature) / 2.0f;  // Center of the temperature range
        float startX = 0;  // Starting x-coordinate (for calculation purposes)
        float endX = mapHeight - 1;  // Map height determines the length of the cosine arc
        float arcLength = endX - startX;  // Total vertical distance of the arc (number of rows)

        // Loop through each row (Y-coordinate in the map)
        for (int row = 0; row < mapHeight; row++) {
            // Map the row to the cosine function's input range (0 to pi)
            float normalizedX = (row - startX) / arcLength * (float)Math.PI;  // Map row to range [0, pi]

            // Calculate the corresponding y-value using cosine (Cosine will range from -1 to 1)
            float cosineValue = (float)Math.Cos(normalizedX);

            // Normalize cosine to the temperature range [minTemperature, maxTemperature]
            float baseTemperature = baseY + amplitude * cosineValue;

            // Store the base temperature for this row
            rowTemperatureMap[row] = baseTemperature;
        }

        return rowTemperatureMap;  // Return the array of base temperatures per row
    }


    void DrawCosineArc(SpriteBatch spriteBatch, Texture2D pixel, Color color, float amplitude) {
        float baseY = 300.0f;
        float startX = 100;  // Starting x-coordinate on screen
        float endX = 700;    // Ending x-coordinate on screen
        float arcLength = endX - startX;  // Total horizontal distance of the arc

        int previousX = (int)startX;
        float previousY = baseY + amplitude * (float)Math.Cos(0);  // Start at Cos(0) = 1

        for (float x = startX; x <= endX; x++) {
            // Map the screen's x-value to the cosine function's input range (0 to pi)
            float normalizedX = (x - startX) / arcLength * (float)Math.PI;  // Map x to range [0, pi]

            // Calculate the corresponding y-value using cosine
            float y = baseY + amplitude * (float)Math.Cos(normalizedX);  // Cos(0) to Cos(pi)

            // Draw a line between (previousX, previousY) and (x, y)
            DrawLine(spriteBatch, pixel, color, previousX, previousY, x, y);

            // Update the previous point
            previousX = (int)x;
            previousY = y;
        }
    }

    void DrawCosineArcAndMapToRows(SpriteBatch spriteBatch, Texture2D pixel, Color color, float amplitude, int numRows, float[] temperatureMap) {
        float baseY = 300.0f;
        float startX = 100;  // Starting x-coordinate on screen
        float endX = 700;    // Ending x-coordinate on screen
        float arcLength = endX - startX;  // Total horizontal distance of the arc

        int previousX = (int)startX;
        float previousY = baseY + amplitude * (float)Math.Cos(0);  // Start at Cos(0) = 1

        // Calculate the step size for each row (equally spaced along X-axis)
        float stepSize = arcLength / (numRows - 1);  // Horizontal step between rows

        for (int row = 0; row < numRows; row++) {
            // Calculate the current x-coordinate for the row
            float x = startX + row * stepSize;

            // Map the screen's x-value to the cosine function's input range (0 to pi)
            float normalizedX = (x - startX) / arcLength * (float)Math.PI;  // Map x to range [0, pi]

            // Calculate the corresponding y-value using cosine
            float y = baseY + amplitude * (float)Math.Cos(normalizedX);  // Cos(0) to Cos(pi)

            // Store the Y-value in the temperatureMap array (could be normalized if needed)
            temperatureMap[row] = y;

            // Draw a line between (previousX, previousY) and (x, y) for visual feedback
            DrawLine(spriteBatch, pixel, color, previousX, previousY, x, y);

            // Update the previous point
            previousX = (int)x;
            previousY = y;
        }
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
