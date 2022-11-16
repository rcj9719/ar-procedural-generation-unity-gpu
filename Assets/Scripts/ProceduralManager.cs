using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ProceduralManager : MonoBehaviour
{
    /// <summary>
    /// A UI Text object that will receive feedback to the user.
    /// </summary>
    public TextMeshProUGUI FeedbackTextUI;

    public void GenerateVines(Pose seedPose)
    {
        // Implement Vines
        FeedbackTextUI.text = "VINES" + seedPose.position;
        GameObject newGameObj = new GameObject("plant1");
        newGameObj.transform.position = seedPose.position;
        newGameObj.AddComponent<LSystemTurtle>();
    }

    public void GenerateGrass(Pose seedPose)
    {
        // Implement Grass
        FeedbackTextUI.text = "GRASS" + seedPose.position;
    }

}
