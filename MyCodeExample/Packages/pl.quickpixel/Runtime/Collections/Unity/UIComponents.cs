using UnityEngine;
using UnityEngine.UI;

namespace QuickPixel.Collections.Unity
{
    public readonly struct UIComponents
    {
        public readonly Canvas Canvas;
        public readonly CanvasGroup CanvasGroup;
        public readonly GraphicRaycaster GraphicRaycaster;

        public UIComponents(Canvas canvas, CanvasGroup canvasGroup, GraphicRaycaster graphicRaycaster)
        {
            Canvas = canvas;
            CanvasGroup = canvasGroup;
            GraphicRaycaster = graphicRaycaster;
        }

        public UIComponents(MonoBehaviour monoBehaviour)
        {
            Canvas = monoBehaviour.GetComponent<Canvas>();
            CanvasGroup = monoBehaviour.GetComponent<CanvasGroup>();
            GraphicRaycaster = monoBehaviour.GetComponent<GraphicRaycaster>();
        }
    }
}