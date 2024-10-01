namespace MGDFClone.Features {
    public static class PerlinNoiseV2 {
        public static float[] GeneratePerlinNoise(int width, int height, int octaveCount) {
            // Generate base white noise
            float[] baseNoise = GenerateWhiteNoise(width, height);

            // Create a flat array to hold each octave's smooth noise
            float[] perlinNoise = new float[width * height];
            float persistance = 0.7f;

            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;

            // Blend noise together for each octave
            for (int octave = 0; octave < octaveCount; octave++) {
                // Generate smooth noise for the current octave
                float[] smoothNoise = GenerateSmoothNoise(baseNoise, width, height, octave);

                // Accumulate Perlin noise values
                amplitude *= persistance;
                totalAmplitude += amplitude;

                for (int i = 0; i < width * height; i++) {
                    perlinNoise[i] += smoothNoise[i] * amplitude;
                }
            }

            // Normalize the Perlin noise values
            for (int i = 0; i < width * height; i++) {
                perlinNoise[i] /= totalAmplitude;
            }

            return perlinNoise;
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

        public static float[] GenerateSmoothNoise(float[] baseNoise, int width, int height, int octave) {
            float[] smoothNoise = new float[width * height];
            int samplePeriod = 1 << octave; // 2 ^ octave
            float sampleFrequency = 1.0f / samplePeriod;

            for (int i = 0; i < width; i++) {
                // Calculate horizontal sampling indices
                int sample_i0 = (i / samplePeriod) * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; // Wrap around
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++) {
                    // Calculate vertical sampling indices
                    int sample_j0 = (j / samplePeriod) * samplePeriod;
                    int sample_j1 = (sample_j0 + samplePeriod) % height; // Wrap around
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    // Calculate indices for baseNoise and smoothNoise
                    int index0 = sample_i0 + sample_j0 * width; // Top-left corner
                    int index1 = sample_i1 + sample_j0 * width; // Top-right corner
                    int index2 = sample_i0 + sample_j1 * width; // Bottom-left corner
                    int index3 = sample_i1 + sample_j1 * width; // Bottom-right corner

                    // Blend the top two corners
                    float top = Interpolate(baseNoise[index0], baseNoise[index1], horizontal_blend);

                    // Blend the bottom two corners
                    float bottom = Interpolate(baseNoise[index2], baseNoise[index3], horizontal_blend);

                    // Final blend
                    smoothNoise[i + j * width] = Interpolate(top, bottom, vertical_blend);
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
