using BitMiracle.LibTiff.Classic;
using System;
using System.Threading;
using UnityEngine;

namespace TrekVRApplication {

    public class FileResourceDebug : MonoBehaviour {

        private const string TiffFilePath = "D:/Alvin/Downloads/subset.tif";

        private bool _ready = false;

        private void Awake() {

            OpenTiffResourceThreaded(() => {
                _ready = true;
            });

        }

        private void Update() {
            if (_ready) {
                OpenTiffResourceThreaded();
                _ready = false;
            }
        }

        private void OpenTiffResource() {

            using (TiffImage tiff = new TiffImage(TiffFilePath)) {

            }

        }

        private void OpenTiffResourceThreaded(Action callback = null) {

            ThreadPool.QueueUserWorkItem(state => {
                OpenTiffResource();
                callback?.Invoke();
            });

        }

    }

}
