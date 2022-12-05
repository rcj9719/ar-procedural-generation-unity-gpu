using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using Unity.Burst.Intrinsics;
//using UnityEditor.PackageManager;

public struct Seed
{
    public ARAnchor seedAnchor;
    public Pose seedPose;
    public GameObject seedVis;
}

[RequireComponent(typeof(ARSessionOrigin), typeof(ARAnchorManager), typeof(ARRaycastManager))]
public class MainController : MonoBehaviour
{
    InteractiveUIWrapper interactiveUIScript;

    /// <summary>
    /// The ARSession used
    /// </summary>
    public ARSession SessionCore;
    public List<GameObject> lSystems = new List<GameObject>();
    public ProceduralManager ProceduralManager;

    /// <summary>
    /// The active ARSessionOrigin
    /// </summary>
    private ARSessionOrigin _sessionOrigin;
    private Camera _arCamera;

    private bool onTouchHold = false;
    private int lSystemNo = 0;
    private GameObject seedPrefab;
    private GameObject placedObject;

    /// <summary>
    /// The active ARAnchorManager
    /// </summary>
    private ARAnchorManager _anchorManager;
    private ARPlaneManager _planeManager;
    private ARRaycastManager _raycastManager;
    private List<ARRaycastHit> _hits;
    private List<Seed> _seeds;

    // --------------------------
    // DEBUG

    /// <summary>
    /// UI element to display <see cref="ARSessionState"/>.
    /// </summary>
    public TextMeshProUGUI SessionState;

    /// <summary>
    /// The Unity Awake() method.
    /// </summary>
    public void Awake()
    {
        //UpdateFeedbackText("", error: false);
        _sessionOrigin = GetComponent<ARSessionOrigin>();
        if (_sessionOrigin == null)
        {
            Debug.LogError("Failed to find ARSessionOrigin.");
            //UpdateFeedbackText("Failed to find ARSessionOrigin", error: true);
        }

        _anchorManager = GetComponent<ARAnchorManager>();
        if (_anchorManager == null)
        {
            Debug.LogError("Failed to find ARAnchorManager.");
            //UpdateFeedbackText("Failed to find ARAnchorManager", error: true);
        }

        _planeManager = GetComponent<ARPlaneManager>();
        if (_planeManager == null)
        {
            Debug.LogError("Failed to find ARPlaneManager.");
            //UpdateFeedbackText("Failed to find ARAnchorManager", error: true);
        }

        _raycastManager = GetComponent<ARRaycastManager>();
        if (_raycastManager == null)
        {
            Debug.LogError("Failed to find aRRaycastManager.");
            //UpdateFeedbackText("Failed to find ARAnchorManager", error: true);
        }

        _arCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
        if (_arCamera == null)
        {
            Debug.LogError("Failed to find arCamera.");
            //UpdateFeedbackText("Failed to find ARAnchorManager", error: true);
        }

        GameObject thePlayer = GameObject.Find("InteractiveUIWrapper");
        this.interactiveUIScript = thePlayer.GetComponent<InteractiveUIWrapper>();

        seedPrefab = lSystems[0];

        _hits = new List<ARRaycastHit>();
        _seeds = new List<Seed>();
        placedObject = null;
    }

    Seed CreateSeed(in ARRaycastHit hit)
    {
        Seed seed;
        /*
        // If we hit a plane, try to "attach" the anchor to the plane
        if (hit.trackable is ARPlane plane)
        {
            var planeManager = GetComponent<ARPlaneManager>();
            if (planeManager)
            {
                var oldPrefab = _anchorManager.anchorPrefab;
                _anchorManager.anchorPrefab = seedPrefab;
                seed.seedAnchor = _anchorManager.AttachAnchor(plane, hit.pose);
                seed.seedPose = hit.pose;
                seed.seedVis = new GameObject();
                _anchorManager.anchorPrefab = oldPrefab;

                Debug.Log($"Created anchor attachment for plane (id: {seed.seedAnchor.nativePtr}).");
                return seed;
            }
        }
        */
        // Otherwise, just create a regular anchor at the hit pose

        // Note: the anchor can be anywhere in the scene hierarchy
        placedObject = Instantiate(seedPrefab, hit.pose.position, hit.pose.rotation);

        // Make sure the new GameObject has an ARAnchor component
        seed.seedAnchor = placedObject.GetComponent<ARAnchor>();
        if (seed.seedAnchor == null)
        {
            seed.seedAnchor = placedObject.AddComponent<ARAnchor>();
        }
        Debug.Log($"Created regular anchor (id: {seed.seedAnchor.nativePtr}).");
        seed.seedPose = hit.pose;
        seed.seedVis = placedObject;
        return seed;
    }

    // Update is called once per frame
    void Update()
    {
        // Only consider single-finger touches that are beginning
        Touch touch;
        if (Input.touchCount < 1) { return; }

        touch = Input.GetTouch(0);
        var touchPosition = touch.position;
        bool isOverUI = touchPosition.IsPointOverUIObject();

        if (_raycastManager.Raycast(touch.position, _hits))
        {
            if (!isOverUI && Input.GetTouch(0).phase == TouchPhase.Began && placedObject == null)
            {
                Ray ray = _arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    Seed seed = CreateSeed(_hits[0]);
                    if (lSystemNo == 0)
                    {
                        ProceduralManager.GenerateGrass(seed.seedPose);
                    }
                    _seeds.Add(seed);

                    SessionState.text = "No of Anchors: " + _seeds.Count;
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && placedObject != null)
            {
                placedObject.transform.position = _hits[0].pose.position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                placedObject = null;
            }
        }
    }

    public void SetPlatformActive(bool active)
    {
        _sessionOrigin.enabled = active;
        SessionCore.gameObject.SetActive(active);
    }


    public void ClearAll()
    {
        lSystemNo = 0;
        foreach (Seed seed in _seeds)
        {
            Destroy(seed.seedVis.gameObject);
            Destroy(seed.seedAnchor.gameObject);
        }
        _seeds.Clear();
        SessionState.text = "No of Anchors: " + _seeds.Count;
    }

    public void PlaceGrass()
    {
        lSystemNo = 0;
    }

    public void PlaceLSystem1()
    {
        lSystemNo = 1;
        seedPrefab = lSystems[lSystemNo];
    }

    public void PlaceLSystem2()
    {
        lSystemNo = 2;
        seedPrefab = lSystems[lSystemNo];
    }

    public void PlaceLSystem3()
    {
        lSystemNo = 3;
        seedPrefab = lSystems[lSystemNo];
    }
}
