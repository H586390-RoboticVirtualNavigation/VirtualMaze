using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Session GUI should not know anything about the session it contains. it should only be resposible for
/// interacting with the user. Therefore SessionItems are generated based on SessionController's list.
/// </summary>
public class SessionConfigGUIController : BasicGUIController {
    //drag from Unity Editor
    public Button addSessionButton;
    public GameObject sessionItemPrefab;
    public ScrollRect scrollRect;
    public SessionController sessionController;

    private void Awake() {
        addSessionButton.onClick.AddListener(OnAddButtonClicked);
        sessionController.OnConfigChanged.AddListener(UpdateSettingsGUI);
    }

    private void OnAddButtonClicked() {
        Session s = sessionController.AddSession();
        CreateSessionItem(s);
    }

    public override void UpdateSettingsGUI() {
        ClearSessionItems();
        foreach (Session session in sessionController.Sessions) {
            CreateSessionItem(session);
        }
    }

    private void ClearSessionItems() {
        foreach (Transform child in scrollRect.content.transform) {
            Destroy(child.gameObject);
        }
    }

    private GameObject CreateSessionItem(Session s) {
        //instantiate new level
        GameObject session = Instantiate(sessionItemPrefab, scrollRect.content.transform, false);
        SessionPrefabScript sessionScript = session.GetComponent<SessionPrefabScript>();
        sessionScript.levels = Session.AllLevels;

        sessionScript.onValueChanged.AddListener(sessionController.UpdateSessionNameAt);
        sessionScript.onItemRemove.AddListener(sessionController.RemoveSessionAt);
        sessionScript.onNumTrialsChanged.AddListener(sessionController.UpdateSessionNumTrialAt);

        sessionScript.SetSession(s);

        return session;
    }
}
