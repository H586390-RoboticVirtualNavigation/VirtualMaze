using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// To improve efficiency and allow scripts to be executed when a menu is to
/// be hidden, set alpha of canvasgroup to 0 instead of disabling the GameObject.
/// 
/// Althouogh online sources state to just disable the canvas component, disabled canvas 
/// componenets still receive click events. Therefore, this method is used
/// </summary>
public class MenuController : MonoBehaviour {
    //Drag in Unity Editor
    public Button experimentMenuBtn;
    public Button hardwareControlMenuBtn;
    public Button robotMovementMenuBtn;
    public Button dataGenerationMenuBtn;

    public CanvasGroup experimentMenuCanvas;
    public CanvasGroup hardwareControlMenuCanvas;
    public CanvasGroup robotMovementMenuCanvas;
    public CanvasGroup dataGenerationMenuCanvas;

    private CanvasGroup currentMenu;

    private void Awake() {
        experimentMenuBtn.onClick.AddListener(OnExperimentButtonClicked);
        hardwareControlMenuBtn.onClick.AddListener(OnHardwareControlButtonClicked);
        robotMovementMenuBtn.onClick.AddListener(OnRobotMovementButtonClicked);
        dataGenerationMenuBtn.onClick.AddListener(OnDataGenerationButtonClicked);

        //default menu is experiment
        SetVisibility( experimentMenuCanvas, true);
        currentMenu = experimentMenuCanvas;

        //hide others
        SetVisibility(hardwareControlMenuCanvas, false);
        SetVisibility(robotMovementMenuCanvas, false);
        SetVisibility(dataGenerationMenuCanvas, false);
    }

    private void OnDataGenerationButtonClicked() {
        ShowMenu(dataGenerationMenuCanvas);
    }

    private void OnHardwareControlButtonClicked() {
        ShowMenu(hardwareControlMenuCanvas);
    }

    private void OnRobotMovementButtonClicked() {
        ShowMenu(robotMovementMenuCanvas);
    }

    private void OnExperimentButtonClicked() {
        ShowMenu(experimentMenuCanvas);
    }

    /// <summary>
    /// Hides current CanvasGroup and shows the desired CanvasGroup
    /// </summary>
    /// <param name="menuCanvas">CanvasGroup to show</param>
    private void ShowMenu(CanvasGroup menuCanvas) {
        //do nothing if current menu is already shown.
        if (menuCanvas.Equals(currentMenu)) return;

        SetVisibility(currentMenu, false);
        SetVisibility(menuCanvas, true);

        currentMenu = menuCanvas;
    }

    /// <summary>
    /// Helper method to hide or show a canvas group.
    /// </summary>
    /// <param name="canvasGroup">canvas group to hide or show</param>
    /// <param name="shown">true to show, false to hide</param>
    private void SetVisibility(CanvasGroup canvasGroup, bool shown) {
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
