using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ProceduralManager : MonoBehaviour
{
    /// <summary>
    /// A UI Text object that will receive feedback to the user.
    /// </summary>
    public TextMeshProUGUI FeedbackTextUI;
    public GameObject grassPrefab;
    public GrassInteractor grassInteractor;

    //Generate random position for grass blades given the seed position
    public Vector3 GenerateRandomPosition(Vector3 position)
    {
        float x, y, z;
        x = Random.Range(position.x - 0.3f, position.x + 0.3f);
        y = position.y;
        z = Random.Range(position.z - 0.3f, position.z + 0.3f);
        return new Vector3(x, y, z);
    }

    public void GenerateGrass(Pose seedPose)
    {
        grassPrefab.GetComponent<ProceduralGrassRenderer>().interactor = grassInteractor;
        /*
        // Implement Vines
        FeedbackTextUI.text = "GRASS" + seedPose.position;

        // generate a field of grass
        for (int j = -3; j <= 3; j++)
        {
            for (int i = -3; i < 3; i++)
            {

                float x = seedPose.position.x + (0.5f * (float)j);
                float y = seedPose.position.y;
                float z = seedPose.position.z + (0.5f * (float)i);
                Vector3 randomPosReference = new(x, y, z);

                Instantiate(grassPrefab, GenerateRandomPosition(randomPosReference), Quaternion.Euler(0, 0, 0));

            }
        }*/

        //debug use
        Instantiate(grassPrefab, seedPose.position, Quaternion.Euler(0, 0, 0));
    }

}
