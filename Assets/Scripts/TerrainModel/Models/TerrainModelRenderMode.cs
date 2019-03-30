using System;

namespace TrekVRApplication {

    [Flags]
    public enum TerrainModelRenderMode : int {

        Default = 0,
        Disabled = 1 << 0,
        NoTextures = 1 << 1,
        Overlay = 1 << 2

    }

}
