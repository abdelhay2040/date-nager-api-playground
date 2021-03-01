using HolidayOptimizer.Models;
using HolidayOptimizer.Utilities;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Linq;
using System;

namespace HolidayOptimizerTests
{
    /*
      * Few Test Cases just for demonstration, but for real life should be enough test cases to cover all scenarios 
     */
    public class HolidayIntervalsProcessorTests
    {
        [Fact]
        public void CalculateHolidayIntervals_ShouldRturnMapedUTCIntervals()
        {
            Dictionary<string, IList<string>> contryTimeZones = new Dictionary<string, IList<string>>();
            contryTimeZones.Add("Au", new List<string>() { "UTC+01:00", "UTC", "UTC-01:00" });
            Dictionary<int, HashSet<string>> daysAndCountryCodesMap = new Dictionary<int, HashSet<string>>();
            daysAndCountryCodesMap.Add(1, new HashSet<string>() { "Au" });// vacation at first day of the year at Au
            List<IHoldiayInterval> intervals = HolidayIntervalsProcessor.CalculateHolidayIntervals(contryTimeZones, daysAndCountryCodesMap);
            List<IHoldiayInterval> expectedResult = new List<IHoldiayInterval>() { new HoldiayInterval(25, 49), new HoldiayInterval(24, 48), new HoldiayInterval(23, 47) };

            expectedResult.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void MergeHolidayIntervals_ShouldMergedIntervals()
        {
            List<IHoldiayInterval> intervals = new List<IHoldiayInterval>() { new HoldiayInterval(25, 49), new HoldiayInterval(24, 48), new HoldiayInterval(23, 47) };
            var mergedInterval =HolidayIntervalsProcessor.MergeHolidayIntervals(intervals);
            List<HoldiayInterval> expectedResult = new List<HoldiayInterval>() { new HoldiayInterval(23, 49)};
            expectedResult.Should().BeEquivalentTo(mergedInterval);
        }
    }
}
