namespace backend.Model
{
    public class PaymentModel
    {
        public string Holder { get; set; }
        public string CardNumber { get; set; }
        public string Expiry { get; set; }
        public string Cvv { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}