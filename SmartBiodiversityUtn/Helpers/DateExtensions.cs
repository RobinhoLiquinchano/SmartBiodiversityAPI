using System;

namespace SmartBiodiversityUtn.Helpers
{
    public static class DateExtensions
    {
        public static DateTime ToEcuadorTime(this DateTime utcDate)
        {
            try
            {
                // "America/Guayaquil" es el identificador IANA para Ecuador, compatible con Linux/Docker
                var ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");

                // Aseguramos que .NET sepa que la fecha original es UTC antes de convertirla
                if (utcDate.Kind == DateTimeKind.Unspecified)
                {
                    utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
                }

                return TimeZoneInfo.ConvertTimeFromUtc(utcDate, ecuadorTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                // Respaldo manual (UTC-5) en caso de que el contenedor de Docker no tenga las zonas horarias instaladas
                return utcDate.AddHours(-5);
            }
        }
    }
}