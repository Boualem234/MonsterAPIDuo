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
    public class MonstersController : ControllerBase
    {
        private readonly MonsterContext _context;

        public MonstersController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet("IsConnected")]
        public ActionResult<bool> IsConnected()
        {
            return Ok(true);
        }

        // GET: api/Monsters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Monster>>> GetMonsters()
        {
            return await _context.Monsters.ToListAsync();
        }

        // GET: api/Monsters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Monster>> GetMonster(int id)
        {
            var monster = await _context.Monsters.FindAsync(id);

            if (monster == null)
            {
                return NotFound();
            }

            return monster;
        }

        [HttpGet("random/{count}")]
        public async Task<ActionResult<IEnumerable<Monster>>> GetRandomMonsters(int count)
        {
            var totalCount = await _context.Monsters.CountAsync();
            if (totalCount == 0)
            {
                return NotFound("Aucun monstre trouvé.");
            }

            // si le user demande plus que ce qu'on a on renvoie tout
            if (count >= totalCount)
            {
                return await _context.Monsters.ToListAsync();
            }

            var allIds = await _context.Monsters
                .Select(m => m.idMonster)
                .ToListAsync();

            // tirage aléatoire de 300 IDs uniques
            var random = new Random();
            var randomIds = allIds
                .OrderBy(x => random.Next())
                .Take(count)
                .ToList();

            // on récupère ensuite les monstres correspondant à ces IDs
            var monsters = await _context.Monsters
                .Where(m => randomIds.Contains(m.idMonster))
                .ToListAsync();

            return Ok(monsters);
        }



        // PUT: api/Monsters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMonster(int id, Monster monster)
        {
            if (id != monster.idMonster)
            {
                return BadRequest();
            }

            _context.Entry(monster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MonsterExists(id))
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

        // POST: api/Monsters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Monster>> PostMonster(Monster monster)
        {
            _context.Monsters.Add(monster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonster", new { id = monster.idMonster }, monster);
        }

        // DELETE: api/Monsters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMonster(int id)
        {
            var monster = await _context.Monsters.FindAsync(id);
            if (monster == null)
            {
                return NotFound();
            }

            _context.Monsters.Remove(monster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MonsterExists(int id)
        {
            return _context.Monsters.Any(e => e.idMonster == id);
        }
    }
}
