using RentAdvisor.Server.Database;
using RentAdvisor.Server.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Discussion_Forum.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDatabaseContext _context;
        private readonly UserManager<User> _userManager;

        public UsersController(AppDatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("{id}"), Authorize]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("current"), Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var obfuscatedUser = new ObfuscatedUser();
            obfuscatedUser.Id = userId;
            obfuscatedUser.Email = user.Email;
            obfuscatedUser.Name = user.UserName;
            obfuscatedUser.Score = user.Score;
            obfuscatedUser.Roles = await _userManager.GetRolesAsync(user);
            obfuscatedUser.Title = user.Title;
            obfuscatedUser.Badges = user.Badges;

            return Ok(obfuscatedUser);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }
            
            var obfuscatedUser = new ObfuscatedUser();
            obfuscatedUser.Id = user.Id;
            obfuscatedUser.Email = user.Email;
            obfuscatedUser.Name = user.UserName;
            obfuscatedUser.Score = user.Score;
            obfuscatedUser.Roles = await _userManager.GetRolesAsync(user);
            obfuscatedUser.Title = user.Title;
            obfuscatedUser.Badges = user.Badges;

            return Ok(obfuscatedUser);
        }

        [HttpPost("{id}/role"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserRole(Guid id, [FromBody] string roleToAdd)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.AddToRoleAsync(user, roleToAdd);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}/role"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserRole(Guid id, [FromBody] string roleToDelete)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            await _userManager.RemoveFromRoleAsync(user, roleToDelete);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //[HttpPut("{id}"), Authorize]
        //public async Task<IActionResult> UpdateUser(Guid id, User updatedUser)
        //{
        //    if (id.ToString() != updatedUser.Id)
        //    {
        //        return BadRequest("User id not matching.");
        //    }

        //    _context.Update(updatedUser);

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        [HttpGet("roles"), Authorize]
        public async Task<ActionResult<IEnumerable<IdentityRole>>> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id.ToString());
        }

        class ObfuscatedUser
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Score { get; set; }
            public IList<string> Roles { get; set; }
            public ICollection<Badge> Badges { get; set; }
            public Title Title { get; set; }
        }
    }
}