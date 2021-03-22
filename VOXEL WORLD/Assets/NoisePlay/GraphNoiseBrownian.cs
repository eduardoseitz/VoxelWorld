using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNoiseBrownian : MonoBehaviour
{
    [SerializeField] float increment = 0.01f;
    [SerializeField] int octaves = 6;
    [SerializeField] float persistance = 0.8f;
    float time = 0;

    void Update()
    {
        time += increment;
        float blueGraph = CalculateFractalBrownianMotion(time, octaves, persistance);
        Grapher.Log(blueGraph, "BrownianPerlin", Color.blue);
    }

    float CalculateFractalBrownianMotion(float time, int octaves, float persistance)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1; // How much each sucessfull wave add to the total value
        float maxValue = 0;
        // Make as many waves(octaves) of perlin noise and combine them
        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(time * frequency, 1) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }
}
