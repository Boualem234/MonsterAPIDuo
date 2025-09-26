using MyLittleRPG_ElGuendouz.Data.Context;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Models;
using MyLittleRPG_ElGuendouz.Controllers;

namespace MyLittleRPG_ElGuendouz.Models
{
    public class MonstreMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

        public MonstreMaintenanceService(IServiceProvider serviceProvider, ILogger<MonstreMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        private async Task ValidateAndGenerateMonsters(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            int monsterCount = await context.InstanceMonstre.CountAsync(cancellationToken);

            if (monsterCount < 300)
            {
                int monstersToGenerate = 300 - monsterCount;
                _logger.LogWarning("Nombre de monstres insuffisant ({Count}/300). Génération automatique de {ToGenerate} monstres...", monsterCount, monstersToGenerate);

                var newMonsters = await GetRandomMonstersAsync(monsterCount, cancellationToken);

                _logger.LogInformation("{Count} monstres ont été générés avec succès.", monstersToGenerate);
            }
            else
            {
                _logger.LogInformation("Nombre de monstres suffisant : {Count}/300", monsterCount);
            }
        }

        //Conçu pour s'exécuter une seule fois et contenir une boucle.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonstreMaintenanceService démarré.");

            await ValidateAndGenerateMonsters(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                    await ValidateAndGenerateMonsters(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la vérification périodique des monstres.");
                }
            }

            _logger.LogInformation("MonstreMaintenanceService arrêté.");
        }

        public async Task<List<Monster>> GetRandomMonstersAsync(int count, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            int totalCount = await context.Monsters.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new List<Monster>();
            }

            if (count >= totalCount)
            {
                return await context.Monsters.ToListAsync(cancellationToken);
            }

            var allIds = await context.Monsters
                .Select(m => m.idMonster)
                .ToListAsync(cancellationToken);

            var random = new Random();
            var randomIds = allIds
                .OrderBy(_ => random.Next())
                .Take(count)
                .ToList();

            var monsters = await context.Monsters
                .Where(m => randomIds.Contains(m.idMonster))
                .ToListAsync(cancellationToken);

            return monsters;
        }

    }
}
