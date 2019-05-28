using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WpfApp1
{
    public class Settings : INotifyPropertyChanged
    {
        public static string GetFfmpegDirectoryFromConfig()
        {
            return ConfigurationSettings.AppSettings["FFMPegBinaries"].ToString();
        }

        private string _FirstSubtitlePath;
        public string FirstSubtitlePath {
            get { return _FirstSubtitlePath; }
            set
            {
                _FirstSubtitlePath = value;
                OnPropertyChanged("FirstSubtitlePath");
            }
        }
        private string _SecondSubtitlePath;
        public string SecondSubtitlePath
        {
            get { return _SecondSubtitlePath; }
            set
            {
                _SecondSubtitlePath = value;
                OnPropertyChanged("SecondSubtitlePath");
            }
        }
        private string _AudioFilePath;
        public string AudioFilePath
        {
            get { return _AudioFilePath; }
            set
            {
                _AudioFilePath = value;
                OnPropertyChanged("AudioFilePath");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public Settings()
        {
            _FirstSubtitlePath = @"C:\Users\Vojta\Documents\rocnikac\subtitles.srt";
            _SecondSubtitlePath = @"C:\Users\Vojta\Documents\rocnikac\subtitles3.srt";
            _AudioFilePath = @"C:\Users\Vojta\Documents\rocnikac\7_1.mp4";
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Delta time range, where subtitles are considered similar enough.
        public static int MilisecondSpreadSimilarness = 50;

        // Treshold ercentage of subtitle pairs, where they are considered different.
        public static double SimilarnessPercentageTreshold = 0.5;

        public static string TimeSpanStringFormat = "h\\:mm\\:ss\\,fff";
    }


}
