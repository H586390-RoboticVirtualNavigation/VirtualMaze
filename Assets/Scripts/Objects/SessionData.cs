using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sample Session Data in textual format
/// 
/// 16 0.01696440 0.0000 -10.0000 0.0000 
/// </summary>
public class SessionData {
    public int flag { get; private set; } = -1;
    public float timeDelta { get; private set; } = -1;
    public float posX { get; private set; } = -1;
    public float posZ { get; private set; } = -1;
    public float rotY { get; private set; } = -1;
    public SessionTrigger trigger { get { return (SessionTrigger)((flag / 10) * 10); } }

    public string rawData { get; private set; }

    public float timeDeltaMs { get { return timeDelta * 1000; } }

    public SessionData(int flag, float timeDelta, float posX, float posZ, float rotY) {
        this.flag = flag;
        this.timeDelta = timeDelta;
        this.posX = posX;
        this.posZ = posZ;
        this.rotY = rotY;
    }
}
