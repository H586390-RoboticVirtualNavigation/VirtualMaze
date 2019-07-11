public class PlaybackEvent : PlaybackData {
    public readonly string message;
    public readonly SessionTrigger trigger;

    public PlaybackEvent(string message, SessionTrigger trigger, DataTypes type, uint timestamp) : base(type, timestamp) {
        this.message = message;
        this.trigger = trigger;
    }
}
