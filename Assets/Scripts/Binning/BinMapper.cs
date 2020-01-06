using UnityEngine;

public abstract class BinMapper {
    public const int NoGroup = -1;

    /// <summary>
    /// Returns the groupID of the object to assign a binwall
    /// </summary>
    /// <param name="sceneObject">Object to assign a binwall</param>
    /// <returns>Group ID of the object</returns>
    public abstract int GetGroupID(GameObject sceneObject);

    /// <summary>
    /// Assigns the size and number of bins in each Binwall
    /// </summary>
    /// <param name="group">Group id of the object</param>
    /// <param name="numBinAtLength">Required number of bins for the length of the floor</param>
    /// <returns>The configurations required for the binwall</returns>
    public abstract BinWallConfig MapObjectToBinWallConfig(int group, int numBinAtLength);

    /// <summary>
    /// Calculate the position of the binwall to based on the group and Transform of the object.
    /// 
    /// Calls <see cref="GetLocation(int, GameObject)"/>  to extract the location of the object.
    /// </summary>
    /// <param name="group">Group id of the object</param>
    /// <param name="obj">Object to assign a Binwall to</param>
    /// <returns></returns>
    public abstract Location GetLocationOfBinWall(int group, GameObject obj);

    /// <summary>
    /// Using the group id, extract the location of the GameObject
    /// </summary>
    /// <param name="group">Group id of the object</param>
    /// <param name="obj">Object to assign a Binwall to</param>
    /// <returns></returns>
    protected abstract Location GetLocation(int group, GameObject obj);

    /// <summary>
    /// Maps id of the bin in the binwall to the proper ID required.
    /// </summary>
    /// <param name="wall">Wall which the bin belongs to</param>
    /// <param name="bin">The bin to map</param>
    /// <returns>Actual ID of the bin to be saved</returns>
    public abstract int MapBinToId(BinWall wall, Bin bin);

    /// <summary>
    /// Binwalls are cached within <see cref="BinWallManager"/> according to the distinct configurations of the binwall.
    /// </summary>
    /// <example>
    /// <para>Example:</para> 
    /// If Wall A and B has the same wall size but different number of bins, return 1 for Wall A, 0 for Wall B
    /// </example>
    /// <param name="objGroup">integer representing the group the object belongs to</param>
    /// <returns>integer representing the wall cache the wall should belong in</returns>
    public abstract int MapGroupToWallCache(int objGroup);

    /// <summary>
    /// Used to help identify special cases when reusing cached Binwalls.
    /// </summary>
    /// <param name="group">Group that the object belongs to</param>
    /// <param name="obj">Object to assign a binwall to</param>
    /// <returns></returns>
    /// <see cref="BinWallManager.AssignBinwall(GameObject, GameObject, BinMapper)"/>
    public abstract string GetSpecialCacheId(int group, GameObject obj);
}

public class DoubleTeeBinMapper : BinMapper {
    public const int Poster_group = 0;
    public const int CueImage = 1;
    public const int HintImage = 2;
    public const int Ceiling = 3;
    public const int Ground = 4;

    public const int MazeWallZNeg = 5;
    public const int MazeWallZPos = 6;
    public const int MazeWallXPos = 7;
    public const int MazeWallXNeg = 8;
    public const int PillarWalls = 9;

    /* Range of walls numbers in the group. 
     * Eg. wall_7, wall_8, wall_9, ..., wall_12 */
    public readonly int[] MazeWallZNegGrp = { 7, 12 };
    public readonly int[] MazeWallZPosGrp = { 19, 24 };
    public readonly int[] MazeWallXPosGrp = { 13, 18 };
    public readonly int[] MazeWallXNegGrp = { 1, 6 };

    /* Array of the largest number in each group and the resultant map to the group.
     * Mainly for convenience, see usage. */
    public readonly int[] MazeWallDivider = { 6, 12, 18, 24 };
    public readonly int[] MazeWallGrps = { MazeWallXNeg, MazeWallZNeg, MazeWallXPos, MazeWallZPos };

    /* Maze wall numbers corresponding to the direction where the red arrow of the 
     * object shares the direction of the global orientation. Red Arrow when the tool
     * Handle Rotation is set to local in the editor.
     * 
     * https://docs.unity3d.com/Manual/PositioningGameObjects.html
     */
    private readonly int[] ZPosPillarWalls = { 12, 15, 29, 26 };
    private readonly int[] ZNegPillarWalls = { 1, 4, 7, 10 };
    private readonly int[] XPosPillarWalls = { 25, 24, 8, 6 };
    private readonly int[] XNegPillarWalls = { 20, 3, 5, 21 };

