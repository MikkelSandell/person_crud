using System;

namespace CrudApp.Backend.Util
{
    public static class PersonUtil
    {
        public static bool TryParseBirthDateFromCpr(string? cpr, out DateTime birthDate)
        {
            birthDate = default;
            if (string.IsNullOrWhiteSpace(cpr) || cpr.Length < 6)
            {
                return false;
            }

            var digits = cpr.Replace("-", string.Empty).Replace(" ", string.Empty);
            if (digits.Length < 6)
            {
                return false;
            }

            if (!int.TryParse(digits.Substring(0, 2), out var day)) return false;
            if (!int.TryParse(digits.Substring(2, 2), out var month)) return false;
            if (!int.TryParse(digits.Substring(4, 2), out var yearTwo)) return false;

            var currentYearTwo = DateTime.UtcNow.Year % 100;
            var century = yearTwo > currentYearTwo ? 1900 : 2000;
            var year = century + yearTwo;

            try
            {
                birthDate = new DateTime(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static int? CalculateAgeFromCpr(string? cpr)
        {
            if (!TryParseBirthDateFromCpr(cpr, out var birthDate))
            {
                return null;
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        public static string? GetStarSign(DateTime birthDate)
        {
            var (month, day) = (birthDate.Month, birthDate.Day);
            return month switch
            {
                1 => day <= 19 ? "Capricorn" : "Aquarius",
                2 => day <= 18 ? "Aquarius" : "Pisces",
                3 => day <= 20 ? "Pisces" : "Aries",
                4 => day <= 19 ? "Aries" : "Taurus",
                5 => day <= 20 ? "Taurus" : "Gemini",
                6 => day <= 20 ? "Gemini" : "Cancer",
                7 => day <= 22 ? "Cancer" : "Leo",
                8 => day <= 22 ? "Leo" : "Virgo",
                9 => day <= 22 ? "Virgo" : "Libra",
                10 => day <= 22 ? "Libra" : "Scorpio",
                11 => day <= 21 ? "Scorpio" : "Sagittarius",
                12 => day <= 21 ? "Sagittarius" : "Capricorn",
                _ => null
            };
        }
    }
}
