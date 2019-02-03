using System.Collections;
using UnityEngine;

// TODO make as a trial to collecting all rewards.
public class ShuffledRewardsLevel : BasicLevelController {
    private int[] order = null;
    private int index = 0;

    protected override void Setup() {
        restartOnTaskFail = false;

        if (order == null) {
            order = new int[rewards.Length];
            // fill array with reward indices
            for (int i = 0; i < order.Length; i++) {
                order[i] = i;
            }
            Shuffle(order);
        }
    }

    protected override bool IsTrialCompleteCondition() {
        bool completed = (index == order.Length);
        if (completed) {
            Shuffle(order);
            index = 0;
        }
        return completed;
    }

    protected override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        return order[index++];
    }

    /// <summary>
    /// Shuffles the array
    /// </summary>
    /// <param name="arrayToShuffle"></param>
    void Shuffle(int[] arrayToShuffle) {
        // shuffle array
        for (var j = order.Length - 1; j > 0; j--) {
            var r = Random.Range(0, j);
            var tmp = order[j];
            order[j] = order[r];
            order[r] = tmp;
        }
    }
}
