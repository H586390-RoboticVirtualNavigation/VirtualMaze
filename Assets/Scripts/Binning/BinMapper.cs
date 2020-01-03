using System.Collections.Generic;
using UnityEngine;

public abstract class BinMapper {
    public const int NoGroup = -1;
    public const int Poster = 0;

    public abstract int MapObjectToGroup(GameObject sceneObject);
    public abstract BinWallConfig MapObjectToBinWallConfig(int group, int numBinAtLength);
    public abstract Location MapObjectToLocation(int group, GameObject sceneObject);
    public abstract int MapBinToId(BinWall wall, Bin bin);
    public abstract int MapGroupToCache(int group);
}

public class DoubleTeeBinMapper : BinMapper {
    public const int Ceiling = 1;
    public const int Ground = 2;

    // Maze Walls Y -90 rot
    public const int MazeWallZNeg = 3;
    public readonly int[] MazeWallZPosGrp = { 19, 24 };

    public const int MazeWallZPos = 4;
    public readonly int[] MazeWallZNegGrp = { 7, 12 };

    public const int MazeWallXPos = 5;
    public readonly int[] MazeWallXPosGrp = { 13, 18 };

    public const int MazeWallXNeg = 6;
    public readonly int[] MazeWallXNegGrp = { 1, 6 };

    public readonly int[] MazeWallDivider = { 6, 12, 18, 24 };
    public readonly int[] MazeWallGrps = { MazeWallXNeg, MazeWallZNeg, MazeWallXPos, MazeWallZPos };


    public const int PillarWalls = 7;

    public override int MapBinToId(BinWall wall, Bin bin) {
        switch (wall.group) {
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

            case Poster:
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

    //decides numbering
    private List<int> ZPosPillarWalls = new List<int>() { 12, 15, 29, 26 };
    private List<int> ZNegPillarWalls = new List<int>() { 1, 4, 7, 10 };
    private List<int> XPosPillarWalls = new List<int>() { 25, 24, 8, 6 };
    private List<int> XNegPillarWalls = new List<int>() { 20, 3, 5, 21 };

    //decides offset
    private List<int> pillar1 = new List<int>() { 6, 29, 21, 10 };
    private List<int> pillar2 = new List<int>() { 1, 5, 25, 26 };
    private List<int> pillar3 = new List<int>() { 7, 8, 12, 20 };
    private List<int> pillar4 = new List<int>() { 3, 4, 24, 15 };

    private int MapPillarsToID(BinWall wall, Bin bin) {
        int objId = getOwnerId(wall.owner);

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

    private int getOwnerId(string objName) {
        int lastIndex = objName.LastIndexOf('_') + 1;
        return int.Parse(objName.Substring(lastIndex));
    }

    public override int MapObjectToGroup(GameObject sceneObject) {
        string objName = sceneObject.name;
        switch (objName) {
            case "Ceiling":
                return Ceiling;
            case "Ground":
                return Ground;
            //case "RewardArea":
            //    return NoGroup;
            default:
                int lastIndex = objName.LastIndexOf('_') + 1;
                if (lastIndex > 0) {
                    switch (objName.Substring(0, lastIndex)) {
                        case "wall_": //surrounding walls
                            int wall_id = getOwnerId(objName);
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
                        return Poster;
                    }
                }
                return -1;
        }
    }

    public override Location MapObjectToLocation(int group, GameObject sceneObject) {
        Location location;
        if (group == Poster) {
            Transform posterWall = sceneObject.GetComponent<Poster>().AttachedTo.transform;
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
            case Poster:
                location.rotation *= Quaternion.Euler(0, -90, 0);
                break;
            case NoGroup:
                return location;
            default:
                break;
        }

        return location;
    }

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

            case Poster:
            case PillarWalls:
                return new BinWallConfig(binWidth, binM_Height, 5, 3);

            case NoGroup:
                return new BinWallConfig();
            default:
                throw new System.NotSupportedException("Unknown group");
        }
    }

    public override int MapGroupToCache(int group) {
        switch (group) {
            case Ground:
            case Ceiling:
                return 0;

            case MazeWallXNeg:
            case MazeWallXPos:
            case MazeWallZNeg:
            case MazeWallZPos:
                return 1;

            case Poster:
            case PillarWalls:
                return 2;

            default:
                throw new System.NotSupportedException("Unknown cache group");
        }
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