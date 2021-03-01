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

        private Dictionary<string, int> _countriesWithUniqueHolidays = new Dictionary<string, int>();
        private CountryModel _countriyWithMaxHolidays = new CountryModel();
        private Dictionary<int, HashSet<string>> _dayOfTheYearsMap = new Dictionary<int, HashSet<string>>();
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
        /*
         * request all of the holidays of the countries for the passed year if the data for that year
         * is not cached , if it is cached it will get the cached data
        */
        private void RequestAllCountriesHolidays(int year)
        {
            if (year <= 1900 || year >= 2050)
            {
                throw new UserFriendlyException("Please check the year you have entered should be between 1900 and 2050");
            }

            if (GetCachedProccedHolidays(year)) return;

            foreach (var countryCode in _countryCodes)
            {
                var publicHolidays = DateSystem.GetPublicHoliday(year, countryCode);
                ProcessCountryHolidays(countryCode, publicHolidays);
            }

            InizializeCountriesWithUniqueVacations();
            CacheProccedHolidays(year);

        }

        /* ProcessCountryHolidays has multiple purposes:
         * 1. Calls the country with max holidays tracker (TrackCountriyWithMaxHolidays)
         * 2. Calls MapHolidaysIntoDayOfTheYear
        */
        private void ProcessCountryHolidays(string countryCode, IEnumerable<PublicHoliday> publicHolidays)
        {

            TrackCountriyWithMaxHolidays(countryCode, publicHolidays.ToList().Count);

            foreach (var publicHoliday in publicHolidays)
            {
                TrackMonthWithMaxHolidays(publicHoliday.Date.Month);
                MapHolidaysIntoDayOfTheYear(countryCode, publicHoliday.Date);
            }
        }

        /* 
         * InizializeCountriesWithUniqueVacations purpose is to maintain _countriesWithUniqueVacations updated 
         * with countries of unique holidays (the day is a holiday only in this country i.e. day.Value.Count == 1)
        */
        private void InizializeCountriesWithUniqueVacations()
        {
            foreach (var day in _dayOfTheYearsMap)
            {
                if (day.Value.Count == 1) // if the day have only one country
                {
                    string country = day.Value.ElementAt(0);
                    _countriesWithUniqueHolidays[country] = _countriesWithUniqueHolidays.GetValueOrDefault(country) + 1; // add it to the map that contains countries and how many unique days it have
                }
            }
        }

        /* 
         * TrackCountriyWithMaxHolidays purpose is to keep _countriyWithMaxHolidays updated 
         * with they country of maximum number of holidays
        */
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

        /* 
         * TrackCountriyWithMaxHolidays puts a list of country codes that have holiday at this day
         * for each day of the year
        */
        private void MapHolidaysIntoDayOfTheYear(string countryCode, DateTime dateTime)
        {
            int dayOfTheYear = dateTime.DayOfYear;
            if (!_dayOfTheYearsMap.ContainsKey(dayOfTheYear))
            {
                _dayOfTheYearsMap.Add(dayOfTheYear, new HashSet<string>() { countryCode });
            }
            else
            {
                _dayOfTheYearsMap[dayOfTheYear].Add(countryCode);
            }
        }
        /* 
        * select the country which has the most uniquue holiday from the _countriesWithUniqueVacations
        */
        private CountryModel CountryWithMaxUniqueHolidays()
        {
            var countryWithMaxUniqueHolidays = _countriesWithUniqueHolidays.Aggregate((l, r) => l.Value > r.Value ? l : r);
            CountryModel countryHoliday = new CountryModel(countryWithMaxUniqueHolidays.Key, countryWithMaxUniqueHolidays.Value);
            return countryHoliday;
        }
        /* 
        * cache variables per year , add the year as prefix or postfix for the variables  
        */
        private void CacheProccedHolidays(int year)
        {
            _memoryCache.Set(year + "cached", true);
            _memoryCache.Set("countriyWithMaxHolidays" + year, _countriyWithMaxHolidays);
            _memoryCache.Set("countriesWithUniqueVacations" + year, _countriesWithUniqueHolidays);
            _memoryCache.Set("daysAndCountryCodesMap" + year, _dayOfTheYearsMap);
            _memoryCache.Set("holidayiesPerMonth" + year, _holidayiesPerMonth);
        }
        /* 
       *  will return true if is is cached false otherwise
       *  if it the year is cached assign the global variable to the cached variables 
       */
        private bool GetCachedProccedHolidays(int year)
        {
            bool yearCached;
            bool isYearCached = _memoryCache.TryGetValue(year + "cached", out yearCached);
            if (isYearCached)
            {
                _memoryCache.TryGetValue("countriyWithMaxHolidays" + year, out _countriyWithMaxHolidays);
                _memoryCache.TryGetValue("countriesWithUniqueVacations" + year, out _countriesWithUniqueHolidays);
                _memoryCache.TryGetValue("daysAndCountryCodesMap" + year, out _dayOfTheYearsMap);
                _memoryCache.TryGetValue("holidayiesPerMonth" + year, out _holidayiesPerMonth);

            }
            return isYearCached;
        }
        /*
        * request countries time zones and maintian a dictionry of country and it's time zones 
        */
        private async Task<Dictionary<string, IList<string>>> RequestContryZonesAsync()
        {
            Dictionary<string, IList<string>> timeZones;

            //check if it is already cached 
            bool timeZoneExist = _memoryCache.TryGetValue("timeZones", out timeZones);
            if (timeZoneExist)
                return timeZones; //return cached values 

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

            var holidayIntervals = HolidayIntervalsProcessor.CalculateHolidayIntervals(timeZones, _dayOfTheYearsMap);
            var mergedHolidayIntervals = HolidayIntervalsProcessor.MergeHolidayIntervals(holidayIntervals);

            var longestHolidaySequance = mergedHolidayIntervals[mergedHolidayIntervals.Count - 1].End - mergedHolidayIntervals[mergedHolidayIntervals.Count - 1].Start;
            string formatedString = String.Format("The longest lasting sequence of holidays around the world is {0} hours",
              longestHolidaySequance);

            return formatedString;
        }
        #endregion HttpGet

    }
}
