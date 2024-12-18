using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAdvisor.Server.Database;
using RentAdvisor.Server.Models.Entities;

namespace RentAdvisor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly AppDatabaseContext _context;

        public PropertiesController(AppDatabaseContext context)
        {
            _context = context;
        }

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
        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> PutProperty(Guid id, PropertyPutRequest propertyRequest)
        {
            if (id != propertyRequest.Id)
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
            };

            _context.Properties.Update(updatedProperty);

            _context.Entry(@updatedProperty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PropertyExists(id))
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

        // POST: api/Properties
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost, Authorize]
        public async Task<ActionResult<Property>> PostProperty(PropertyPostRequest @propertyRequest)
        {
            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = @propertyRequest.Name,
                Address = @propertyRequest.Address,
                Description = @propertyRequest.Description,
                Features = @propertyRequest.Features,
            };
            _context.Properties.Add(@property);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProperty", new { id = @property.Id }, @property);
        }

        // DELETE: api/Properties/5
        [HttpDelete("{id}"), Authorize(Roles = "Admin,Moderator,PropertyOwner")]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            var @property = await _context.Properties.FindAsync(id);
            if (@property == null)
            {
                return NotFound();
            }

            _context.Properties.Remove(@property);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PropertyExists(Guid id)
        {
            return _context.Properties.Any(e => e.Id == id);
        }

        public class PropertyPostRequest
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string[] Features { get; set; }
        }

        public class PropertyPutRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string[] Features { get; set; }
        }
    }
}
