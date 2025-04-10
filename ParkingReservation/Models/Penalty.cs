namespace ParkingReservation.Models
{
    public class Penalty
    {
        public int PenaltyId { get; set; }
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime AppliedTime { get; set; }
    }
}
