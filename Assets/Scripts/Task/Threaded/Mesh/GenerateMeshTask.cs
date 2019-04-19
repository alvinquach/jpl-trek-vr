namespace TrekVRApplication {

    public abstract class GenerateMeshTask<T> : ThreadedTask<float, T[]> where T : MeshData {

        protected T[] _meshData;

        protected float _progress = 0.0f;
        public override float Progress => _progress;

        protected sealed override T[] Task() {
            Generate();
            return _meshData;
        }

        /// <summary>Generate the mesh data and store it in the member variable.</summary>
        protected abstract void Generate();

    }

}
