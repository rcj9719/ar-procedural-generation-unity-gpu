using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class placementController : MonoBehaviour
{
    [SerializeField]
    public GameObject placedPrefab;

    private GameObject placedObject;

    [SerializeField]
    private ARRaycastManager aRRaycastManager;

    private Vector2 touchPosition = default;

    private bool onTouchHold = false;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private Camera arCam;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        placedObject = null;
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
    }

    //bool tryGetTouchPosition(out Vector2 touchPosition)
    //{
    //    if(Input.touchCount > 0)
    //    {
    //        touchPosition = Input.GetTouch(0).position;
    //        return true;
    //    }

    //    touchPosition = default;
    //    return false;
    //}

    // Update is called once per frame
    void Update()
    {
        //    if(!tryGetTouchPosition(out Vector2 touchPosition))
        //    {
        //        return;
        //    }

        //    if(aRRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        //    {

        //    }
        UnityEngine.Touch touch = Input.GetTouch(0);

        if (Input.touchCount < 1) { return; }

        if(aRRaycastManager.Raycast(touch.position, hits))
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && placedObject == null)
            {
                Ray ray = arCam.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider.gameObject.tag == "Spawnable")
                    {
                        placedObject = hitObject.collider.gameObject;
                    }
                    else
                    {
                        placedObject = Instantiate(placedPrefab, hits[0].pose.position, Quaternion.identity);
                    }
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && placedObject != null)
            {
                placedObject.transform.position = hits[0].pose.position;
            }
            if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                placedObject = null;
            }
        }

        //*********************************************************************************************
        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);

        //    touchPosition = touch.position;

        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //        RaycastHit hitObject;

        //        if (Physics.Raycast(ray, out hitObject))
        //        {
        //            if(hitObject.transform.name.Contains("PlacedObject"))
        //            {
        //                onTouchHold = true;
        //            }
        //        }
        //    }

        //    if(touch.phase == TouchPhase.Moved)
        //    {
        //        touchPosition = touch.position;
        //    }


        //    if(touch.phase == TouchPhase.Ended)
        //    {
        //        onTouchHold = false;
        //    }


        //    if(onTouchHold)
        //    {
        //        if(aRRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        //        {
        //            Pose hitPose = hits[0].pose;
        //            if(placedObject == null)
        //            {
        //                placedObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
        //            }
        //            else
        //            {
        //                if (onTouchHold)
        //                {
        //                    placedObject.transform.position = hitPose.position;
        //                    placedObject.transform.rotation = hitPose.rotation;
        //                }
        //            }
        //        }
        //    }
        //}
            

    }

}
