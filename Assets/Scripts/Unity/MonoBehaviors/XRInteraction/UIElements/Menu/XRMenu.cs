using UnityEngine;
using System;

namespace TrekVRApplication {

    [Obsolete("Menu interaction is now done using an embedded browser.")]
    public class XRMenu : MonoBehaviour {

        public float distance = 2.0f;

        public float height = 0.69f;

        // Use this for initialization
        void Start() {
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update() {

        }

    }

}
