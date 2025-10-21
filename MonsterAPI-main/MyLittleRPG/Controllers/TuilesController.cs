using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;
using MyLittleRPG_ElGuendouz.Services;
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

        // GET: api/Tuiles/5/3
        [HttpGet("{x}/{y}")]
        public async Task<ActionResult<TuileAvecMonstresDto>> GetTuile(int x, int y)
        {
            var tuile = await _context.Tuiles.FindAsync(x, y) ?? await CreateAndSaveTuileAsync(x, y);
            var monstre = await GetMonstreAsync(x, y);

            return new TuileAvecMonstresDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                Type = tuile.Type,
                EstTraversable = tuile.EstTraversable,
                ImageURL = tuile.ImageURL,
                Monstres = monstre
            };
        }

        private async Task<Tuile> CreateAndSaveTuileAsync(int x, int y)
        {
            var tuile = GenerateTuile(x, y);
            _context.Tuiles.Add(tuile);
            await _context.SaveChangesAsync();
            return tuile;
        }

        private async Task<MonstreDto?> GetMonstreAsync(int x, int y)
        {
            var instance = await _context.InstanceMonstre
                .FirstOrDefaultAsync(m => m.PositionX == x && m.PositionY == y);

            if (instance == null) return null;

            var monstreInstance = await _context.Monsters
                .FirstOrDefaultAsync(m => m.idMonster == instance.monstreID);

            return monstreInstance == null ? null : new MonstreDto
            {
                Id = instance.monstreID,
                Niveau = instance.niveau,
                Force = monstreInstance.forceBase,
                Defense = monstreInstance.defenseBase,
                HP = monstreInstance.pointVieBase,
                SpriteUrl = monstreInstance.spriteUrl
            };
        }

        private Tuile GenerateTuile(int positionX, int positionY)
        {
            var random = new Random();
            var adjacents = GetAdjacentTuiles(positionX, positionY);

            int forestCount = adjacents.Count(t => t.Type == TypeTuile.FORET);
            int roadCount = adjacents.Count(t => t.Type == TypeTuile.ROUTE);
            int waterCount = adjacents.Count(t => t.Type == TypeTuile.EAU);

            int roll = random.Next(1, 101);
            var (type, estTraversable) = DetermineTuileType(roll, forestCount, roadCount, waterCount);

            string imageURL = $"images/{type.ToString().ToLower()}.png";
            return new Tuile(positionX, positionY, type, estTraversable, imageURL);
        }

        private List<Tuile> GetAdjacentTuiles(int positionX, int positionY)
        {
            return _context.Tuiles
                .Where(t =>
                    (t.PositionX == positionX - 1 && t.PositionY == positionY) ||
                    (t.PositionX == positionX + 1 && t.PositionY == positionY) ||
                    (t.PositionX == positionX && t.PositionY == positionY - 1) ||
                    (t.PositionX == positionX && t.PositionY == positionY + 1))
                .ToList();
        }

        private (TypeTuile type, bool estTraversable) DetermineTuileType(int roll, int forestCount, int roadCount, int waterCount)
        {
            if (roll <= 20 + forestCount * 10) return (TypeTuile.FORET, true);
            if (roll <= 40 + roadCount * 10) return (TypeTuile.ROUTE, true);
            if (roll <= 60 + waterCount * 10) return (TypeTuile.EAU, false);
            if (roll <= 70) return (TypeTuile.MONTAGNE, false);
            if (roll <= 85) return (TypeTuile.HERBE, true);
            return (TypeTuile.VILLE, true);
        }

        private bool TuileExists(int x, int y)
        {
            return _context.Tuiles.Any(e => e.PositionX == x && e.PositionY == y);
        }
    }
}
