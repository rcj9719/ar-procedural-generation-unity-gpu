using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InteractiveUIWrapper : MonoBehaviour
{

    public Button clearAllButton;

    public Toggle debugToggle;
    public GameObject debugUI;

    public Button grassButton;
    public Button lSystemButton1;
    public Button lSystemButton2;
    public Button lSystemButton3;

    public GameObject grassButtonSelected;
    public GameObject lSystemButtonSelected1;
    public GameObject lSystemButtonSelected2;
    public GameObject lSystemButtonSelected3;

    public UnityEvent clearAllEvent;
    public UnityEvent generationDropdownEvent;
    public UnityEvent generateGrassEvent;
    public UnityEvent generateLSystem1Event;
    public UnityEvent generateLSystem2Event;
    public UnityEvent generateLSystem3Event;


    // Start is called before the first frame update
    void Start()
    {
        clearAllButton.gameObject.SetActive(true);
        grassButton.gameObject.SetActive(true);
        lSystemButton1.gameObject.SetActive(true);
        lSystemButton2.gameObject.SetActive(true);
        lSystemButton3.gameObject.SetActive(true);
        debugToggle.gameObject.SetActive(true);
    }

    public void clearAllButtonClicked()
    {
        grassButtonSelected.SetActive(true);
        lSystemButtonSelected1.SetActive(false);
        lSystemButtonSelected2.SetActive(false);
        lSystemButtonSelected3.SetActive(false);

        clearAllEvent.Invoke();
        Debug.Log("Objects Cleared");
    }

    public void generationDropdownUpdated()
    {
        generationDropdownEvent.Invoke();
        Debug.Log("Procedural generation object updated");
    }

    public void grassClicked()
    {
        grassButtonSelected.SetActive(true);
        lSystemButtonSelected1.SetActive(false);
        lSystemButtonSelected2.SetActive(false);
        lSystemButtonSelected3.SetActive(false);

        generateGrassEvent.Invoke();
        Debug.Log("Generating Grass");
    }

    public void lSystemButton1Clicked()
    {
        grassButtonSelected.SetActive(false);
        lSystemButtonSelected1.SetActive(true);
        lSystemButtonSelected2.SetActive(false);
        lSystemButtonSelected3.SetActive(false);

        generateLSystem1Event.Invoke();
        Debug.Log("Generating LSystem1");
    }

    public void lSystemButton2Clicked()
    {
        grassButtonSelected.SetActive(false);
        lSystemButtonSelected1.SetActive(false);
        lSystemButtonSelected2.SetActive(true);
        lSystemButtonSelected3.SetActive(false);

        generateLSystem2Event.Invoke();
        Debug.Log("Generating LSystem2");
    }
    public void lSystemButton3Clicked()
    {
        grassButtonSelected.SetActive(false);
        lSystemButtonSelected1.SetActive(false);
        lSystemButtonSelected2.SetActive(false);
        lSystemButtonSelected3.SetActive(true);

        generateLSystem3Event.Invoke();
        Debug.Log("Generating LSystem3");
    }

    public void debugViewUpdated()
    {
        debugUI.SetActive(!debugUI.activeSelf);
        Debug.Log("Occlusion Updated");
    }

}
