using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale *= 0.02f;
        GameObject gameObject = new GameObject("empty1");
        gameObject.transform.parent = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
