using System.Collections.Generic;
using UnityEngine;

public class BinWallManager : ScriptableObject {
    const int GroundCeiling = 0;
    const int PillarWalls = 1;
    const int MazeWalls = 2;

    private static Dictionary<int, List<BinWall>> wallCache = new Dictionary<int, List<BinWall>>();

    private static HashSet<string> activated = new HashSet<string>();

    public static void BinObject(GameObject obj, GameObject binWallPrefab, BinMapper mapper) {
        if (activated.Contains(obj.name)) {
            return;
        }

        activated.Add(obj.name);

        int group = mapper.MapObjectToGroup(obj);
        if (group == BinMapper.Poster) {
            string attached = Poster.GetNameOfAttachedTo(obj);
            if (activated.Contains(attached)) {
                return;
            }
            else {
                activated.Add(attached);
            }
        }

        Location location = mapper.MapObjectToLocation(group, obj);

        //identification
        BinWall binWall = GetAvailableBinWall(group, binWallPrefab, mapper);

        binWall?.AttachTo(location, obj.name, group);
    }

    private static void PositionBinWall(string objName) {

    }

    public static void Reset() {
        activated.Clear();
        foreach (List<BinWall> pool in wallCache.Values) {
            foreach (BinWall wall in pool) {
                wall.Reset();
                wall.gameObject.SetActive(false);
            }
        }
    }

    private static BinWall GetAvailableBinWall(int group, GameObject binWallPrefab, BinMapper mapper) {
        if (group == BinMapper.NoGroup) {
            return null;
        }

        BinWallConfig config = mapper.MapObjectToBinWallConfig(group, 40);

        int cache_id = mapper.MapGroupToCache(group);

        if (cache_id == 1 && activated.Contains(group.ToString())) {
            return null;
        }

        activated.Add(group.ToString());

        if (wallCache.TryGetValue(cache_id, out List<BinWall> pool)) {
            foreach (BinWall wall in pool) {
                if (!wall.gameObject.activeSelf) {
                    return wall;
                }
            }
        }
        else {
            pool = new List<BinWall>();
            wallCache.Add(cache_id, pool);
        }

        BinWall binWall = Instantiate(binWallPrefab).GetComponent<BinWall>();

        binWall.CreateWall(config);
        pool.Add(binWall);
        return binWall;
    }
}
