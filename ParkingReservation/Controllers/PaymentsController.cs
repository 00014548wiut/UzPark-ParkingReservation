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
    public class PaymentsController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public PaymentsController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Payments/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            if (payment == null || payment.ReservationId <= 0 || payment.Amount <= 0)
            {
                return BadRequest("Invalid payment details.");
            }

            payment.PaymentDate = DateTime.Now; // Sets the payment date to current time
            _context.Payments.Add(payment);

            if (payment.IsPenalty == 1)
            {
                Penalty penalty = new Penalty();
                penalty.Amount = 10;
                penalty.ReservationId = payment.ReservationId;
                penalty.AppliedTime = DateTime.Now;
                _context.Penalties.Add(penalty);
            }

            Reservation reservation = await _context.Reservations.FindAsync(payment.ReservationId);
            if(reservation != null)
            {
                reservation.IsCompleted = true;
                _context.Entry(reservation).State = EntityState.Modified;
            }

            var location = await _context.Locations.Where(x => x.LocationId == reservation.LocationId).FirstOrDefaultAsync();
            if (location != null)
            {
                location.AvailableSpots = location.AvailableSpots + 1;
                _context.Entry(location).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreatePayment), new { id = payment.PaymentId }, payment);
        }

        // GET: api/Payments/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments()
        {
            var payments = await (from payment in _context.Payments
                                  join reservation in _context.Reservations
                                      on payment.ReservationId equals reservation.ReservationId
                                  join user in _context.Users
                                      on reservation.ClientId equals user.Id
                                  join location in _context.Locations
                                      on reservation.LocationId equals location.LocationId
                                    join car in _context.Cars
                                    on reservation.CarId equals car.CarId
                                  where reservation.IsActive == true 
                                  select new
                                  {
                                      ClientName = user.Name,
                                      PaymentId = payment.PaymentId,
                                      Amount = location.Price,
                                      CarNumber = car.CarNumber, 
                                      LocationName = location.LocationName,
                                      StartTime = reservation.StartTime,
                                      EndTime = reservation.EndTime
                                  }).ToListAsync();
            return Ok(payments);
        }

        // GET: api/Payments/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound("Payment record not found.");
            }

            return Ok(payment);
        }

        // GET: api/Payments/Get/{clientId}
        [HttpGet("GetPaymentByclientId/{clientId}")]
        public async Task<ActionResult<Payment>> GetPaymentByclientId(int clientId)
        {
            var payments = await (from payment in _context.Payments
                                  join reservation in _context.Reservations
                                      on payment.ReservationId equals reservation.ReservationId
                                  join user in _context.Users
                                      on reservation.ClientId equals user.Id
                                  join location in _context.Locations
                                      on reservation.LocationId equals location.LocationId
                                  join car in _context.Cars
                                  on reservation.CarId equals car.CarId
                                  where reservation.IsActive == true && user.Id == clientId
                                  select new
                                  {
                                      ClientName = user.Name,
                                      PaymentId = payment.PaymentId,
                                      Amount = location.Price,
                                      CarNumber = car.CarNumber,
                                      LocationName = location.LocationName,
                                      StartTime = reservation.StartTime,
                                      EndTime = reservation.EndTime
                                  }).ToListAsync();

            if (payments == null)
            {
                return NotFound("Payment record not found.");
            }

            return Ok(payments);
        }

        // DELETE: api/Payments/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound("Payment record not found.");
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            return Ok("Payment record deleted successfully.");
        }
    }
}
