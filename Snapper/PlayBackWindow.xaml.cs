using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Snapper.Extensions;
using Snapper.Properties;
using Image = System.Drawing.Image;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Snapper
{
    /// <summary>
    /// Interaction logic for PlayBackWindow.xaml
    /// </summary>
    public partial class PlayBackWindow : Window
    {
        private string[] _images;
        private readonly DispatcherTimer _resizeTimer;
        private string _selectedDate;

        public PlayBackWindow()
        {
            InitializeComponent();
            _resizeTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 0, 100)};
            _resizeTimer.Tick += ResizeTimerTick;
        }

        void ResizeTimerTick(object sender, EventArgs e)
        {
            _resizeTimer.Stop();

            if (_selectedDate != null)
                DrawSlots(_selectedDate);
        }

        private Image _lastImage;
        private bool _label;

        private static ImageSource ImageToBitmapImage(Image image)
        {
            BitmapImage bitmapImage;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void MyCalendarSelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            //fix for calendar hijacking the mouse :(
            CaptureMouse();
            ReleaseMouseCapture();

            if (!MyCalendar.SelectedDate.HasValue) return;

            _selectedDate = MyCalendar.SelectedDate.Value.ToString("yyyy-MM-dd");

            DrawSlots(_selectedDate);

            if (_images != null && _images.Count() > 0)
                ShowImage(_images[0]);

        }

        private void DrawSlots(string selectedDate)
        {

            ClearSlots();

            Debug.Print("Selected date: " + selectedDate);

            var savePath = Settings.Default.ScreenShotsDirectory + "/";

            if (Directory.Exists(savePath + selectedDate))
            {
                _images = Directory.GetFiles(savePath + selectedDate);
                Debug.Print("images: " + _images.Length);

                var imglist = _images.Select(Path.GetFileNameWithoutExtension).ToList();

                imglist.Sort();
                var firstHour = Int32.Parse(imglist[0].Substring(0, 2));
                var lastHour = Int32.Parse(imglist[imglist.Count - 1].Substring(0, 2)) + 1;

                var slots = (lastHour - firstHour) * 60 * 60 / 10;
                var slotWidth = ActualWidth / slots;

                for (int i = 0; i < slots; i++)
                {

                    var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, firstHour, 0, 0);
                    time = time.AddSeconds(i * 10);


                    var value = time.ToShortTimeString().Substring(0, 5);
                    //value = value.Replace(":", "-");
                    if (value.EndsWith("00") && _label == false)
                    {
                        var tb = new Label { Content = value, Margin = new Thickness(0, 15, 0, 0) };
                        Canvas.SetLeft(tb, i * slotWidth);
                        Slots.Children.Add(tb);
                        _label = true;
                    }
                    else if (!value.EndsWith("00"))
                    {
                        _label = false;
                    }

                    value = value.Replace(":", "-");
                    if (imglist.Any(l => l.StartsWith(value)))
                    {
                        var slotRect = new Rectangle
                                           {
                                               Height = 20,
                                               Width = slotWidth,
                                               StrokeThickness = 2,
                                               ToolTip = new ToolTip {Content = time.ToShortTimeString()}
                                           };


                        var file = _images.Where(l => l.Contains(value)).First();
                        slotRect.DataContext = file;
                        slotRect.MouseEnter += BorderMouseEnter;
                        slotRect.Stroke = new SolidColorBrush(Colors.RoyalBlue);
                        Canvas.SetLeft(slotRect, i * slotWidth);
                        Slots.Children.Add(slotRect);
                    }
                }

            }

            
            Debug.Print("Done drawing slots");

        }

        void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var slotRect = sender as Rectangle;
            if (slotRect == null) return;

            var file = (string)slotRect.DataContext;

            ShowImage(file);
        }

        private void ShowImage(string file)
        {
            if (_lastImage != null)
                _lastImage.Dispose();

            Time1.Margin = new Thickness((SnapshotGrid.ActualWidth / 2) - (Time1.ActualWidth / 2),5,5,5);
            Time2.Margin = new Thickness((SnapshotGrid.ActualWidth / 2) - (Time1.ActualWidth / 2) + 1, 6, 5, 5);
            
            var filename = Path.GetFileNameWithoutExtension(file);
            if (filename != null) Time1.Content = filename.Replace("-", ":");
            if (filename != null) Time2.Content = filename.Replace("-", ":");

            var image = Image.FromFile(file);
            Snapshot.Source = ImageToBitmapImage(image);

            _lastImage = image;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void ClearSlots()
        {
            Slots.Children.Clear();
        }

        public void ShowDate(string dateString)
        {
            MyCalendar.SelectedDate = DateTime.Parse(dateString);
            _selectedDate = dateString;

            DrawSlots(dateString);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            /* test för att markera datum som har data i sig
            var range = new CalendarDateRange(new DateTime(2011, 1, 1), new DateTime(2011, 6, 26));
            MyCalendar.BlackoutDates.Add(range);
            */

            DirInfo.Content = "Läser från " + Settings.Default.ScreenShotsDirectory;
            ShowDate(DateTime.Now.ToShortDateString());

        }

        private void FolderClick(object sender, MouseButtonEventArgs e)
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = Settings.Default.ScreenShotsDirectory
            };

            var result = dlg.ShowDialog(this.GetIWin32Window());
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.ScreenShotsDirectory = dlg.SelectedPath;
                Settings.Default.Save();
            }
            DirInfo.Content = "Läser från " + Settings.Default.ScreenShotsDirectory;
            DrawSlots(_selectedDate);
        }
    }
}
