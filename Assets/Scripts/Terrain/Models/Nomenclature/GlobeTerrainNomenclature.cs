using System.Collections.Generic;
using UnityEngine;

namespace TrekVRApplication {

    public class GlobeTerrainNomenclature {

        public string UUID { get; }

        public GameObject Pin { get; }

        public GlobeTerrainNomenclature(string uuid, GameObject pin) {
            UUID = uuid;
            Pin = pin;
        }

    }

}