using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class GrassManager : MonoBehaviour
{
    /// <summary>
    /// A UI Text object that will receive feedback to the user.
    /// </summary>
    public TextMeshProUGUI FeedbackTextUI;
    public GameObject grassPrefab;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material redMat;
    [SerializeField] private Material yellowMat;
    public GrassInteractor grassInteractor;

    // Generate random position for grass blades given the seed position
    public Vector3 GenerateRandomPosition(Vector3 position)
    {
        float x, y, z;
        x = Random.Range(position.x - 0.3f, position.x + 0.3f);
        y = position.y;
        z = Random.Range(position.z - 0.3f, position.z + 0.3f);
        return new Vector3(x, y, z);
    }

    private void ChangeGrassColor(string color)
    {
        switch (color)
        {

            case "green":
                grassPrefab.GetComponent<ProceduralGrassRenderer>().material = greenMat;
                break;

            case "red":
                grassPrefab.GetComponent<ProceduralGrassRenderer>().material = redMat;
                break;

            case "yellow":
                grassPrefab.GetComponent<ProceduralGrassRenderer>().material = yellowMat;
                break;

            default:
                //Use the default color in the grass prefab; nothing need to be added.
                break;
        }
    }

    // Now the grass can have:
    // "green"
    // "yellow"
    // "red"
    // three colors; pass them as param to generate accordingly.
    public void GenerateGrass(ref Seed seed, string color)
    {
        grassPrefab.GetComponent<ProceduralGrassRenderer>().interactor = grassInteractor;
        ChangeGrassColor(color);

        // Implement Grass
        FeedbackTextUI.text = "GRASS" + seed.seedPose.position;
        Debug.LogError("[GenerateGrass] seedvis.name:" + seed.seedVis.name);
        
        // generate a field of grass
        for (int j = -3; j <= 3; j++)
        {
            for (int i = -3; i < 3; i++)
            {
                float x = seed.seedPose.position.x + (0.5f * (float)j);
                float y = seed.seedPose.position.y;
                float z = seed.seedPose.position.z + (0.5f * (float)i);
                Vector3 randomPosReference = new(x, y, z);

                GameObject grass = Instantiate(grassPrefab, GenerateRandomPosition(randomPosReference), Quaternion.Euler(0, 0, 0));
                grass.transform.SetParent(seed.seedVis.transform);
                
            }
        }

        //debug use
        //GameObject grass = Instantiate(grassPrefab, seed.seedPose.position, Quaternion.Euler(0, 0, 0));
        //grass.transform.SetParent(seed.seedVis.transform);

    }

}
