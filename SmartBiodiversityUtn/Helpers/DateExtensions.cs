using System;

namespace SmartBiodiversityUtn.Helpers
{
    public static class DateExtensions
    {
      
        public static DateTime ToEcuadorTime(this DateTime utcDate)
        {
            try
            {
                var ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");

                if (utcDate.Kind == DateTimeKind.Unspecified)
                {
                    utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
                }

                return TimeZoneInfo.ConvertTimeFromUtc(utcDate, ecuadorTimeZone);
            }
            catch (TimeZoneNotFoundException)
            {
                return utcDate.AddHours(-5);
            }
        }

        public static DateTime? ToEcuadorTime(this DateTime? utcDate)
        {
            if (!utcDate.HasValue) return null;

            return utcDate.Value.ToEcuadorTime();
        }

        public static DateTime ToUtc(this DateTime localDate)
        {
            if (localDate.Kind == DateTimeKind.Utc)
                return localDate;

            // Si es Unspecified, asumimos que viene en hora de Ecuador
            var ecuadorTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");
            var unspecified = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, ecuadorTimeZone);
        }

        public static DateTime? ToUtc(this DateTime? localDate)
        {
            if (!localDate.HasValue) return null;
            return localDate.Value.ToUtc();
        }
    }
}