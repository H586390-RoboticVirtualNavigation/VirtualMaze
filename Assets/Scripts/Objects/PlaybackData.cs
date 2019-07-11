public abstract class PlaybackData {
    public readonly DataTypes type;
    public readonly uint timestamp;
    public bool HasSpike;

    public PlaybackData(DataTypes type, uint timestamp) {
        this.type = type;
        this.timestamp = timestamp;
    }
}
