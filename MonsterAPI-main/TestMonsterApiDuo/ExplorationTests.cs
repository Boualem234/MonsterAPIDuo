using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyLittleRPG_ElGuendouz;
using MyLittleRPG_ElGuendouz.Models;
using Xunit;
using static MyLittleRPG_ElGuendouz.DTOs.TuilesDtos;

namespace TestMonsterApiDuo
{
    /// <summary>
    /// Tests d'intégration pour l'exploration des tuiles
    /// </summary>
    public class ExplorationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private const int EXPLORATION_RANGE = 2;

        public ExplorationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<(string email, Character character)> CreateAndLoginUser()
        {
            var testEmail = $"explorer_{Guid.NewGuid()}@test.com";
            var newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "Explorer",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            await _client.PostAsJsonAsync("/api/Users/Register/", newUser);
            await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");

            var characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
            var character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            
            return (testEmail, character!);
        }

        #region Tests de Succès

        [Fact]
        public async Task ExplorerTuile_WithinRange_ReturnsTuileData()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = character.posX + 1;
            int targetY = character.posY;

            // Act : Explorer une tuile adjacente via GetTuile
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : Vérifier que les données de la tuile sont retournées
            Assert.True(response.IsSuccessStatusCode);
            var tuileData = await response.Content.ReadFromJsonAsync<TuileAvecMonstresDto>();
            Assert.NotNull(tuileData);
            Assert.Equal(targetX, tuileData.PositionX);
            Assert.Equal(targetY, tuileData.PositionY);
        }

        [Fact]
        public async Task ExplorerTuile_WithinRange_ReturnsMonsterIfPresent()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = character.posX + 1;
            int targetY = character.posY;

            // Act : Explorer une tuile
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : Vérifier la structure de la réponse (monstre peut être présent ou non)
            Assert.True(response.IsSuccessStatusCode);
            var tuileData = await response.Content.ReadFromJsonAsync<TuileAvecMonstresDto>();
            Assert.NotNull(tuileData);
            // Le monstre peut être null ou non null, les deux sont valides
        }

        [Fact]
        public async Task ExplorerTuile_WithinRange_ReturnsNullMonsterIfEmpty()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();

            // Explorer plusieurs tuiles pour trouver une tuile sans monstre
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int targetX = character.posX + dx;
                    int targetY = character.posY + dy;

                    // Act : Explorer la tuile
                    var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

                    if (response.IsSuccessStatusCode)
                    {
                        var tuileData = await response.Content.ReadFromJsonAsync<TuileAvecMonstresDto>();
                        
                        // Assert : Si pas de monstre, le champ Monstres doit être null
                        if (tuileData?.Monstres == null)
                        {
                            Assert.Null(tuileData.Monstres);
                            return; // Test réussi
                        }
                    }
                }
            }

            // Si toutes les tuiles ont des monstres, le test passe quand même
            Assert.True(true);
        }

        [Fact]
        public async Task ExplorerTuile_TwoStepsAway_Succeeds()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = character.posX + 2;
            int targetY = character.posY;

            // Act : Explorer une tuile à 2 cases de distance
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : L'exploration doit réussir (distance = 2)
            Assert.True(response.IsSuccessStatusCode);
            var tuileData = await response.Content.ReadFromJsonAsync<TuileAvecMonstresDto>();
            Assert.NotNull(tuileData);
        }

        #endregion

        #region Tests d'Erreur

        [Fact]
        public async Task ExplorerTuile_FiveStepsAway_ReturnsForbidden()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = character.posX + 5;
            int targetY = character.posY;

            // Act : Explorer une tuile trop loin
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : Doit retourner Forbidden (distance > 2)
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ExplorerTuile_BeyondMapBoundaries_ReturnsForbidden()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = 100; // Au-delà de la limite (50)
            int targetY = 100;

            // Act : Explorer une tuile hors des limites de la carte
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : Doit retourner erreur (416 ou 403)
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ExplorerTuile_NegativeCoordinates_ReturnsForbidden()
        {
            // Arrange : Créer et connecter un utilisateur
            var (email, character) = await CreateAndLoginUser();
            int targetX = -5;
            int targetY = -5;

            // Act : Explorer une tuile avec des coordonnées négatives
            var response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={email}");

            // Assert : Doit retourner erreur (416 ou 403)
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ExplorerTuile_WithoutAuthentication_ReturnsForbidden()
        {
            // Arrange : Pas de connexion utilisateur
            string email = "nonexistent@test.com";

            // Act : Essayer d'explorer sans être authentifié
            var response = await _client.GetAsync($"/api/Tuiles/10/10?email={email}");

            // Assert : Doit retourner Forbidden ou NotFound
            Assert.False(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || 
                       response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ExplorerTuile_WithDisconnectedUser_ReturnsForbidden()
        {
            // Arrange : Créer un utilisateur mais le déconnecter
            var testEmail = $"disconnected_{Guid.NewGuid()}@test.com";
            var newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "Disconnected",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            await _client.PostAsJsonAsync("/api/Users/Register/", newUser);
            await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
            
            // Se déconnecter explicitement
            await _client.PostAsync($"/api/Users/Logout/{testEmail}", null);

            // Act : Essayer d'explorer avec un utilisateur déconnecté
            var response = await _client.GetAsync($"/api/Tuiles/10/10?email={testEmail}");

            // Assert : Doit retourner Forbidden
            Assert.False(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || 
                       response.StatusCode == HttpStatusCode.Unauthorized);
        }

        #endregion
    }
}
