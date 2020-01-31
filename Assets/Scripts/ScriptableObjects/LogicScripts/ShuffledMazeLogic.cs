﻿using UnityEngine;

[CreateAssetMenu]
public class ShuffledMazeLogic : MazeLogic {
    private int[] order = null;
    private int index = 0;

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        return order[index++];
    }

    public override Sprite GetTargetImage(RewardArea[] rewards, int targetIndex) {
        return rewards[targetIndex].cueImage;
    }

    public override bool IsTrialCompleteAfterCurrentTask(bool currentTaskSuccess) {
        bool completed = (index == order.Length);
        if (completed) {
            Shuffle(order);
            index = 0;
        }
        return completed;
    }

    public override void Setup(RewardArea[] rewards) {
        index = 0;
        order = new int[rewards.Length];

        // fill array with reward indices
        for (int i = 0; i < order.Length; i++) {
            order[i] = i;
        }

        Shuffle(order);
    }

    public override bool ShowCue(int targetIndex) {
        return true;
    }

    /// <summary>
    /// Shuffles the array
    /// </summary>
    /// <param name="arrayToShuffle"></param>
    public static void Shuffle(int[] arrayToShuffle) {
        // shuffle array
        for (var j = arrayToShuffle.Length - 1; j > 0; j--) {
            var r = Random.Range(0, j);
            var tmp = arrayToShuffle[j];
            arrayToShuffle[j] = arrayToShuffle[r];
            arrayToShuffle[r] = tmp;
        }
    }
}
