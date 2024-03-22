using UnityEngine.UI;

namespace MyCodeExample
{
    public static class GraphicExtensions
    {
        public static void SetColorAlpha<T>(this T graphic, float alpha) where T : Graphic
        {
            var currentColor = graphic.color;
            currentColor.a = alpha;
            graphic.color = currentColor;
        }
    }
}