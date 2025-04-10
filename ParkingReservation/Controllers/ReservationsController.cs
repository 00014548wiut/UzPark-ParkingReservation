using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public ReservationsController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Reservations/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            if (reservation == null || reservation.ClientId <= 0 || reservation.CarId <= 0 || reservation.LocationId <= 0)
            {
                return BadRequest("Invalid reservation details.");
            }

            // Получаем локацию
            Location location = await _context.Locations.FindAsync(reservation.LocationId);
            if (location == null)
            {
                return NotFound("Location not found.");
            }

            // Проверяем доступные места
            if (location.AvailableSpots <= 0)
            {
                return Conflict("No available parking spots at this location.");
            }

            // Создаём резервацию
            reservation.IsActive = true;
            reservation.IsCompleted = false;
            reservation.NoShow = false;

            _context.Reservations.Add(reservation);

            // Уменьшаем количество свободных мест
            location.AvailableSpots -= 1;
            _context.Entry(location).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateReservation), new { id = reservation.ReservationId }, reservation);
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllReservations()
        {
            var reservations = await _context.Reservations
                .Where(x => x.IsActive == true)
                .Join(
                    _context.Users, // The Users table
                    reservation => reservation.ClientId, // Foreign key in Reservations
                    user => user.Id, // Primary key in Users
                    (reservation, user) => new
                    {
                        reservation.ReservationId,
                        ClientName = user.Name,
                        reservation.CarId,
                        reservation.LocationId,
                        reservation.StartTime,
                        reservation.EndTime,
                        reservation.IsCompleted,
                        reservation.NoShow,
                        reservation.IsActive
                    }
                )
                .ToListAsync();

            return Ok(reservations);
        }

        // GET: api/Reservations/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations               
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }

            return Ok(reservation);
        }

        // PUT: api/Reservations/Complete/{id}
        [HttpPut("Complete/{id}")]
        public async Task<IActionResult> CompleteReservation(int id, [FromBody] Reservation reservation)
        {
            var reservations = await _context.Reservations.FindAsync(id);
            if (reservations == null)
            {
                return NotFound("Reservation not found.");
            }
            reservations.StartTime = reservation.StartTime;
            reservations.EndTime = reservation.EndTime;           

            await _context.SaveChangesAsync();

            return Ok("Reservation completed successfully.");
        }

        // DELETE: api/Reservations/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }

            reservation.IsActive = false; // Marking as inactive instead of physical deletion

            var location = await _context.Locations.Where(x => x.LocationId == reservation.LocationId).FirstOrDefaultAsync();
            if (location != null)
            {
                location.AvailableSpots = location.AvailableSpots + 1;
                _context.Entry(location).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok("Reservation marked as inactive.");
        }

        [HttpGet]
        [Route("GetReservationDetails")]
        public async Task<IActionResult> GetReservationDetails()
        {
            var reservationDetails = await (from r in _context.Reservations
                                            join c in _context.Cars on r.CarId equals c.CarId
                                            join l in _context.Locations on r.LocationId equals l.LocationId
                                            join u in _context.Users on r.ClientId equals u.Id
                                            join p in _context.Payments on r.ReservationId equals p.ReservationId into paymentGroup
                                            from pg in paymentGroup.DefaultIfEmpty() // Left join
                                            where r.IsActive == true
                                            select new
                                            {
                                                r.ReservationId,
                                                r.ClientId,
                                                r.CarId,
                                                r.LocationId,
                                                r.StartTime,
                                                r.EndTime,
                                                c.CarNumber,
                                                l.LocationName,
                                                l.Price,
                                                UserName = u.Name,
                                                u.PhoneNumber,
                                                u.Email,
                                                PaymentStatus = pg.PaymentMethod == null ? "PENDING" : "PAID"
                                            }).ToListAsync();

            return Ok(reservationDetails);
        }

        [HttpGet]
        [Route("GetReservationDetailsByClient/{clientId}")]
        public async Task<IActionResult> GetReservationDetailsByClient(int clientId)
        {
            var reservationDetails = await (from r in _context.Reservations
                                            join c in _context.Cars on r.CarId equals c.CarId
                                            join l in _context.Locations on r.LocationId equals l.LocationId
                                            join u in _context.Users on r.ClientId equals u.Id
                                            join p in _context.Payments on r.ReservationId equals p.ReservationId into paymentGroup
                                            from pg in paymentGroup.DefaultIfEmpty() // Left join
                                            where r.ClientId == clientId && r.IsActive == true
                                            select new
                                            {
                                                r.ReservationId,
                                                r.ClientId,
                                                r.CarId,
                                                r.LocationId,
                                                r.StartTime,
                                                r.EndTime,
                                                c.CarNumber,
                                                l.LocationName,
                                                l.Price,
                                                UserName = u.Name,
                                                u.PhoneNumber,
                                                u.Email,
                                                PaymentStatus = pg.PaymentMethod == null ? "PENDING" : "PAID"
                                            }).ToListAsync();

            if (reservationDetails == null || !reservationDetails.Any())
            {
                return NotFound(new { Message = "No reservations found for the specified ClientId." });
            }

            return Ok(reservationDetails);
        }
    }
}
