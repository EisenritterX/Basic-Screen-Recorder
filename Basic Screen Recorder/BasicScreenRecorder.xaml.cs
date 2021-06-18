using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Basic_Screen_Recorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool folderSelected = false;
        string outputPath = "";
        string finalVideoName = "FinalVideo.mp4";

        // Timer
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();


        Stopwatch watch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            VideoRecorder.TakeScreenshot(outputPath, CaptureMode.Window);

        }
        
        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {

            outputPath = PathSelector.PathSelection();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0,FPSSelection());
            dispatcherTimer.Start();

        }

        private int FPSSelection()
        {
            if(Rad30FPS.IsChecked == true)
            {
                return 33;
            }
            if (Rad60FPS.IsChecked == true)
            {
                return 17;
            }
            if (Rad120FPS.IsChecked == true)
            {
                return 8;
            }
            return 100;
        }
    }
}
