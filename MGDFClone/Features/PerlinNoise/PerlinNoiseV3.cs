namespace MGDFClone.Features.PerlinNoise
{
    public static class PerlinNoiseV3
    {
        static Random random = new Random();
        public static float[] GeneratePerlinNoise(int width, int height, int octaves)
        {
            float[][] baseNoise = GenerateWhiteNoise(width, height);
            float[] returnValue = new float[width * height];
            float[][][] smoothNoise = new float[octaves][][];
            float persistance = 0.7f;
            for (int i = 0; i < octaves; i++)
            {
                smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
            }
            float[][] perlinNoise = GetEmptyArray<float>(width, height); //an array of floats initialised to 0
            float amplitude = 1.0f;
            float totalAmplitude = 0.0f;
            for (int octave = octaves - 1; octave >= 0; octave--)
            {
                amplitude *= persistance;
                totalAmplitude += amplitude;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                    }
                }
            }
            //normalisation
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //We need to normalize to the total aplitude - sort of like clamping to normalized coords.
                    perlinNoise[i][j] /= totalAmplitude;
                }
            }
            for (int i = 0; i < width * height; i++)
            {
                int col = i % width;
                int row = i / width;
                returnValue[i] = perlinNoise[col][row];
            }

            return returnValue;
        }

        public static float[][] GenerateWhiteNoise(int width, int height)
        {
            float[][] noise = GetEmptyArray<float>(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    noise[i][j] = (float)random.NextDouble() % 1;
                }
            }
            return noise;
        }
        public static float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
        {
            int width = baseNoise.Length;
            int height = baseNoise[0].Length;
            float[][] smoothNoise = GetEmptyArray<float>(width, height);
            // * samplePeriod is calculated using bit-shifting (1 << octave), which is equivalent to 2 octave. 
            // * This determines the distance between sampling points. As octave increases, the sample period increases, 
            // * and fewer points are used, resulting in a coarser sampling.
            int samplePeriod = 1 << octave; // calculates 2 ^ k
                                            //sampleFrequency is the inverse of samplePeriod. It controls how much influence nearby points have when blending values,
                                            //with a smaller frequency resulting in a smoother transition between values.
            float sampleFrequency = 1.0f / samplePeriod;
            for (int i = 0; i < width; i++)
            {
                //calculate the horizontal sampling indices
                //sample_i0 is the index of the nearest left sampling point. It is calculated as i / samplePeriod * samplePeriod
                int sample_i0 = i / samplePeriod * samplePeriod;
                int sample_i1 = (sample_i0 + samplePeriod) % width; //wrap around

                //horizontal_blend is calculated as the fractional distance of i between sample_i0 and sample_i1.
                //It determines how much sample_i0 and sample_i1 should contribute to the final value.
                //The formula (i - sample_i0) * sampleFrequency gives a value between 0 and 1, where 0 means i is exactly at sample_i0,
                //and 1 means it is at sample_i1
                float horizontal_blend = (i - sample_i0) * sampleFrequency;

                for (int j = 0; j < height; j++)
                {
                    //calculate the vertical sampling indices
                    //For each row j in the array, similar to the horizontal case, sample_j0 is the nearest upper sampling point.
                    int sample_j0 = j / samplePeriod * samplePeriod;
                    //sample_j1 is the next sampling point below sample_j0 and wraps around the height using the modulus operation.
                    int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
                                                                         //vertical_blend is calculated similarly to horizontal_blend, as the fractional distance between sample_j0 and sample_j1.
                    float vertical_blend = (j - sample_j0) * sampleFrequency;

                    //blend the top two corners
                    //top is the horizontal interpolation between the values at the top-left corner (sample_i0, sample_j0) and top-right corner
                    //(sample_i1, sample_j0), using horizontal_blend as the weight.
                    float top = Interpolate(baseNoise[sample_i0][sample_j0],
                        baseNoise[sample_i1][sample_j0], horizontal_blend);

                    //blend the bottom two corners
                    //bottom is the interpolation between the bottom-left (sample_i0, sample_j1) and bottom-right (sample_i1, sample_j1) values.
                    float bottom = Interpolate(baseNoise[sample_i0][sample_j1],
                        baseNoise[sample_i1][sample_j1], horizontal_blend);

                    //final blend
                    smoothNoise[i][j] = Interpolate(top, bottom, vertical_blend);
                }
            }
            return smoothNoise;
        }

        public static T[][] GetEmptyArray<T>(int width, int height)
        {
            T[][] image = new T[width][];

            for (int i = 0; i < width; i++)
            {
                image[i] = new T[height];
            }

            return image;
        }

        public static float Interpolate(float x0, float x1, float alpha)
        {
            return x0 * (1 - alpha) + alpha * x1;
        }
    }
}
