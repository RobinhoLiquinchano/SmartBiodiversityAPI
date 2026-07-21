using System;

namespace SmartBiodiversityUtn.Helpers
{
    public static class DateExtensions
    {
        /// <summary>
        /// Convierte una fecha UTC a la hora local de Ecuador (America/Guayaquil, UTC-5).
        /// </summary>
        /// <param name="utcDate">Fecha en UTC (ej. DateTime.UtcNow o un valor leído de la BD).</param>
        /// <returns>La misma fecha convertida a hora de Ecuador.</returns>
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

        public static DateTime? ToEcuadorTime(this DateTime? utcDate)
        {
            if (!utcDate.HasValue) return null;

            return utcDate.Value.ToEcuadorTime();
        }
    }
}