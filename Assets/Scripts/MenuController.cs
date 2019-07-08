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
    public Button dataViewerMenuBtn;

    public CanvasGroup experimentMenuCanvas;
    public CanvasGroup hardwareControlMenuCanvas;
    public CanvasGroup robotMovementMenuCanvas;
    public CanvasGroup dataGenerationMenuCanvas;
    public CanvasGroup dataViewerMenuCanvas;

    private CanvasGroup currentMenu;

    private void Awake() {
        experimentMenuBtn.onClick.AddListener(OnExperimentButtonClicked);
        hardwareControlMenuBtn.onClick.AddListener(OnHardwareControlButtonClicked);
        robotMovementMenuBtn.onClick.AddListener(OnRobotMovementButtonClicked);
        dataGenerationMenuBtn.onClick.AddListener(OnDataGenerationButtonClicked);
        dataViewerMenuBtn.onClick.AddListener(OnDataViewerButtonClicked);

        //default menu is experiment
        experimentMenuCanvas.SetVisibility(true);
        currentMenu = experimentMenuCanvas;

        //hide others
        hardwareControlMenuCanvas.SetVisibility(false);
        robotMovementMenuCanvas.SetVisibility(false);
        dataGenerationMenuCanvas.SetVisibility(false);
        dataViewerMenuCanvas.SetVisibility(false);
    }

    private void OnDataGenerationButtonClicked() {
        ShowMenu(dataGenerationMenuCanvas);
    }

    private void OnDataViewerButtonClicked() {
        ShowMenu(dataViewerMenuCanvas);
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

        currentMenu.SetVisibility(false);
        menuCanvas.SetVisibility(true);

        currentMenu = menuCanvas;
    }
}
