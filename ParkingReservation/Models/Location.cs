namespace ParkingReservation.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public decimal Price { get; set; }
        public int TotalSpots { get; set; }
        public int AvailableSpots { get; set; }
        public int CreatedId { get; set; }
    }
}
