using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctionsDemo
{
    public class RentalPayment : TableEntity
    {
        public RentalPayment(string partKey, string RowKey )
        {
            this.PartitionKey = partKey;
            this.RowKey = RowKey;
        }
        public RentalPayment(){}
        public string paymentId { get; set; }
        public string RentalUnitId { get; set; }
        public string CustomerId { get; set; }
        public string Email { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}