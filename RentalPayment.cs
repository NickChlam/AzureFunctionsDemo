using System.ComponentModel.DataAnnotations;

namespace AzureFunctionsDemo
{
    public class RentalPayment 
    {
        
        public RentalPayment(){}
        public int paymentId { get; set; }
        [Required]
        [StringLength(4)]
        public string RentalUnitId { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        [StringLength(30)]
        public string CustomerName {get; set;}
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^\d+\.\d{0,2}$")]
        [Range(0, 9999999999999999.99)]
        public decimal PaymentAmount { get; set; }
    }
}