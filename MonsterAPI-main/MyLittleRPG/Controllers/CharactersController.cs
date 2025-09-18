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
    public class CharactersController : ControllerBase
    {
        private readonly MonsterContext _context;

        public CharactersController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet("Load/{email}")]
        public ActionResult<Character> LoadCharacter(string email)
        {
            (bool, Models.User) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return NotFound(); 
            Models.Character? character = _context.Character.FirstOrDefault(c => c.utilisateurId == userConnected.Item2.utilisateurId);
            if (character is null) return NotFound();
            else return Ok(character);
        }

        [HttpGet("Deplacement/{x}/{y}/{email}")]
        public async Task<ActionResult<Character>> Deplacer(int x, int y, string email)
        {
            (bool, Models.User) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return NotFound();
            Models.Character? character = _context.Character.FirstOrDefault(c => c.utilisateurId == userConnected.Item2.utilisateurId);
            if (character is null) return NotFound();
            else
            {
                await _context.Character
                    .Where(c => c.utilisateurId == userConnected.Item2.utilisateurId)
                    .ExecuteUpdateAsync(c => c.SetProperty(cc => cc.posX, x).SetProperty(cc => cc.posY, y));
                await _context.SaveChangesAsync();
                return Ok(character);
            }
        }

        // GET: api/Characters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharacter()
        {
            return await _context.Character.ToListAsync();
        }

        // GET: api/Characters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Character>> GetCharacter(int id)
        {
            var character = await _context.Character.FindAsync(id);

            if (character == null)
            {
                return NotFound();
            }

            return Ok(character);
        }

        // PUT: api/Characters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacter(int id, Character character)
        {
            if (id != character.idPersonnage)
            {
                return BadRequest();
            }

            _context.Entry(character).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacterExists(id))
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

        // POST: api/Characters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Character>> PostCharacter(Character character)
        {
            _context.Character.Add(character);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCharacter", new { id = character.idPersonnage }, character);
        }

        // DELETE: api/Characters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacter(int id)
        {
            var character = await _context.Character.FindAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            _context.Character.Remove(character);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CharacterExists(int id)
        {
            return _context.Character.Any(e => e.idPersonnage == id);
        }
    }
}
