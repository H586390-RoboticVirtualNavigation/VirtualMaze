using UnityEngine;

[CreateAssetMenu(menuName = "MazeLogic/StandardMazeLogic")]
public class StandardMazeLogic : MazeLogic {
    /// <summary>
    /// Returns the next non-repeating target for the subject.
    /// </summary>
    /// <param name="currentTarget">Index of the current target</param>
    /// <returns>Index of the next target</returns>
    public override int GetNextTarget(int currentTarget, RewardArea[] rewards) {
        if (rewards.Length == 2) {
            return 1 - currentTarget; //returns 0 or 1
        }

        // minimally inclusive, maximally exclusive, therefore you will get random numbers between 0 to the number of rewards
        int nextTarget = Random.Range(0, rewards.Length);

        // retries if the random target number generated is the same as the current target number
        while (rewards.Length != 1 && nextTarget == currentTarget) {
            nextTarget = Random.Range(0, rewards.Length);
        }

        return nextTarget;
    }

    public override bool IsTrialCompleteAfterReward(bool currentTaskSuccess) {
        return true; // 1 poster per trial
    }

    public override void Setup(RewardArea[] rewards) {
        //nothing to do here
    }
}
