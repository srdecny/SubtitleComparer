using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubtitlesParser.Classes;
using DiffMatchPatch;
using System.ComponentModel;

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
        public static int Delta = 50;

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
                    if (Math.Abs(i.StartTime - j.StartTime) <= Delta)
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

            result.Sort();
            return result;
        }
    }

   public class SubtitlePair : IComparable<SubtitlePair>
    {
        public SubtitleItem First;
        public SubtitleItem Second;

        public SubtitlePair(SubtitleItem first, SubtitleItem second)
        {
            First = first;
            Second = second;
        }

        public int CompareTo(SubtitlePair other)
        {
            int thisStartTime = Math.Max(this.First.StartTime, this.Second.StartTime);
            int otherStartTime = Math.Max(other.First.StartTime, other.Second.StartTime);

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

    public class SubtitlePairViewModel : INotifyPropertyChanged
    {
        public string FirstContent { get; set; }
        public string SecondContent { get; set; }
        public string Diff { get; set; }
        public int FirstStart { get; set; }
        public int SecondStart { get; set; }
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

            foreach (var s in pair.First.Lines)
            {
                FirstContent += s + ' ' ;
            }

            foreach (var s in pair.Second.Lines)
            {
                SecondContent += s + ' ';
            }
            if (FirstContent == SecondContent ) Diff = "SAME";
            else Diff = "DIFFERENT";
        }

        public event PropertyChangedEventHandler PropertyChanged;   

        private string GenerateDiff(string first, string second)
        {
            var dmp = DiffMatchPatchModule.Default;
            var diffs = dmp.DiffMain(first, second);
            return "AAAA";
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    


    
}
