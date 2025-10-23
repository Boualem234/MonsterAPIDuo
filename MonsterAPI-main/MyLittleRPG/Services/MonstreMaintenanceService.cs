using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Services
{
    public class MonstreMaintenanceService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonstreMaintenanceService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);
        private readonly Random _random = new Random();

        public MonstreMaintenanceService(IServiceProvider serviceProvider, ILogger<MonstreMaintenanceService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MonstreMaintenanceService démarré.");
            await CheckAndRepopulateMonstersAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                    await CheckAndRepopulateMonstersAsync(stoppingToken);
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

        private async Task CheckAndRepopulateMonstersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            int currentCount = await context.InstanceMonstre.CountAsync(cancellationToken);

            if (currentCount < 300)
            {
                int toGenerate = 300 - currentCount;
                _logger.LogWarning("Seulement {Current}/300 monstres. Génération automatique de {ToGenerate} monstres...", currentCount, toGenerate);

                var baseMonsters = await GetRandomMonstersAsync(toGenerate, cancellationToken);

                var emptyTiles = await context.Tuiles
                           .Where(t => t.EstTraversable
                                       && t.Type != TypeTuile.VILLE
                                       && !(t.PositionX == 10 && t.PositionY == 10) // Exclure la position (10, 10)
                                       && !context.InstanceMonstre.Any(m => m.PositionX == t.PositionX && m.PositionY == t.PositionY))
                           .ToListAsync(cancellationToken);

                emptyTiles = emptyTiles.OrderBy(_ => _random.Next()).Take(toGenerate).ToList();

                var villes = await context.Tuiles.Where(t => t.Type == TypeTuile.VILLE).ToListAsync(cancellationToken);

                for (int i = 0; i < baseMonsters.Count; i++)
                {
                    var monster = baseMonsters[i];
                    var tile = emptyTiles[i];

                    int niveau = 1;
                    if (villes.Count > 0)
                        niveau = villes.Max(v => Math.Abs(v.PositionX - tile.PositionX) + Math.Abs(v.PositionY - tile.PositionY));

                    var instance = new InstanceMonstre
                    {
                        PositionX = tile.PositionX,
                        PositionY = tile.PositionY,
                        monstreID = monster.idMonster,
                        niveau = niveau,
                        pointsVieMax = monster.pointVieBase + niveau,
                        pointsVieActuels = monster.pointVieBase + niveau
                    };

                    context.InstanceMonstre.Add(instance);
                }

                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Generated} monstres ont été ajoutés. Total = 300.", toGenerate);
            }
            else
            {
                _logger.LogInformation("Nombre de monstres stable : {Count}/300", currentCount);
            }
        }

        private async Task<List<Monster>> GetRandomMonstersAsync(int count, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonsterContext>();

            int totalCount = await context.Monsters.CountAsync(cancellationToken);
            if (totalCount == 0) return new List<Monster>();

            return await context.Monsters
                .OrderBy(m => Guid.NewGuid())
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }
}
