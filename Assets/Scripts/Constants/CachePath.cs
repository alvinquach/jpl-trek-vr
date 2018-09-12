using UnityEngine;

/// <summary>
///     Contains folder paths for various types of persistent cached files.
/// </summary>
public class CachePath {

    /// <summary>
    ///     Root directory path of where persistent (remains after application
    ///     is closed) cached files are stored.
    /// </summary>
    public static readonly string PersistentRoot = Application.persistentDataPath;

    /// <summary>
    ///     Root directory path of where temporary cached files are stored.
    /// </summary>
    public static readonly string TemporaryRoot = Application.temporaryCachePath;

    public const string DataElevationModel = "dem";

    public const string Test = "test";

}