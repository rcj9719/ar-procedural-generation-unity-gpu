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

public struct Seed
{
    public ARAnchor seedAnchor;
    public Pose seedPose;
    //public GameObject seedVis;
}

[RequireComponent(typeof(ARSessionOrigin), typeof(ARAnchorManager), typeof(ARRaycastManager))]
public class MainController : MonoBehaviour
{
    InteractiveUIWrapper interactiveUIScript;

    /// <summary>
    /// The ARSession used
    /// </summary>
    public ARSession SessionCore;
    public GameObject seedPrefab;
    public ProceduralManager ProceduralManager;

    /// <summary>
    /// The active ARSessionOrigin
    /// </summary>
    private ARSessionOrigin _sessionOrigin;

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
            Debug.LogError("Failed to find ARRaycastManager.");
            //UpdateFeedbackText("Failed to find ARAnchorManager", error: true);
        }

        GameObject thePlayer = GameObject.Find("InteractiveUIWrapper");
        this.interactiveUIScript = thePlayer.GetComponent<InteractiveUIWrapper>();

        _hits = new List<ARRaycastHit>();
        _seeds = new List<Seed>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void HandleRaycast(ARRaycastHit hit)
    {
        if (hit.trackable is ARPlane plane)
        {
            // Do something with 'plane':
            Debug.Log($"Hit a plane with alignment {plane.alignment}");
        }
        else
        {
            // What type of thing did we hit?
            Debug.Log($"Raycast hit a {hit.hitType}");
        }
    }

    Seed CreateSeed(in ARRaycastHit hit)
    {

        Seed seed;

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
                _anchorManager.anchorPrefab = oldPrefab;

                Debug.Log($"Created anchor attachment for plane (id: {seed.seedAnchor.nativePtr}).");
                return seed;
            }
        }

        // Otherwise, just create a regular anchor at the hit pose

        // Note: the anchor can be anywhere in the scene hierarchy
        var instantiatedObject = Instantiate(seedPrefab, hit.pose.position, hit.pose.rotation);
        // Make sure the new GameObject has an ARAnchor component
        seed.seedAnchor = instantiatedObject.GetComponent<ARAnchor>();
        if (seed.seedAnchor == null)
        {
            seed.seedAnchor = instantiatedObject.AddComponent<ARAnchor>();
        }
        Debug.Log($"Created regular anchor (id: {seed.seedAnchor.nativePtr}).");
        seed.seedPose = hit.pose;
        return seed;
    }

    public void GenerateProcedural(Pose seedPose)
    {
        if (interactiveUIScript.generationDropdown.options[interactiveUIScript.generationDropdown.value].text == "Generate Vines")
        {
            ProceduralManager.GenerateVines(seedPose);
        }
        if (interactiveUIScript.generationDropdown.options[interactiveUIScript.generationDropdown.value].text == "Generate Grass")
        {
            ProceduralManager.GenerateGrass(seedPose);
        }
    }



    // Update is called once per frame
    void Update()
    {
        // Only consider single-finger touches that are beginning
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) { return; }

        var touchPosition = touch.position;
        bool isOverUI = touchPosition.IsPointOverUIObject();

        // Perform AR raycast to any kind of trackable
        if (!isOverUI && _raycastManager.Raycast(touchPosition, _hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes))
        {
            Seed seed = CreateSeed(_hits[0]);
            GenerateProcedural(seed.seedPose);
            _seeds.Add(seed);
            SessionState.text = "No of Anchors: " + _seeds.Count;
        }
    }

    public void SetPlatformActive(bool active)
    {
        _sessionOrigin.enabled = active;
        SessionCore.gameObject.SetActive(active);
    }


    public void ClearAll()
    {
        foreach (Seed seed in _seeds)
        {
            Destroy(seed.seedAnchor.gameObject);
        }
        _seeds.Clear();
        SessionState.text = "No of Anchors: " + _seeds.Count;
    }

    public void DropdownValueChanged()
    {
        ////call procedural generation
        //int val = interactiveUIScript.generationDropdown.value;
        //ProceduralManager.GenerateProcedural(val, _anchors);
    }
}
