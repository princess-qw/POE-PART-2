
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("WeatherLogs")]
    public class WeatherLog
    {
        [Key]
        [Column("Weather_Log_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WeatherLogId { get; set; }

        [Column("Event_ID")]
        public int EventId { get; set; }

        [Column("Logged_At")]
        public DateTime LoggedAt { get; set; }

        [Column("Temperature_C", TypeName = "decimal(5,2)")]
        public decimal? TemperatureC { get; set; }

        [Column("Humidity_Percent", TypeName = "decimal(5,2)")]
        public decimal? HumidityPercent { get; set; }

        [StringLength(50)]
        [Column("Condition")]
        public string? Condition { get; set; }

        [Column("Wind_Speed_Kmh", TypeName = "decimal(6,2)")]
        public decimal? WindSpeedKmh { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Event? Event { get; set; }
    }
}
