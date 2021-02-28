using HolidayOptimizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayOptimizer.Utilities
{

    public static class HolidayIntervalsProcessor
    {
        #region private functions 
        // convert day of the year into interval of start and end at UTC time line
        private static HoldiayInterval CalculateHolidayInterval(int day, double utc, char sign)
        {
            double start = day * 24 + (utc * (sign == '-' ? -1 : 1));
            double end = start + 24;
            return new HoldiayInterval(start, end);
        }
        #endregion
        #region public  functions 
        // loop through the country and convert thier holdays into UTC time scale using the diffrent time zones  
        public static List<HoldiayInterval> CalculateHolidayIntervals(Dictionary<string, IList<string>> timeZones,
            Dictionary<int, HashSet<string>> _daysAndCountryCodesMap)
        {
            List<HoldiayInterval> res = new List<HoldiayInterval>();
            foreach (var holday in _daysAndCountryCodesMap)
            {
                foreach (var country in holday.Value)
                {
                    foreach (var timeZone in timeZones[country])
                    {
                        if (timeZone != "UTC")// the format is +01:00 forexample
                        {
                            double utc = TimeSpan.Parse(timeZone.Substring(4)).TotalHours;// 
                            char sign = timeZone.Substring(3, 4)[0];
                            res.Add(CalculateHolidayInterval(holday.Key, utc, sign));
                        }
                        else
                        {
                            res.Add(CalculateHolidayInterval(holday.Key, 0, '+'));

                        }

                    }
                }
            }
            return res;

        }
        // use the line sweep Algorithm to merge diffrent holidays intervals
        public static List<HoldiayInterval> MergeHolidayIntervals(List<HoldiayInterval> intervals)
        {
            int len = intervals.Count;
            List<HoldiayInterval> res = new List<HoldiayInterval>();
            if (len < 0) return null;
            List<HoldiayInterval> sortedList = intervals.OrderBy(o => o.Start).ToList();

            double start = sortedList[0].Start;
            double end = sortedList[0].End;

            for (int i = 0; i < len; ++i)
            {
                if (sortedList[i].Start <= end)
                {
                    end = Math.Max(sortedList[i].End, end);
                }
                else
                {
                    res.Add(new HoldiayInterval(start, end));

                    start = sortedList[i].Start;
                    end = sortedList[i].End;
                }
            }
            res.Add(new HoldiayInterval(start, end));
            return res.OrderBy(o => o.End - o.Start).ToList(); ;
        }

        #endregion public  functions 

    }
}