    /* Maze wall numbers grouped according to the pillars they belong in. */
    private readonly int[] pillar1 = { 6, 29, 21, 10 };
    private readonly int[] pillar2 = { 1, 5, 25, 26 };
    private readonly int[] pillar3 = { 7, 8, 12, 20 };
    private readonly int[] pillar4 = { 3, 4, 24, 15 };

    /// <summary>
    /// See <see cref="BinMapper.MapBinToId(BinWall, Bin)"/>
    /// </summary>
    public override int MapBinToId(BinWall wall, Bin bin) {
        switch (bin.Group) {
            case HintImage:
                return 2;

            case CueImage:
                return 1;

            case Ground:
                return bin.id + 3;

            case Ceiling:
                int h = wall.numHeight - 1 - bin.id / wall.numWidth;
                return bin.id % wall.numWidth + h * wall.numWidth + 1603;

            case MazeWallZNeg:
                h = bin.id / wall.numWidth;
                int numInRing = wall.numWidth * 4 * h + wall.numWidth;
                return numInRing + bin.id % wall.numWidth + 3 + 3200;

            case MazeWallZPos:
                h = bin.id / wall.numWidth;
                numInRing = wall.numWidth * 4 * h + wall.numWidth * 3;
                return numInRing + bin.id % wall.numWidth + 3 + 3200;

            case MazeWallXNeg:
                h = bin.id / wall.numWidth;
                numInRing = wall.numWidth * 4 * h;
                return numInRing + bin.id % wall.numWidth + 3 + 3200;

            case MazeWallXPos:
                h = bin.id / wall.numWidth;
                numInRing = wall.numWidth * 4 * h + wall.numWidth * 2;
                return numInRing + bin.id % wall.numWidth + 3 + 3200;

            case Poster_group:
                return bin.id;

            case PillarWalls:
                return MapPillarsToID(wall, bin);
            case NoGroup:
                break;

            default:
                break;
        }
        return -1;
    }

    /// <summary>
    /// See <see cref="BinMapper.GetGroupID(GameObject)"/>
    /// </summary>
    public override int GetGroupID(GameObject sceneObject) {
        string objName = sceneObject.name;
        switch (objName) {
            case "HintBinCollider":
                return HintImage;
            case "CueBinCollider":
                return CueImage;
            case "Ceiling":
                return Ceiling;
            case "Ground":
                return Ground;
            default:
                int lastIndex = objName.LastIndexOf('_') + 1;
                if (lastIndex > 0) {
                    switch (objName.Substring(0, lastIndex)) {
                        case "wall_": //surrounding walls
                            int wall_id = GetOwnerId(objName);
                            for (int i = 0; i < MazeWallDivider.Length; i++) {
                                if (wall_id <= MazeWallDivider[i]) {
                                    return MazeWallGrps[i];
                                }
                            }
                            break;
                        case "m_wall_"://pillars
                            return PillarWalls;
                    }
                }
                else {
                    if (objName.Contains("Poster")) {
                        //objname contains "poster"
                        return Poster_group;
                    }
                }
                return NoGroup;
        }
    }

    /// <summary>
    /// See <see cref="BinMapper.GetLocationOfBinWall(int, GameObject)"/>
    /// </summary>
    public override Location GetLocationOfBinWall(int group, GameObject sceneObject) {
        Location location = GetLocation(group, sceneObject);

        switch (group) {
            case Ground:
            case Ceiling:
                location.position.z = 0.13f;
                location.position.x = -0.22f;
                location.rotation *= Quaternion.Euler(90, 0, 0);
                break;

            case MazeWallXNeg:
            case MazeWallXPos:
                location.position.z = 0.13f;
                location.rotation *= Quaternion.Euler(0, -90, 0);
                break;

            case MazeWallZNeg:
            case MazeWallZPos:
                location.position.x = -0.22f;
                location.rotation *= Quaternion.Euler(0, -90, 0);
                break;

            case PillarWalls:
            case Poster_group:
                location.rotation *= Quaternion.Euler(0, -90, 0);
                break;

            case HintImage: //no group since it already exist in the scene
            case CueImage: //no group since it already exist in the scene
            case NoGroup:
                return location;

            default:
                break;
        }

        return location;
    }

