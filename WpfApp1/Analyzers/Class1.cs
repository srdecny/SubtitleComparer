using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.SubtitlePair;

namespace WpfApp1
{
    public static class Analyzers
    {
        // Returns True if both subtitles are similar, but just out of sync.
        public static bool AnalyzeOutOfSync(Subtitles first, Subtitles second)
        {

            int sampleCount = 50;
            if (GetSubtitleTimingDiffs(first, second, sampleCount).All(x => x <= Settings.MilisecondSpreadSimilarness)) return false;
            else return true;
        }

        // Returns True if subtitles are somewhat similar, False if not.
        public static bool AnalyzeSimilarness(Subtitles first, Subtitles second)
        {
            var timingDiffs = GetSubtitleTimingDiffs(first, second);
            int similarCount = timingDiffs.Count(x => x <= Settings.MilisecondSpreadSimilarness);
            if (similarCount / timingDiffs.Count > Settings.SimilarnessPercentageTreshold) return false;
            else return true;
        }

        // Computes the timing differences between subtitles in linear order.
        // If count is -1, computes all Diffs.
        public static List<int> GetSubtitleTimingDiffs(Subtitles first, Subtitles second, int count = -1)
        {
            List<int> diffs = new List<int>();
            if (count == -1) count = Math.Min(first.SubtitleItems.Count, second.SubtitleItems.Count);
            using (var e1 = first.SubtitleItems.GetEnumerator())
            {
                using (var e2 = second.SubtitleItems.GetEnumerator())
                {
                    List<int> averageDistances = new List<int>();
                    for (int i = 0; i < count; i++)
                    {
                        while (e1.MoveNext() && e2.MoveNext())
                        {
                            var FirstItem = e1.Current;
                            var SecondItem = e2.Current;

                            diffs.Add(Math.Abs(FirstItem.StartTime - SecondItem.StartTime));
                        }
                    }

                }
            }

            return diffs;
        }
    }
}
