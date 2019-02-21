namespace TrekVRApplication {

    public abstract class GenerateMeshTask : ThreadedTask<float, MeshData[]> {

        protected MeshData[] _meshData;

        protected float _progress = 0.0f;

        public override float GetProgress() {
            return _progress;
        }

        protected sealed override MeshData[] Task() {
            Generate();
            return _meshData;
        }

        /// <summary>Generate the mesh data and store it in the member variable.</summary>
        protected abstract void Generate();

    }

}
