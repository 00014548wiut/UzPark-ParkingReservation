namespace ParkingReservation.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public int LocationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public bool NoShow { get; set; }

    }
}
