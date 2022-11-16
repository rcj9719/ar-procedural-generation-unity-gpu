using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantClick: MonoBehaviour
{
    public ComputeShader computeShader;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 20));

            //print("the position is " + point.x + " " + point.y);
            //For this 2D generation we only need the plane's normal and the point(which we already have). Assume normal is (0, 0, -1)
            //this is what we call using AR
            GameObject newGameObj = new GameObject("plant1");
            newGameObj.transform.position = point;
            newGameObj.AddComponent<LSystemTurtle>();


        }
    }

    void GeneratePlant(Vector3 point, Vector3 normal)
    {

    }
}
