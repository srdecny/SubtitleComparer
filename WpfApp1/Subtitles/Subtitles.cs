using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitlesParser.Classes;
using System.ComponentModel;
using System.Collections;
using System.IO;
using System.Windows;


namespace WpfApp1.SubtitlePair
{
    public class Subtitles
    {
        public readonly List<SubtitleItem> SubtitleItems;

        public Subtitles(List<SubtitleItem> items)
        {
            SubtitleItems = items;
        }
    }

    public static class SubtitlePairHelper
    {   
        public static SubtitleItem CreateEmptySubtitleItem()
        {
            var item = new SubtitleItem();
            return item;
        }

        // Inefficient.
        public static List<SubtitlePair> GenerateSubtitlePairs(Subtitles first, Subtitles second)
        {
            var result = new List<SubtitlePair>();
            foreach (var i in first.SubtitleItems)
            {
                bool AddedPair = false;

                foreach (var j in second.SubtitleItems)
                {
                    if (Math.Abs(i.StartTime - j.StartTime) <= Settings.MilisecondSpreadSimilarness)
                    {
                        result.Add(new SubtitlePair(i, j));
                        AddedPair = true;
                        break;
                    }
                }

                if (!AddedPair) result.Add(new SubtitlePair(i, CreateEmptySubtitleItem()));
            }

            var SecondStartTimes = from pair in result
                                   select pair.Second.StartTime;

            foreach (var j in second.SubtitleItems)
            {
                if (!(SecondStartTimes.Any(start => start == j.StartTime)))
                {
                    result.Add(new SubtitlePair(CreateEmptySubtitleItem(), j));
                }
            }

            return result;
        }

        public static string CreateSubtitles(List<SubtitleItem> subtitles)
        {
            return "aa";
        }
   
    }

   public class SubtitlePair
    {
        public SubtitleItem First;
        public SubtitleItem Second;

        public SubtitlePair(SubtitleItem first, SubtitleItem second)
        {
            First = first;
            Second = second;

        }


        
    }

    public class SubtitlePairViewModel : INotifyPropertyChanged
    {
        public string FirstContent { get; set; }
        public string SecondContent { get; set; }

        // For displaying in GUI
        public TimeSpan FirstStartTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(FirstStart); }
            set
            {
                FirstStart = (int)value.TotalMilliseconds;
                OnPropertyChanged("FirstStart");
            }
        }

        public TimeSpan SecondStartTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(SecondStart); }
            set
            {
                SecondStart = (int)value.TotalMilliseconds;
                OnPropertyChanged("SecondStart");
            }

        }

        public TimeSpan FirstEndTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(FirstEnd); }
            set
            {
                FirstEnd = (int)value.TotalMilliseconds;
                OnPropertyChanged("FirstEnd");
            }
        }

        public TimeSpan SecondEndTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(SecondEnd); }
            set
            {
                SecondEnd = (int)value.TotalMilliseconds;
                OnPropertyChanged("SecondEnd");
            }

        }


        public int FirstStart { get; set; }
        public int SecondStart { get; set; }

        public int FirstEnd { get; set; }

        public int SecondEnd { get; set; }

        private bool isSelected;
        public bool IsSelected {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ICollectionView SubtitlePairs { get; set; }

        public SubtitlePairViewModel(SubtitlePair pair)
        {
            FirstContent = "";
            SecondContent = "";
            IsSelected = false;

            FirstStart = pair.First.StartTime;
            SecondStart = pair.Second.StartTime;

            FirstEnd = pair.First.EndTime;
            SecondEnd = pair.Second.EndTime;

            foreach (var s in pair.First.Lines)
            {
                FirstContent += s + ' ' ;
            }

            foreach (var s in pair.Second.Lines)
            {
                SecondContent += s + ' ';
            }
        }

        public string FirstSubtitleAsString()
        {
            return ConvertSubtitleToString(true);
        }

        public string SecondSubtitleAsString()
        {
            return ConvertSubtitleToString(false);
        }

        private string ConvertSubtitleToString(bool convertFirst)
        {
            if (convertFirst)
            {
                return $"{FirstStartTimeSpan.ToString(Settings.TimeSpanStringFormat)} --> {FirstEndTimeSpan.ToString(Settings.TimeSpanStringFormat)}"
                    + '\n'
                    + FirstContent;
            }
            else
            {
                return $"{SecondStartTimeSpan.ToString(Settings.TimeSpanStringFormat)} --> {SecondEndTimeSpan.ToString(Settings.TimeSpanStringFormat)}"
                    + '\n'
                    + SecondContent;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class SubtitleViewModelSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            SubtitlePairViewModel first = x as SubtitlePairViewModel;
            SubtitlePairViewModel second = y as SubtitlePairViewModel;

            int thisStartTime = Math.Max(first.FirstStart, first.SecondStart);
            int otherStartTime = Math.Max(second.FirstStart, second.SecondStart);

            if (thisStartTime > otherStartTime)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

    }

    


    
}
