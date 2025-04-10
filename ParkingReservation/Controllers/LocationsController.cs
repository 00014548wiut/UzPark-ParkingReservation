using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParkingReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public LocationsController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Locations/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateLocation([FromBody] Location location)
        {
            if (location == null || string.IsNullOrEmpty(location.LocationName) || location.TotalSpots < 1)
            {
                return BadRequest("Invalid location details.");
            }

            location.AvailableSpots = location.TotalSpots; // Initialize available spots
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateLocation), new { id = location.LocationId }, location);
        }

        // GET: api/Locations/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Location>>> GetAllLocations()
        {
            var locations = await _context.Locations.ToListAsync();
            return Ok(locations);
        }

        // GET: api/Locations/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound("Location not found.");
            }

            return Ok(location);
        }

        // PUT: api/Locations/Update/{id}
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] Location updatedLocation)
        {
            if (id != updatedLocation.LocationId)
            {
                return BadRequest("Location ID mismatch.");
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound("Location not found.");
            }

            location.LocationName = updatedLocation.LocationName;
            location.TotalSpots = updatedLocation.TotalSpots;
            location.AvailableSpots = updatedLocation.AvailableSpots;
            location.Price = updatedLocation.Price; 

            _context.Entry(location).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Location updated successfully.");
        }

        // DELETE: api/Locations/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound("Location not found.");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return Ok("Location deleted successfully.");
        }

        // GET: api/Locations/GetAll
        [HttpGet]
        [Route("GetAllLocationByID")]
        public async Task<ActionResult<IEnumerable<Location>>> GetAllLocationByID(int id)
        {
            var locations = await _context.Locations.Where(x => x.CreatedId == id).ToListAsync();
            return Ok(locations);
        }
    }
}
