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
    public partial class MainWindow : Window
    {
        public static RoutedCommand PauseCommand = new RoutedCommand();
        public static RoutedCommand ForwardCommand = new RoutedCommand();
        public static RoutedCommand BackwardCommand = new RoutedCommand();

        public void Executed_PausePlay(object sender, ExecutedRoutedEventArgs e)
        {
            if (VideoElement.IsPaused) VideoElement.Play();
            else VideoElement.Pause();
        }

        public void CanExecute_PausePlay(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = VideoElement.IsLoaded;
        }

        public async void Executed_Forward(object sender, ExecutedRoutedEventArgs e)
        {
            if (VideoElement.IsPlaying) await VideoElement.Pause();
            await VideoElement.Seek(VideoElement.Position + new TimeSpan(0, 0, 5));
            await VideoElement.Play();
        }

        public async void Executed_Backward(object sender, ExecutedRoutedEventArgs e)
        {
            if (VideoElement.IsPlaying) await VideoElement.Pause();
            await VideoElement.Seek(VideoElement.Position - new TimeSpan(0, 0, 5));
            await VideoElement.Play();
        }

        public void CanExecute_Forward(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = VideoElement.IsLoaded;
        }

        public void CanExecute_Backward(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = VideoElement.IsLoaded;
        }


    }
}
