using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Basic_Screen_Recorder
{
    public enum CaptureMode
    {
        Screen, Window
    }
    class VideoRecorder
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow(); // External method for foreground window

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow(); // External method for desktop window

        // Struct of Capture Rectangle
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        private static List<string> inputImageSequence = new List<string>();   // List of screenshots taken
        private static int fileCount = 1;  // Counter for screenshots

        private static string videoName = "video.mp4";

        public static Image Capture(CaptureMode mode, int resX, int resY)
        {
            return mode.Equals(CaptureMode.Screen)? CaptureWindow(GetDesktopWindow(),resX,resY):CaptureWindow(GetForegroundWindow(), resX, resY);
        }

        public static Bitmap CaptureWindow(IntPtr handle, int resX, int resY)
        {
            var rect = new Rect();
            GetWindowRect(handle, ref rect);
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);
            //result.SetResolution(1024, 768);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                graphics.DrawImage(result, 0, 0, resX, resY);
            }
            return result;
        }

        public static void TakeScreenshot(string temppath, CaptureMode mode, int resX, int resY)
        {
                //watch.Start();
            
                string name = temppath + "//screenshot-" + fileCount + ".png";
                Capture(mode,resX, resY).Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;

                Capture(mode, resX, resY).Dispose(); // Dispose the image to prevent memory leak
          
        }


        public static void SaveVideo(int width, int height, int frameRate, string outputPath)
        {
            using (VideoFileWriter vfWriter = new VideoFileWriter())
            {
                vfWriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4);

                foreach (string imageLoc in inputImageSequence)
                {
                    Bitmap imageFrame = Image.FromFile(imageLoc) as Bitmap;
                    vfWriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }
                vfWriter.Close();
            }
        }
    }
}
