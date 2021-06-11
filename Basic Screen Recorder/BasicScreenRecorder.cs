using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

using Accord.Video.FFMPEG;
using System.Drawing.Imaging;
using NAudio.Wave;


namespace Basic_Screen_Recorder
{
    class BasicScreenRecorder
    {
        // Video Variables
        private Rectangle bounds;
        // Output Path
        private string outputPath = "";
        // Temporary Path
        private string tempPath = "";
        // Screenshot identifier
        private int fileCount = 1;
        private List<string> inputImageSequence = new List<string>();

        // File Variables
        // Audio File
        private string audioName = "mic.wav";
        private string sysAudioName = "sysAudio.wav";
        // Video File
        private string videoName = "video.mp4";
        private string finalName = "FinalVideo.mp4";

        // Time Variable:
        Stopwatch watch = new Stopwatch();

        // Audio Capturer  Writer
        private WaveFileWriter RecordedAudioWriter = null;
        // Redefine the capturer instance with a new instance of the LoopbackCapture class
        private WasapiLoopbackCapture CaptureInstance = null;

        //Audiio variable:
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

        /// <summary>
        /// Bounds is size of screen
        /// outPath is where the output folder is
        /// </summary>
        /// <param name="b"></param>
        /// <param name="outPath"></param>
        public BasicScreenRecorder(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenShots");

            bounds = b;
            outputPath = outPath;
        }

        /// <summary>
        /// Create Temporary Folder
        /// </summary>
        /// <param name="name"></param>
        private void CreateTempFolder(string name)
        {
            if (Directory.Exists("D://"))
            {
                string pathName = $"D://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
            else
            {
                string pathName = $"C://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
        }


        /// <summary>
        /// Delete temporate file
        /// </summary>
        /// <param name="targetDir"></param>
        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);         
            }

            foreach(string dir in dirs)
            {
                DeletePath(dir);
            }
            Directory.Delete(targetDir, false);
        }

        /// <summary>
        /// Delete all file except exception fiile
        /// </summary>
        /// <param name="targetFile"></param>
        /// <param name="excFile"></param>
        private void DeleteFilesExcept(string targetFile, string excFile)
        {
            string[] files = Directory.GetFiles(targetFile);

            foreach(string file in files)
            {
                if (file != excFile)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        public void CleanUp()
        {
            if (Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }

        /// <summary>
        /// Get Elapsed Time
        /// </summary>
        /// <returns></returns>
        public string GetElapsed()
        {
            return string.Format("(0:D2):(1:D2}:{2:D2)", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
        }

        public void RecordVideo()
        {
            watch.Start();

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size); //Creates Screenshot
                }
                string name = tempPath + "//screenshot-" + fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;

                bitmap.Dispose();
            }
        }

        public void RecordAudio()
        {
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsounds", "", 0, 0);
        }

        public void RecordSystemAudio()
        {
            string sysAudioPath = outputPath + "//" + sysAudioName;

            this.CaptureInstance = new WasapiLoopbackCapture();
            this.RecordedAudioWriter = new WaveFileWriter(sysAudioPath, CaptureInstance.WaveFormat);

            // When the capturer receives audio, start writing the buffer into the mentioned file
            CaptureInstance.DataAvailable += (s, a) =>
            {
                // Write buffer into the file of the writer instance
                RecordedAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);
            };

            // When the Capturer Stops, dispose instances of the capturer and writer
            CaptureInstance.RecordingStopped += (s, a) =>
            {
                RecordedAudioWriter.Dispose();
                RecordedAudioWriter = null;
                CaptureInstance.Dispose();
            };

            // Start audio recording !
            CaptureInstance.StartRecording();
        }

        private void SaveVideo(int width, int height, int frameRate)
        {
            using(VideoFileWriter vfWriter = new VideoFileWriter())
            {
                vfWriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4);

                foreach(string imageLoc in inputImageSequence)
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLoc) as Bitmap;
                    vfWriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                }
                vfWriter.Close();
            }
        }

        private void SaveAudio()
        {
            string audioPath = "save recsound" + outputPath + "//" + audioName;
            NativeMethods.record(audioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }

        private void CombineVideoAndAudio(string video, string audio)
        {
            string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = command
            };

            using(Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        public void Stop()
        {
            watch.Stop();

            int width = bounds.Width;
            int height = bounds.Height;
            int frameRate = 10;

            SaveAudio();
            SaveVideo(width, height, frameRate);

            CombineVideoAndAudio(videoName, audioName);

            DeletePath(tempPath);

            DeleteFilesExcept(outputPath, outputPath + "//" + finalName);
        }
    }
}
