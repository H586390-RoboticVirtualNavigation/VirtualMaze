using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// To improve efficiency and allow scripts to be executed when a menu is to
/// be hidden, set alpha of canvasgroup to 0 instead of disabling the GameObject.
/// </summary>
public class MenuController : MonoBehaviour {
    //Drag in Unity Editor
    public Button experimentMenuBtn;
    public Button hardwareControlMenuBtn;
    public Button robotMovementMenuBtn;

    public CanvasGroup experimentMenuCanvas;
    public CanvasGroup hardwareControlMenuCanvas;
    public CanvasGroup robotMovementMenuCanvas;

    private CanvasGroup currentMenu;

    private void Awake() {
        experimentMenuBtn.onClick.AddListener(OnExperimentButtonClicked);
        hardwareControlMenuBtn.onClick.AddListener(OnHardwareControlButtonClicked);
        robotMovementMenuBtn.onClick.AddListener(OnRobotMovementButtonClicked);

        //default menu is experiment
        SetVisible( experimentMenuCanvas, true);
        currentMenu = experimentMenuCanvas;

        SetVisible(hardwareControlMenuCanvas, false);
        SetVisible(robotMovementMenuCanvas, false);
    }

    private void OnHardwareControlButtonClicked() {
        showMenu(hardwareControlMenuCanvas);
    }

    private void OnRobotMovementButtonClicked() {
        showMenu(robotMovementMenuCanvas);
    }

    private void OnExperimentButtonClicked() {
        showMenu(experimentMenuCanvas);
    }

    private void showMenu(CanvasGroup menuCanvas) {
        //do nothing if current menu is already shown.
        if (menuCanvas.Equals(currentMenu)) return;

        SetVisible(currentMenu, false);
        SetVisible(menuCanvas, true);

        currentMenu = menuCanvas;
    }

    private void SetVisible(CanvasGroup canvasGroup, bool shown) {
        if (shown) {
            canvasGroup.alpha = 1;
        }
        else {
            canvasGroup.alpha = 0;
        }
        canvasGroup.blocksRaycasts = shown;
        canvasGroup.interactable = shown;
    }
}
