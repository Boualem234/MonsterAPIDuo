using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstanceMonstresController : ControllerBase
    {
        private readonly MonsterContext _context;

        public InstanceMonstresController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/InstanceMonstres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InstanceMonstre>>> GetInstanceMonstre()
        {
            return await _context.InstanceMonstre.ToListAsync();
        }

        // GET: api/InstanceMonstres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InstanceMonstre>> GetInstanceMonstre(int id)
        {
            var instanceMonstre = await _context.InstanceMonstre.FindAsync(id);

            if (instanceMonstre == null)
            {
                return NotFound();
            }

            return instanceMonstre;
        }

        // PUT: api/InstanceMonstres/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInstanceMonstre(int id, InstanceMonstre instanceMonstre)
        {
            if (id != instanceMonstre.PositionX)
            {
                return BadRequest();
            }

            _context.Entry(instanceMonstre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InstanceMonstreExists(id))
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

        // POST: api/InstanceMonstres
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InstanceMonstre>> PostInstanceMonstre(InstanceMonstre instanceMonstre)
        {
            _context.InstanceMonstre.Add(instanceMonstre);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (InstanceMonstreExists(instanceMonstre.PositionX))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetInstanceMonstre", new { id = instanceMonstre.PositionX }, instanceMonstre);
        }

        // DELETE: api/InstanceMonstres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstanceMonstre(int id)
        {
            var instanceMonstre = await _context.InstanceMonstre.FindAsync(id);
            if (instanceMonstre == null)
            {
                return NotFound();
            }

            _context.InstanceMonstre.Remove(instanceMonstre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InstanceMonstreExists(int id)
        {
            return _context.InstanceMonstre.Any(e => e.PositionX == id);
        }
    }
}
