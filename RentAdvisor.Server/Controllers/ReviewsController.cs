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
            try
            {
                // Filter reviews by the user's score
                var reviews = await _context.Reviews
                    .OrderByDescending(r => r.User.Score)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return NotFound(new { Message = "No reviews found for the given score threshold." });
                }

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred while fetching reviews.", Details = ex.Message });
            }
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
        public async Task<IActionResult> PutReview(Guid id, ReviewPutRequest reviewrequest)
        {
            if (id != reviewrequest.Id)
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

            if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != reviewrequest.UserId)
            {
                return Unauthorized();
            }

            var updatedReview = new Review
            {
                Id = reviewrequest.Id,
                Title = reviewrequest.Title,
                Description = reviewrequest.Description,
                UserId = reviewrequest.UserId,
                PropertyId = reviewrequest.PropertyId
            };

            _context.Reviews.Update(updatedReview);
            _context.Entry(updatedReview).State = EntityState.Modified;

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
            if (user != null)
            {
                user.Score += 3;
                _context.Users.Update(user);
                checkUpdateTitle(user.Id);
                checkReviewBadges(user.Id);
            }
            
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

            var reviewUser = await _context.Users.FindAsync(review.UserId);
            if (reviewUser != null)
            {
                reviewUser.Score -= 3;
                _context.Users.Update(reviewUser);
                checkUpdateTitle(reviewUser.Id);
            }
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(Guid id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }

        private void checkUpdateTitle(string UserId)
        {
            var user = _context.Users.Find(UserId);
            if (user == null)
            {
                return;
            }
            var titles = _context.Titles.ToList();
            for (int i = 0; i < titles.Count; i++)
            {
                if (user.Score >= titles[i].RequiredPoints)
                {
                    user.TitleId = titles[i].Id;
                }
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        private void checkReviewBadges(string UserId)
        {
            var user = _context.Users.Find(UserId);
            if (user == null)
            {
                return;
            }
            var reviews = _context.Reviews.Where(r => r.UserId == UserId).ToList();
            var badges = _context.Badges.ToList();
            foreach (var badge in badges)
            {
                if (badge.Name.Equals("First Review") && reviews.Count >= 1)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("Fifth Review") && reviews.Count >= 5)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("Tenth Review") && reviews.Count >= 10)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("50th Review") && reviews.Count >= 50)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("100th Review") && reviews.Count >= 100)
                {
                    user.Badges.Add(badge);
                }
            }
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public class ReviewPutRequest
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string UserId { get; set; }
            public Guid PropertyId { get; set; }
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
