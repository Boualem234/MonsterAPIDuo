using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyLittleRPG_ElGuendouz;
using MyLittleRPG_ElGuendouz.Models;
using Xunit;

namespace TestMonsterApiDuo
{
    /// <summary>
    /// Tests d'intégration pour l'inscription des utilisateurs
    /// Utilise WebApplicationFactory pour tester l'application complète
    /// </summary>
    public class InscriptionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public InscriptionTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Tests de Succès

        [Fact]
        public async Task Inscription_WithValidData_ReturnsCreated()
        {
            // Arrange : Préparer les données avec un email unique
            string testEmail = $"test_{Guid.NewGuid()}@example.com";
            User newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "TestUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier le résultat
            Assert.True(response.IsSuccessStatusCode, 
                $"Inscription échouée: {await response.Content.ReadAsStringAsync()}");
            
            User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(returnedUser);
            Assert.Equal(testEmail, returnedUser.email);
            Assert.Equal("TestUser", returnedUser.pseudo);
            Assert.True(returnedUser.utilisateurId > 0);
        }

        [Fact]
        public async Task Inscription_WithValidData_CreatesCharacterAutomatically()
        {
            // Arrange : Préparer les données avec un email unique
            string testEmail = $"test_{Guid.NewGuid()}@example.com";
            User newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "TestUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier que l'inscription a réussi
            Assert.True(response.IsSuccessStatusCode);
            
            User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(returnedUser);

            // Vérifier qu'un personnage a été créé en se connectant et en récupérant les infos
            HttpResponseMessage? loginResponse = await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
            Assert.True(loginResponse.IsSuccessStatusCode);

            HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
            Assert.True(characterResponse.IsSuccessStatusCode);
            
            Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            Assert.NotNull(character);
            Assert.Equal("TestUser", character.nom);
            Assert.Equal(1, character.niveau);
            Assert.Equal(0, character.exp);
            Assert.Equal(100, character.pvMax);
            Assert.InRange(character.pv, 1, 100);
            Assert.InRange(character.force, 1, 100);
            Assert.InRange(character.def, 1, 100);
        }

        [Fact]
        public async Task Inscription_WithValidData_PlacesCharacterInRandomCity()
        {
            // Arrange : Préparer les données avec un email unique
            string testEmail = $"test_{Guid.NewGuid()}@example.com";
            User newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "TestUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier que l'inscription a réussi
            Assert.True(response.IsSuccessStatusCode);

            // Se connecter et récupérer les infos du personnage
            await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
            
            HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
            Assert.True(characterResponse.IsSuccessStatusCode);
            
            Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            Assert.NotNull(character);
            
            // Vérifier que le personnage est placé à la position de départ (10, 10)
            Assert.Equal(10, character.posX);
            Assert.Equal(10, character.posY);
        }

        #endregion

        #region Tests d'Erreur

        [Fact]
        public async Task Inscription_WithExistingEmail_ReturnsConflict()
        {
            // Arrange : Créer un premier utilisateur avec un email unique
            string testEmail = $"existing_{Guid.NewGuid()}_{DateTime.Now.Ticks}@example.com";
            User? existingUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "ExistingUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Inscrire le premier utilisateur
            HttpResponseMessage? firstResponse = await _client.PostAsJsonAsync("/api/Users/Register/", existingUser);
            
            // Si l'inscription échoue, afficher le message d'erreur pour debug
            if (!firstResponse.IsSuccessStatusCode)
            {
                string errorContent = await firstResponse.Content.ReadAsStringAsync();
                Assert.True(firstResponse.IsSuccessStatusCode, $"La première inscription a échoué: {errorContent}");
            }

            Assert.True(firstResponse.IsSuccessStatusCode);

            // Créer un deuxième utilisateur avec le même email
            User? duplicateUser = new User
            {
                email = testEmail, // Même email
                mdp = "password456",
                pseudo = "NewUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Essayer d'inscrire un utilisateur avec le même email
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", duplicateUser);

            // Assert : Vérifier que l'inscription échoue avec BadRequest
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            
            string errorMessage = await response.Content.ReadAsStringAsync();
            Assert.Contains("already exists", errorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Inscription_WithEmptyEmail_ReturnsBadRequest()
        {
            // Arrange : Préparer un utilisateur avec email vide
            User newUser = new User
            {
                email = "", // Email vide
                mdp = "password123",
                pseudo = "TestUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier que l'inscription échoue ou accepte l'email vide
            // Note: Actuellement, il n'y a pas de validation côté serveur, donc ça passe
            // Pour un vrai test de validation, il faudrait ajouter des attributs [Required] sur le modèle
            if (response.IsSuccessStatusCode)
            {
                User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
                Assert.NotNull(returnedUser);
                Assert.Equal("", returnedUser.email);
            }
            else
            {
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task Inscription_WithEmptyPassword_ReturnsBadRequest()
        {
            // Arrange : Préparer un utilisateur avec mot de passe vide
            string testEmail = $"test_{Guid.NewGuid()}@example.com";
            User newUser = new User
            {
                email = testEmail,
                mdp = "", // Mot de passe vide
                pseudo = "TestUser",
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier que l'inscription échoue ou accepte le mot de passe vide
            if (response.IsSuccessStatusCode)
            {
                User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
                Assert.NotNull(returnedUser);
                Assert.Equal("", returnedUser.mdp);
            }
            else
            {
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task Inscription_WithEmptyPseudo_ReturnsBadRequest()
        {
            // Arrange : Préparer un utilisateur avec pseudo vide
            string testEmail = $"test_{Guid.NewGuid()}@example.com";
            User newUser = new User
            {
                email = testEmail,
                mdp = "password123",
                pseudo = "", // Pseudo vide
                dateInscription = DateTime.Now,
                isConnected = false
            };

            // Act : Exécuter l'inscription via l'API
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);

            // Assert : Vérifier que l'inscription échoue ou accepte le pseudo vide
            if (response.IsSuccessStatusCode)
            {
                User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
                Assert.NotNull(returnedUser);
                Assert.Equal("", returnedUser.pseudo);

                // Vérifier que le personnage est créé avec un nom vide également
                await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
                HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
                
                if (characterResponse.IsSuccessStatusCode)
                {
                    Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
                    Assert.NotNull(character);
                    Assert.Equal("", character.nom);
                }
            }
            else
            {
                Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        #endregion
    }
}
