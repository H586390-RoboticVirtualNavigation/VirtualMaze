﻿using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/HiddenRewardMazeLogic")]
public class HiddenRewardMazeLogic : StandardMazeLogic {
    public override void Setup(RewardArea[] rewards) {
        base.Setup(rewards);
        foreach (RewardArea area in rewards) {
            SetRewardTargetVisible(area, false);
        }

        TrackEnterProximity(true);
        TrackExitTriggerZone(true);
    }

    protected void TrackExitTriggerZone(bool enable) {
        if (enable) {
            LevelController.OnExitedTriggerZone += TriggerZoneExit;
        }
        else {
            LevelController.OnExitedTriggerZone -= TriggerZoneExit;
        }
    }

    protected void TrackEnterProximity(bool enable) {
        if (enable) {
            LevelController.InRewardProximityEvent += TriggerZoneEnter;
        }
        else {
            LevelController.InRewardProximityEvent -= TriggerZoneEnter;
        }
    }

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        LevelController.InTriggerZoneListener += WhileInTriggerZone;
        return base.GetNextTarget(currentTarget, rewards);
    }

    private void WhileInTriggerZone(RewardArea rewardArea, bool isTarget) {
        if (isTarget && !rewardArea.target.gameObject.activeInHierarchy) {
            SetRewardTargetVisible(rewardArea, true);
        }
        if (Input.GetKeyDown("space")) {
            LevelController.InTriggerZoneListener -= WhileInTriggerZone;
            }
    }

    private void TriggerZoneExit(RewardArea rewardArea, bool isTarget) {
        SetRewardTargetVisible(rewardArea, false);
    }

    private void TriggerZoneEnter(RewardArea rewardArea, bool isTarget) {
        if (isTarget && Input.GetKeyDown("space")) {
            SetRewardTargetVisible(rewardArea, true);
        }
    }

    protected void SetRewardTargetVisible(RewardArea area, bool visible) {
        area.target.gameObject.SetActive(visible);
    }

    public override void Cleanup(RewardArea[] rewards) {
        base.Cleanup(rewards);
        TrackEnterProximity(false);
        TrackExitTriggerZone(false);
    }

    public override void ProcessReward(RewardArea rewardArea, bool success) {
        base.ProcessReward(rewardArea, success);
        SetRewardTargetVisible(rewardArea, false);
    }
}
