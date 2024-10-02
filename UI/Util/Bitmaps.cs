using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CTecUtil.UI.Util
{
    public class Bitmaps
    {
        /// <summary>
        /// Get a BitmapImage from the CTecUtil resources in the "UI/Images" folder.<br/>
        /// NB: the image is presumed to be a .png if a .jpg, .gif or .bmp suffix is not found.
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        internal static BitmapImage GetBitmap(string imageName) => GetBitmap("CtecUtil", "UI/Images", imageName);

        
        /// <summary>
        /// Get a BitmapImage from the specified assembly's <param>imagesRoot</param> folder.<br/>
        /// NB: the image is presumed to be a .png if a .jpg, .gif or .bmp suffix is not found.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to which the image resource belongs</param>
        /// <param name="imagesRoot">The image's folder</param>
        /// <param name="imageName">The image name (.png is assumed if a .jpg, .gif or .bmp suffix is not found)</param>
        /// <returns></returns>
        public static BitmapImage GetBitmap(string assemblyName, string imagesRoot, string imageName)
        {
            try
            {
                if (imageName != null)
                {
                    if (!imageName.EndsWith(".jpg")
                     || !imageName.EndsWith(".gif")
                     || !imageName.EndsWith(".bmp"))
                        imageName = imageName + ".png";
                    return imageName != null ? new BitmapImage(new Uri("pack://application:,,,/" + assemblyName + ";" + TextProcessing.CombineWithForwardSlashes("component", imagesRoot, imageName))) : null;
                }
            }
            catch { }
            return null;
        }
    }
}
