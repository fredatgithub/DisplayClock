using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DisplayClock
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly DispatcherTimer _timer;

    // Rayon et centre de l'horloge analogique
    private const double ClockRadius = 175; // moitié de 350
    private Point _center;

    public MainWindow()
    {
      InitializeComponent();

      _timer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(1)
      };
      _timer.Tick += Timer_Tick;
      _timer.Start();

      Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _center = new Point(AnalogCanvas.ActualWidth / 2, AnalogCanvas.ActualHeight / 2);

      if (_center.X == 0 || _center.Y == 0)
      {
        AnalogCanvas.SizeChanged += AnalogCanvas_SizeChanged;
      }
      else
      {
        InitAnalogClock();
      }

      UpdateClocks();
    }

    private void AnalogCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      _center = new Point(AnalogCanvas.ActualWidth / 2, AnalogCanvas.ActualHeight / 2);
      InitAnalogClock();
      AnalogCanvas.SizeChanged -= AnalogCanvas_SizeChanged;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      UpdateClocks();
    }

    private void UpdateClocks()
    {
      DateTime now = DateTime.Now;

      // Digital
      DigitalClockText.Text = now.ToString("HH:mm:ss");

      // Analogique
      UpdateAnalogHands(now);
    }

    private void InitAnalogClock()
    {
      TicksCanvas.Children.Clear();

      Canvas.SetLeft(TicksCanvas, _center.X - ClockRadius);
      Canvas.SetTop(TicksCanvas, _center.Y - ClockRadius);
      TicksCanvas.Width = ClockRadius * 2;
      TicksCanvas.Height = ClockRadius * 2;

      for (int i = 0; i < 12; i++)
      {
        double angle = i * 30 * Math.PI / 180;
        double innerRadius = ClockRadius - 15;
        double outerRadius = ClockRadius - 5;

        double x1 = ClockRadius + innerRadius * Math.Sin(angle);
        double y1 = ClockRadius - innerRadius * Math.Cos(angle);

        double x2 = ClockRadius + outerRadius * Math.Sin(angle);
        double y2 = ClockRadius - outerRadius * Math.Cos(angle);

        var tick = new Line
        {
          X1 = x1,
          Y1 = y1,
          X2 = x2,
          Y2 = y2,
          Stroke = Brushes.White,
          StrokeThickness = 3
        };

        TicksCanvas.Children.Add(tick);
      }
    }

    private void UpdateAnalogHands(DateTime now)
    {
      double secondAngle = (now.Second / 60.0) * 360 * Math.PI / 180;
      double minuteAngle = ((now.Minute + now.Second / 60.0) / 60.0) * 360 * Math.PI / 180;
      double hourAngle = ((now.Hour % 12 + now.Minute / 60.0) / 12.0) * 360 * Math.PI / 180;

      double secondLength = ClockRadius - 20;
      double minuteLength = ClockRadius - 30;
      double hourLength = ClockRadius - 60;

      SetHand(HourHand, hourAngle, hourLength, 8);
      SetHand(MinuteHand, minuteAngle, minuteLength, 6);
      SetHand(SecondHand, secondAngle, secondLength, 2);
    }

    private void SetHand(Line hand, double angle, double length, double thickness)
    {
      if (_center.X == 0 || _center.Y == 0)
        return;

      hand.StrokeThickness = thickness;
      hand.X1 = _center.X;
      hand.Y1 = _center.Y;
      hand.X2 = _center.X + length * Math.Sin(angle);
      hand.Y2 = _center.Y - length * Math.Cos(angle);
    }
  }
}
