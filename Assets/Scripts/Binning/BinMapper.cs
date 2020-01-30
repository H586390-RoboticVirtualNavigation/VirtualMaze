using UnityEngine;

public abstract class BinMapper {
    public const int NoGroup = -1;
    public const int DEFAULT_NUM_BIN_LENGTH = 40;

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

    /// <summary>
    /// Place the binwall based on the location of the object identified
    /// </summary>
    /// <param name="group">group the object belongs to</param>
    /// <param name="obj">object identified</param>
    /// <param name="binWall">binwall to be relocated</param>
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

    /// <summary>
    /// Return True if the given group represents multiple objects in the scene.
    /// </summary>
    /// <param name="group">group id of the object identified</param>
    /// <returns>Return True if the given group represents multiple objects in the scene.</returns>
    public abstract bool IsSingleWallGroup(int group);

    /// <summary>
    /// The length of the maximum possible distance in the maze squared for raycasting optimization purposes.
    /// </summary>
    /// <returns>Return length of the maximum possible distance in the maze squared</returns>
    public abstract float MaxPossibleSqDistance();
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