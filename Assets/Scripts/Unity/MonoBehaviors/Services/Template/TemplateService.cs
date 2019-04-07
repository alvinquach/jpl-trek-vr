using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    [DisallowMultipleComponent]
    public class TemplateService : SingletonMonoBehaviour<TemplateService> {

        private Dictionary<string, GameObject> _templates = new Dictionary<string, GameObject>();

        public GameObject GetTemplate(string name) {
            if (!_templates.TryGetValue(name, out GameObject result)) {
                _templates[name] = result = transform.Find(name).gameObject;
            }
            return result;
        }

    }

}