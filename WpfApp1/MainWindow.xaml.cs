using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Threading;
using WpfApp1.Extensions;
using WpfApp1.SubtitlePair;
using WpfApp1.Dialogues;
using System.Collections.ObjectModel;
using System.Text;
using System.Net;

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

        public MainWindow()
        {
            Unosquare.FFME.MediaElement.FFmpegDirectory = Settings.GetFfmpegDirectoryFromConfig();

            InitializeComponent();
            VideoElement.LoadedBehavior = MediaState.Manual;
            VideoElement.Source = new Uri(AppSettings.AudioFilePath);
            VideoDurationTextBox.DataContext = VideoElement;
            this.DataContext = AppSettings;
            LoadSubtitlesAndAudio();
        }

        private void LoadSubtitlesAndAudio()
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            using (var stream = new FileStream(AppSettings.FirstSubtitlePath, FileMode.Open))
            {
                FirstSub = new Subtitles(parser.ParseStream(stream));
            }
            using (var stream = new FileStream(AppSettings.SecondSubtitlePath, FileMode.Open))
            {
                SecondSub = new Subtitles(parser.ParseStream(stream));
            }

            var result = SubtitlePairHelper.GenerateSubtitlePairs(FirstSub, SecondSub);
            var foo = new List<SubtitlePairViewModel>();
            foreach (var item in result)
            {
                foo.Add(new SubtitlePairViewModel(item));
            }
                
            SubtitlePairBox.ItemsSource = foo;
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource);
            view.Filter = ToggleDiffFilter;
            view.CustomSort = new SubtitleViewModelSorter();
            //AnalyzeSubtitles();
        }

        private bool ToggleDiffFilter(object item)
        {
            SubtitlePairViewModel model = item as SubtitlePairViewModel;
            if (ToggleDiffCheckBox.IsChecked.Value)
            {
                return model.FirstContent != model.SecondContent;
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
            MessageBox.Show("Error during video playback. Check the FFMPeg binaries folder.");
        }

        // Update the selected items and center the screen.
        private void VideoElement_PositionChanged(object sender, Unosquare.FFME.Events.PositionChangedRoutedEventArgs e)
        {
            var items = SubtitlePairBox.ItemsSource as List<SubtitlePairViewModel>;
            SubtitlePairBox.SelectedItems.Clear();

            // Microbenchmarked a manual for loop, but there's no performance difference.
            
            var firstItem = items.Where(x => x.FirstStartTimeSpan > VideoElement.Position).OrderBy(x => x.FirstStartTimeSpan).First();
            var secondItem = items.Where(x => x.SecondStartTimeSpan > VideoElement.Position).OrderBy(x => x.SecondStartTimeSpan).First();

            if (firstItem.FirstStart < secondItem.SecondStart)
            {
                firstItem.IsSelected = true;
            }
            else
            {
                secondItem.IsSelected = true;
            }

            SubtitlePairBox.UpdateLayout();

            // TODO: Handle a case where the items too much out of sync.
            SubtitlePairBox.ScrollToCenterOfView(firstItem);
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Executed_PausePlay(sender, null);
        }

        void MainWindow_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine("...");
        }

        private string ShowFileSelectionDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) return openFileDialog.FileName;
            else return "";
        }

        private void FirstSubtitleFileButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.FirstSubtitlePath = ShowFileSelectionDialog();
             if (AppSettings.FirstSubtitlePath != "") LoadSubtitlesAndAudio();
        }

        private void SecondSubtitleButton_Click(object sender, RoutedEventArgs e)
        {
           AppSettings.SecondSubtitlePath = ShowFileSelectionDialog();
            if (AppSettings.SecondSubtitlePath != "") LoadSubtitlesAndAudio();
        }

        private void AudioFileButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.AudioFilePath = ShowFileSelectionDialog();
            if (AppSettings.AudioFilePath != "") LoadSubtitlesAndAudio();
        }

        private void AnalyzeSubtitles()
        {
            if (Analyzers.AnalyzeOutOfSync(FirstSub, SecondSub))
            {
                MessageBox.Show("Loaded subtitles seem to be out of sync with each other.");
            }
            else if (Analyzers.AnalyzeSimilarness(FirstSub, SecondSub))
            {
                MessageBox.Show("Loaded subtitles aren't similar at all. Do they belong to the same audio file?");
            }
        }

        private void FirstSubtitleExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            if (dialog.ShowDialog() == true)
            {

                StringBuilder sb = new StringBuilder();
                var items = SubtitlePairBox.ItemsSource as List<SubtitlePairViewModel>;
                int count = 1;

                foreach (var subtitle in items.Where(x => x.FirstContent != "").OrderBy(x => x.FirstStartTimeSpan))
                {
                    sb.Append(count);
                    sb.AppendLine();
                    sb.Append(subtitle.FirstSubtitleAsString());
                    sb.AppendLine();
                    sb.AppendLine();
                    count++;
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
            }

        }

        private void SecondSubtitleExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            if (dialog.ShowDialog() == true)
            {

                StringBuilder sb = new StringBuilder();
                var items = SubtitlePairBox.ItemsSource as List<SubtitlePairViewModel>;
                int count = 1;

                foreach (var subtitle in items.Where(x => x.SecondContent != "").OrderBy(x => x.SecondStartTimeSpan))
                {
                    sb.Append(count);
                    sb.AppendLine();
                    sb.Append(subtitle.SecondSubtitleAsString());
                    sb.AppendLine();
                    sb.AppendLine();
                    count++;
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource).Refresh();
        }

        private string getSubtitleUrl()
        {
            UrlDialogue d = new UrlDialogue();
            if(d.ShowDialog() == true)
            {
                return d.Answer;
            }
            else
            {
                return "";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            getSubtitleUrl();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            getSubtitleUrl();
        }
    }

    public class StringToTimespanConverter : IValueConverter
    {
        // TimeSpan -> String
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).ToString(Settings.TimeSpanStringFormat);
        }

        // String -> TimeSpan
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timespan;
            if (TimeSpan.TryParseExact(value.ToString(), Settings.TimeSpanStringFormat, CultureInfo.InvariantCulture, out timespan))
            {
                return timespan;
            }
            else
            {
                return null;
            }
        }
    }
}
