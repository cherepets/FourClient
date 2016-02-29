using FourToolkit.Esent;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Library.Statistics
{
    public interface IKeywordStatistics
    {
        double Score(string s);
        void UpdateWith(string s);
    }

    public class KeywordStatistics : StatisticsBase, IKeywordStatistics
    {
        private const string Table = "Keywords";

        private List<Keyword> _keywords;

        public double Score(string s)
        {
            var categories = DetectCategories(s);
            if (!categories.Any()) return default(double);
            var weights = GetCategoryWeights();
            var total = weights.Sum(w => w.Value);
            if (total == 0) return default(double);
            double weight = 0;
            foreach (var category in categories)
                weight += weights[category];
            return weight / total;
        }
        
        public void UpdateWith(string s)
        {
            var keywords = DetectKeywords(s);
            foreach (var keyword in keywords)
                IncreaseCount(keyword);
        }

        private IList<Keyword> Keywords
        {
            get
            {
                if (_keywords == null)
                    _keywords = Query(db =>
                    {
                        using (var table = db.Tables[Table].Open())
                        {
                            var rows = table.SelectFirstRows(int.MaxValue);
                            var keywords = rows?.Select(r => EsentSerializer.Deserialize<Keyword>(r)).ToList() ?? new List<Keyword>();
                            return keywords;
                        }
                    }) as List<Keyword>;
                return _keywords;
            }
        }

        private IDictionary<int, int> GetCategoryWeights()
        {
            var weights = new Dictionary<int, int>();
            var groups = _keywords.GroupBy(k => k.Category);
            foreach (var group in groups)
            {
                var count = group.Sum(k => k.Count);
                weights.Add(group.Key, count);
            }
            return weights;
        }

        private IList<int> DetectCategories(string s)
        {
            var categories = new List<int>();
            foreach (var keyword in Keywords)
            {
                if (!categories.Contains(keyword.Category) && s.ToLower().Contains(keyword.Word.ToLower()))
                    categories.Add(keyword.Category);
            }
            return categories;
        }

        private IList<Keyword> DetectKeywords(string s)
        {
            var keywords = new List<Keyword>();
            foreach (var keyword in Keywords)
            {
                if (!keywords.Contains(keyword) && s.ToLower().Contains(keyword.Word.ToLower()))
                    keywords.Add(keyword);
            }
            return keywords;
        }

        private bool IncreaseCount(Keyword keyword)
        {
            return (bool)Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    keyword.Count++;
                    var updated = table.Update(r => r["Word"].AsString == keyword.Word, "Count", keyword.Count) > 0;
                    return updated;
                }
            });
        }
    }
}
