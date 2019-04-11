using UnityEngine;

namespace TrekVRApplication {

    public abstract class TerrainOverlayObject : MonoBehaviour {

        public TerrainOverlayController Controller { get; private set; }

        public abstract Material Material { get; set; }

        private int _depth = 0;
        public int Depth {
            get => _depth;
            set {
                if (_depth != value) {
                    _depth = value;
                    Vector3 position = transform.localPosition;
                    transform.localPosition = new Vector3(position.x, position.y, value);
                }
            }
        }

        public bool Enabled {
            get => gameObject.activeSelf;
            set {
                gameObject.SetActive(value);
            }
        }

        protected virtual void Awake() {
            Controller = GetComponentInParent<TerrainOverlayController>();
        }

        protected virtual void OnEnable() {
            Controller.UpdateTexture();
        }

    }

}
