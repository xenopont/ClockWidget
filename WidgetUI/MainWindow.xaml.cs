using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WidgetUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const double DegreeInMillisecondSh = 360.0 / 60000.0;
        private const double DegreeInSecondSh = 360.0 / 60.0;
        private const double DegreeInSecondMh = 360.0 / 3600.0;
        private const double DegreesInMinuteMh = 360.0 / 60.0;
        private const double DegreesInSecondHh = 360.0 / 43200.0;
        private const double DegreesInMinuteHh = 360.0 / 720.0;
        private const double DegreesInHourHh = 360.0 / 12.0;

        private readonly ApplicationSettings _settings = new();
        private readonly string _applicationSettingsFilename = ApplicationSettings.FileName();

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer dt = new();
            dt.Tick += TimerTick;
            dt.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dt.Start();

            // settings
            _settings.Load(_applicationSettingsFilename);
            ApplySettings();
            AutostartMenuItem.IsChecked = ApplicationSettings.IsAutostart();

            // weather
            Weather.AddListener(WeatherListener);
            Weather.Start();
        }

        private void WeatherListener(Weather.WeatherData data)
        {
            // to get rid of double/float "-0"
            var intTemperature = (int) Math.Round(data.Temperature, 0);
            WeatherTextBox.Text = $"{data.City}, {data.CountryCode} {intTemperature}°C";
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            
            DragMove();
            _settings.Values.Top = Top;
            _settings.Values.Left = Left;
            _settings.Save(_applicationSettingsFilename);
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            HandleHandles(now);
            HandleChevron(now);
        }

        private readonly Duration _chevronColorAnimationDuration = new(TimeSpan.FromMilliseconds(2550));
        private const long ChevronColorTtl = TimeSpan.TicksPerSecond * 472;
        private long _lastChevronTick;
        private void HandleChevron(in DateTime now)
        {
            if (now.Ticks - _lastChevronTick < ChevronColorTtl)
            {
                return;
            }

            if (Application.Current.Resources["ChevronBrush"] is not SolidColorBrush currentBrush)
            {
                return;
            }

            SolidColorBrush newBrush = new(currentBrush.Color);
            ColorAnimation animation = new(ColorEngine.NextColor, _chevronColorAnimationDuration);
            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            Application.Current.Resources["ChevronBrush"] = newBrush;

            _lastChevronTick = now.Ticks;
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
            SecondHand.RenderTransform = new RotateTransform(DegreeInSecondSh * sec /*+ DegreeInMillisecondSh * ms*/);
        }

        private void HandleMinuteHand(int min, int sec)
        {
            MinuteHand.RenderTransform = new RotateTransform(DegreesInMinuteMh * min + DegreeInSecondMh * sec);
        }

        private void HandleHourHand(int hs, int min, int sec)
        {
            HourHand.RenderTransform = new RotateTransform(DegreesInHourHh * hs + DegreesInMinuteHh * min + DegreesInSecondHh * sec);
        }

        private void HandleText(int hs, int min, int sec)
        {
            TimeText.Text = $"{hs:D2}:{min:D2}:{sec:D2}";
        }

        private void AlwaysOnTopClick(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            AlwaysOnTopMenuItem.IsChecked = _settings.Values.AlwaysOnTop = Topmost;
            _settings.Save(_applicationSettingsFilename);
        }

        private void ApplySettings()
        {
            Topmost = AlwaysOnTopMenuItem.IsChecked = _settings.Values.AlwaysOnTop;
            Top = _settings.Values.Top;
            Left = _settings.Values.Left;
        }

        private void AutostartMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (ApplicationSettings.IsAutostart())
            {
                AutostartMenuItem.IsChecked = !ApplicationSettings.DeleteAutostart();
            }
            else
            {
                AutostartMenuItem.IsChecked = ApplicationSettings.SetAutostart();
            }
        }
    }
}
