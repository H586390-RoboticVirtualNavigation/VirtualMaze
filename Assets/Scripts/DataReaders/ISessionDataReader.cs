using System;

public interface ISessionDataReader : IDisposable {
    SessionData CurrentData { get; }
    int CurrentIndex { get; }
    bool HasNext { get; }

    /// <summary>
    /// A value of 0 to 1 representing the read progress of the file.
    /// </summary>
    float ReadProgress { get; }

    bool Next();
    void MoveToNextTrigger();
    void MoveToNextTrigger(SessionTrigger trigger);
}
