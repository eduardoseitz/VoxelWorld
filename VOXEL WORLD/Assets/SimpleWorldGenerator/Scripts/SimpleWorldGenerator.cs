using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWorldGenerator : MonoBehaviour
{
    [SerializeField] int chunckSizeZ = 16;
    [SerializeField] int chunckSizeX = 16;
    [SerializeField] int chunckSizeY = 16;
    [SerializeField] int randomnessY = 5;
    [SerializeField] Material dirtMat;
    [SerializeField] Material grassMat;
    [SerializeField] Material stoneMat;
    [SerializeField] int maxStoneHeight = 10;

    private GameObject _newCube;

    // Start is called before the first frame update
    void Start()
    {
        GenerateWorld();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // z is foward, x is right and y is up
    private void GenerateWorld() 
    {
        for (int z = 0; z < chunckSizeZ; z++)
        {
            for (int x = 0; x < chunckSizeX; x++)
            {
                int _randomHeight = Random.Range(-randomnessY, randomnessY);
                for (int y = 0; y < chunckSizeY + _randomHeight; y++)
                {
                    _newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _newCube.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.identity);

                    if (y == chunckSizeY + _randomHeight - 1)
                        _newCube.GetComponent<Renderer>().material = grassMat;
                    else if (y < maxStoneHeight + _randomHeight)
                        _newCube.GetComponent<Renderer>().material = stoneMat;
                    else
                        _newCube.GetComponent<Renderer>().material = dirtMat;
                }
            }
        }
    }
}
