using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementSystem.Application;
using TaskManagementSystem.Dto;
using TaskManagementSystem.Data; // Предполагается, что у вас есть модель User

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public UsersController(IUserService userService, ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var result = await _userService.RegisterAsync(userDto);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetUser), new { id = result.User.Id }, result.User);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _userService.AuthenticateAsync(loginDto);
            if (token != null)
            {
                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid credentials");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpGet("with-email")]
        public IActionResult GetUsersWithEmail()
        {
            var usersWithEmail = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .Select(u => new { u.Id, u.Username, u.Email })
                .ToList();

            return Ok(usersWithEmail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest("User ID mismatch");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = userDto.Name;
            user.Email = userDto.Email;
            // Обновите другие поля по мере необходимости

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}
