using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ClockWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double DegreeInMillisecondSH = 360.0 / 60000.0;
        private const double DegreeInSecondSH = 360.0 / 60.0;
        private const double DegreeInSecondMH = 360.0 / 3600.0;
        private const double DegreesInMinuteMH = 360.0 / 60.0;
        private const double DegreesInSecondHH = 360.0 / 43200.0;
        private const double DegreesInMinuteHH = 360.0 / 720.0;
        private const double DegreesInHourHH = 360.0 / 12.0;

        private readonly ColorEngine ColorEngine = new();
        private readonly ApplicationSettings Settings = new();
        private readonly string ApplicationSettingsFilename = ApplicationSettings.FileName();

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer dt = new();
            dt.Tick += new EventHandler(TimerTick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dt.Start();

            // settings
            Settings.Load(ApplicationSettingsFilename);
            ApplySettings();
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            HandleHandles(now);
            HandleChevron(now);
        }

        private readonly Duration chevronColorAnimationDuration = new(TimeSpan.FromMilliseconds(2550));
        private const long chevronColorTTL = TimeSpan.TicksPerSecond * 472;
        private long lastChevronTick = 0;
        private void HandleChevron(in DateTime now)
        {
            if (now.Ticks - lastChevronTick < chevronColorTTL)
            {
                return;
            }

            if (Application.Current.Resources["ChevronBrush"] is not SolidColorBrush currentBrush)
            {
                return;
            }

            SolidColorBrush newBrush = new(currentBrush.Color);
            ColorAnimation animation = new(ColorEngine.NextColor, chevronColorAnimationDuration);
            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            Application.Current.Resources["ChevronBrush"] = newBrush;

            lastChevronTick = now.Ticks;
        }

        private void HandleHandles(in DateTime now)
        {
            HandleSecondHand(now.Second, now.Millisecond);
            HandleMinuteHand(now.Minute, now.Second);
            HandleHourHand(now.Hour, now.Minute, now.Second);
            HandleText(now.Hour, now.Minute, now.Second);
        }

        private void HandleSecondHand(int sec, int ms)
        {
            SecondHand.RenderTransform = new RotateTransform(DegreeInSecondSH * sec + DegreeInMillisecondSH * ms);
        }

        private void HandleMinuteHand(int min, int sec)
        {
            MinuteHand.RenderTransform = new RotateTransform(DegreesInMinuteMH * min + DegreeInSecondMH * sec);
        }

        private void HandleHourHand(int hs, int min, int sec)
        {
            HourHand.RenderTransform = new RotateTransform(DegreesInHourHH * hs + DegreesInMinuteHH * min + DegreesInSecondHH * sec);
        }

        private void HandleText(int hs, int min, int sec)
        {
            TimeText.Text = $"{hs:D2}:{min:D2}:{sec:D2}";
        }

        private void AlwaysOnTopClick(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            AlwaysOnTopMenuItem.IsChecked = Settings.Values.AlwaysOnTop = Topmost;
            Settings.Save(ApplicationSettingsFilename);
        }

        private void ApplySettings()
        {
            Topmost = AlwaysOnTopMenuItem.IsChecked = Settings.Values.AlwaysOnTop;
        }
    }
}
