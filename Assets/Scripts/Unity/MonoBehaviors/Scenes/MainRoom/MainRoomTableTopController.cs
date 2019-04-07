using System;
using UnityEngine;

namespace TrekVRApplication.Scenes.MainRoom {

    public class MainRoomTableTopController : MonoBehaviour {

        // TODO Make this a const (combine with LocalTerrainModel.ViewTransitionDuration).
        private const float UpAnimationDuration = 1.6f;

        private const float DownAnimationDuration = 0.2f;

        private readonly Vector3 DownPosition = Vector3.zero;

        // TODO Change this to constant
        private readonly Vector3 UpPosition = new Vector3(0, 0.5f, 0);

        private Type _currentTerrainModelType;

        private Vector3 _startPosition = Vector3.zero;

        private float _animationProgress = 1f;

        private void Start() {
            TerrainModelManager.Instance.OnCurrentTerrainModelChange += OnShowTerrainModel;
        }

        private void Update() {
            if (_currentTerrainModelType != null && _animationProgress < 1f) {
                float delta;
                Vector3 targetPosition;
                if (_currentTerrainModelType == typeof(LocalTerrainModel)) {
                    delta = Time.deltaTime / UpAnimationDuration;
                    targetPosition = UpPosition;
                }
                else {
                    delta = Time.deltaTime / DownAnimationDuration;
                    targetPosition = DownPosition;
                }
                _animationProgress = MathUtils.Clamp(_animationProgress + delta, 0, 1);
                transform.localPosition = Vector3.Lerp(_startPosition, targetPosition, _animationProgress);
            }
        }

        private void OnDestroy() {
            TerrainModelManager.Instance.OnCurrentTerrainModelChange -= OnShowTerrainModel;
        }

        private void OnShowTerrainModel(TerrainModel terrainModel) {
            if (!terrainModel) {
                _currentTerrainModelType = null;
                _animationProgress = 1f;
                return;
            }
            Type modelType = terrainModel.GetType();
            if (_currentTerrainModelType != modelType) {
                _currentTerrainModelType = modelType;
                _animationProgress = 0f;
                _startPosition = transform.localPosition;
            }
        }

    }

}

