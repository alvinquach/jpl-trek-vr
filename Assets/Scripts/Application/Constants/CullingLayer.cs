namespace TrekVRApplication {

    public enum CullingLayer : int {

        Default = 0,
        TransparentFX = 1,
        Ignore_Raycast = 2,
        Water = 4,
        UI = 5,
        TerrainShadowCaster = 8,
        Terrain = 9,
        World = 10,
        RenderToTexture = 11

    }

}
