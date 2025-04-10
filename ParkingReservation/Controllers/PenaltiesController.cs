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
    public class PenaltiesController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public PenaltiesController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Penalties/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreatePenalty([FromBody] Penalty penalty)
        {
            if (penalty == null || penalty.ReservationId <= 0 || penalty.Amount <= 0)
            {
                return BadRequest("Invalid penalty details.");
            }

            penalty.AppliedTime = DateTime.Now; // Sets the applied time to the current time
            _context.Penalties.Add(penalty);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreatePenalty), new { id = penalty.PenaltyId }, penalty);
        }

        // GET: api/Penalties/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Penalty>>> GetAllPenalties()
        {
            var penalties = await _context.Penalties
                .ToListAsync();
            return Ok(penalties);
        }

        // GET: api/Penalties/Get/{id}
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<Penalty>> GetPenalty(int id)
        {
            var penalty = await _context.Penalties
                .FirstOrDefaultAsync(p => p.PenaltyId == id);

            if (penalty == null)
            {
                return NotFound("Penalty record not found.");
            }

            return Ok(penalty);
        }

        // DELETE: api/Penalties/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePenalty(int id)
        {
            var penalty = await _context.Penalties.FindAsync(id);
            if (penalty == null)
            {
                return NotFound("Penalty record not found.");
            }

            _context.Penalties.Remove(penalty);
            await _context.SaveChangesAsync();

            return Ok("Penalty record deleted successfully.");
        }
    }
}
