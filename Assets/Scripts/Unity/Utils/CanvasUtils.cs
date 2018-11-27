using UnityEngine;
using UnityEngine.UI;

namespace TrekVRApplication {

    public class CanvasUtils {

        public static GameObject CreateEmpty(GameObject gameObject = null) {
            if (!gameObject) {
                gameObject = new GameObject();
                gameObject.name = "Canvas";
            }

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 2;

            GraphicRaycaster graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            return gameObject;
        }

    }

}