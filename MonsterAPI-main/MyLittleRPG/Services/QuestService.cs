using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Services
{
    public class QuestService : BackgroundService
    {
        private const int NBR_QUETES = 3;
        private const int NB_MINUTES = 10;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuestService> _logger;
        private readonly Random rand;

        public QuestService(IServiceScopeFactory scopeFactory, ILogger<QuestService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            rand = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CheckAndGenerateQuestsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(NB_MINUTES), stoppingToken);
                await CheckAndGenerateQuestsAsync(stoppingToken);
            }
        }

        private async Task CheckAndGenerateQuestsAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

                var characters = await context.Character.ToListAsync(stoppingToken);

                foreach (var character in characters)
                {
                    int questCount = await context.Quest
                        .CountAsync(q => q.idPersonnage == character.idPersonnage, stoppingToken);

                    if (questCount < NBR_QUETES)
                    {
                        int toAdd = NBR_QUETES - questCount;
                        _logger.LogInformation($"Personnage {character.nom} : ajout de {toAdd} quête(s).");

                        var monsters = await context.Monsters.ToListAsync(stoppingToken);
                        var usedTypes = await context.Quest
                            .Where(q => q.idPersonnage == character.idPersonnage)
                            .Select(q => q.Type)
                            .ToListAsync();

                        for (int i = 0; i < toAdd; i++)
                        {
                            Quest newQuest;
                            int result;

                            do
                            {
                                result = rand.Next(1, 4);

                            } while (
                                (result == 1 && usedTypes.Contains("monstres")) ||
                                (result == 2 && usedTypes.Contains("tuile")) ||
                                (result == 3 && usedTypes.Contains("niveau"))
                            );

                            switch (result)
                            {
                                case 1:
                                    {
                                        var randomMonster = monsters.Count > 0 ? monsters[rand.Next(monsters.Count)] : null;

                                        newQuest = new Quest
                                        {
                                            Type = "monstres",
                                            NvRequis = null,
                                            NbMonstresATuer = rand.Next(1, 6),
                                            NbMonstresTues = 0,
                                            TypeMonstre = randomMonster?.type1 ?? "Inconnu",
                                            TuileASeRendreX = null,
                                            TuileASeRendreY = null,
                                            Termine = false,
                                            idPersonnage = character.idPersonnage
                                        };
                                        usedTypes.Add("monstres");
                                        break;
                                    }

                                case 2:
                                    {
                                        int x, y;
                                        Tuile tuile;
                                        do
                                        {
                                            x = rand.Next(1, 51);
                                            y = rand.Next(1, 51);

                                            tuile = context.Tuiles.First(t => t.PositionX == x && t.PositionY == y);
                                        } while (!tuile.EstTraversable);

                                        newQuest = new Quest
                                        {
                                            Type = "tuile",
                                            NvRequis = null,
                                            NbMonstresATuer = null,
                                            NbMonstresTues = null,
                                            TypeMonstre = null,
                                            TuileASeRendreX = x,
                                            TuileASeRendreY = y,
                                            Termine = false,
                                            idPersonnage = character.idPersonnage
                                        };
                                        usedTypes.Add("tuile");
                                        break;
                                    }

                                case 3:
                                    {
                                        newQuest = new Quest
                                        {
                                            Type = "niveau",
                                            NvRequis = character.niveau + rand.Next(1, 6),
                                            NbMonstresATuer = null,
                                            NbMonstresTues = null,
                                            TypeMonstre = null,
                                            TuileASeRendreX = null,
                                            TuileASeRendreY = null,
                                            Termine = false,
                                            idPersonnage = character.idPersonnage
                                        };
                                        usedTypes.Add("niveau");
                                        break;
                                    }

                                default:
                                    continue;
                            }

                            context.Quest.Add(newQuest);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur pendant la vérification des quêtes : {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}