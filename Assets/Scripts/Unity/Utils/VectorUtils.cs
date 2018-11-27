using UnityEngine;

namespace TrekVRApplication {

    public class VectorUtils : MonoBehaviour {

        public static void Print(Vector3 v) {
            Debug.Log($"({v.x}, {v.y}, {v.z})");
        }
    }

}
