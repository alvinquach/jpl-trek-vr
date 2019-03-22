using UnityEngine;
using System.Collections.Generic;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TemplateService : MonoBehaviour {

        public static TemplateService Instance { get; private set; }

        private Dictionary<string, GameObject> _templates = new Dictionary<string, GameObject>();

        void Awake() {
            if (!Instance) {
                Instance = this;
            }
            else if (Instance != this) {
                Destroy(gameObject);
                return;
            }
        }

        public GameObject GetTemplate(string name) {
            if (!_templates.TryGetValue(name, out GameObject result)) {
                _templates[name] = result = transform.Find(name).gameObject;
            }
            return result;
        }

    }

}