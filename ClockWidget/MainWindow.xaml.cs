﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer dt = new();
            dt.Tick += new EventHandler(TimerTick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dt.Start();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            HandleHandles();
        }

        private void HandleHandles()
        {
            DateTime now = DateTime.Now;
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
    }
}