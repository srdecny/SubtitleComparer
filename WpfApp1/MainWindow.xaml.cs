using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.SubtitlePair;
using System.Collections.ObjectModel;
using SubtitlesParser;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using WpfApp1.Extensions;
using System.Windows.Threading;
using Unosquare.FFME.Shared;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Settings AppSettings { get; set; } = new Settings();
        Subtitles FirstSub;
        Subtitles SecondSub;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Unosquare.FFME.MediaElement.FFmpegDirectory = @"C:\Users\srdecny\Documents\ffmpeg";
            AppSettings.FirstSubtitlePath = @"C:\Users\srdecny\Documents\subtitles.srt";

            InitializeComponent();
            VideoElement.LoadedBehavior = MediaState.Manual;
            VideoElement.Source = new Uri(AppSettings.AudioFilePath);
            LoadSubtitlesAndAudio();
            VideoDurationTextBox.DataContext = VideoElement;
            this.DataContext = AppSettings;
        }

        private void LoadSubtitlesAndAudio()
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            FirstSub = new Subtitles(parser.ParseStream(new FileStream(AppSettings.FirstSubtitlePath, FileMode.Open)));
            SecondSub = new Subtitles(parser.ParseStream(new FileStream(AppSettings.SecondSubtitlePath, FileMode.Open)));

            var result = SubtitlePairHelper.GenerateSubtitlePairs(FirstSub, SecondSub);
            var foo = new List<SubtitlePairViewModel>();
            foreach (var item in result)
            {
                foo.Add(new SubtitlePairViewModel(item));
            }
                
            SubtitlePairBox.ItemsSource = foo;
            var view = CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource);
            view.Filter = ToggleDiffFilter;

        }

        private bool ToggleDiffFilter(object item)
        {
            SubtitlePairViewModel model = item as SubtitlePairViewModel;
            if (ToggleDiffCheckBox.IsChecked.Value)
            {
                    return model.Diff == "DIFFERENT";
            }
            else
            {
                return true;
            }
        }

        private void ToggleDiffCheckBox_Checked(object sender, RoutedEventArgs e)
        {
                CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource).Refresh();
        }

        private void ToggleDiffCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource).Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                VideoElement.Source = new Uri(dialog.FileName);
            }
        }

        private async void SubtitlePairBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is SubtitlePairViewModel item)
            {
                await VideoElement.Pause();
                // item.Start is in miliseconds, 10000 ms = 1 tick.
                await VideoElement.Seek(new TimeSpan((long)item.FirstStart * 10000));
                await VideoElement.Play();
            }
                
        }

        private void VideoElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine("...");
        }

        // Update the selected items and center the screen.
        private void VideoElement_PositionChanged(object sender, Unosquare.FFME.Events.PositionChangedRoutedEventArgs e)
        {
            var items = SubtitlePairBox.ItemsSource as List<SubtitlePairViewModel>;
            SubtitlePairBox.SelectedItems.Clear();

            // Microbenchmarked a manual for loop, but there's no performance difference.
            
            var firstItem = items.Where(x => new TimeSpan((long)x.FirstStart * 10000) > VideoElement.Position).First();
            var secondItem = items.Where(x => new TimeSpan((long)x.SecondStart * 10000) > VideoElement.Position).First();

            firstItem.IsSelected = true;
            secondItem.IsSelected = true;   
            SubtitlePairBox.UpdateLayout();

            // TODO: Handle a case where the items too much out of sync.
            SubtitlePairBox.ScrollToCenterOfView(firstItem);
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (VideoElement.IsPlaying)
            {
                await VideoElement.Pause();
            }
            else
            {
                await VideoElement.Play();
            }


        }

        void MainWindow_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine("...");
        }

        private string ShowFileSelectionDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) return openFileDialog.FileName;
            else throw new Exception("Dialog did not close properly.");
        }

        private void FirstSubtitleFileButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.FirstSubtitlePath = ShowFileSelectionDialog();
            LoadSubtitlesAndAudio();
        }

        private void SecondSubtitleButton_Click(object sender, RoutedEventArgs e)
        {
           AppSettings.SecondSubtitlePath = ShowFileSelectionDialog();
            LoadSubtitlesAndAudio();
        }

        private void AudioFileButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.AudioFilePath = ShowFileSelectionDialog();
            LoadSubtitlesAndAudio();
        }
    }
}
