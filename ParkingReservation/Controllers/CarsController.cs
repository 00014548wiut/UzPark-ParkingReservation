using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public CarsController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Cars/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateCar([FromBody] Car car)
        {
            if (car == null || string.IsNullOrEmpty(car.CarNumber))
            {
                return BadRequest("Car details are incomplete.");
            }

            if (await _context.Cars.AnyAsync(c => c.CarNumber == car.CarNumber && c.ClientId == car.ClientId))
            {
                return Conflict("Car with the same number already exists for this client.");
            }

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateCar), new { id = car.CarId }, car);
        }

        // GET: api/Cars/GetByClient/{clientId}
        [HttpGet("GetByClient/{clientId}")]
        public async Task<ActionResult<IEnumerable<Car>>> GetCarsByClient(int clientId)
        {
            var carsWithUser = await _context.Cars
            .Where(c => c.ClientId == clientId)
            .Join(
                _context.Users,
                car => car.ClientId,
                user => user.Id,
                (car, user) => new
                {
                    car.CarId,
                    car.CarNumber,
                    car.ClientId,
                    UserName = user.Name
                })
            .ToListAsync();

                if (!carsWithUser.Any())
                {
                    return NotFound("No cars found for this client.");
                }

                return Ok(carsWithUser);
        }

        // PUT: api/Cars/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car updatedCar)
        {
            if (id != updatedCar.CarId || string.IsNullOrEmpty(updatedCar.CarNumber))
            {
                return BadRequest("Car ID mismatch or incomplete car details.");
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound("Car not found.");
            }

            car.CarNumber = updatedCar.CarNumber;
            car.ClientId = updatedCar.ClientId;

            _context.Entry(car).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Car updated successfully.");
        }

        // DELETE: api/Cars/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound("Car not found.");
            }

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok("Car deleted successfully.");
        }
    }
}
