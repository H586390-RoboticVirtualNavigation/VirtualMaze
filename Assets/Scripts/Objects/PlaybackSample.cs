using UnityEngine;

public class PlaybackSample : PlaybackData {
    public readonly Vector2 gaze;
    public readonly Vector3 pos;
    public readonly float rotY;

    public PlaybackSample(Vector2 gaze, Vector3 pos, float rotY, uint timestamp) : base(DataTypes.SAMPLE_TYPE, timestamp) {
        this.gaze = gaze.ConvertToUnityOriginCoordinate();
        this.pos = pos;
        this.rotY = rotY;
    }
}