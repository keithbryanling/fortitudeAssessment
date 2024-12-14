using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FortitudeAsia.Models
{
    public class Response
    {
        public int result { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Range(1, long.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public long? totalamount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Range(1, long.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public long? totaldiscount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Range(1, long.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public long? finalamount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? resultmessage { get; set; }
    }
}
