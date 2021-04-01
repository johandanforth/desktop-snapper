using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Snapper.Util;

namespace Snapper.Tests
{
    [TestClass]
    public class ScreenSnapperTests
    {
        [TestMethod]
        public void SnapMainScreen()
        {
            var ss = new ScreenSnapper();
            ss.SnapAllScreensAndSave(Path.GetTempPath(), ImageFormat.Jpeg, 50);
            Thread.Sleep(1000);
        }

        [TestMethod]
        public void TakeFullScreenShot()
        {
            var filename = Path.GetTempFileName() + Guid.NewGuid() + ".jpeg";

            var bounds = Screen.PrimaryScreen.Bounds;

            using (var image = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                    SaveJpeg(image, filename, 50);
                }
            }

            //test
            Assert.IsTrue(File.Exists(filename));
            var testImage = Image.FromFile(filename);
            Assert.IsTrue(testImage.RawFormat.Guid == ImageFormat.Jpeg.Guid,
                "Wrong format, was " + testImage.RawFormat);
        }

        private static void SaveJpeg(Image image, string filename, int quality)
        {
            using (var encoderParams = new EncoderParameters(1))
            {
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));

                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                var jpegCodec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                image.Save(filename, jpegCodec, encoderParams);
            }
        }
    }
}