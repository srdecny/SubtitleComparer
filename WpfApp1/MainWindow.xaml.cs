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
using SubtitlesParser;
using System.IO;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Subtitles FirstSub;
        Subtitles SecondSub;
        public MainWindow()
        {
            InitializeComponent();
            LoadSubtitles();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void LoadSubtitles()
        {
            var parser = new SubtitlesParser.Classes.Parsers.SubParser();

            FirstSub = new Subtitles(parser.ParseStream(new FileStream(@"C:\Users\Vojta\Documents\Zapoctak\06.srt", FileMode.Open)));
            SecondSub = new Subtitles(parser.ParseStream(new FileStream(@"C:\Users\Vojta\Documents\Zapoctak\07.srt", FileMode.Open)));

            var result = SubtitlePairHelper.GenerateSubtitlePairs(FirstSub, SecondSub);
            var foo = new List<Foo>();
            foreach (var item in result)
            {
                foo.Add(new Foo(item));
            }

            SubtitlePairBox.ItemsSource = foo;

        }
    }
}