    /// <summary>
    /// See <see cref="BinMapper.MapObjectToBinWallConfig(int, int)"/>
    /// </summary>
    public override BinWallConfig MapObjectToBinWallConfig(int group, int numBinAtLength) {
        //standard, for walls, ceiling and floor
        float binWidth = 0.625f; //25/40

        float binM_Height = 0.6f; //3 unity units/5 bins

        switch (group) {
            case Ground:
            case Ceiling:
                return new BinWallConfig(binWidth, binWidth, 25, 25);

            case MazeWallXNeg:
            case MazeWallXPos:
            case MazeWallZNeg:
            case MazeWallZPos:
                return new BinWallConfig(binWidth, binWidth, 25, 5);

            case Poster_group:
            case PillarWalls:
                return new BinWallConfig(binWidth, binM_Height, 5, 3);

            case HintImage: //no group since it already exist in the scene
            case CueImage: //no group since it already exist in the scene
            case NoGroup:
                return new BinWallConfig();
            default:
                throw new System.NotSupportedException("Unknown group");
        }
    }

    /// <summary>
    /// See <see cref="BinMapper.MapGroupToWallCache(int)"/>
    /// </summary>
    public override int MapGroupToWallCache(int group) {
        switch (group) {
            case Ground:
            case Ceiling:
                return 0;

            case MazeWallXNeg:
            case MazeWallXPos:
            case MazeWallZNeg:
            case MazeWallZPos:
                return 1;

            case Poster_group:
            case PillarWalls:
                return 2;

            case HintImage: //return any unused number
            case CueImage: //return any unused number
                return 3;

            default:
                throw new System.NotSupportedException("Unknown cache group");
        }
    }

    /// <summary>
    /// See <see cref="BinMapper.GetSpecialCacheId(int, GameObject)"/>
    /// </summary>
    public override string GetSpecialCacheId(int group, GameObject obj) {
        switch (group) {
            case Poster_group:
                return Poster.GetNameOfAttachedTo(obj);
            default:
                return null;
        }
    }

    /// <summary>
    /// See <see cref="BinMapper.GetLocation(int, GameObject)"/>
    /// </summary>
    protected override Location GetLocation(int group, GameObject obj) {
        switch (group) {
            case Poster_group:
                Transform posterWall = obj.GetComponent<Poster>().AttachedTo.transform;
                return Location.CopyTransform(posterWall.transform);

            default:
                return Location.CopyTransform(obj.transform);
        }
    }

    private int MapPillarsToID(BinWall wall, Bin bin) {
        int objId = GetOwnerId(wall.owner);

        int ringNum = wall.numWidth * 4;
        int height = bin.id / wall.numWidth;

        int offset = 4 * wall.numHeight * wall.numWidth;

        if (pillar1.Contains(objId)) {
            offset *= 0;
        }
        else if (pillar2.Contains(objId)) {
            offset *= 1;
        }
        else if (pillar3.Contains(objId)) {
            offset *= 2;
        }
        else if (pillar4.Contains(objId)) {
            offset *= 3;
        }

        if (ZPosPillarWalls.Contains(objId)) {
            return ringNum * height + 1 * wall.numWidth + wall.numWidth - bin.id % wall.numWidth + offset;
        }
        else if (XPosPillarWalls.Contains(objId)) {
            return ringNum * height + 2 * wall.numWidth + wall.numWidth - bin.id % wall.numWidth + offset;
        }
        else if (ZNegPillarWalls.Contains(objId)) {
            return ringNum * height + 3 * wall.numWidth + wall.numWidth - bin.id % wall.numWidth + offset;
        }
        else if (XNegPillarWalls.Contains(objId)) {
            return ringNum * height + wall.numWidth - bin.id % wall.numWidth + offset;
        }
        return bin.id;
    }

    private int GetOwnerId(string objName) {
        int lastIndex = objName.LastIndexOf('_') + 1;
        return int.Parse(objName.Substring(lastIndex));
    }
}

public struct Location {
    public Vector3 position;
    public Quaternion rotation;

    public Location(Vector3 position, Quaternion rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public static Location CopyTransform(Transform t) {
        return new Location(t.position, t.rotation);
    }
}