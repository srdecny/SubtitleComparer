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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool benchmark = false;

        Subtitles FirstSub;
        Subtitles SecondSub;
        public MainWindow()
        {
            Unosquare.FFME.MediaElement.FFmpegDirectory = @"C:\Users\srdecny\Downloads\ffmpeg-4.0.2-win64-shared\ffmpeg-4.0.2-win64-shared\bin\";
            InitializeComponent();
            VideoElement.LoadedBehavior = MediaState.Manual;
            VideoElement.Source = new Uri(@"C:\Users\srdecny\Documents\7_1.mp4");
            LoadSubtitles();
            VideoDurationTextBox.DataContext = VideoElement;
        }

        private void LoadSubtitles()
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            FirstSub = new Subtitles(parser.ParseStream(new FileStream(@"C:\Users\srdecny\Documents\subtitles.srt", FileMode.Open)));
            SecondSub = new Subtitles(parser.ParseStream(new FileStream(@"C:\Users\srdecny\Documents\subtitles2.srt", FileMode.Open)));

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
                VideoElement.Pause();
                // item.Start is in miliseconds, 10000 ms = 1 tick.
                await VideoElement.Seek(new TimeSpan(item.Start * 10000));
                VideoElement.Play();
            }
                
        }

        private void VideoElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine("...");
        }

        private void VideoElement_PositionChanged(object sender, Unosquare.FFME.Events.PositionChangedRoutedEventArgs e)
        {
            var items = SubtitlePairBox.ItemsSource as List<SubtitlePairViewModel>;
            SubtitlePairBox.SelectedItems.Clear();
            
            // Microbenchmarked a manual for loop, but there's no performance difference.
            var item = items.Where(x => new TimeSpan(x.Start * 10000) > VideoElement.Position).First();
            item.IsSelected = true;            
            SubtitlePairBox.UpdateLayout();
            SubtitlePairBox.ScrollIntoView(item);
            //((ListViewItem)SubtitlePairBox.ItemContainerGenerator.ContainerFromIndex(SubtitlePairBox.SelectedIndex)).Focus();
        }
    }
}
