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
        private const int MAX_POS_X = 50;
        private const int MAX_POS_Y = 50;
        private const int RANDOM_MAX = 101;
        private const int EXPLORATION_RANGE = 2;

        const int BASE_FORET = 20;
        const int BASE_ROUTE = 40;
        const int BASE_EAU = 60;
        const int BASE_MONTAGNE = 70;
        const int BASE_HERBE = 85;

        const int BONUS_FORET = 10;
        const int BONUS_ROUTE = 10;
        const int BONUS_EAU = 10;

        private readonly MonsterContext _context;

        public TuilesController(MonsterContext context)
        {
            _context = context;
        }

        // endpoint pour obtenir une tuile à des coordonnées spécifiques
        [HttpGet("{x}/{y}")]
        public async Task<ActionResult<TuileAvecMonstresDto>> GetTuile(int x, int y, [FromQuery] string? email = null)
        {
            // Vérifier les limites de la carte
            if (x < 0 || x > MAX_POS_X || y < 0 || y > MAX_POS_Y)
            {
                return StatusCode(StatusCodes.Status416RangeNotSatisfiable, "Coordonnées hors limites (0 à 50).");
            }

            // Si un email est fourni, valider l'authentification et la distance
            if (!string.IsNullOrWhiteSpace(email))
            {
                // Vérifier que l'utilisateur existe et est connecté
                User? user = await _context.User.FirstOrDefaultAsync(u => u.email == email);
                if (user == null || !user.isConnected)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Utilisateur non trouvé ou non connecté.");
                }

                // Charger le personnage de l'utilisateur
                Character? character = await _context.Character.FirstOrDefaultAsync(c => c.utilisateurId == user.utilisateurId);
                if (character == null)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Personnage non trouvé.");
                }

                // Calculer la distance entre le personnage et la tuile cible (distance de Chebyshev)
                int distanceX = Math.Abs(character.posX - x);
                int distanceY = Math.Abs(character.posY - y);
                int distance = Math.Max(distanceX, distanceY);

                // Vérifier que la tuile est à portée d'exploration
                if (distance > EXPLORATION_RANGE)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, "Tuile trop éloignée pour être explorée.");
                }
            }

            // Récupérer ou créer la tuile
            Tuile tuile = await _context.Tuiles.FindAsync(x, y) ?? await CreateAndSaveTuileAsync(x, y);
            MonstreDto? monstre = await GetMonstreAsync(x, y);

            return new TuileAvecMonstresDto
            {
                PositionX = tuile.PositionX,
                PositionY = tuile.PositionY,
                Type = tuile.Type,
                EstTraversable = tuile.EstTraversable,
                ImageURL = tuile.ImageURL,
                Monstres = monstre!
            };
        }

        // Crée une nouvelle tuile, la sauvegarde dans la base de données, et la retourne
        private async Task<Tuile> CreateAndSaveTuileAsync(int x, int y)
        {
            Tuile tuile = GenerateTuile(x, y);
            _context.Tuiles.Add(tuile);
            await _context.SaveChangesAsync();
            return tuile;
        }

        // Récupère les informations du monstre à une position donnée
        private async Task<MonstreDto?> GetMonstreAsync(int x, int y)
        {
            InstanceMonstre? instance = await _context.InstanceMonstre
                .FirstOrDefaultAsync(m => m.PositionX == x && m.PositionY == y);

            if (instance == null) return null;

            Monster? monstreInstance = await _context.Monsters
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


        // Génère une tuile basée sur les tuiles adjacentes
        private Tuile GenerateTuile(int positionX, int positionY)
        {
            Random random = new Random();
            List<Tuile> adjacents = GetAdjacentTuiles(positionX, positionY);

            int forestCount = adjacents.Count(t => t.Type == TypeTuile.FORET);
            int roadCount = adjacents.Count(t => t.Type == TypeTuile.ROUTE);
            int waterCount = adjacents.Count(t => t.Type == TypeTuile.EAU);

            int roll = random.Next(1, RANDOM_MAX);
            (TypeTuile, bool) typeEstTraversable = DetermineTuileType(roll, forestCount, roadCount, waterCount);
            TypeTuile type = typeEstTraversable.Item1;
            bool estTraversable = typeEstTraversable.Item2;

            string imageURL = $"images/{type.ToString().ToLower()}.png";
            return new Tuile(positionX, positionY, type, estTraversable, imageURL);
        }

        // Récupère les tuiles adjacentes à une position donnée
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

        // Détermine le type de tuile basé sur les tuiles adjacentes
        private (TypeTuile type, bool estTraversable) DetermineTuileType(int roll, int forestCount, int roadCount, int waterCount)
        {
            if (roll <= BASE_FORET + forestCount * BONUS_FORET) return (TypeTuile.FORET, true);
            if (roll <= BASE_ROUTE + roadCount * BONUS_ROUTE) return (TypeTuile.ROUTE, true);
            if (roll <= BASE_EAU + waterCount * BONUS_EAU) return (TypeTuile.EAU, false);
            if (roll <= BASE_MONTAGNE) return (TypeTuile.MONTAGNE, false);
            if (roll <= BASE_HERBE) return (TypeTuile.HERBE, true);
            return (TypeTuile.VILLE, true);
        }
    }
}
