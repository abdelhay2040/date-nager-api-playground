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
        /*
         * convert day of the year into interval of start and end at UTC time line
         */

        private static IHoldiayInterval CalculateHolidayInterval(int day, double utc, char sign)
        {
            double start = day * 24 + (utc * (sign == '-' ? -1 : 1));//sign if it is for example "UTC+01:00" or "UTC-01:00"
            double end = start + 24;
            // for example first day of the year with time zone +1 the interval would be from 25 to 49
            return new HoldiayInterval(start, end);
        }
        #endregion
        #region public  functions
        /*
         * convert holidays with diffrent time zones into time a agnostic
         * loop through the country and convert thier holdays into UTC time scale using the diffrent time zones 
        */
        public static List<IHoldiayInterval> CalculateHolidayIntervals(Dictionary<string, IList<string>> coutryTimeZones,
            Dictionary<int, HashSet<string>> _daysAndCountryCodesMap)
        {
            List<IHoldiayInterval> res = new List<IHoldiayInterval>();
            foreach (var holday in _daysAndCountryCodesMap)
            {
                foreach (var country in holday.Value)
                {
                    foreach (var timeZone in coutryTimeZones[country])
                    {
                        if (timeZone != "UTC")// the format is +01:00 for example and "UTC" is special case means 00:00
                        {
                            double utc = TimeSpan.Parse(timeZone.Substring(4)).TotalHours; 
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
        /*
        * This function take list of intervals and merge overlapped intervals and will return the merged results
        * Example input [2,3][1,4] the output should be [1,4]
        * used line sweep algorithm
        */
        public static List<IHoldiayInterval> MergeHolidayIntervals(List<IHoldiayInterval> intervals)
        {
            int len = intervals.Count;
            List<IHoldiayInterval> mergedHolidayIntervals = new List<IHoldiayInterval>();
            if (len < 0) return null;
            List<IHoldiayInterval> sortedList = intervals.OrderBy(o => o.Start).ToList();

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
                    mergedHolidayIntervals.Add(new HoldiayInterval(start, end));

                    start = sortedList[i].Start;
                    end = sortedList[i].End;
                }
            }
            mergedHolidayIntervals.Add(new HoldiayInterval(start, end));
            return mergedHolidayIntervals.OrderBy(o => o.End - o.Start).ToList(); ;
        }

        #endregion public  functions 

    }
}
