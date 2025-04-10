namespace ParkingReservation.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserRole { get; set; }
        public string PassportCode { get; set; }
        public string PassportSerial { get; set; }
        public int IsActive { get; set; } = 1;

    }
}
