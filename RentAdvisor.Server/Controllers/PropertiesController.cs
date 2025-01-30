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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RentAdvisor.Server.Database;
using RentAdvisor.Server.Models.Entities;
using Property = RentAdvisor.Server.Models.Entities.Property;

namespace RentAdvisor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly AppDatabaseContext _context;
        private readonly UserManager<User> _userManager;

        public PropertiesController(AppDatabaseContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Property
        // GET: api/Properties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Property>>> GetProperty()
        {
            return await _context.Properties.ToListAsync();
        }

        // GET: api/Properties/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> GetProperty(Guid id)
        {
            var @property = await _context.Properties.FindAsync(id);

            if (@property == null)
            {
                return NotFound();
            }

            return @property;
        }

        // GET: api/Properties/5/Reviews
        [HttpGet("{id}/Reviews")]
        public async Task<ActionResult<IEnumerable<Review>>> GetPropertyReviews(Guid id)
        {
            var @property = await _context.Properties.FindAsync(id);
            if (@property == null)
            {
                return NotFound();
            }
            return await _context.Reviews.Where(r => r.PropertyId == id).ToListAsync();
        }

        // PUT: api/Properties/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{propertyId}"), Authorize]
        public async Task<IActionResult> PutProperty(Guid propertyId, PropertyPutRequest propertyRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }
                var userRoles = await _userManager.GetRolesAsync(user);

                if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != propertyRequest.UserId)
                {
                    return Unauthorized();
                }

                if (propertyId != propertyRequest.Id)
                {
                    return BadRequest();
                }

                var updatedProperty = new Property
                {
                    Id = propertyRequest.Id,
                    Name = propertyRequest.Name,
                    Address = propertyRequest.Address,
                    Description = propertyRequest.Description,
                    Features = propertyRequest.Features,
                    UserId = propertyRequest.UserId
                };

                _context.Properties.Update(updatedProperty);

                _context.Entry(@updatedProperty).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }         
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (!PropertyExists(propertyId))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(new { Message = "An error occurred while updating the property.", Details = ex.Message });
                }
            }
            return NoContent();
        }

        // POST: api/Properties
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize]
        public async Task<ActionResult<Property>> PostProperty(PropertyPostRequest @propertyRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Data validation
                if (string.IsNullOrWhiteSpace(propertyRequest.Name))
                {
                    return BadRequest(new { Message = "Property name is required." });
                }

                if (string.IsNullOrWhiteSpace(propertyRequest.Address))
                {
                    return BadRequest(new { Message = "Property address is required." });
                }

                // Validate UserId
                var user = await _context.Users.FindAsync(propertyRequest.UserId);
                if (user == null)
                {
                    return BadRequest(new { Message = "Invalid UserId. The user does not exist." });
                }

                // Ensure property name is unique for the user
                var existingProperty = await _context.Properties
                    .Where(p => p.UserId == propertyRequest.UserId && p.Name == propertyRequest.Name)
                    .FirstOrDefaultAsync();
                if (existingProperty != null)
                {
                    return BadRequest(new { Message = "A property with the same name already exists for this user." });
                }

                var property = new Property
                {
                    Id = Guid.NewGuid(),
                    Name = @propertyRequest.Name,
                    Address = @propertyRequest.Address,
                    Description = @propertyRequest.Description,
                    Features = @propertyRequest.Features,
                    UserId = @propertyRequest.UserId
                };
                _context.Properties.Add(@property);
                await _context.SaveChangesAsync();

                await UploadPhotos(property.Id, @propertyRequest.UserId, @propertyRequest.Photos);

                user.Score += 6;
                _context.Users.Update(user);
                checkUpdateTitle(user.Id);
                checkPropertiesBadge(user.Id);

                await transaction.CommitAsync();

                return CreatedAtAction("GetProperty", new { id = @property.Id }, @property);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return BadRequest(new { Message = "An error occurred while creating the property.", Details = ex.Message });
            }            
        }

        // DELETE: api/Properties/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin,Moderator,PropertyOwner")]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var @property = await _context.Properties.FindAsync(id);
                if (@property == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }
                var userRoles = await _userManager.GetRolesAsync(user);

                if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != @property.UserId)
                {
                    return Unauthorized();
                }

                List<PropertyPhotos>? photos = _context.PropertiesPhotos.Where(photo => photo.PropertyId == id).ToList();

                foreach (PropertyPhotos photo in photos)
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), photo.PhotoPath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                _context.PropertiesPhotos.RemoveRange(photos);
                _context.Properties.Remove(@property);

                var propertyUser = await _context.Users.FindAsync(@property.UserId);
                if(propertyUser != null)
                {
                    propertyUser.Score -= 6;
                    _context.Users.Update(propertyUser);
                    checkUpdateTitle(propertyUser.Id);
                }                  
                                
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return BadRequest(new { Message = "An error occurred while deleting the property.", Details = ex.Message });
            }
        }

        private bool PropertyExists(Guid id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }
        #endregion
        #region Photos
        // GET: api/Properties/5
        [HttpGet("Photos/{photoId}")]
        public async Task<ActionResult> GetPropertyPhoto(Guid photoId)
        {
            string? photoPath = await _context.PropertiesPhotos.Where(p => p.Id == photoId).Select(p => p.PhotoPath).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(photoPath))
            {
                return NotFound("Photo not found.");
            }

            string? fileExtension = Path.GetExtension(photoPath)?.ToLower();
            string mimeType = fileExtension switch
            {
                ".jpeg" => "image/jpeg",
                ".jpg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileStream = new FileStream(photoPath, FileMode.Open, FileAccess.Read);
            return File(fileStream, mimeType);
        }

        [HttpPost("Photos/{propertyId}"), Authorize]
        public async Task<IActionResult> UploadPhotos(Guid propertyId, string UserId, List<IFormFile> photos)
        {
            try
            {
                if (photos == null || photos.Count == 0)
                {
                    throw new Exception("No files were uploaded.");
                }

                var property = await _context.Properties.FindAsync(propertyId);
                if (property == null)
                {
                    throw new Exception("Property not found.");
                }

                string photosPath = Path.Combine(Directory.GetCurrentDirectory(), "Photos");

                if (!Directory.Exists(photosPath))
                {
                    Directory.CreateDirectory(photosPath);
                }

                var uploadedPhotos = new List<PropertyPhotos>();

                foreach (IFormFile photo in photos)
                {
                    if (photo.Length > 0)
                    {
                        string[] allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
                        string extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(extension))
                        {
                            throw new Exception("Unsupported file type.");
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        string filePath = Path.Combine(photosPath, uniqueFileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        PropertyPhotos propertyPhoto = new PropertyPhotos
                        {
                            Id = Guid.NewGuid(),
                            PhotoPath = Path.Combine("Photos", uniqueFileName),
                            PropertyId = propertyId,
                            UserId = UserId
                        };
                        uploadedPhotos.Add(propertyPhoto);
                    }
                }

                _context.PropertiesPhotos.AddRange(uploadedPhotos);
                await _context.SaveChangesAsync();
                return Ok(new {Message = "Files uploaded and saved successfully!", Photos = uploadedPhotos });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred while creating the photos.", Details = ex.Message });
            }
        }

        [HttpDelete("Photos/{photoId}"), Authorize]
        public async Task<IActionResult> DeletePhoto(Guid photoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                
                var photo = await _context.PropertiesPhotos.FindAsync(photoId);
                if (photo == null)
                {
                    throw new Exception("Photo not found.");
                }

                if (!userRoles.Contains("Admin") && !userRoles.Contains("Moderator") && userId != photo.UserId)
                {
                    throw new Exception("User unauthorized");
                }

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), photo.PhotoPath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                _context.PropertiesPhotos.Remove(photo);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new {Message = "Photo deleted successfully!" });
            }
            catch(Exception ex) 
            {
                await transaction.RollbackAsync();
                return BadRequest(new { Message = "An error occurred while creating the photos.", Details = ex.Message });
            }
        }
        #endregion
        #region Tools

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

        private void checkPropertiesBadge(string UserId)
        {
            var user = _context.Users.Find(UserId);
            if (user == null)
            {
                return;
            }
            var userPropertiesCount = _context.Properties.Where(p => p.UserId == UserId).Count();
            var badges = _context.Badges.ToList();

            foreach (var badge in badges)
            {
                if (badge.Name.Equals("First Property") && userPropertiesCount >= 1)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("Fifth Property") && userPropertiesCount >= 5)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("Tenth Property") && userPropertiesCount >= 10)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("50th Property") && userPropertiesCount >= 50)
                {
                    user.Badges.Add(badge);
                }
                else if (badge.Name.Equals("100th Property") && userPropertiesCount >= 100)
                {
                    user.Badges.Add(badge);
                }
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public class PropertyPostRequest
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string[] Features { get; set; }
            public List<IFormFile>? Photos { get; set; }
            public string UserId { get; set; }
        }

        public class PropertyPutRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string[] Features { get; set; }
            public string UserId { get; set; }
        }
        #endregion
    }
}
