namespace backend.Model
{
    public class PurchaseDetailModel
    {
        public string SeatClass { get; set; }

        public int SeatCount { get; set; }

        public decimal Subtotal { get; set; }
        // derivated
        public decimal PricePerSeat
        {
            get
            {
                if (SeatCount > 0)
                    return Subtotal / SeatCount;
                return 0;
            }
        }    
    }
}