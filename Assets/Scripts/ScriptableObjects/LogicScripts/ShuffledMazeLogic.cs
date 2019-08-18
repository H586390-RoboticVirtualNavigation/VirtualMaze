using UnityEngine;

[CreateAssetMenu]
public class ShuffledMazeLogic : MazeLogic {
    private int[] order = null;
    private int index = 0;

    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        return order[index++];
    }

    public override bool IsTrialCompleteAfterReward(bool currentTaskSuccess) {
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
