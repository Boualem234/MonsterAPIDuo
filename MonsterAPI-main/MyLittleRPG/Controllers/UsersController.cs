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
        const int POSITION_DEPART_X = 10;
        const int POSITION_DEPART_Y = 10;
        const int PV_MAX = 100;
        const int STATS_MIN = 1;
        const int STATS_MAX = 101;

        private readonly MonsterContext _context;

        public UsersController(MonsterContext context)
        {
            _context = context;
        }


        // Il s'agit d'un endpoint pour la connexion d'un utilisateur.
        [HttpGet("Login/{email}/{password}")]
        public async Task<ActionResult<bool>> Login(string email, string password)
        {
            User? user = _context.User.FirstOrDefault(u => u.email == email);
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

        // Il s'agit d'un endpoint pour l'inscription d'un nouvel utilisateur.
        [HttpPost("Register/")]
        public async Task<ActionResult<User>> Register(User user)
        {
            // Validation des champs requis
            if (string.IsNullOrWhiteSpace(user.email))
                return BadRequest("Email is required");
            
            if (string.IsNullOrWhiteSpace(user.mdp))
                return BadRequest("Password is required");
            
            if (string.IsNullOrWhiteSpace(user.pseudo))
                return BadRequest("Pseudo is required");

            if (_context.User.Any(u => u.email == user.email))
                return BadRequest("This user already exists");

            User? lastUser = _context.User
                .OrderByDescending(u => u.utilisateurId)
                .FirstOrDefault();

            user.utilisateurId = (lastUser?.utilisateurId ?? 0) + 1;

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            Character? lastCharacter = _context.Character
                .OrderByDescending(c => c.idPersonnage)
                .FirstOrDefault();

            Character character = new Character()
            {
                idPersonnage = (lastCharacter?.idPersonnage ?? 0) + 1,
                nom = user.pseudo,
                niveau = 1,
                exp = 0,
                pv = new Random().Next(STATS_MIN, STATS_MAX),
                pvMax = PV_MAX,
                force = new Random().Next(STATS_MIN, STATS_MAX),
                def = new Random().Next(STATS_MIN, STATS_MAX),
                posX = POSITION_DEPART_X,
                posY = POSITION_DEPART_Y,
                utilisateurId = user.utilisateurId,
                dateCreation = DateTime.Now
            };

            _context.Character.Add(character);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // Il s'agit d'un endpoint pour la déconnexion d'un utilisateur.
        [HttpPost("Logout/{email}")]
        public async Task<ActionResult<bool>> Logout(string email)
        {
            (bool, User?) userConnected = _context.DoesExistAndConnected(email);
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
