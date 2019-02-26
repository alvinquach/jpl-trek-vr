namespace TrekVRApplication {

    public class SearchFacetCount<T> {

        public readonly T name;

        public readonly int count;

        public SearchFacetCount(T name, int count) {
            this.name = name;
            this.count = count;
        }

        public SearchFacetCount(T name, long count) : this(name, (int)count) {

        }

    }

}
