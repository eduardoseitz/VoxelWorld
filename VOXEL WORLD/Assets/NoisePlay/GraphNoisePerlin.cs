using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNoisePerlin : MonoBehaviour 
{
	float time = 0;
    float highFrequency = 0.01f; // Higher frequency is more plain

    float time1 = 0;
    float lowFrequecy = 0.001f; // Short frequency is more plain

    float time2; // Combining both and we get a frequency that has both planices and hills

    void Update () 
	{
        time += highFrequency;
        float yellowGraph = Mathf.PerlinNoise(time, 1);
        //Grapher.Log(yellowGraph, "ShortFrequencyPerlin", Color.yellow);

        time1 += lowFrequecy;
        float greenGraph = Mathf.PerlinNoise(time1, 1);
        //Grapher.Log(greenGraph, "HighFrequencyPerlin", Color.green);

        time2 = time + time1; // Combine the 2 frequencies
        time2 = time / 2; // Average the value to normalized it
        float redGraph = CovertFrequencyScale(0, 32, 0, 1, time2); // Change frequecy range from 0 up to 1 to 0 up to 32
        Grapher.Log(redGraph, "CombinedFrequencyPerlin", Color.red);
    }

    float CovertFrequencyScale(float newMin, float newMax, float oldMin, float oldMax, float value)
    {
        return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(oldMin, oldMax, value));
    }
}
