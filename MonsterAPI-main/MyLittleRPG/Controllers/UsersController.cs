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
    public class UsersController : ControllerBase
    {
        private readonly MonsterContext _context;

        public UsersController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/Monsters
        [HttpGet]
        public User[] GetUsers()
        {
            return _context.User.ToArray();
        }

        [HttpGet("Login/{email}/{password}")]
        public async Task<ActionResult<bool>> Login(string email, string password)
        {
            Models.User? user = _context.User.FirstOrDefault(u => u.email == email);
            if (user is null) return NotFound(false);
            if (user.mdp != password) return Unauthorized(false);
            else
            {
                await _context.User
                    .Where(u => u.email == email)
                    .ExecuteUpdateAsync(u => u.SetProperty(uu => uu.isConnected, true));
                await _context.SaveChangesAsync();
                return Ok(true);
            }
        }

        [HttpPost("Register/")]
        public async Task<ActionResult<User>> Register(User user)
        {
            if (_context.User.Any(u => u.email == user.email))
                return BadRequest("This user already exists");

            var lastUser = _context.User
                .OrderByDescending(u => u.utilisateurId)
                .FirstOrDefault();

            user.utilisateurId = (lastUser?.utilisateurId ?? 0) + 1;

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            var lastCharacter = _context.Character
                .OrderByDescending(c => c.idPersonnage)
                .FirstOrDefault();

            Character character = new Character()
            {
                idPersonnage = (lastCharacter?.idPersonnage ?? 0) + 1,
                nom = user.pseudo,
                niveau = 1,
                exp = 0,
                pv = new Random().Next(1, 101),
                pvMax = 100,
                force = new Random().Next(1, 101),
                def = new Random().Next(1, 101),
                posX = 10,
                posY = 10,
                utilisateurId = user.utilisateurId,
                dateCreation = DateTime.Now
            };

            _context.Character.Add(character);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("Logout/{email}")]
        public async Task<ActionResult<bool>> Logout(string email)
        {
            (bool, Models.User) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return NotFound();
            else
            {
                await _context.User
                    .Where(u => u.email == email)
                    .ExecuteUpdateAsync(u => u.SetProperty(uu => uu.isConnected, false));
                await _context.SaveChangesAsync();
                return Ok(true);
            }
        }
    }
}
