using UnityEngine;

public class PlaybackSample : PlaybackData {
    public readonly Vector2 gaze;
    public readonly Vector3 pos;
    public readonly float rotY;

    public PlaybackSample(Vector2 gaze, Vector3 pos, float rotY, DataTypes type, uint timestamp) : base(type, timestamp) {
        this.gaze = gaze;
        this.pos = pos;
        this.rotY = rotY;
    }
}