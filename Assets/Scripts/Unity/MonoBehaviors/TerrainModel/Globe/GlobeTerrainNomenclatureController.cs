using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static TrekVRApplication.ServiceManager;

namespace TrekVRApplication {

    /// <summary>
    ///     This is very ugly code and with a bad algorithm and needs to be redone.
    /// </summary>
    public class GlobeTerrainNomenclatureController : MonoBehaviourWithTaskQueue {

        // TODO Move this somewhere else
        private const int NomenclatureCount = 8;

        /// <summary>
        ///     How much the coordinates have to change by before
        ///     an update is triggered.
        /// </summary>s
        private const float CoordinateChangeThreshold = 0.05f;

        private const float AngleRange = 30f;

        private const float UpdateRate = 0.91f;

        private XRInteractableGlobeTerrain _interactionController;

        private Camera _eye;

        private GameObject _pinsContainer;

        private readonly IList<GlobeTerrainNomenclature> _nomenclatures = new List<GlobeTerrainNomenclature>();

        private float _timeSinceLastUpdate;

        private Vector2 _currentCoordinates = Vector2.zero;

        private bool _repositionPinsOnVisible;

        private bool _visible = false;
        public bool Visible {
            get => _visible;
            set {
                _visible = value;
                if (_visible &&_repositionPinsOnVisible) {
                    RepositionPins();
                    _repositionPinsOnVisible = false;
                }
                SetPinsVisiblity(value);
            }
        }

        private void Awake() {
            _pinsContainer = new GameObject("PinsContainer");
            _pinsContainer.transform.SetParent(transform, false);
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.OnCurrentModelPhyscisMeshUpdated += OnPhyscisMeshUpdated;
            terrainModelManager.OnEnableTerrainInteractionChange += OnTerrainInteractionChange;
        }

        private void Start() {
            _interactionController = GetComponent<XRInteractableGlobeTerrain>();
            _eye = UserInterfaceManager.Instance.XRCamera;
        }

        private void OnEnable() {
            if (_visible && _repositionPinsOnVisible) {
                RepositionPins();
                _repositionPinsOnVisible = false;
            }
        }

        protected override void Update() {
            base.Update();

            if (!_visible) {
                return;
            }

            if (_timeSinceLastUpdate > 0) {
                _timeSinceLastUpdate -= Time.deltaTime;
            }
            else {
                // This method currently only takes account of the coordinates that is facing the user.
                // It does not care about the distance or the actual delta angle.
                // It needs to be reworked.

                Vector3 origin = _eye.transform.position;
                Vector3 direction = transform.position - origin;
                if (!Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, 1 << (int)CullingLayer.Terrain)) {
                    return;
                }

                if (!hit.collider.GetComponent<GlobeTerrainNomenclatureController>()) {
                    return;
                }
                Vector2 newCoordinates = hit.textureCoord;

                // Check if the coordinates have changed enough.
                if (!CheckWrappedCoordinateChange(newCoordinates.x, _currentCoordinates.x)
                    && !CheckCoordinateChange(newCoordinates.y, _currentCoordinates.y)) {

                    return;
                }

                _currentCoordinates = newCoordinates;
                Vector2 latLon = BoundingBoxUtils.UVToCoordinates(UnrestrictedBoundingBox.Global, newCoordinates);
                BoundingBox bbox = new BoundingBox(
                    latLon.y - AngleRange,
                    MathUtils.Clamp(latLon.x - AngleRange, -90),
                    latLon.y + AngleRange,
                    MathUtils.Clamp(latLon.x + AngleRange, 0, 90)
                );
                Debug.Log(bbox.ToString());
                SearchWebService.GetNomenclatures(bbox, 15, res => {
                    QueueTask(() => UpdatePins(res.Items));
                });
                _timeSinceLastUpdate = UpdateRate;
            }
        }

        private void OnDestroy() {
            TerrainModelManager terrainModelManager = TerrainModelManager.Instance;
            terrainModelManager.OnCurrentModelPhyscisMeshUpdated -= OnPhyscisMeshUpdated;
            terrainModelManager.OnEnableTerrainInteractionChange -= OnTerrainInteractionChange;
        }

        private void UpdatePins(IList<SearchResultItem> items) {
            GameObject pinTemplate = TemplateService.Instance.GetTemplate(GameObjectName.PinTemplate);
            foreach (GlobeTerrainNomenclature nomenclature in _nomenclatures.ToList()) {
                if (!items.Any(i => i.UUID == nomenclature.UUID)) {
                    Destroy(nomenclature.Pin);
                    _nomenclatures.Remove(nomenclature);
                }
            }
            foreach (SearchResultItem item in items) {
                if (_nomenclatures.Any(r => r.UUID == item.UUID)) {
                    continue;
                }
                BoundingBox boundingBox = BoundingBoxUtils.ParseBoundingBox(item.BoundingBox);
                Vector3 direction = BoundingBoxUtils.MedianDirection(boundingBox);
                GameObject pin = Instantiate(pinTemplate);
                pin.transform.SetParent(_pinsContainer.transform, false);
                pin.transform.forward = -(transform.rotation * direction);
                pin.GetComponentInChildren<Text>().text = item.Name;
                pin.SetActive(true);
                PositionPin(pin);
                _nomenclatures.Add(new GlobeTerrainNomenclature(item.UUID, pin));
            }
        }

        private void OnPhyscisMeshUpdated() {
            if (!_visible || !isActiveAndEnabled) {
                _repositionPinsOnVisible = true;
            }
            else {
                RepositionPins();
            }
        }

        private void OnTerrainInteractionChange(bool enabled) {
            if (!enabled || (enabled && _visible)) {
                SetPinsVisiblity(enabled);
            }
        }

        private void RepositionPins() {
            foreach(GlobeTerrainNomenclature nomenclature in _nomenclatures) {
                PositionPin(nomenclature.Pin);
            }
        }

        private void SetPinsVisiblity(bool visible) {
            foreach (GlobeTerrainNomenclature nomenclature in _nomenclatures) {
                nomenclature.Pin.SetActive(visible);
            }
        }

        private void PositionPin(GameObject pin) {
            // Assumes the planet's radius is not greater than 1.
            Vector3 direction = pin.transform.forward;
            Vector3 origin = transform.position - direction;
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, 1, 1 << (int)CullingLayer.Terrain)) {
                return;
            }
            pin.transform.position = hit.point;
        }

        private bool CheckWrappedCoordinateChange(float a, float b) {
            a = a > 0.5 ? a - 1 : a;
            b = b > 0.5 ? b - 1 : b;
            return CheckCoordinateChange(a, b);
        }

        private bool CheckCoordinateChange(float a, float b) {
            return Mathf.Abs(a - b) > CoordinateChangeThreshold;
        }

    }

}