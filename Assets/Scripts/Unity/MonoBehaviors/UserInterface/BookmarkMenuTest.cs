using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TrekVRApplication.TerrainModelConstants;

namespace TrekVRApplication {

    public class BookmarkMenuTest : MonoBehaviourWithTaskQueue {

        private IBookmarksWebService _bookmarksWebService = TrekBookmarksWebService.Instance;

        private IList<Bookmark> _bookmarks;

        private List<GameObject> _pins = new List<GameObject>();

        private GameObject _pinTemplate;

        public GameObject buttonTemplate;

        public GlobeTerrainModel globe;

        private void OnEnable() {
            if (!_pinTemplate) {
                _pinTemplate = TemplateService.Instance.GetTemplate(GameObjectName.PinTemplate);
            }
            if (_bookmarks == null) {
                _bookmarksWebService.GetBookmarks((bookmarks) => {
                    _bookmarks = bookmarks;
                    QueueTask(ProcessBookmarks);
                });
            }
            ActivatePins(true);
        }

        private void OnDisable() {
            ActivatePins(false);
        }

        private void ProcessBookmarks() {
            for (int i = 0; i < _bookmarks.Count; i++) {
                Bookmark bookmark = _bookmarks[i];
                BoundingBox bbox = BoundingBoxUtils.ParseBoundingBox(bookmark.bbox);
                Vector2 centerCoords = BoundingBoxUtils.MedianLatLon(bbox);

                if (buttonTemplate != null) {
                    GameObject obj = Instantiate(buttonTemplate, transform);
                    obj.transform.localPosition += 24 * i * Vector3.down;

                    XRMenuPlanetNavigationButton menuElem = obj.GetComponent<XRMenuPlanetNavigationButton>();
                    menuElem.latitude = centerCoords.x;
                    menuElem.longitude = centerCoords.y;

                    Text text = obj.GetComponentInChildren<Text>();
                    text.text = bookmark.title;

                    obj.SetActive(true);
                }

                if (_pinTemplate && globe) {

                    GameObject pin = Instantiate(_pinTemplate, globe.transform);

                    pin.transform.forward = globe.transform.forward;

                    pin.transform.Rotate(globe.transform.right, -centerCoords.x, Space.World);
                    pin.transform.Rotate(globe.transform.up, -centerCoords.y, Space.World);

                    pin.transform.position = globe.transform.position + globe.transform.localScale.x * globe.Radius * TerrainModelScale * pin.transform.forward;

                    pin.transform.forward = globe.transform.position - pin.transform.position;

                    pin.transform.localScale = 4 * pin.transform.localScale;

                    Text text = pin.GetComponentInChildren<Text>();
                    text.text = bookmark.title;

                    _pins.Add(pin);
                }

                ActivatePins(true);

            }
        }

        private void ActivatePins(bool active) {
            foreach (GameObject pin in _pins) {
                pin.SetActive(active);
            }
        }

    }

}
