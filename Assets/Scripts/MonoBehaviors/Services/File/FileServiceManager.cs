using UnityEngine;
using System.Collections;
using System.IO;

[DisallowMultipleComponent]
public class FileServiceManager : MonoBehaviour {

    /// <summary>
    ///     The instance of the FileServiceManager that is present in the scene.
    ///     There should only be one FileServiceManager in the entire scene.
    /// </summary>
    public static FileServiceManager Instance { get; private set; }

    void Awake() {

        if (Instance == null) {
            Instance = this;
        }

    }
}
