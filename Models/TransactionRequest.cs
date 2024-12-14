using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FortitudeAsia.Models
{
    public class TransactionRequest
    {
        [Required]
        public string partnerkey { get; set; }
        [Required]
        public string partnerrefno { get; set; }
        [Required]
        public string partnerpassword { get; set; }
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public long totalamount { get; set; }
        public List<ItemDetail> ?items { get; set; }
        [Required]
        public DateTime timestamp { get; set; }
        [Required]
        public string sig { get; set; }
    }

    public class ItemDetail
    {
        [Required]
        public string partneritemref { get; set; }
        [Required]
        public string name { get; set; }
        [Range(1, 5, ErrorMessage = "Quantity must be between 1 and 5.")]
        public int? qty { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = "Unit price must be a positive value.")]
        public long? unitprice { get; set; }
    }
}
