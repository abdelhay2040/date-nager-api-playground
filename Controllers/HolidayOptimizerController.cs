using Abp.UI;
using HolidayOptimizer.Models;
using HolidayOptimizer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nager.Date;
using Nager.Date.Model;
using RESTCountries.Models;
using RESTCountries.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace swaggertest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayOptimizerController : ControllerBase
    {
        #region Private variable
        private readonly string[] _countryCodes;
        private IMemoryCache _memoryCache;

        private Dictionary<string, int> _countriesWithUniqueVacations = new Dictionary<string, int>();
        private CountryModel _countriyWithMaxHolidays = new CountryModel();
        private Dictionary<int, HashSet<string>> _daysCountryCodesMap = new Dictionary<int, HashSet<string>>();
        private int[] _holidayiesPerMonth = new int[12];
        #endregion

        #region Contructor
        public HolidayOptimizerController(IMemoryCache memoryCache)
        {
            _countryCodes = new[]//@todo Refactor Add to file and read from the file 
        {
            "AD", "AR", "AT", "AU", "AX",  "BB", "BE", "BG", "BO", "BR", "BS", "BW", "BY", "BZ", "CA", "CH", "CL", "CN", "CO", "CR", "CU", "CY",  "CZ", "DE", "DK", "DO", "EC", "EE", "EG", "ES", "FI", "FO", "FR", "GA", "GB", "GD", "GL", "GR", "GT",  "GY", "HN", "HR", "HT", "HU", "IE", "IM", "IS", "IT", "JE", "JM", "LI", "LS", "LT", "LU", "LV", "MA",  "MC", "MD", "MG", "MK", "MT", "MX", "MZ", "NA", "NI", "NL", "NO", "NZ", "PA", "PE", "PL", "PR",  "PT", "PY", "RO", "RS", "RU", "SE", "SI", "SJ", "SK", "SM", "SR", "SV", "TN", "TR", "UA", "US", "UY",  "VA", "VE", "ZA"
        };
            this._memoryCache = memoryCache;

        }
        #endregion Contructor
        #region Private Functions 

        private void RequestAllCountriesHolidays(int year)
        {
            if (year < 1900 || year > 2050)
            {
                throw new UserFriendlyException("Please check the year you have entered");
            }

            if (GetCashedProccedHolidays(year)) return;

            foreach (var countryCode in _countryCodes)
            {
                var publicHolidays = DateSystem.GetPublicHoliday(year, countryCode);
                ProcessCountryHolidays(countryCode, publicHolidays);
            }
            InizializeCountriesWithUniqueVacations();
            CacheProccedHolidays(year);

        }

        private void ProcessCountryHolidays(string countryCode, IEnumerable<PublicHoliday> publicHolidays)
        {

            TrackCountriyWithMaxHolidays(countryCode, publicHolidays.ToList().Count);

            foreach (var publicHoliday in publicHolidays)
            {
                TrackMonthWithMaxHolidays(publicHoliday.Date.Month);
                FlatteningHolidays(countryCode, publicHoliday.Date); 
            }
        }

        private void InizializeCountriesWithUniqueVacations()
        {
            foreach (var day in _daysCountryCodesMap)
            {
                if (day.Value.Count == 1) // if the day have only one country
                {
                    string country = day.Value.ElementAt(0);
                    _countriesWithUniqueVacations[country] = _countriesWithUniqueVacations.GetValueOrDefault(country) + 1; // add it to the map that contains countries and how many unique days it have
                }
            }
        }

        private void TrackCountriyWithMaxHolidays(string countryCode, int holidays)
        {

            if (holidays > _countriyWithMaxHolidays.Holidays)
            {
                _countriyWithMaxHolidays.Holidays = holidays;
                _countriyWithMaxHolidays.CountryCode = countryCode;
            }
        }

        private void TrackMonthWithMaxHolidays(int month)
        {
            _holidayiesPerMonth[month - 1]++;
        }

        private void FlatteningHolidays(string countryCode, DateTime dateTime)//for each day of the year put a list of countries code that have holiday at this day
        {
            int dayOfTheYear = dateTime.DayOfYear;
            if (!_daysCountryCodesMap.ContainsKey(dayOfTheYear))
            {
                _daysCountryCodesMap.Add(dayOfTheYear, new HashSet<string>() { countryCode });
            }
            else
            {
                _daysCountryCodesMap[dayOfTheYear].Add(countryCode);
            }
        }

        private CountryModel CountryWithMaxUniqueHolidays()
        {
            var countryWithMaxUniqueHolidays = _countriesWithUniqueVacations.Aggregate((l, r) => l.Value > r.Value ? l : r);
            CountryModel countryHoliday = new CountryModel(countryWithMaxUniqueHolidays.Key, countryWithMaxUniqueHolidays.Value);
            return countryHoliday;
        }

        private void CacheProccedHolidays(int year)
        {

            _memoryCache.Set(year + "cashed", true);
            _memoryCache.Set("countriyWithMaxHolidays" + year, _countriyWithMaxHolidays);
            _memoryCache.Set("countriesWithUniqueVacations" + year, _countriesWithUniqueVacations);
            _memoryCache.Set("daysAndCountryCodesMap" + year, _daysCountryCodesMap);
            _memoryCache.Set("holidayiesPerMonth" + year, _holidayiesPerMonth);
        }
        private bool GetCashedProccedHolidays(int year)// will return true if is is cashed false otherwise 
        {
            bool yearCached;
            bool isYearCached = _memoryCache.TryGetValue(year + "cashed", out yearCached);
            if (isYearCached)
            {
                _memoryCache.TryGetValue("countriyWithMaxHolidays" + year, out _countriyWithMaxHolidays);
                _memoryCache.TryGetValue("countriesWithUniqueVacations" + year, out _countriesWithUniqueVacations);
                _memoryCache.TryGetValue("daysAndCountryCodesMap" + year, out _daysCountryCodesMap);
                _memoryCache.TryGetValue("holidayiesPerMonth" + year, out _holidayiesPerMonth);
               
            }
            return isYearCached;
        }

        private async Task<Dictionary<string, IList<string>>> RequestContryZonesAsync()
        {
            Dictionary<string, IList<string>> timeZones;
            bool timeZoneExist = _memoryCache.TryGetValue("timeZones", out timeZones);//check if it is already cashed 
            if (timeZoneExist)
                return timeZones;// return cashed values 

            timeZones = new Dictionary<string, IList<string>>();
            foreach (var countryCode in _countryCodes)
            {
                Country result = await RESTCountriesAPI.GetCountryByCodeAsync(countryCode);
                if (!timeZones.ContainsKey(countryCode))
                {
                    timeZones.Add(countryCode, result.Timezones);

                }
            }
            _memoryCache.Set("timeZones", timeZones);
            return timeZones;
        }
        #endregion Private Functions



        #region HttpGet
        [HttpGet("Get Country With Max Holidays")]
        public string GetCountryWithMaxHolidays(int year)
        {
            RequestAllCountriesHolidays(year);
            string formatedString = String.Format("{0} is the  the country has the most holidays with {1} holidays",
                _countriyWithMaxHolidays.CountryCode, _countriyWithMaxHolidays.Holidays);
            return formatedString;
        }


        [HttpGet("Get Month With Max Holidays")]
        public string GetMonthWithMaxHolidays(int year)
        {
            RequestAllCountriesHolidays(year);
            int maxHolidayiesPerMonth = _holidayiesPerMonth.Max();
            int maxIndex = _holidayiesPerMonth.ToList().IndexOf(maxHolidayiesPerMonth);
            string formatedString = String.Format("{0} is the  the Month that has the most holidays with {1} holidays",
               CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(maxIndex), maxHolidayiesPerMonth);
            return formatedString;
        }

        [HttpGet("Get Country With Max Unique Holidays")]
        public string GetCountryWithMaxUniqueHolidays(int year)
        {
            RequestAllCountriesHolidays(year);
            var country = CountryWithMaxUniqueHolidays();
            string formatedString = String.Format("{0} is the  the country has the most unique holidays with {1} holidays",
                country.CountryCode, country.Holidays);
            return formatedString;
        }

        [HttpGet("Get Longest Lasting Holidays Sequance")]
        public async Task<string> GetLongestLastingHolidaySequanceAsync(int year)
        {
            RequestAllCountriesHolidays(year);
            var timeZones = await RequestContryZonesAsync();

            var holidayIntervals = HolidayIntervalsProcessor.CalculateHolidayIntervals(timeZones, _daysCountryCodesMap);
            var mergedHolidayIntervals = HolidayIntervalsProcessor.MergeHolidayIntervals(holidayIntervals);

            var longestHolidaySequance = mergedHolidayIntervals[mergedHolidayIntervals.Count - 1].End - mergedHolidayIntervals[mergedHolidayIntervals.Count - 1].Start;
            string formatedString = String.Format("The longest lasting sequence of holidays around the world is {0} hours",
              longestHolidaySequance);

            return formatedString;
        }
        #endregion HttpGet

    }
}
