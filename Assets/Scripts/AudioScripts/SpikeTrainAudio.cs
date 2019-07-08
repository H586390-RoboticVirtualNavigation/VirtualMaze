using UnityEngine;


public class SpikeTrainAudio : MonoBehaviour {
    System.Random rand = new System.Random();


    private void OnAudioFilterRead(float[] data, int channels) {
        data[0] = -0.5f;
        data[1] = 1f;
        data[2] = 0.5f;
        data[3] = -1f;
    }
}
