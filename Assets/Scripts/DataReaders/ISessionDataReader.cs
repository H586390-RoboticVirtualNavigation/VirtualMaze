using System;

public interface ISessionDataReader : IDisposable {
    SessionData CurrentData { get; }
    int CurrentIndex { get; }
    bool HasNext { get; }
    bool Next();
    void MoveToNextTrigger();
    void MoveToNextTrigger(SessionTrigger trigger);
}
