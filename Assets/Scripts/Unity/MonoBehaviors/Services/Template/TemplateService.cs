using UnityEngine;
using System.Collections.Generic;
using System;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TemplateService : MonoBehaviour {

        public static TemplateService Instance { get; private set; }

        private Dictionary<string, GameObject> _templates = new Dictionary<string, GameObject>();

        public TemplateService() {
            if (!Instance) {
                Instance = this;
            } else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instance of {GetType().Name} is allowed.");
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