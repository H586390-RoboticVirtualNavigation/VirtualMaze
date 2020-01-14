using System;
using System.Collections.Generic;
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
    /// <returns>The configurations required for the binwall</returns>
    public abstract BinWallConfig MapObjectToBinWallConfig(int group);

    public abstract void PlaceBinWall(int group, GameObject obj, BinWall binWall);

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
    public abstract bool IsSingleWallGroup(int group);

    public abstract float MaxPossibleSqDistance();
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
    private readonly int[] PillarBlue = { 6, 29, 21, 10 };
    private readonly int[] PillarGreen = { 1, 5, 25, 26 };
    private readonly int[] PillarYellow = { 7, 8, 12, 20 };
    private readonly int[] PillarRed = { 3, 4, 24, 15 };

    private const int NumberOfWallsInPillars = 4;

    private Dictionary<int, int> groupOffsetTable = new Dictionary<int, int>();

    //standard, for walls, ceiling and floor
    private float binWidth = 0.625f; //25/40
    private float binM_Height = 0.6f; //3 unity units/5 bins

    /* Known values aquired from the scene */
    private const float FloorWidth = 25f; // width is enough since it is square
    private const float WallHeight = 5f; //4.93 in the scene but rounded up
    private const float PillarWallHeight = 3f;

    private int maxSqDist = Mathf.CeilToInt(FloorWidth * FloorWidth);

    public DoubleTeeBinMapper(int numOfBinsForFloorLength) {
        binWidth = FloorWidth / numOfBinsForFloorLength;
        binM_Height = PillarWallHeight / Mathf.CeilToInt(PillarWallHeight / binWidth);

        int numFloorBins = numOfBinsForFloorLength * numOfBinsForFloorLength;
        int numWallsBins = numOfBinsForFloorLength * Mathf.RoundToInt(WallHeight / binWidth);

        groupOffsetTable[CueImage] = 1; // first bin no need for offset but actual id is 1 based
        groupOffsetTable[HintImage] = groupOffsetTable[CueImage] + 1;
        groupOffsetTable[Ground] = groupOffsetTable[HintImage] + 1;// cue and hint image
        groupOffsetTable[Ceiling] = groupOffsetTable[Ground] + numFloorBins;

        groupOffsetTable[MazeWallXNeg] = groupOffsetTable[Ceiling] + numFloorBins;
        groupOffsetTable[MazeWallZNeg] = groupOffsetTable[MazeWallXNeg];
        groupOffsetTable[MazeWallXPos] = groupOffsetTable[MazeWallZNeg];
        groupOffsetTable[MazeWallZPos] = groupOffsetTable[MazeWallXPos];

        groupOffsetTable[PillarWalls] = groupOffsetTable[MazeWallZPos] + numWallsBins * 4;
    }

    /// <summary>
    /// See <see cref="BinMapper.MapBinToId(BinWall, Bin)"/>
    /// </summary>
    public override int MapBinToId(BinWall wall, Bin bin) {
        int group = bin.Group;

        if (group == Poster_group) {
            group = GetGroupID(wall.parent);
        }

        int offset = groupOffsetTable[group];

        /* multiplied by 4 because of 4 walls */
        switch (group) {
            /* Single bins, actual ids computed in offset table */
            case HintImage:
            case CueImage:
                return offset;

            case Ground:
                return bin.id + offset;

            case Ceiling:
                int h = wall.numHeight - 1 - bin.id / wall.numWidth;
                return bin.id % wall.numWidth + h * wall.numWidth + offset;

            case MazeWallZNeg:
                h = bin.id / wall.numWidth;
                int numInRing = (wall.numWidth * 4) * h + wall.numWidth;
                return numInRing + bin.id % wall.numWidth + offset;

            case MazeWallZPos:
                h = bin.id / wall.numWidth;
                numInRing = (wall.numWidth * 4) * h + wall.numWidth * 3;
                return numInRing + bin.id % wall.numWidth + offset;

            case MazeWallXNeg:
                h = bin.id / wall.numWidth;
                numInRing = (wall.numWidth * 4) * h;
                return numInRing + bin.id % wall.numWidth + offset;

            case MazeWallXPos:
                h = bin.id / wall.numWidth;
                numInRing = (wall.numWidth * 4) * h + wall.numWidth * 2;
                return numInRing + bin.id % wall.numWidth + offset;

            case PillarWalls:
                return MapPillarsToID(wall, bin) + offset;

            case NoGroup:
            default:
                throw new Exception("NoGroup or unexpected group used to actual ID");
        }
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
    /// See <see cref="BinMapper.PlaceBinWall"/>
    /// </summary>
    public override void PlaceBinWall(int group, GameObject sceneObject, BinWall binWall) {
        Location location;
        GameObject posterWall = null;
        if (group == Poster_group) {
            posterWall = sceneObject.GetComponent<Poster>().AttachedTo;
            location = Location.CopyTransform(posterWall.transform);
        }
        else {
            location = Location.CopyTransform(sceneObject.transform);
        }

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
            default:
                break;
        }

        if (posterWall != null) {
            binWall?.AttachTo(location, posterWall, GetGroupID(posterWall));
        }
        else {
            binWall?.AttachTo(location, sceneObject, group);
        }
    }

    /// <summary>
    /// See <see cref="BinMapper.MapObjectToBinWallConfig(int, int)"/>
    /// </summary>
    public override BinWallConfig MapObjectToBinWallConfig(int group) {
        switch (group) {
            case Ground:
            case Ceiling:
                return new BinWallConfig(binWidth, binWidth, FloorWidth, FloorWidth);

            case MazeWallXNeg:
            case MazeWallXPos:
            case MazeWallZNeg:
            case MazeWallZPos:
                return new BinWallConfig(binWidth, binWidth, FloorWidth, WallHeight);

            case Poster_group:
            case PillarWalls:
                return new BinWallConfig(binWidth, binM_Height, WallHeight, PillarWallHeight);

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

    public override bool IsSingleWallGroup(int group) {
        return MazeWallGrps.Contains(group);
    }

    private Dictionary<string, Tuple<int, int>> PillarOffsetTable = new Dictionary<string, Tuple<int, int>>();

    private int MapPillarsToID(BinWall wall, Bin bin) {

        int objId = GetOwnerId(wall.owner);

        int ringNum = wall.numWidth * NumberOfWallsInPillars;
        int height = bin.id / wall.numWidth;

        /* wall id runs in the other direction as compared to the actual bin ids required.
         * -1 to map the bins properly when fillping their direction*/
        int remaindingBins = (wall.numWidth - 1) - bin.id % wall.numWidth;//

        int pillarOffset = 0;
        int pillarDirectionOffset = 0;

        if (PillarOffsetTable.TryGetValue(wall.owner, out Tuple<int, int> offsets)) {
            pillarOffset = offsets.Item1;
            pillarDirectionOffset = offsets.Item2;
        }
        else {
            /* pillar Green requires no offset */ //
            if (PillarBlue.Contains(objId)) {
                pillarOffset = NumberOfWallsInPillars * wall.NumberOfBins;
            }
            else if (PillarRed.Contains(objId)) {
                pillarOffset = 2 * NumberOfWallsInPillars * wall.NumberOfBins;
            }
            else if (PillarYellow.Contains(objId)) {
                pillarOffset = 3 * NumberOfWallsInPillars * wall.NumberOfBins;
            }

            //XNegPillarWalls requires no offset due to the direction it is facing
            if (ZPosPillarWalls.Contains(objId)) {
                pillarDirectionOffset = 1 * wall.numWidth;
            }
            else if (XPosPillarWalls.Contains(objId)) {
                pillarDirectionOffset = 2 * wall.numWidth;
            }
            else if (ZNegPillarWalls.Contains(objId)) {
                pillarDirectionOffset = 3 * wall.numWidth;
            }

            PillarOffsetTable[wall.owner] = new Tuple<int, int>(pillarOffset, pillarDirectionOffset);
        }

        return ringNum * height + remaindingBins + pillarOffset + pillarDirectionOffset;
    }

    private int GetOwnerId(string objName) {
        int lastIndex = objName.LastIndexOf('_') + 1;
        return int.Parse(objName.Substring(lastIndex));
    }

    public override float MaxPossibleSqDistance() {
        return maxSqDist;
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