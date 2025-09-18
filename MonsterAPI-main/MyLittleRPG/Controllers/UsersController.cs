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
        public List<User> GetUsers()
        {
            return _context.User.ToList();
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
            if (_context.User.Any(u => u.email == user.email)) return BadRequest("This user already exists");
            user.utilisateurId = GetUsers().ToArray()[^1].utilisateurId + 1;
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpGet("Logout/{email}")]
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
