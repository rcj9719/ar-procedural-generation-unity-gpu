using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractiveUIWrapper : MonoBehaviour
{

    public Button clearAllButton;
    public TMP_Dropdown generationDropdown;
    //public Toggle showSurfaceToggle;
    //public Toggle showARTextToggle;
    //public Toggle occlusionToggle;
    //public Toggle debugToggle;

    public UnityEvent clearAllEvent;
    public UnityEvent generationDropdownEvent;
    
    //public UnityEvent showSurfaceToggleEvent;
    //public UnityEvent showARTextToggleEvent;
    //public UnityEvent occlusionToggleEvent;
    //public UnityEvent debugToggleEvent;


    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("********************** InteractiveUI 1 ****************************");
        clearAllButton.gameObject.SetActive(true);
        generationDropdown.gameObject.SetActive(true);
        //debugToggle.gameObject.SetActive(true);
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void clearAllButtonClicked()
    {
        clearAllEvent.Invoke();
        Debug.Log("Objects Cleared");
    }

    public void generationDropdownUpdated()
    {
        generationDropdownEvent.Invoke();
        Debug.Log("Procedural generation object updated");
    }

    //public void showSurfaceToggleUpdated()
    //{
    //    showSurfaceToggleEvent.Invoke();
    //    Debug.Log("Surface Visibility Updated");
    //}

    //public void showARTextToggleUpdated()
    //{
    //    showARTextToggleEvent.Invoke();
    //    Debug.Log("AR Text Visibility Updated");
    //}

    //public void occlusionUpdated()
    //{
    //    occlusionToggleEvent.Invoke();
    //    Debug.Log("Occlusion Updated");
    //}
    //public void debugViewUpdated()
    //{
    //    debugToggleEvent.Invoke();
    //    Debug.Log("Occlusion Updated");
    //}

}
