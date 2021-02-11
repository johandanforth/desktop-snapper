using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Snapper.Util
{
    public class ScreenSnapper : IScreenSnapper
    {
        private string _imagePath;
        private Rectangle _bounds;
        private ImageFormat _imageFormat;
        private long _quality;

        public void SnapScreenAndSave(string imagePath, Screen screen, ImageFormat imageFormat, long qualityPercent)
        {
            var bounds = screen.Bounds;
            _bounds = bounds;
            _imagePath = imagePath;
            _imageFormat = imageFormat;
            _quality = qualityPercent;

            var snapperThread = new Thread(SnapAndSaveByBounds) { Priority = ThreadPriority.Lowest };
            snapperThread.Start();
        }

        public void SnapAllScreensAndSave(string imagePath, ImageFormat imageFormat, int qualityPercent)
        {
            var bounds = new Rectangle(SystemInformation.VirtualScreen.X, SystemInformation.VirtualScreen.Y, 
                SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            _bounds = bounds;
            _imagePath = imagePath;
            _imageFormat = imageFormat;
            _quality = qualityPercent;

            var snapperThread = new Thread(SnapAndSaveByBounds) { Priority = ThreadPriority.Lowest };
            snapperThread.Start();
        }

        public void SnapActiveWindowAndSave(string imagePath, ImageFormat imageFormat, int qualityPercent)
        {
            var activeWindowInfo = new WindowFuncs().GetActiveWindow();
            var bounds = activeWindowInfo.Bounds;
            _bounds = bounds;
            _imagePath = imagePath;
            _imageFormat = imageFormat;
            _quality = qualityPercent;

            Debug.Print("Snapped - " + activeWindowInfo.ActiveProgramTitle );

            var snapperThread = new Thread(SnapAndSaveByBounds) { Priority = ThreadPriority.Lowest };
            snapperThread.Start();
        }

        private void SnapAndSaveByBounds()
        {
            using (var image = new Bitmap(_bounds.Width, _bounds.Height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    try
                    {
                        graphics.CopyFromScreen(_bounds.X, _bounds.Y, 0, 0, _bounds.Size, CopyPixelOperation.SourceCopy);
                        SaveJpeg(image);
                    }
                    catch (Exception snapException)
                    {
                        //TODO: Handle exceptions?

                        /*
                        if (!EventLog.SourceExists("Snapper"))
                        {
                            EventLog.CreateEventSource("Snapper", "Application");
                            EventLog.WriteEntry("Snapper","Created event source",EventLogEntryType.Information);
                        }
                        EventLog.WriteEntry("Snapper","Fel vid SnapAndSaveByBounds():" + snapException.Message + "\n\n" + snapException.StackTrace,
                            EventLogEntryType.Error);
                         */
                    }
                }
            }
        }

        private void SaveJpeg(Image image)
        {
            using (var encoderParams = new EncoderParameters(1))
            {
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, _quality);
                if (!Directory.Exists(_imagePath))
                    Directory.CreateDirectory(_imagePath);

                try
                {
                    var jpegCodec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                    var filename = _imagePath + "/" + DateTime.Now.ToString("HH-mm-ss") + "." + _imageFormat;
                    image.Save(filename, jpegCodec, encoderParams);
                }
                catch (Exception saveException)
                {

                    //MessageBox.Show("Fel vid SaveJpeg()" + saveException.Message);
                    /*
                    var eventLog = new EventLog { Source = "Snapper" };
                    eventLog.WriteEntry("Fel vid SaveJpeg():" + saveException.Message + "\n\n" + saveException.StackTrace,
                        EventLogEntryType.Error);
                     */
                }
            }
        }

    }
}