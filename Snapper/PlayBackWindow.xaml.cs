using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using Snapper.Extensions;
using Snapper.Helpers;
using Snapper.Properties;

using Image = System.Drawing.Image;
using Label = System.Windows.Controls.Label;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Snapper
{
    /// <summary>
    ///     Interaction logic for PlayBackWindow.xaml
    /// </summary>
    public partial class PlayBackWindow : Window
    {
        private readonly DebounceDispatcher _debounceDispatcher = new DebounceDispatcher();
        //private readonly DispatcherTimer _resizeTimer;
        private string[] _images;
        private bool _label;

        private Image _lastImage;
        private DateTime _oldDate;

        private DateTime _selectedDate;

        public PlayBackWindow()
        {
            InitializeComponent();

            //_resizeTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            //_resizeTimer.Tick += ResizeTimerTick;
            //SlotSlider.Visibility = Visibility.Hidden;
        }

        public string SelectedDateString
        {
            get => _selectedDate.ToShortDateString();
            set
            {
                _selectedDate = DateTime.Parse(value);
                Date1.Content = SelectedDateString;
                Date2.Content = SelectedDateString;
                MyCalendar.SelectedDate = DateTime.Parse(SelectedDateString);
                MyCalendar.DisplayDate = DateTime.Parse(SelectedDateString);
                if(_oldDate.Year != _selectedDate.Year || _oldDate.Month != _selectedDate.Month)
                    LoadBlackoutDates();
                _oldDate = _selectedDate;
            }
        }

        public string SelectedDateFilesPath => Path.Combine(Settings.Default.ScreenShotsDirectory, SelectedDateString);

        private void ResizeTimerTick(object sender, EventArgs e)
        {
            //_resizeTimer.Stop();
            // DrawSlots();
            // _resizeTimer.Start();
        }

        private static ImageSource ImageToBitmapImage(Image image)
        {
            BitmapImage bitmapImage;

            using(var ms = new MemoryStream())
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

            if(!MyCalendar.SelectedDate.HasValue)
                return;

            SelectedDateString = MyCalendar.SelectedDate.Value.ToString("yyyy-MM-dd");

            ShowSelectedDate();
        }

        private void ShowSelectedDate()
        {
            LoadImages();
            DrawSlots();

            if (_images == null || !_images.Any())
            {
                Trace.TraceInformation("Nothing snapped this day");
                Snapshot.Source = null;
            }
            else
            {
                Trace.TraceInformation($"Showing first image of the day: {_images.First()}");
                ShowImage(_images.First());
            }
        }

        private void LoadImages()
        {
            _images = Directory.Exists(SelectedDateFilesPath)
                ? Directory.GetFiles(SelectedDateFilesPath)
                : new List<string>().ToArray();
        }

        private List<string> _slots = new List<string>();

        private void DrawSlots()
        {
            ClearSlots();

            if(_images.Length == 0)
                return;

            var imglist = _images.Select(Path.GetFileNameWithoutExtension).OrderBy(s => s).ToList();

            var firstHour = int.Parse(imglist[0].Substring(0, 2));
            var lastHour = int.Parse(imglist[imglist.Count - 1].Substring(0, 2)) + 1;

            var slots = (lastHour - firstHour) * 60 * 60 / 10;
            var slotWidth = ActualWidth / slots;

            //SlotSlider.Maximum = slots;
            //SlotSlider.Minimum = 0;
            //SlotSlider.Value = 0;
            //SlotSlider.TickFrequency = 1;
            //SlotSlider.IsSnapToTickEnabled = true;


            for(var i = 0; i < slots; i++)
            {
                var time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, firstHour, 0, 0);
                time = time.AddSeconds(i * 10);

                var value = time.ToShortTimeString().Substring(0, 5);

                if(value.EndsWith("00") && _label == false)
                {
                    var tb = new Label { Content = value, Margin = new Thickness(0, 15, 0, 0) };
                    Canvas.SetLeft(tb, i * slotWidth);
                    Slots.Children.Add(tb);
                    _label = true;
                }
                else if(!value.EndsWith("00"))
                {
                    _label = false;
                }

                value = value.Replace(":", "-");

                if(!imglist.Any(l => l.StartsWith(value)))
                {
                    _slots.Add(null);
                    continue;
                }

                var file = _images.First(l => l.Contains(value));
                _slots.Add(file);

                var slotRect = new Rectangle
                {
                    Height = 20,
                    Width = slotWidth,
                    StrokeThickness = 2,
                    ToolTip = new ToolTip { Content = time.ToShortTimeString() }
                };

                slotRect.DataContext = file;
                slotRect.MouseEnter += BorderMouseEnter;
                slotRect.MouseLeftButtonDown += BorderMouseClicked;
                slotRect.Stroke = new SolidColorBrush(Colors.RoyalBlue);
                Canvas.SetLeft(slotRect, i * slotWidth);
                Slots.Children.Add(slotRect);
            }
        }

        //private Rectangle _indicatoRectangle = null;


        private void BorderMouseClicked(object sender, MouseButtonEventArgs e)
        {
            ShowImageIfExists(sender, e.LeftButton);
        }

        private void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            ShowImageIfExists(sender, e.LeftButton);
        }

        private void ShowImageIfExists(object sender, MouseButtonState mouseButton)
        {
            if (mouseButton != MouseButtonState.Pressed)
                return;

            var slotRect = sender as Rectangle;

            if(slotRect == null)
            {
                return;
            }

            //if (_indicatoRectangle == null)
            //{
            //    _indicatoRectangle = new Rectangle();
            //    SlotPanel.Children.Add(_indicatoRectangle);
            //}

            //_indicatoRectangle.Height = 20;
            //_indicatoRectangle.Width=5;
            //_indicatoRectangle.Margin = new Thickness(0, -20, 0, 0);
            //_indicatoRectangle.Stroke = new SolidColorBrush(Colors.Red);
            //var slotPos = slotRect.TransformToAncestor(SlotPanel)
            //    .Transform(new Point(0, 0));
            //Canvas.SetLeft(slotRect,slotPos.X);
            //Canvas.SetTop(slotRect,slotPos.Y);
            

            var file = (string)slotRect.DataContext;

            ShowImage(file);
        }

        private void ShowImage(string file)
        {
            if(_lastImage != null)
                _lastImage.Dispose();

            var filename = Path.GetFileNameWithoutExtension(file);
            if(filename != null)
                Time1.Content = filename.Replace("-", ":");
            if(filename != null)
                Time2.Content = filename.Replace("-", ":");

            var image = Image.FromFile(file);
            Snapshot.Source = ImageToBitmapImage(image);

            _lastImage = image;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_resizeTimer.Stop();
            //_resizeTimer.Start();

            _debounceDispatcher.Debounce(500, p => { DrawSlots(); });
        }

        private void ClearSlots()
        {
            _slots.Clear();
            Slots.Children.Clear();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            /* TODO: Mark dates in the calendar that actually has data
            var range = new CalendarDateRange(new DateTime(2011, 1, 1), new DateTime(2011, 6, 26));
            MyCalendar.BlackoutDates.Add(range);
            */

            DirInfo.Content = "snap-folder: " + Settings.Default.ScreenShotsDirectory;

            SelectedDateString = DateTime.Now.ToShortDateString();

            MyCalendar.SelectedDate = DateTime.Parse(SelectedDateString);
            MyCalendar.SelectionMode = CalendarSelectionMode.SingleDate;

            LoadImages();
            DrawSlots();
        }

        private void FolderClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = Settings.Default.ScreenShotsDirectory
            };

            var result = dlg.ShowDialog(this.GetIWin32Window());

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Default.ScreenShotsDirectory = dlg.SelectedPath;
                Settings.Default.Save();
            }

            DirInfo.Content = "snap-folder: " + Settings.Default.ScreenShotsDirectory;

            LoadImages();
            DrawSlots();
        }

        private void MyCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            Trace.TraceInformation("Month view changed to " + MyCalendar.DisplayDate.ToShortDateString());
            if(e.AddedDate.HasValue && e.AddedDate.Value.Month != DateTime.Parse(SelectedDateString).Month)
                LoadBlackoutDates();


        }

        private void LoadBlackoutDates()
        {
            MyCalendar.BlackoutDates.Clear();

            var date = MyCalendar.DisplayDate;
            var from = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
            var to = from.AddMonths(3).AddDays(-1);

            foreach(var day in EachDay(from, to))
            {
                var datePath = Path.Combine(Settings.Default.ScreenShotsDirectory, day.ToShortDateString());

                if(!Directory.Exists(datePath))
                    MyCalendar.BlackoutDates.Add(new CalendarDateRange(day));
                else if(!Directory.GetFiles(Path.Combine(datePath)).Any())
                    MyCalendar.BlackoutDates.Add(new CalendarDateRange(day));
                //else directory and files exists

            }
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for(var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        //private void SlotSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (_slots.Count == 0) return;
        //    Trace.TraceInformation("Slider changed to " + SlotSlider.Value);

        //    if (_slots.Count < (int)SlotSlider.Value) return;

        //    var slotImagePath = _slots[(int)SlotSlider.Value];
        //    Trace.TraceInformation("Slot File: " + slotImagePath);
        //    if(slotImagePath != null)
        //        ShowImage(slotImagePath);

        //    //_debounceDispatcher.Debounce(500, p =>
        //    //{
        //    //});

        //}
        public void ShowDate(string dateString)
        {
            SelectedDateString = dateString;
            ShowSelectedDate();
        }
    }
}