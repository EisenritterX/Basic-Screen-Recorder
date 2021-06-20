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
        int resolutionWidth = 1024, resolutionHeight = 768; //Default video resolution @1024x768


        // Timer
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();


        Stopwatch watch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            StopRecording.IsEnabled = false;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            VideoRecorder.TakeScreenshot(outputPath, CaptureMode.Screen,1024,768);

        }
        
        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {

            outputPath = PathSelector.PathSelection();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0,100); // Default interval is 100 milliseconds
            dispatcherTimer.Start();
            StopRecording.IsEnabled = true;

        }

        private int FPSSelection()
        {
            if(Rad30FPS.IsChecked == true)
            {
                return 30;
            }
            if (Rad60FPS.IsChecked == true)
            {
                return 60;
            }
            if (Rad120FPS.IsChecked == true)
            {
                return 120;
            }
            return 100;
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            VideoRecorder.SaveVideo(resolutionWidth, resolutionHeight, FPSSelection(), outputPath);
            AudioRecorder.SaveAudio(outputPath);

        }
    }
}
