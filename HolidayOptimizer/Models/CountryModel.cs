using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayOptimizer.Models
{
    public class CountryModel : ICountryModel
    {
        public string CountryCode { get; set; }
        public int Holidays { get; set; }
        public CountryModel()
        {

        }
        public CountryModel(string countryCode, int holidays)
        {
            CountryCode = countryCode;
            Holidays = holidays;
        }
    }
}
