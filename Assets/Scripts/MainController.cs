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
using UnityEngine.Rendering.UI;

public struct Seed
{
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
    public GrassInteractor grassInteractor;
    public GrassManager GrassManager;
    public List<GameObject> gardens = new List<GameObject>();

    /// <summary>
    /// The active ARSessionOrigin
    /// </summary>
    private ARSessionOrigin _sessionOrigin;
    private Camera _arCamera;

    //private bool onTouchHold = false;
    private int lSystemNo = -1;
    private int gardenNo = -1;
    private GameObject seedPrefab;
    private bool isAutomaticOn = false;
    
    private GameObject placedObject;
    private Dictionary<int, string> grassColor = new Dictionary<int, string>();


    /// <summary>
    /// The active ARAnchorManager
    /// </summary>
    private ARAnchorManager _anchorManager;
    private ARTrackedObjectManager _trackedObjManager;
    private ARPlaneManager _planeManager;
    private ARRaycastManager _raycastManager;
    private List<ARRaycastHit> _hits;
    private List<Seed> _seeds;

    // --------------------------
    // DEBUG

    /// <summary>
    /// UI element to display <see cref="ARSessionState"/>.
    /// </summary>
    public TextMeshProUGUI DebuggingUIText;
    public TextMeshProUGUI FeebackUIText;

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
        }

        _anchorManager = GetComponent<ARAnchorManager>();
        if (_anchorManager == null)
        {
            Debug.LogError("Failed to find ARAnchorManager.");
        }

        _trackedObjManager = GetComponent<ARTrackedObjectManager>();
        if (_trackedObjManager == null)
        {
            Debug.LogError("Failed to find ARTrackedObjectManager.");
        }

        _planeManager = GetComponent<ARPlaneManager>();
        if (_planeManager == null)
        {
            Debug.LogError("Failed to find ARPlaneManager.");
        }

        _raycastManager = GetComponent<ARRaycastManager>();
        if (_raycastManager == null)
        {
            Debug.LogError("Failed to find aRRaycastManager.");
        }

        _arCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
        if (_arCamera == null)
        {
            Debug.LogError("Failed to find arCamera.");
        }

        GameObject thePlayer = GameObject.Find("InteractiveUIWrapper");
        this.interactiveUIScript = thePlayer.GetComponent<InteractiveUIWrapper>();

        seedPrefab = lSystems[0];

        _hits = new List<ARRaycastHit>();
        _seeds = new List<Seed>();
        placedObject = null;

        grassColor[0] = "green";
        grassColor[1] = "yellow";
        grassColor[2] = "red";

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

        if (_raycastManager.Raycast(touchPosition, _hits))
        {
            if (!isOverUI && touch.phase == TouchPhase.Began && placedObject == null)
            {
                Ray ray = _arCamera.ScreenPointToRay(touchPosition);
                ARRaycastHit hit = _hits[0];
                Debug.Log("[MainController.Update] AR Hit Trackable: " + hit.trackable.name);
                Debug.Log("[MainController.Update] AR Hit Type: " + hit.hitType);

                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    Debug.Log("[MainController.Update] AR Hit Object: " + hitObject.collider.gameObject.name);
                    if (isAutomaticOn)
                    {
                        
                    }

                    if (lSystemNo >= 0 && lSystemNo < lSystems.Count)
                    {
                        Seed seed;
                        seed.seedPose = _hits[0].pose;
                        if (lSystemNo >= 0 && lSystemNo < 3)
                        {
                            seed.seedVis = Instantiate(lSystems[lSystemNo]);
                            GrassManager.GenerateGrass(ref seed, grassColor[lSystemNo]);
                            
                        }
                        else
                        {
                            placedObject = Instantiate(seedPrefab, hit.pose.position, hit.pose.rotation);
                            seed.seedVis = placedObject;
                        }
                        _seeds.Add(seed);
                        DebuggingUIText.text = "Total no of items placed: " + _seeds.Count;
                    }

                    if (gardenNo >= 0 && gardenNo < gardens.Count)
                    {
                        Seed seed;
                        seed.seedPose = _hits[0].pose;
                        seed.seedVis = null;
                        Debug.LogError("[Update] seed initialized to null");
                        placedObject = Instantiate(seedPrefab, hit.pose.position, hit.pose.rotation);
                        seed.seedVis = placedObject;
                        GrassManager.GenerateGrass(ref seed, grassColor[gardenNo]);
                        
                        _seeds.Add(seed);
                        DebuggingUIText.text = "Total no of items placed: " + _seeds.Count;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && placedObject != null)
            {
                placedObject.transform.position = _hits[0].pose.position;
            }
            if (touch.phase == TouchPhase.Ended)
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
            Debug.LogError("[ClearAll] seed.name:" + seed.seedVis.name);
            Destroy(seed.seedVis.gameObject);
        }
        
        _seeds.Clear();
        DebuggingUIText.text = "No of Trees: " + _seeds.Count;
    }

    public void PlaceLSystem(int lNo)
    {
        Debug.Log("[MainController.PlaceLSystem] LsystemNo: " + lNo);
        lSystemNo = lNo;
        gardenNo = -1;
        if (lSystemNo > lSystems.Count)
        {
            Debug.LogError("Insuffienient prefabs initialized");
        }
        else
        {
            seedPrefab = lSystems[lSystemNo];
        }
    }

    public void PlaceGarden(int lNo)
    {
        Debug.Log("[MainController.PlaceGarden] GardenNo: " + lNo);
        gardenNo = lNo;
        lSystemNo = -1;
        if (gardenNo > gardens.Count)
        {
            Debug.LogError("Insuffienient prefabs initialized");
        }
        else
        {
            seedPrefab = gardens[gardenNo];
        }
    }

    public void AutoPlacementLSystem(bool isOn)
    {
        isAutomaticOn = isOn;
        Debug.LogError("[MainController.AutoPlacementLSystem] isAutomaticOn = " + isAutomaticOn);
    }
}
