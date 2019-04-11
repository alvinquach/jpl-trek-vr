namespace TrekVRApplication {

    public struct TerrainModelMeshMetadata {

        public string DemUUID { get; set; }

        public float Radius { get; set; }

        public float HeightScale { get; set; }

        private int _lodLevels;
        /// <summary>
        ///     The number of LOD levels to be generated, excluding LOD 0 and physics LOD.
        /// </summary>
        public int LodLevels {
            get => _lodLevels;
            set => _lodLevels = MathUtils.Clamp(value, 0);
        }

        private int _baseDownsample;
        /// <summary>
        ///     <para>
        ///         The amount of downsampling applied to the DEM file to generate the 
        ///         mesh (LOD 0). The actual amount of downsampling applied is 2^value.
        ///     </para>
        ///     <para>
        ///         For example, a value of 0 will have no downsampling, while a value
        ///         of 3 will downsample the DEM image by a factor of 8.
        ///     </para>
        /// </summary>
        public int BaseDownsample {
            get => _baseDownsample;
            set => _baseDownsample = MathUtils.Clamp(value, 0);
        }

        /// <summary>
        ///     <para>
        ///         The amount of downsampling applied to the DEM file to generate the 
        ///         physics mesh. The actual amount of downsampling applied is 2^value.
        ///     </para>
        ///     <para>
        ///         For example, a value of 0 will have no downsampling, while a value
        ///         of 3 will downsample the DEM image by a factor of 8.
        ///     </para>
        ///     <para>
        ///         Set this to a negative number to indicate that a physics mesh does
        ///         not need to be generated.
        ///     </para>
        /// </summary>
        public int PhysicsDownsample { get; set; }

        /// <summary>
        ///     Whether or not an addtional LOD level is to be generated for physics.
        ///     Computed based on the physics downsampling level, the base downsampling
        ///     level, and the number of LOD levels.
        /// </summary>
        public bool GenerateAdditionalPhysicsLod {
            get => !(PhysicsDownsample < 0) && (PhysicsDownsample < BaseDownsample || PhysicsDownsample > BaseDownsample + LodLevels);
        }

        /// <summary>
        ///     The index of the physics LOD mesh in the generated mesh array.
        /// </summary>
        public int PhyiscsLodMeshIndex {
            get => PhysicsDownsample < 0 ? -1 : GenerateAdditionalPhysicsLod ? LodLevels + 1 : PhysicsDownsample - BaseDownsample;
        }

        /// <summary>
        ///     The total number of LOD levels to be generated, including LOD 0 and physics LOD.
        /// </summary>
        public int TotalLodLevels {
            get => LodLevels + (GenerateAdditionalPhysicsLod ? 2 : 1);
        }

    }

}