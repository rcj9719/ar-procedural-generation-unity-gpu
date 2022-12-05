using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class NoiseGenerationItems
{
    public GameObject LSystemPrefab;
    public float minRange;
    public float maxRange;
}
public class LSystemGenerator : MonoBehaviour
{
    public bool noiseGeneration = false;
    public int noisePlaneSize = 10;
    private float[,] planeTile;
    public List<NoiseGenerationItems> rules = new List<NoiseGenerationItems>();

    #region 2D Noise Generation
    float randomGenerateNumber()
    {
        return UnityEngine.Random.Range(0.01f, 2.00f);
    }

    void noiseMapGenerate()
    {
        planeTile = new float[noisePlaneSize, noisePlaneSize];
        for (int i = 0; i < noisePlaneSize; i++)
        {

            for (int j = 0; j < noisePlaneSize; j++)
            {
                //planeTile[i, j] = Mathf.PerlinNoise(i, j);
                planeTile[i, j] = UnityEngine.Random.Range(0.0f, 1.0f);
                //Debug.Log(planeTile[i, j]);
            }
        }
    }

    void noiseGenerate()
    {
        //naive on3 for now
        for (int i = 0; i < noisePlaneSize; i++)
        {
            for (int j = 0; j < noisePlaneSize; j++)
            {
                foreach (NoiseGenerationItems a in rules)
                {
                    if (planeTile[i, j] >= a.minRange)
                    {
                        if (planeTile[i,j] <= a.maxRange)
                        {
                            Vector3 pos = new Vector3(i, 0, j);
                            Instantiate(a.LSystemPrefab, pos, Quaternion.identity);
                        } else
                        {
                            continue;
                        }

                    } else
                    {
                        continue;
                    }
                }
            }
        }
    }

    private void Awake()
    {
        if (noiseGeneration)
        {
            noiseMapGenerate();
        }
    }

    #endregion

    // Update is called once per frame
    void Start()
    {
        if (noiseGeneration)
        {
            noiseGenerate();
        }
    }
}
