# date-nager-api-playground
Exploring the popular Nager.Date project to query the worldwide public holidays.

Used .Net core to explore the Nager.Date and installed the package via [nugget](https://www.nuget.org/packages/Nager.Date).
The following country codes were used to explore the Nager Date API: `["AD", "AR", "AT", "AU", "AX", "BB", "BE", "BG", "BO", "BR", "BS", "BW", "BY", "BZ", "CA", "CH", "CL", "CN", "CO", "CR", "CU", "CY", "CZ", "DE", "DK", "DO", "EC", "EE", "EG", "ES", "FI", "FO", "FR", "GA", "GB", "GD", "GL", "GR", "GT", "GY", "HN", "HR", "HT", "HU", "IE", "IM", "IS", "IT", "JE", "JM", "LI", "LS", "LT", "LU", "LV", "MA", "MC", "MD", "MG", "MK", "MT", "MX", "MZ", "NA", "NI", "NL", "NO", "NZ", "PA", "PE", "PL", "PR", "PT", "PY", "RO", "RS", "RU", "SE", "SI", "SJ", "SK", "SM", "SR", "SV", "TN", "TR", "UA", "US", "UY", "VA", "VE", "ZA"]`

Used Swagger to document the exploration endpoints. 

Used [RestCountries.eu](https://restcountries.eu/) API to lookup timezone information per country.

### How to Run?
<todo>how to run</todo>

### Exploration
Exploration of the Nager.Date was completed by developed the following /get endpoints:
* `/GetCountryWithMaxHolidays` gets the country that had the most holidays, for a given year.
* `/GetMonthWithMaxHolidays` get the month that had most holidays globally, for a given year.
* `/GetCountryWithMaxUniqueHolidays` retrieve the country that had the most unique holidays (days that no other country had a holiday), for a given year.
* `GetLongestLastingHolidaysSequence` retrieves the longest lasting sequence of holidays globally using [Sweep Line Algorithm](https://en.wikipedia.org/wiki/Sweep_line_algorithm).

### Features
* Caching of results.
* Handled possible Errors/Exceptions e.g. Years out of range [1900,2050].
* Unit-tests using xUnit.
* `Year` is an input to any of the exploration endpoints
