using System;
using UnityEngine;

namespace TrekVRApplication.Scenes.MainRoom {

    [DisallowMultipleComponent]
    public class MainRoomTerrainControlPanelGroup : MonoBehaviour {

        public static MainRoomTerrainControlPanelGroup Instance { get; private set; }

        [SerializeField]
        private TerrainControlPanel _screen;

        [SerializeField]
        private MainRoomTerrainControlPanelInstance _controlPanel1;

        [SerializeField]
        private MainRoomTerrainControlPanelInstance _controlPanel2;

        [SerializeField]
        private MainRoomTerrainControlPanelInstance _controlPanel3;

        [SerializeField]
        private MainRoomTerrainControlPanelInstance _controlPanel4;

        private MainRoomTerrainControlPanelInstance _activeControlPanel;
        public MainRoomTerrainControlPanelInstance ActiveControlPanel {
            get => _activeControlPanel;
            private set {
                if (_activeControlPanel != value) {
                    if (_activeControlPanel) {
                        _activeControlPanel.Deactivate();
                    }
                    _activeControlPanel = value;
                    if (_activeControlPanel) {
                        _activeControlPanel.Activate(_screen);
                    }
                }
            }
        }

        public MainRoomTerrainControlPanelGroup() {
            if (Instance == null) {
                Instance = this;
            }
            else if (Instance != this) {
                Destroy(this);
                throw new Exception($"Only one instance of {GetType().Name} is allowed.");
            }
        }

        private void Start() {
            ActiveControlPanel = _controlPanel1;
        }

        public void ActivateControlPanel(MainRoomTerrainControlPanelInstance instance) {
            ActiveControlPanel = instance;
        }

    }

}
