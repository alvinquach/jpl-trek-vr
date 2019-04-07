using UnityEngine;

namespace TrekVRApplication.Scenes.MainRoom {

    [DisallowMultipleComponent]
    public class MainRoomTerrainControlPanelGroup : SingletonMonoBehaviour<MainRoomTerrainControlPanelGroup> {

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

        private void Start() {
            ActiveControlPanel = _controlPanel1;
            TerrainModelManager.Instance.OnEnableTerrainInteractionChange += EnableControlPanels;
        }

        private void OnDestroy() {
            TerrainModelManager.Instance.OnEnableTerrainInteractionChange -= EnableControlPanels;
        }

        public void ActivateControlPanel(MainRoomTerrainControlPanelInstance instance) {
            ActiveControlPanel = instance;
        }

        private void EnableControlPanels(bool enabled) {
            if (_controlPanel1) {
                _controlPanel1.SetEnabled(enabled);
            }
            if (_controlPanel2) {
                _controlPanel2.SetEnabled(enabled);
            }
            if (_controlPanel3) {
                _controlPanel3.SetEnabled(enabled);
            }
            if (_controlPanel4) {
                _controlPanel4.SetEnabled(enabled);
            }
        }

    }

}
