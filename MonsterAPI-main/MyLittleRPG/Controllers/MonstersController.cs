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

        // POST: api/Monsters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Monster>> PostMonster(Monster monster)
        {
            _context.Monsters.Add(monster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMonster", new { id = monster.idMonster }, monster);
        }

        private bool MonsterExists(int id)
        {
            return _context.Monsters.Any(e => e.idMonster == id);
        }
    }
}
