using UnityEngine;

public class ProximityRewardArea : RewardArea {
    protected override void Start() {
        base.Start();
        target.gameObject.SetActive(false);
    }

    protected override void OnTriggerStay(Collider other) {
        base.OnTriggerStay(other);
        if (!target.gameObject.activeInHierarchy) {
            target.gameObject.SetActive(true);
        }
    }

    protected void OnTriggerExit(Collider other) {
        target.gameObject.SetActive(false);
    }
}
