using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ParkingReservation.Models
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Penalty> Penalties { get; set; }       

        public DbSet<Payment> Payments { get; set; }
    }
}