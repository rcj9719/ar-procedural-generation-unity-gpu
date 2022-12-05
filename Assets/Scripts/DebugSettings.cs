using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class DebugChangedEvent : UnityEvent <DebugSettings> { }

[ExecuteInEditMode]
public class DebugSettings : MonoBehaviour
{
    // ------------------------------------
    public static DebugSettings Shared { get; private set; }
    // ------------------------------------

    [SerializeField] private DebugChangedEvent _debugChangedEvent;

    [SerializeField] private bool _allDebugOff = false;
    [SerializeField] private bool _displayLsystemDebug = true;
    [SerializeField] private bool _displayGrassDebug = false;

    public bool DisplayLsystemDebug { get 
        { return !_allDebugOff && _displayLsystemDebug; } 
    }
    public bool DisplayGrassDebug { get 
        { return !_allDebugOff && _displayGrassDebug; }
    }

    // ------------------------------------

    void Awake() {
        if (Shared != null && Shared != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Shared = this; 
        } 
    }

    void OnValidate()
    {
        // if (!Application.isPlaying) {
        //     Debug.LogWarning("DEBUG Changes won't be made unless the app is running");
        // }
        _debugChangedEvent.Invoke(this);
    }
}