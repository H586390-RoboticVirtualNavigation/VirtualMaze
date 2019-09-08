public class PlaybackEvent : PlaybackData {
    public readonly string message;
    public readonly SessionTrigger trigger;

    public PlaybackEvent(string message, SessionTrigger trigger, uint timestamp) : base(DataTypes.MESSAGEEVENT, timestamp) {
        this.message = message;
        this.trigger = trigger;
    }
}
