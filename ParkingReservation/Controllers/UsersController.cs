using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingReservation.Models;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ParkingReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ParkingDbContext _context;

        public UsersController(ParkingDbContext context)
        {
            _context = context;
        }

        // POST: api/Users/Create
        [HttpPost("Create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("User details are incomplete.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.IsActive == 1))
            {
                return Conflict("User with the same email already exists.");
            }
            string messageBody = "Registration is successful. Please go to your email and complete the verification.";
            string IsEmailSent = SendEmailTOUser(user.Email, user.Name, messageBody, "Registration is successful.");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
        }

        // GET: api/Users/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var users = await _context.Users
                .Where(x => x.UserRole == "USER")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.UserRole,
                    u.PassportCode,
                    u.PassportSerial,
                    u.PhoneNumber,
                    u.Password
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/GetAll
        [HttpGet("GetAllLocalAdmins")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllLocalAdmins()
        {
            var users = await _context.Users
                .Where(x => x.UserRole == "LOCALADMIN")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.UserRole,
                    u.PassportCode,
                    u.PassportSerial,
                    u.PhoneNumber,
                    u.Password
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginRequest)
        {
            var user = await _context.Users
                .Where(u => u.Email == loginRequest.Email && u.Password == loginRequest.Password && u.IsActive == 1)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.UserRole
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized("Invalid credentials or inactive account.");
            }
            return Ok(user);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User model)
        {
            if (id != model.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Update properties
            existingUser.Name = model.Name;
            existingUser.PhoneNumber = model.PhoneNumber;
            existingUser.Email = model.Email;
            existingUser.Password = model.Password;
            existingUser.UserRole = model.UserRole;
            existingUser.PassportCode = model.PassportCode;
            existingUser.PassportSerial = model.PassportSerial;
            existingUser.IsActive = model.IsActive;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully!" });
        }


        // DELETE: api/Users/Delete/{id}
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserRole == "ADMIN")
            {
                return NotFound("User not found.");
            }
            
            user.IsActive = 0;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("User deactivated successfully.");
        }

        // PUT: api/Cars/Update/{id}
        [HttpPut("VerifyEmail/{emailid}")]
        public async Task<IActionResult> VerifyEmail(string emailid)
        {
            var user = await _context.Users.Where(x => x.Email == emailid).FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("No User found with this email address.");
            }

            if (user.IsActive == 1)
            {
                return Conflict("Email already verified.");
            }

            user.IsActive = 1;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully.");
        }

        private string SendEmailTOUser(string email, string message, string messageDetails, string NotificationFor)
        {
            MailMessage mail = new MailMessage();
            var FromEmailAddress = "uzpark.reservation@gmail.com";
            var HostAddress = "smtp.gmail.com";
            var FromMailAddressPwd = "ktgo ygyf wmpx tgkw";
            var HostAddressPort = 587;
            mail.To.Add(email);
            mail.From = new MailAddress(FromEmailAddress);
            mail.Subject = NotificationFor;
            string body = "";
            body += "<h3> Hi, " + message + "</h3>";
            body += "<br>";
            body += messageDetails;
            body += "<br>";
            body += "<br>";
            body += "Here is the verification link. <a href='http://localhost:3000/emailverification?email="+ email+"'>Click here...</a>  ";
            body += "<br>";
            body += "<br>";
            body += "From Parking System";
            mail.Body = body;
            mail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = HostAddress; //Or Your SMTP Server Address
            smtp.Port = Convert.ToInt32(HostAddressPort);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential
            (FromEmailAddress, FromMailAddressPwd);

            smtp.EnableSsl = true;
            smtp.Send(mail);
            return "1";

        }
    }
}
