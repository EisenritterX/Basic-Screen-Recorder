using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Basic_Screen_Recorder
{
    public static class NativeMethods
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
    }

    class AudioRecorder
    {

        // Audio Caputerer & Writer
        private WaveFileWriter MicAudioWriter = null;
        private WaveFileWriter SystemAudioWriter = null;
        // Redefine the capturer instance with a new instance of the LoopbackCapture class
        private WasapiLoopbackCapture CaptureInstance = new WasapiLoopbackCapture();

        #region Variables
        // Audio File
        private static string audioName = "mic.wav";
        private string sysAudioName = "sysAudio.wav";
        #endregion

        #region Methods
        
        public void RecordMicAudio()
        {
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsound", "", 0, 0);
        }

        public void RecordSystemAudio(string path)
        {
            string sysAudioPath = path + "//" + sysAudioName;

            SystemAudioWriter = new WaveFileWriter(sysAudioPath, CaptureInstance.WaveFormat);

            // When the capturer receives audio, start writing the buffer into the mentioned file
            CaptureInstance.DataAvailable += (s, a) =>
            {
                // Write buffer into the file of the writer instance
                SystemAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);
            };

            // When the Capturer Stops, dispose instances of the capturer and writer
            CaptureInstance.RecordingStopped += (s, a) =>
            {
                SystemAudioWriter.Dispose();
                SystemAudioWriter = null;
                CaptureInstance.Dispose();
            };

            // Start audio recording !
            CaptureInstance.StartRecording();
        }

        public static void SaveAudio(string path)
        {
            string audioPath = "save micaudio " + path + "//" + audioName;
            NativeMethods.record(audioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }

        private void SaveSystemAudio(string path)
        {
            CaptureInstance.StopRecording();
        }
        #endregion
    }
}
