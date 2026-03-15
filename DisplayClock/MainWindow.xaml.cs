using System;
using System.Runtime.InteropServices;
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
    #region Économiseur d'écran Windows (SetThreadExecutionState)

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private static void PreventScreensaver()
    {
      SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED | ES_SYSTEM_REQUIRED);
    }

    private static void AllowScreensaver()
    {
      SetThreadExecutionState(ES_CONTINUOUS);
    }

    #endregion

    private readonly DispatcherTimer _timer;

    // Rayon et centre de l'horloge analogique (canvas fixe 350x350)
    private const double ClockRadius = 175; // moitié de 350
    private static readonly Point Center = new Point(175, 175);

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
      Closed += MainWindow_Closed;

      PreventScreensaver();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      InitAnalogClock();
      UpdateClocks();
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
      AllowScreensaver();
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

      Canvas.SetLeft(TicksCanvas, 0);
      Canvas.SetTop(TicksCanvas, 0);
      TicksCanvas.Width = ClockRadius * 2;
      TicksCanvas.Height = ClockRadius * 2;

      double numberRadius = ClockRadius - 35; // position des chiffres (entre le centre et les traits)

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

        // Chiffre de l'heure : 12 en haut, puis 1 à 11
        int hourNumber = i == 0 ? 12 : i;
        double numX = ClockRadius + numberRadius * Math.Sin(angle);
        double numY = ClockRadius - numberRadius * Math.Cos(angle);

        var text = new TextBlock
        {
          Text = hourNumber.ToString(),
          Foreground = Brushes.White,
          FontSize = 18,
          FontWeight = FontWeights.Bold,
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center,
          Width = 24,
          Height = 24
        };

        TicksCanvas.Children.Add(text);
        Canvas.SetLeft(text, numX - 12);
        Canvas.SetTop(text, numY - 12);
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
      hand.StrokeThickness = thickness;
      hand.X1 = Center.X;
      hand.Y1 = Center.Y;
      hand.X2 = Center.X + length * Math.Sin(angle);
      hand.Y2 = Center.Y - length * Math.Cos(angle);
    }
  }
}
