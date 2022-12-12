using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct LSystemSet
{
    //public LSystemGPU item;
    public GameObject lSystemPrefab;
    public Vector3 position;
    public Quaternion orientation;
    public float scale;
}

public class LSystemManager : MonoBehaviour
{
    public float scale = 1.0f;
    [SerializeField]
    public List<LSystemSet> objectList = new List<LSystemSet>();
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Time started for L-system generation " + Time.realtimeSinceStartup);
        foreach (LSystemSet cur in objectList)
        {
            if (cur.lSystemPrefab == null) continue;
            GameObject gameobject = Instantiate(cur.lSystemPrefab, cur.position*scale + this.transform.position, cur.orientation * this.transform.rotation);
            gameobject.transform.localScale *= cur.scale <= 0 ? 1 : cur.scale;
            gameobject.transform.parent = this.transform;
        }
        Debug.Log("Time started for L-system generation " + Time.realtimeSinceStartup);
    }
}
