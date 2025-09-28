using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;
using static MyLittleRPG_ElGuendouz.DTOs.TuilesDtos;

namespace MyLittleRPG_ElGuendouz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuilesController : ControllerBase
    {
        private readonly MonsterContext _context;

        public TuilesController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/Tuiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tuile>>> GetTuiles()
        {
            return await _context.Tuiles.ToListAsync();
        }

        // GET: api/Tuiles/5/3
        [HttpGet("{x}/{y}")]
        public async Task<ActionResult<TuileAvecMonstresDto>> GetTuile(int x, int y)
        {
            var tuile = await _context.Tuiles.FindAsync(x, y);

            if (tuile == null)
            {
                tuile = GenerateTuile(x, y);
                _context.Tuiles.Add(tuile);
                await _context.SaveChangesAsync();
            }

            var monstres = await _context.InstanceMonstre
                .Where(m => m.PositionX == x && m.PositionY == y)
                .Select(m => new MonstreDto
                {
                    Id = m.monstreID,
                    Niveau = m.niveau,
                    Force = m.pointsVieMax, // ou pointsVieActuels
                    Defense = m.pointsVieMax,
                    HP = m.pointsVieActuels
                })
                .ToListAsync();

            var result = new TuileAvecMonstresDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                Type = tuile.Type,
                EstTraversable = tuile.EstTraversable,
                ImageURL = tuile.ImageURL,
                Monstres = monstres
            };

            return result;
        }

        private Tuile GenerateTuile(int positionX, int positionY)
        {
            var random = new Random();

            // Récupérez toutes les tuiles adjacentes
            var adjacents = _context.Tuiles
                .Where(t =>
                    (t.PositionX == positionX - 1 && t.PositionY == positionY) ||
                    (t.PositionX == positionX + 1 && t.PositionY == positionY) ||
                    (t.PositionX == positionX && t.PositionY == positionY - 1) ||
                    (t.PositionX == positionX && t.PositionY == positionY + 1))
                .ToList();

            // tuiles adjacentes
            int forestCount = adjacents.Count(t => t.Type == TypeTuile.FORET);
            int roadCount = adjacents.Count(t => t.Type == TypeTuile.ROUTE);
            int waterCount = adjacents.Count(t => t.Type == TypeTuile.EAU);

            // probabilités
            int roll = random.Next(1, 101); 
            TypeTuile type;
            bool estTraversable;

            if (roll <= 20 + forestCount * 10) // Augmente la probabilité de forêt
            {
                type = TypeTuile.FORET;
                estTraversable = true;
            }
            else if (roll <= 40 + roadCount * 10) // Augmente la probabilité de route
            {
                type = TypeTuile.ROUTE;
                estTraversable = true;
            }
            else if (roll <= 60 + waterCount * 10) // Augmente la probabilité d'eau
            {
                type = TypeTuile.EAU;
                estTraversable = false;
            }
            else if (roll <= 70)
            {
                type = TypeTuile.MONTAGNE;
                estTraversable = false;
            }
            else if (roll <= 85)
            {
                type = TypeTuile.HERBE;
                estTraversable = true;
            }
            else
            {
                type = TypeTuile.VILLE;
                estTraversable = true;
            }

            string imageURL = $"images/{type.ToString().ToLower()}.png";

            return new Tuile(positionX, positionY, type, estTraversable, imageURL);
        }

        // PUT: api/Tuiles/5/3
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{x}/{y}")]
        public async Task<IActionResult> PutTuile(int x, int y, Tuile tuile)
        {
            if (x != tuile.PositionX || y != tuile.PositionY)
            {
                return BadRequest();
            }

            _context.Entry(tuile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TuileExists(x, y))
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

        // POST: api/Tuiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tuile>> PostTuile(Tuile tuile)
        {
            _context.Tuiles.Add(tuile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TuileExists(tuile.PositionX,tuile.PositionY))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTuile", new { id = tuile.PositionX }, tuile);
        }

        // DELETE: api/Tuiles/5/3
        [HttpDelete("{x}/{y}")]
        public async Task<IActionResult> DeleteTuile(int x, int y)
        {
            var tuile = await _context.Tuiles.FindAsync(x, y);
            if (tuile == null)
            {
                return NotFound();
            }

            _context.Tuiles.Remove(tuile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TuileExists(int x, int y)
        {
            return _context.Tuiles.Any(e => e.PositionX == x && e.PositionY == y);
        }
    }
}
