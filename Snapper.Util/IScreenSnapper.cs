using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Snapper.Util
{
    public interface IScreenSnapper
    {
        void SnapScreenAndSave(string imagePath, Screen screen, ImageFormat imageFormat, long qualityPercent);
        void SnapActiveWindowAndSave(string imagePath, ImageFormat imageFormat, int qualityPercent);
    }
}