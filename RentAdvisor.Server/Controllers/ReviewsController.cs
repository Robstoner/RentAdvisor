using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAdvisor.Server.Database;
using RentAdvisor.Server.Models.Entities;

namespace RentAdvisor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDatabaseContext _context;
        private readonly UserManager<User> _userManager;

        public ReviewsController(AppDatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReview()
        {
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> PutReview(Guid id, Review review)
        {
            if (id != review.Id)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != review.UserId)
            {
                return Unauthorized();
            }

            _context.Reviews.Update(review);
            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize]
        public async Task<ActionResult<Review>> PostReview(ReviewPostRequest reviewRequest)
        {
            // check if the property already had a review from the user
            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.PropertyId == reviewRequest.PropertyId && r.UserId == reviewRequest.UserId);
            if (existingReview != null)
            {
                return Conflict("User already reviewed this property");
            }

            var review = new Review
            {
                Id = Guid.NewGuid(),
                Title = reviewRequest.Title,
                Description = reviewRequest.Description,
                UserId = reviewRequest.UserId,
                PropertyId = reviewRequest.PropertyId
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(reviewRequest.UserId);
            user.Score += 3;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReview", new { id = review.Id }, review);
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != review.UserId)
            {
                return Unauthorized();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            user.Score -= 3;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(Guid id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }

        public class ReviewPostRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string UserId { get; set; }
            public Guid PropertyId { get; set; }
        }
    }
}
