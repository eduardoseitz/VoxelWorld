﻿using UnityEngine;

namespace DevPenguin.VOXELWORLD
{
    public class NoiseGenerator
    {
        #region Declarations
        private float _smoothness = 0.0015f; // The increment
        private int _octaves = 3; // How many waves will be combined
        private float _persistance = 0.5f; // The amplitude combined with the value
        private int _maxHeight = 100; // Where is the ground level
        private int _seed;

        private float _time;
        private float _height;
        private float _brownianMotion;
        #endregion

        #region Main Methods
        public NoiseGenerator(float smoothness, int octaves, float persistance, int maxHeight, int terrainSeed)
        {
            _smoothness = smoothness;
            _octaves = octaves;
            _persistance = persistance;
            _maxHeight = maxHeight;
            _seed = terrainSeed;
        }
        #endregion

        #region Helper Methods
        public int GetTerrainHeightNoise(int x, int z, float smoothMultiplier)
        {
            _brownianMotion = CalculateFractalBrownianMotion((x + _seed) * (_smoothness * smoothMultiplier), (z + _seed) * (_smoothness * smoothMultiplier), _octaves, _persistance);
            _height = ConvertFrequencyScale(0, _maxHeight, 0, 1, _brownianMotion);

            return (int)_height;
        }

        public float Get3DNoise(float x, float y, float z, float smoothMultiplier)
        {
            float _XY = CalculateFractalBrownianMotion(x * _smoothness * smoothMultiplier, y * _smoothness * smoothMultiplier, _octaves, _persistance);
            float _YZ = CalculateFractalBrownianMotion(y * _smoothness * smoothMultiplier, z * _smoothness * smoothMultiplier, _octaves, _persistance);
            float _XZ = CalculateFractalBrownianMotion(x * _smoothness * smoothMultiplier, z * _smoothness * smoothMultiplier, _octaves, _persistance);
            float _YX = CalculateFractalBrownianMotion(y * _smoothness * smoothMultiplier, x * _smoothness * smoothMultiplier, _octaves, _persistance);
            float _ZY = CalculateFractalBrownianMotion(z * _smoothness * smoothMultiplier, y * _smoothness * smoothMultiplier, _octaves, _persistance);
            float _ZX = CalculateFractalBrownianMotion(z * _smoothness * smoothMultiplier, x * _smoothness * smoothMultiplier, _octaves, _persistance);

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