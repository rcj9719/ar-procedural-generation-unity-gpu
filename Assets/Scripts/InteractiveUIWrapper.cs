using ElRaccoone.Tweens;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public struct ScrollItemStruct
{
    public Button btn;
    public GameObject btnSelection;
}

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

public class InteractiveUIWrapper : MonoBehaviour
{

    public Button clearAllButton;
    public GameObject debugUI;

    public Button showPanelButton;
    public Image showPanelBtnImage;
    public Button debugButton;
    public Image debugBtnImage;
    public Button presetButton;
    public Image presetBtnImage;
    public Button clearTreesButton;

    public Image scrollViewport;
    public GameObject lsystemScrollViewport;
    public GameObject gardenScrollViewport;

    private List<ScrollItemStruct> lsystemScrollViewButtons = new List<ScrollItemStruct>();
    private List<ScrollItemStruct> gardenScrollViewButtons = new List<ScrollItemStruct>();

    private bool showPanel = false;
    private bool isDebugOn = false;
    private bool isPresetOn = false;
    private GameObject activeLSystemButton;
    private GameObject activeGardenButton;

    public UnityEvent clearAllEvent;
    public IntEvent generateLSystemEvent;
    public IntEvent generateGardenEvent;
    public UnityEvent<bool> autoGenerationEvent;


    void getMenuItems(ref GameObject content, ref List<ScrollItemStruct> list)
    {
        int noOfMenuOptions = content.transform.childCount;
        for (int i = 0; i < noOfMenuOptions; i++)
        {
            Transform child = content.transform.GetChild(i);
            ScrollItemStruct item;
            item.btn = child.Find("Button").gameObject.GetComponent<Button>();
            item.btnSelection = child.Find("SelectionUI").gameObject;

            list.Add(item);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (generateLSystemEvent == null)
            generateLSystemEvent = new IntEvent();

        GameObject lsystemScrollContent = GameObject.Find("LSystemContent");
        getMenuItems(ref lsystemScrollContent, ref lsystemScrollViewButtons);
        activeLSystemButton = lsystemScrollViewButtons[0].btnSelection;

        GameObject gardenScrollContent = GameObject.Find("GardenContent");
        getMenuItems(ref gardenScrollContent, ref gardenScrollViewButtons);
        activeGardenButton = gardenScrollViewButtons[0].btnSelection;
        
        lsystemScrollViewport.SetActive(true);
        gardenScrollViewport.SetActive(false);
        clearTreesButton.gameObject.SetActive(true);

    }

    public void clearAllButtonClicked()
    {
        activeGardenButton.gameObject.SetActive(false);
        activeLSystemButton.gameObject.SetActive(false);
        clearAllEvent.Invoke();
        Debug.Log("Objects Cleared");
    }

    void updateSwitch(bool isOn, Button btn, Image switchImg)
    {
        // Set the state of the Build Play Toggle
        Image buildBGImg = btn.GetComponent<Image>();

        Color newBGCol = isOn ? new Color(250f / 255f, 233f / 255f, 217f / 255f) // Blue BG Pill
                                : new Color(218f / 255f, 220f / 255f, 224f / 255f);  // Grey BG Pill


        Color newCircleCol = isOn ? new Color(250f / 255f, 151f / 255f, 38f / 255f)
                                    : Color.white;

        float animDur = 0.3f;
        buildBGImg.TweenValueColor(newBGCol, animDur, (Color colUpdate) => {
            buildBGImg.color = colUpdate;
        }).SetFrom(buildBGImg.color);

        switchImg.TweenValueColor(newCircleCol, animDur, (Color colUpdate) => {
            switchImg.color = colUpdate;
        }).SetFrom(showPanelBtnImage.color);

        switchImg
            .TweenLocalPositionX(64f * (isOn ? 1f : -1f), animDur)
            .SetEaseExpoInOut();

    }

    public void showPanelUpdated()
    {
        showPanel = !showPanel;
        // Change the automatic generation button to be active or white
        updateSwitch(showPanel, showPanelButton, showPanelBtnImage);

        float animDur = 0.3f;
        scrollViewport.TweenLocalPositionX(475f - (showPanel ? 150f : 0f), animDur)
                .SetEaseExpoInOut();

        Debug.Log("[InteractiveUIWrapper.showPanelUpdated] showPanel = " + showPanel);
    }

    public void viewPresetsUpdated()
    {
        isPresetOn = !isPresetOn;
        clearAllButtonClicked();
        // Change the automatic generation button to be active or white
        updateSwitch(isPresetOn, presetButton, presetBtnImage);

        lsystemScrollViewport.SetActive(!isPresetOn);
        gardenScrollViewport.SetActive(isPresetOn);

        activeGardenButton.SetActive(false);
        activeLSystemButton.SetActive(false);
        activeGardenButton = gardenScrollViewButtons[0].btnSelection;
        activeLSystemButton = lsystemScrollViewButtons[0].btnSelection;

        Debug.Log("[InteractiveUIWrapper.presetUpdated] showPanel = " + isPresetOn);
    }

    public void lSystemButtonClicked(int itemNo)
    {
        Debug.Log("[InteractiveUIWrapper.lSystemButtonClicked] LsystemNo: " + itemNo);

        activeGardenButton.SetActive(false);
        activeLSystemButton.SetActive(false);

        if (isPresetOn)
        {
            gardenScrollViewButtons[itemNo].btnSelection.SetActive(true);
            activeGardenButton = gardenScrollViewButtons[itemNo].btnSelection;
            generateGardenEvent.Invoke(itemNo);
        }
        else
        {
            lsystemScrollViewButtons[itemNo].btnSelection.SetActive(true);
            activeLSystemButton = lsystemScrollViewButtons[itemNo].btnSelection;
            generateLSystemEvent.Invoke(itemNo);
        }

        Debug.Log("Item selected");
    }

    public void debugViewUpdated()
    {
        Debug.Log("isDebugOn:" + isDebugOn);

        isDebugOn = !isDebugOn;
        // Change the automatic generation button to be active or white
        updateSwitch(isDebugOn, debugButton, debugBtnImage);
        
        Debug.Log("isDebugOn switched:" + isDebugOn);

        debugUI.SetActive(!debugUI.activeSelf);
        Debug.Log("Debug UI acive:" + debugUI.activeSelf);
    }

}
