using Serilog;

namespace MGDFClone.Features {
    public static class PerlinNoiseV2 {
        public static float[] GeneratePerlinNoise(int width, int height, int octaveCount) {
            float[] baseNoise = GenerateWhiteNoise(width, height);
            float[] perlinNoise = new float[width * height];
            float persistance = 0.5f;  // Adjust persistence for smoother blending

            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            var logger = new LoggerConfiguration().WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                            .WriteTo.File("perlin_noise_debug.log")
                            .CreateLogger();

            for (int octave = 0; octave < octaveCount; octave++) {
                int frequency = 1 << octave;  // Frequency scaling: 2^octave
                float[] smoothNoise = GenerateSmoothNoise(baseNoise, width, height, frequency);

                LogGrid(smoothNoise, width, height, logger, $"Smooth Noise (Octave {octave})");

                amplitude = MathF.Pow(persistance, octave);  // Reduce amplitude more aggressively
                totalAmplitude += amplitude;

                for (int i = 0; i < width * height; i++) {
                    perlinNoise[i] += smoothNoise[i] * amplitude;
                }
            }

            for (int i = 0; i < width * height; i++) {
                perlinNoise[i] /= totalAmplitude;
            }
            // If needed, clamp the values to ensure they remain in range
            //for (int i = 0; i < width * height; i++) {
            //    perlinNoise[i] = Math.Clamp(perlinNoise[i], 0, 1);
            //}


            LogGrid(perlinNoise, width, height, logger, "Final Perlin Noise");

            return perlinNoise;
        }

        private static void LogGrid(float[] grid, int width, int height, Serilog.ILogger logger, string description) {
            logger.Information(description);
            for (int y = 0; y < height; y++) {
                string row = "";
                for (int x = 0; x < width; x++) {
                    row += $"{grid[x + y * width]:0.00} ";
                }
                logger.Information(row);
            }
            logger.Information(""); // Blank line for spacing
        }

        public static float[] GenerateWhiteNoise(int width, int height) {
            float[] noise = new float[width * height];
            Random random = new Random(); // Instantiate the random number generator

            // Generate random noise values
            for (int i = 0; i < width * height; i++) {
                noise[i] = (float)random.NextDouble();
            }

            return noise;
        }

        private static float[] GenerateSmoothNoise(float[] baseNoise, int width, int height, int frequency) {
            float[] smoothNoise = new float[width * height];

            int samplePeriod = frequency; // Sample points every 'frequency' pixels
            float sampleFrequency = 1.0f / samplePeriod;

            for (int y = 0; y < height; y++) {
                int sampleY0 = (y / samplePeriod) * samplePeriod;
                int sampleY1 = (sampleY0 + samplePeriod) % height; // Wrap around
                float verticalBlend = (y - sampleY0) * sampleFrequency;

                for (int x = 0; x < width; x++) {
                    int sampleX0 = (x / samplePeriod) * samplePeriod;
                    int sampleX1 = (sampleX0 + samplePeriod) % width; // Wrap around
                    float horizontalBlend = (x - sampleX0) * sampleFrequency;

                    // Interpolate between the four sample points
                    float top = Interpolate(baseNoise[sampleY0 * width + sampleX0], baseNoise[sampleY0 * width + sampleX1], horizontalBlend);
                    float bottom = Interpolate(baseNoise[sampleY1 * width + sampleX0], baseNoise[sampleY1 * width + sampleX1], horizontalBlend);

                    smoothNoise[y * width + x] = Interpolate(top, bottom, verticalBlend);
                }
            }

            return smoothNoise;
        }

        public static void ApplyMapFalloff(float[] elevationMap, int width, int height, float islandFactor) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    // Convert (x, y) to a single index in the flat array
                    int index = x + y * width;

                    // Calculate the distance from the center for falloff calculation
                    float distance = _distanceFromCenter(x, y, width, height);

                    // Apply the falloff to the noise value
                    float falloffValue = _falloff(distance, islandFactor);
                    elevationMap[index] *= falloffValue; // Modify the existing noise value
                }
            }
        }

        private static float _distanceFromCenter(int x, int y, int width, int height) {
            float dx = (float)(x - width / 2) / (width / 2);
            float dy = (float)(y - height / 2) / (height / 2);
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // Falloff function: Low values near the edges, high values near the center
        private static float _falloff(float distance, float islandFactor) {
            return 1 - (float)Math.Pow(distance, islandFactor);
        }

        public static float Interpolate(float x0, float x1, float alpha) {
            return x0 * (1 - alpha) + alpha * x1;
        }
    }
}
