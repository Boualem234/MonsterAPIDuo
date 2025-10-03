using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;
using MyLittleRPG_ElGuendouz.DTOs;

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
            (bool, User) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return NotFound(); 
            Character? character = _context.Character.FirstOrDefault(c => c.utilisateurId == userConnected.Item2.utilisateurId);
            if (character is null) return NotFound();
            else return Ok(character);
        }

        [HttpPut("Deplacement/{x}/{y}/{email}")]
        public async Task<ActionResult<CombatResultDto>> Deplacer(int x, int y, string email)
        {
            // on regarde si l'utilisateur est connecté
            (bool, User) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return NotFound("Utilisateur non connecté");

            // on regarde si l'utilisateur a un personnage (normalement oui)
            Character? character = _context.Character.FirstOrDefault(c => c.utilisateurId == userConnected.Item2.utilisateurId);
            if (character is null) return NotFound("Personnage non trouvé");

            // verif si il y a un monstre sur la tuile
            var instanceMonstre = _context.InstanceMonstre.FirstOrDefault(m => m.PositionX == x && m.PositionY == y);
            
            if (instanceMonstre != null)
            {
                var monstre = _context.Monsters.FirstOrDefault(m => m.idMonster == instanceMonstre.monstreID);
                if (monstre == null) return NotFound("Monstre non trouvé");

                int originalX = character.posX;
                int originalY = character.posY;

                // calcul des dégâts
                var random = new Random();
                double facteur = random.NextDouble() * (1.25 - 0.8) + 0.8;
                int degatsMonstre = (int)((character.force - (monstre.defenseBase + instanceMonstre.niveau)) * facteur);
                int degatsJoueur = (int)(((monstre.forceBase + instanceMonstre.niveau) - character.def) * facteur);
                if (degatsMonstre <= 0) degatsMonstre = 0;
                if (degatsJoueur <= 0) degatsJoueur = 0;

                // appliquer les dégâts
                instanceMonstre.pointsVieActuels -= degatsMonstre;
                character.pv -= degatsJoueur;

                // combat
                bool resultat = false;
                string message = string.Empty;
                if (instanceMonstre.pointsVieActuels <= 0)
                {
                    // victoire du joueur
                    _context.InstanceMonstre.Remove(instanceMonstre);
                    character.posX = x;
                    character.posY = y;
                    // gain d'expérience
                    int xpGagnee = monstre.experienceBase + instanceMonstre.niveau * 10;
                    character.exp += xpGagnee;
                    // gestion du niveau
                    int seuilNiveau = character.niveau * 100;
                    if (character.exp >= seuilNiveau)
                    {
                        character.niveau++;
                        character.force++;
                        character.def++;
                        character.pvMax++;
                        character.pv = character.pvMax;
                        message = $"Victoire ! Niveau augmenté. Expérience gagnée : {xpGagnee}";
                        resultat = true;
                    }
                    else
                    {
                        message = $"Victoire ! Expérience gagnée : {xpGagnee}";
                        resultat = true;
                    }
                }
                else if (character.pv <= 0)
                {
                    // defaite player
                    character.posX = 0;
                    character.posY = 0;
                    character.pv = character.pvMax;
                    message = "Défaite ! Vous êtes téléporté à la ville et vos HP sont restaurés.";
                    resultat = false;
                }
                else
                {
                    // le joueur reste sur sa position d'origine
                    message = "Combat indécis. Vous pouvez retenter plus tard.";
                    resultat = false;
                }

                await _context.SaveChangesAsync();
                return Ok(new CombatResultDto
                {
                    Combat = true,
                    Resultat = resultat,
                    Message = message,
                    Character = new CharacterStateDto
                    {
                        PosX = character.posX,
                        PosY = character.posY,
                        Pv = character.pv,
                        PvMax = character.pvMax,
                        Niveau = character.niveau,
                        Exp = character.exp,
                        Force = character.force,
                        Def = character.def,
                        Nom = character.nom
                    },
                    Monstre = instanceMonstre.pointsVieActuels > 0 ? new MonstreStateDto
                    {
                        Pv = instanceMonstre.pointsVieActuels,
                        PosX = instanceMonstre.PositionX,
                        PosY = instanceMonstre.PositionY,
                        Nom = monstre.nom,
                        Niveau = instanceMonstre.niveau
                    } : null
                });
            }
            else
            {
                // deplacement sans combat
                character.posX = x;
                character.posY = y;
                await _context.SaveChangesAsync();
                return Ok(new CombatResultDto
                {
                    Combat = false,
                    Character = new CharacterStateDto
                    {
                        PosX = character.posX,
                        PosY = character.posY,
                        Pv = character.pv,
                        PvMax = character.pvMax,
                        Niveau = character.niveau,
                        Exp = character.exp,
                        Force = character.force,
                        Def = character.def,
                        Nom = character.nom
                    }
                });
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
