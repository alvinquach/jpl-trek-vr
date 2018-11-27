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
            if (!_templates.ContainsKey(name)) {
                _templates[name] = transform.Find(name).gameObject;
            }
            return _templates[name];
        }

    }

}