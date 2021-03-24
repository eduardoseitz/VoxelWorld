using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class NoiseGenerator
    {
        #region Declarations
        private int _seed;

        private float _height;
        private float _brownianMotion;
        #endregion

        #region Main Methods
        public NoiseGenerator(int terrainSeed)
        {
            _seed = terrainSeed;
        }
        #endregion

        #region Helper Methods
        public int GetTerrainHeightNoise(int x, int z, float smoothness, int octaves, float persistance, int groundHeight)
        {
            _brownianMotion = CalculateFractalBrownianMotion((x + _seed) * (smoothness), (z + _seed) * (smoothness), octaves, persistance);
            _height = ConvertFrequencyScale(0, groundHeight, 0, 1, _brownianMotion);

            return (int)_height;
        }

        public float Get3DNoise(float x, float y, float z, float smoothness, int octaves, float persistance)
        {
            float _XY = CalculateFractalBrownianMotion(x * smoothness, y * smoothness, octaves, persistance);
            float _YZ = CalculateFractalBrownianMotion(y * smoothness, z * smoothness, octaves, persistance);
            float _XZ = CalculateFractalBrownianMotion(x * smoothness, z * smoothness, octaves, persistance);
            float _YX = CalculateFractalBrownianMotion(y * smoothness, x * smoothness, octaves, persistance);
            float _ZY = CalculateFractalBrownianMotion(z * smoothness, y * smoothness, octaves,  persistance);
            float _ZX = CalculateFractalBrownianMotion(z * smoothness, x * smoothness, octaves, persistance);

            return (_XY + _YZ + _XZ + _YX + _ZY + _ZX) / 6f;
        }

        private float ConvertFrequencyScale(float newMin, float newMax, float oldMin, float oldMax, float value)
        {
            return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(oldMin, oldMax, value));
        }

        private float CalculateFractalBrownianMotion(float x, float z, int octaves, float persistance)
        {
            float _frequency = 1; // Higher frequencies make the wave periods shorter (zoomed out)
            float _amplitude = 1; // How much each sucessfull wave add to the total value
            float _maxValue = 0;
            float _total = 0;

            // Make as many waves(octaves) of perlin noise and combine them
            for (int i = 0; i < octaves; i++)
            {
                _total += Mathf.PerlinNoise(x * _frequency, z * _frequency) * _amplitude;

                _maxValue += _amplitude;

                _amplitude *= persistance;
                _frequency *= 2;
            }

            return _total / _maxValue;
        }
        #endregion
    }
}
