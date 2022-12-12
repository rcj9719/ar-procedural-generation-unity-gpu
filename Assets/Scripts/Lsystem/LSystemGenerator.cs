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
    public int noisePlaneSize = 10;
    public float randomizer = 1f;
    public float scale = 1.0f;
    private float[,] planeTile;
    public List<NoiseGenerationItems> rules = new List<NoiseGenerationItems>();

    #region 2D Noise Generation
    float randomGenerateNumber()
    {
        return UnityEngine.Random.Range(-randomizer, randomizer);
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
        Vector3 offset = new Vector3(-noisePlaneSize / 2, 0, -noisePlaneSize / 2);
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
                            Vector3 pos = new Vector3(i + randomGenerateNumber() , 0, j + randomGenerateNumber());
                            pos += offset;
                            Instantiate(a.LSystemPrefab, pos*scale, Quaternion.identity);
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
        noiseMapGenerate();
    }

    #endregion

    // Update is called once per frame
    void Start()
    {
       noiseGenerate();
    }
}
