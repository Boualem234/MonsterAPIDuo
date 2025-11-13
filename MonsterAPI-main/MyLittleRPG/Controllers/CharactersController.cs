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
        const double FACTEUR_MIN = 0.8;
        const double FACTEUR_MAX = 1.25;
        const int DEGATS_MIN = 10;
        const int DEGATS_MAX = 25;

        const int EXP_PAR_NIVEAU = 100;
        const int BONUS_PAR_NIVEAU_MONSTRE = 10;

        private readonly MonsterContext _context;

        public CharactersController(MonsterContext context)
        {
            _context = context;
        }

        private (bool IsValid, Character? Character) ValidateUserAndCharacter(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return (false, null);

            (bool, User?) userConnected = _context.DoesExistAndConnected(email);
            if (!userConnected.Item1) return (false, null);

            Character? character = _context.Character.FirstOrDefault(c => c.utilisateurId == userConnected.Item2!.utilisateurId);
            return (character != null, character);
        }

        private CharacterStateDto CreateCharacterStateDto(Character character)
        {
            return new CharacterStateDto
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
            };
        }

        private (int degatsMonstre, int degatsJoueur) CalculerDegats(Character character, Monster monstre, InstanceMonstre instanceMonstre)
        {
            Random random = new Random();
            double facteur = random.NextDouble() * (FACTEUR_MAX - FACTEUR_MIN) + FACTEUR_MIN;

            int forceMonstre = monstre.forceBase + instanceMonstre.niveau;
            int defenseMonstre = monstre.defenseBase + instanceMonstre.niveau;

            int degatsMonstre = (int)((character.force - defenseMonstre) * facteur);
            int degatsJoueur = (int)((forceMonstre - character.def) * facteur);

            if (degatsMonstre <= 0) degatsMonstre = random.Next(DEGATS_MIN, DEGATS_MAX);
            if (degatsJoueur <= 0) degatsJoueur = random.Next(DEGATS_MIN, DEGATS_MAX);

            return (degatsMonstre, degatsJoueur);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Character>> CreateCharacter([FromBody] Character character)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.utilisateurId == character.utilisateurId);
            if (user == null)
            {
                return NotFound("Utilisateur inexistant.");
            }

            _context.Character.Add(character);
            await _context.SaveChangesAsync();

            return Ok(character);
        }

        [HttpGet("Load/{email}")]
        public ActionResult<Character> LoadCharacter(string email)
        {
            (bool, Character?) validation = ValidateUserAndCharacter(email);
            if (!validation.Item1) return NotFound();
            return Ok(validation.Item2);
        }

        [HttpPut("Deplacement/{x}/{y}/{email}")]
        public async Task<ActionResult<CombatResultDto>> Deplacer(int x, int y, string email)
        {
            try
            {
                (bool, Character?) validation = ValidateUserAndCharacter(email);
                if (!validation.Item1) return NotFound("Utilisateur non connecté ou personnage non trouvé");

                Character character = validation.Item2!;
                InstanceMonstre? instanceMonstre = _context.InstanceMonstre.FirstOrDefault(m => m.PositionX == x && m.PositionY == y);

                if (x != character.posX + 1 && x != character.posX - 1 && y != character.posY + 1 && y != character.posY - 1 && x != character.villeX && y != character.villeY)
                {
                    return BadRequest("Mouvement invalide");
                }

                // verif si il y a un monstre sur la tuile

                if (instanceMonstre != null)
                {
                    Monster? monstre = _context.Monsters.FirstOrDefault(m => m.idMonster == instanceMonstre.monstreID);
                    if (monstre == null) return NotFound("Monstre non trouvé");

                    // calcul des dégâts
                    (int, int) degats = CalculerDegats(character, monstre, instanceMonstre);
                    int degatsJoueur = degats.Item1, degatsMonstre = degats.Item2;
                    bool resultat = false;
                    string message, messageQuestNiveau = "", messageQuestMonstres = "", messageQuestTuiles = "";

                    // Appliquer les dégâts aux deux combattants
                    instanceMonstre.pointsVieActuels -= degatsMonstre;
                    character.pv -= degatsJoueur;

                    if (instanceMonstre.pointsVieActuels <= 0)
                    {
                        // victoire du joueur
                        Quest? quest_monstres = _context.Quest.First(q => q.idPersonnage == character.idPersonnage && q.Type == "monstres" && q.TypeMonstre == _context.Monsters.First(
                            m => m.idMonster == instanceMonstre.monstreID).type1);
                        quest_monstres.NbMonstresTues++;
                        if(quest_monstres.NbMonstresATuer == quest_monstres.NbMonstresTues)
                        {
                            quest_monstres.Termine = true;
                            quest_monstres.idPersonnage = null;
                            messageQuestMonstres = $"Quête de monstres terminée: Tuer {quest_monstres.NbMonstresATuer} monstres de type {quest_monstres.TypeMonstre}";
                        }
                        _context.InstanceMonstre.Remove(instanceMonstre);
                        character.posX = x;
                        character.posY = y;
                        Quest? quest_tuile = _context.Quest.First(q => q.idPersonnage == character.idPersonnage && q.Type == "tuile");
                        if(character.posX == quest_tuile.TuileASeRendreX && character.posY == quest_tuile.TuileASeRendreY)
                        {
                            quest_tuile.Termine = true;
                            quest_tuile.idPersonnage = null;
                            messageQuestTuiles = $"Quête de tuile terminée: Se rendre à la tuile X{quest_tuile.TuileASeRendreX} Y{quest_tuile.TuileASeRendreY}";
                        }
                        // gain d'expérience
                        int xpGagnee = monstre.experienceBase + instanceMonstre.niveau * BONUS_PAR_NIVEAU_MONSTRE;
                        character.exp += xpGagnee;
                        // gestion du niveau
                        int seuilNiveau = character.niveau * EXP_PAR_NIVEAU;
                        if (character.exp >= seuilNiveau)
                        {
                            character.niveau++;
                            character.force++;
                            character.def++;
                            character.pvMax++;
                            character.pv = character.pvMax;
                            message = $"Victoire ! Niveau augmenté. Expérience gagnée : {xpGagnee}";
                            Quest? quest_niveau = _context.Quest.First(q => q.idPersonnage == character.idPersonnage && q.Type == "niveau" && q.NvRequis == character.niveau);
                            if (quest_niveau is not null)
                            {
                                quest_niveau.Termine = true;
                                quest_niveau.idPersonnage = null;
                                messageQuestNiveau = $"Quête de niveau terminée: Se rendre au niveau {quest_niveau.NvRequis}";
                            }
                        }
                        else
                        {
                            message = $"Victoire ! Expérience gagnée : {xpGagnee}";
                        }
                        resultat = true;
                    }
                    else if (character.pv <= 0)
                    {
                        // defaite player
                        character.pv = character.pvMax;
                        message = "Défaite ! Vous êtes téléporté à la ville et vos HP sont restaurés.";
                        resultat = false;
                    }
                    else
                    {
                        // le joueur reste sur sa position d'origine
                        message = $"Combat indécis: vous avez infligé {degatsMonstre} dégâts et reçu {degatsJoueur}.";
                        resultat = false;
                    }

                    await _context.SaveChangesAsync();
                    return Ok(new CombatResultDto
                    {
                        Combat = true,
                        Resultat = resultat,
                        Message = message,
                        MessageQuestTuile = messageQuestTuiles,
                        MessageQuestMonstres = messageQuestMonstres,
                        MessageQuestNiveau = messageQuestNiveau,
                        Character = CreateCharacterStateDto(character),
                        Monstre = instanceMonstre.pointsVieActuels > 0 ? new MonstreStateDto
                        {
                            Pv = instanceMonstre.pointsVieActuels,
                            PosX = instanceMonstre.PositionX,
                            PosY = instanceMonstre.PositionY,
                            Nom = monstre.nom,
                            Niveau = instanceMonstre.niveau,
                            SpriteUrl = monstre.spriteUrl
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
                        Character = CreateCharacterStateDto(character)
                    });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("Simulate/{monstreId}/{email}")]
        public ActionResult<CombatResultDto> Simulate(int monstreId, string email)
        {
            (bool, Character?) validation = ValidateUserAndCharacter(email);
            if (!validation.Item1) return NotFound("Utilisateur non connecté ou personnage non trouvé");

            Character? character = validation.Item2!;

            InstanceMonstre? instanceMonstre = _context.InstanceMonstre.FirstOrDefault(m => m.monstreID == monstreId);
            if (instanceMonstre == null) return NotFound("Instance de monstre non trouvée");

            Monster? monstre = _context.Monsters.FirstOrDefault(m => m.idMonster == instanceMonstre.monstreID);
            if (monstre == null) return NotFound("Monstre non trouvé");

            // Simulation du combat (sans appliquer les changements)
            (int, int) degats = CalculerDegats(character, monstre, instanceMonstre);
            int degatsJoueur = degats.Item1, degatsMonstre = degats.Item2;

            // Simulation des PV après combat
            int pvJoueurApres = character.pv - degatsJoueur;
            int pvMonstreApres = instanceMonstre.pointsVieActuels - degatsMonstre;

            CharacterStateDto? characterSimule = CreateCharacterStateDto(character);

            MonstreStateDto? monstreSimule = new MonstreStateDto
            {
                Pv = instanceMonstre.pointsVieActuels,
                PosX = instanceMonstre.PositionX,
                PosY = instanceMonstre.PositionY,
                Nom = monstre.nom,
                Niveau = instanceMonstre.niveau,
                SpriteUrl = monstre.spriteUrl
            };

            string message;
            bool resultat = false;

            if (pvMonstreApres <= 0)
            {
                // Victoire simulée
                monstreSimule = null;
                characterSimule.PosX = instanceMonstre.PositionX;
                characterSimule.PosY = instanceMonstre.PositionY;

                int expGagnee = monstre.experienceBase + instanceMonstre.niveau * BONUS_PAR_NIVEAU_MONSTRE;
                characterSimule.Exp += expGagnee;

                int seuilNiveau = character.niveau * EXP_PAR_NIVEAU;
                if (characterSimule.Exp >= seuilNiveau)
                {
                    characterSimule.Niveau++;
                    characterSimule.Force++;
                    characterSimule.Def++;
                    characterSimule.PvMax++;
                    characterSimule.Pv = characterSimule.PvMax;
                    message = $"[SIMULATION] Victoire ! Niveau augmenté. Expérience gagnée : {expGagnee}";
                }
                else
                {
                    characterSimule.Pv = pvJoueurApres;
                    message = $"[SIMULATION] Victoire ! Expérience gagnée : {expGagnee}";
                }
                resultat = true;
            }
            else if (pvJoueurApres <= 0)
            {
                // Défaite simulée
                characterSimule.PosX = 0;
                characterSimule.PosY = 0;
                characterSimule.Pv = character.pvMax;
                monstreSimule.Pv = pvMonstreApres;
                message = "[SIMULATION] Défaite ! Vous seriez téléporté à la ville et vos HP restaurés.";
            }
            else
            {
                // Combat indécis simulé
                characterSimule.Pv = pvJoueurApres;
                monstreSimule.Pv = pvMonstreApres;
                message = "[SIMULATION] Combat indécis.";
            }

            return Ok(new CombatResultDto
            {
                Combat = true,
                Resultat = resultat,
                Message = message,
                Character = characterSimule,
                Monstre = monstreSimule
            });
        }

        [HttpGet("Ville/{email}")]
        public ActionResult<VilleDto> GetVille(string email)
        {
            (bool, Character?) validation = ValidateUserAndCharacter(email);
            if (!validation.Item1) return NotFound("Utilisateur non connecté ou personnage non trouvé");

            Character? character = validation.Item2!;
            return Ok(new VilleDto
            {
                VilleX = character.villeX,
                VilleY = character.villeY
            });
        }

        [HttpPost("Ville/{email}")]
        public async Task<ActionResult> SetVille(string email, [FromBody] VilleDto ville)
        {
            if (ville == null) return BadRequest("Données ville requises");

            (bool, Character?) validation = ValidateUserAndCharacter(email);
            if (!validation.Item1) return NotFound("Utilisateur non connecté ou personnage non trouvé");

            Character? character = validation.Item2!;

            // Mettre à jour les coordonnées de la ville
            character.villeX = ville.VilleX;
            character.villeY = ville.VilleY;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ville mise à jour avec succès",
                ville = new VilleDto
                {
                    VilleX = character.villeX,
                    VilleY = character.villeY
                }
            });
        }
    }
}
