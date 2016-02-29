using FourToolkit.Esent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FourClient.Library.Statistics
{
    public interface ILaunchStatistics
    {
        bool Score(DateTime d);
        void UpdateWith(DateTime d);
    }

    public class LaunchStatistics : StatisticsBase, ILaunchStatistics
    {
        private const string Table = "Launch";

        private List<Launch> _launches;

        public bool Score(DateTime d)
        {
            var ordered = Launches.OrderBy(l => l.Count).ToList();
            if (ordered[0].Hour == d.Hour) return true;
            if (ordered[1].Hour == d.Hour) return true;
            return false;
        }

        public void UpdateWith(DateTime d)
        {
            var launch = Launches.FirstOrDefault(l => l.Hour == d.Hour);
            if (launch == null) return;
                IncreaseCount(launch);
        }

        private IList<Launch> Launches
        {
            get
            {
                if (_launches == null)
                    _launches = Query(db =>
                    {
                        using (var table = db.Tables[Table].Open())
                        {
                            var rows = table.SelectFirstRows(int.MaxValue);
                            var launches = rows?.Select(r => EsentSerializer.Deserialize<Launch>(r)).ToList() ?? new List<Launch>();
                            return launches;
                        }
                    }) as List<Launch>;
                return _launches;
            }
        }

        private bool IncreaseCount(Launch launch)
        {
            return (bool)Query(db =>
            {
                using (var table = db.Tables[Table].Open())
                {
                    launch.Count++;
                    var updated = table.Update(r => r["Hour"].AsInt == launch.Hour, "Count", launch.Count) > 0;
                    return updated;
                }
            });
        }
    }
}
