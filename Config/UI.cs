using System.Windows.Media;

namespace CTecUtil.Config
{
    public class UI
    {
        public static float MinZoom = 0.45f;
        public static float MaxZoom = 1.25f;
        public static float ZoomStep => (MaxZoom - MinZoom) / 16;


        public static ScaleTransform LayoutTransform { get; set; }
    }
}
