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

        public MainWindow()
        {
            Unosquare.FFME.MediaElement.FFmpegDirectory = @"C:\Users\srdecny\Documents\ffmpeg";
            AppSettings.FirstSubtitlePath = @"C:\Users\srdecny\Documents\subtitles.srt";

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
            var view = CollectionViewSource.GetDefaultView(SubtitlePairBox.ItemsSource);
            view.Filter = ToggleDiffFilter;
            AnalyzeSubtitles();
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

        private void Diff_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is TextBlock block)
            {

                var delimiters = ",. ".ToCharArray();

                if (block.Text.Split('\n')[0] == block.Text.Split('\n')[1])
                {
                    block.Text = "SAME";
                    return;
                }
                else
                {
                    var firstWords = block.Text.Split('\n')[0].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var secondWords = block.Text.Split('\n')[1].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                    List<(string, bool)> formattedWordsFirst = new List<(string, bool)>();
                    List<(string, bool)> formattedWordsSecond = new List<(string, bool)>();

                    int smallerSize = Math.Min(firstWords.Count(), secondWords.Count());


                    block.Text = "";

                    for (int i = 0; i < smallerSize; i++)
                    {
                        if (firstWords[i] != secondWords[i])
                        {
                            formattedWordsFirst.Add(ValueTuple.Create(firstWords[i], true));
                            formattedWordsSecond.Add(ValueTuple.Create(secondWords[i], true));
                        }
                        else
                        {
                            formattedWordsFirst.Add(ValueTuple.Create(firstWords[i], false));
                            formattedWordsSecond.Add(ValueTuple.Create(secondWords[i], false));
                        }
                    }

                    if (firstWords.Count() > smallerSize)
                    {
                        for (int i = smallerSize; i < firstWords.Count(); i++)
                        {
                            formattedWordsFirst.Add(ValueTuple.Create(firstWords[i], true));
                        }
                    }
                    else
                    {
                        for (int i = smallerSize; i < secondWords.Count(); i++)
                        {
                            formattedWordsSecond.Add(ValueTuple.Create(secondWords[i], true));
                        }
                    }

                    foreach (var formattedWord in formattedWordsFirst)
                    {
                        if (formattedWord.Item2 == true)
                        {
                            block.Inlines.Add(new Run(formattedWord.Item1 + " ") { Foreground = Brushes.Red });
                        }
                        else
                        {
                            block.Inlines.Add(new Run(formattedWord.Item1 + " "));

                        }
                    }
                    block.Inlines.Add(new Run("\n"));
                    foreach (var formattedWord in formattedWordsSecond)
                    {
                        if (formattedWord.Item2 == true)
                        {
                            block.Inlines.Add(new Run(formattedWord.Item1 + " ") { Foreground = Brushes.Green });
                        }
                        else
                        {
                            block.Inlines.Add(new Run(formattedWord.Item1 + " "));
                        }
                    }

                }


            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Diff_TargetUpdated(sender, null);
        }
    }
}
