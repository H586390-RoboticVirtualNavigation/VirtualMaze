/// <summary>
/// Sample Session Data in textual format
/// 
/// 16 0.01696440 0.0000 -10.0000 0.0000 
/// </summary>
public class SessionData {
    public int flag { get; private set; } = -1;
    public decimal timeDelta { get; private set; } = -1;

    public RobotConfiguration config { get; private set; } = null;

    public SessionTrigger trigger { get; private set; }

    public string rawData { get; private set; }

    public decimal timeDeltaMs { get { return timeDelta * 1000m; } }

    public SessionData(int flag, decimal timeDelta, double posX, double posZ, double rotY) {
        this.flag = flag;
        this.timeDelta = timeDelta;

        config = new RobotConfiguration(posX, posZ, rotY);

        trigger = (SessionTrigger)((flag / 10) * 10);
    }
}
