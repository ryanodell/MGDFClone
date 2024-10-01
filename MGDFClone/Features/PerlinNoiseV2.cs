using System;
namespace MGDFClone.Features;
public static class PerlinNoiseV2 {
    public static float[] GeneratePerlinNoise(float[] baseNoise, int width, int height, int octaveCount) {
        float[][] smoothNoise = new float[octaveCount][]; // Use jagged array to hold smooth noises temporarily.
        float persistance = 0.7f;

        // Generate smooth noise for each octave.
        for (int i = 0; i < octaveCount; i++) {
            smoothNoise[i] = GenerateSmoothNoise(baseNoise, width, height, i);
        }

        // Initialize the Perlin noise map as a flat array
        float[] perlinNoise = new float[width * height];
        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;

        // Blend the noise together
        for (int octave = octaveCount - 1; octave >= 0; octave--) {
            amplitude *= persistance;
            totalAmplitude += amplitude;
            for (int i = 0; i < width * height; i++) {
                perlinNoise[i] += smoothNoise[octave][i] * amplitude;
            }
        }

        // Normalization step
        for (int i = 0; i < width * height; i++) {
            perlinNoise[i] /= totalAmplitude;
        }

        return perlinNoise;
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

    public static float Interpolate(float x0, float x1, float alpha) {
        return x0 * (1 - alpha) + alpha * x1;
    }
}
