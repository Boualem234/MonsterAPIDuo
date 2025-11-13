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
        private const int NBR_QUETES = 3, NB_MINUTES = 10;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<QuestService> _logger;
        private Random rand;

        public QuestService(IServiceScopeFactory scopeFactory, ILogger<QuestService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            rand = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

                        var characters = await context.Character.ToListAsync(stoppingToken);

                        foreach (var character in characters)
                        {
                            int questCount = await context.Quest
                                .CountAsync(q => q.idPersonnage == character.idPersonnage, stoppingToken);

                            if (questCount < NBR_QUETES)
                            {
                                int toAdd = NBR_QUETES - questCount;
                                for (int i = 0; i < toAdd; i++)
                                {
                                    Quest newQuest;
                                    int result = rand.Next(1, 4);

                                    switch (result)
                                    {
                                        case 1:
                                            {
                                                newQuest = new Quest
                                                {
                                                    Type = "monstres",
                                                    NvRequis = null,
                                                    NbMonstresATuer = rand.Next(1, 6),
                                                    NbMonstresTues = 0,
                                                    TypeMonstre = context.Monsters.ToArray()[rand.Next(context.Monsters.Count())].type1,
                                                    TuileASeRendreX = null,
                                                    TuileASeRendreY = null,
                                                    Termine = false,
                                                    idPersonnage = character.idPersonnage
                                                };
                                                context.Quest.Add(newQuest);
                                                break;
                                            }
                                        case 2:
                                            {
                                                newQuest = new Quest
                                                {
                                                    Type = "tuile",
                                                    NvRequis = null,
                                                    NbMonstresATuer = null,
                                                    NbMonstresTues = null,
                                                    TypeMonstre = null,
                                                    TuileASeRendreX = rand.Next(1, 51),
                                                    TuileASeRendreY = rand.Next(1, 51),
                                                    Termine = false,
                                                    idPersonnage = character.idPersonnage
                                                };
                                                context.Quest.Add(newQuest);
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
                                                context.Quest.Add(newQuest);
                                                break;
                                            }
                                    }
                                }

                                await context.SaveChangesAsync(stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur pendant la vérification des quêtes.");
                }
                await Task.Delay(TimeSpan.FromMinutes(NB_MINUTES), stoppingToken);
            }
        }
    }
}