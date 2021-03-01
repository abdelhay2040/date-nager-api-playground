namespace HolidayOptimizer.Models
{
    public interface ICountryModel
    {
        string CountryCode { get; set; }
        int Holidays { get; set; }
    }
}