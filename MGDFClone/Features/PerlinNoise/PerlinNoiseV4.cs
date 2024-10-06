using System.Security.Cryptography;
using System.Text;

namespace MGDFClone.Features.PerlinNoise; 
public static class PerlinNoiseV4 {
    //private static Random random = GetRNG("Test");
    private static Random random = new Random();

    private static Random GetRNG(string seed) {
        using var algo = SHA1.Create();
        var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seed)));
        return new Random(hash);
    }

    public static float[] GeneratePerlinNoise(int width, int height, int octaves) {            
        float[] perlinNoise = new float[width * height];
        //Generate essentially static
        float[] whiteNoise = _generateWhiteNoise(width, height);
        float[][] smoothNoise = new float [octaves][];
        float persistence = 0.7f;
        for (int i = 0; i < octaves; i++) {
            //We're building up a list of smooth noise. To visualize:
            //octave1: [x,x,x,x,x,x,x,x,x]
            //octave2: [x,x,x,x,x,x,x,x,x]
            //octave3: [x,x,x,x,x,x,x,x,x]
            smoothNoise[i] = _generateSmoothNoise(whiteNoise, width, height, octaves);
        }
        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;
        // Blend noise together
        for (int octave = octaves - 1; octave >= 0; octave--) {
            amplitude *= persistence;
            totalAmplitude += amplitude;

            // For each position in the grid, calculate the flat index
            for (int index = 0; index < perlinNoise.Length; index++) {
                perlinNoise[index] += smoothNoise[octave][index] * amplitude;
            }
        }
        //Normalize
        for (int index = 0; index < perlinNoise.Length; index++) {
            perlinNoise[index] /= totalAmplitude;
        }
        return perlinNoise;
    }

    public static float[] _generateSmoothNoise(float[] baseNoise, int width, int height, int octave) {
        float[] smoothNoise = new float[width * height];
        //Sample period is "how far out" we're going to look, lower the octave, those closer it looks. The higher the octave the further it looks out.
        int samplePeriod = 1 << octave;
        //Sample frequency decreases as sample period increases
        float sampleFrequency = 1.0f / samplePeriod;
        for(int i = 0; i < width; i++) {
            //The nearest sampling index to the left
            int sample_i0 = i / samplePeriod * samplePeriod;
            //The nearest sampling index to the right
            int sample_i1 = (sample_i0 + samplePeriod) % width; // Wrap around
            //How much of an influence the horizontally neighborgs will have on the current position
            float horizontal_blend = (i - sample_i0) * sampleFrequency;
            for(int j = 0; j < height; j++) {
                //Nearest sampling index to the top
                int sample_j0 = j / samplePeriod * samplePeriod;
                //Neaest sampling index to the bottom
                int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
                //How much of an influence the horizontally neighborgs will have on the current position
                float vertical_blend = (j - sample_j0) * sampleFrequency;

                //Get the indexes
                int topLeftIndex = sample_j0 * width + sample_i0;
                int topRightIndex = sample_j0 * width + sample_i1;
                int bottomLeftIndex = sample_j1 * width + sample_i0;
                int bottomRightIndex = sample_j1 * width + sample_i1;

                // Blend the top two corners
                float top = _interpolate(baseNoise[topLeftIndex], baseNoise[topRightIndex], horizontal_blend);

                // Blend the bottom two corners
                float bottom = _interpolate(baseNoise[bottomLeftIndex], baseNoise[bottomRightIndex], horizontal_blend);

                // Final blend for the smooth noise value at position (i, j)
                smoothNoise[j * width + i] = _interpolate(top, bottom, vertical_blend);

            }
        }
        return smoothNoise;
    }

    private static float[] _generateWhiteNoise(int width, int height) {
        float[] noise = new float[width * height];
        for(int i = 0; i < width * height; i++) {
            noise[i] = (float)random.NextDouble() % 1;
        }
        return noise;
    }
    private static float _interpolate(float x0, float x1, float alpha) {
        return x0 * (1 - alpha) + alpha * x1;
    }
}
