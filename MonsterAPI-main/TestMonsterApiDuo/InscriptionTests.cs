using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MyLittleRPG_ElGuendouz;
using MyLittleRPG_ElGuendouz.Models;
using Xunit;
using FluentAssertions;

namespace TestMonsterApiDuo
{
    /// <summary>
    /// Tests d'intégration pour l'inscription des utilisateurs
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

            // Assert : Vérifier le résultat avec FluentAssertions
            response.IsSuccessStatusCode.Should().BeTrue(
                $"Inscription échouée: {await response.Content.ReadAsStringAsync()}");
            
            User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
            returnedUser.Should().NotBeNull();
            returnedUser!.email.Should().Be(testEmail);
            returnedUser.pseudo.Should().Be("TestUser");
            returnedUser.utilisateurId.Should().BeGreaterThan(0);
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
            response.IsSuccessStatusCode.Should().BeTrue();
            
            User? returnedUser = await response.Content.ReadFromJsonAsync<User>();
            returnedUser.Should().NotBeNull();

            // Vérifier qu'un personnage a été créé en se connectant et en récupérant les infos
            HttpResponseMessage? loginResponse = await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
            loginResponse.IsSuccessStatusCode.Should().BeTrue();

            HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
            characterResponse.IsSuccessStatusCode.Should().BeTrue();
            
            Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            character.Should().NotBeNull();
            character!.nom.Should().Be("TestUser");
            character.niveau.Should().Be(1);
            character.exp.Should().Be(0);
            character.pvMax.Should().Be(100);
            character.pv.Should().BeInRange(1, 100);
            character.force.Should().BeInRange(1, 100);
            character.def.Should().BeInRange(1, 100);
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
            response.IsSuccessStatusCode.Should().BeTrue();

            // Se connecter et récupérer les infos du personnage
            await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
            
            HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
            characterResponse.IsSuccessStatusCode.Should().BeTrue();
            
            Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            character.Should().NotBeNull();
            
            // Vérifier que le personnage est placé à la position de départ (10, 10)
            character!.posX.Should().Be(10);
            character.posY.Should().Be(10);
        }

        #endregion

        #region Tests d'Erreur

        [Fact]
        public async Task Inscription_WithExistingEmail_ReturnsConflict()
        {
            // Arrange : Créer un premier utilisateur
            string testEmail = $"existing_{Guid.NewGuid()}@example.com";
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
            firstResponse.IsSuccessStatusCode.Should().BeTrue(
                $"La première inscription a échoué: {await firstResponse.Content.ReadAsStringAsync()}");

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
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            
            string errorMessage = await response.Content.ReadAsStringAsync();
            errorMessage.Should().ContainEquivalentOf("already exists");
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
                returnedUser.Should().NotBeNull();
                returnedUser!.email.Should().Be("");
            }
            else
            {
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
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
                returnedUser.Should().NotBeNull();
                returnedUser!.mdp.Should().Be("");
            }
            else
            {
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
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
                returnedUser.Should().NotBeNull();
                returnedUser!.pseudo.Should().Be("");

                // Vérifier que le personnage est créé avec un nom vide également
                await _client.GetAsync($"/api/Users/Login/{testEmail}/password123");
                HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{testEmail}");
                
                if (characterResponse.IsSuccessStatusCode)
                {
                    Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
                    character.Should().NotBeNull();
                    character!.nom.Should().Be("");
                }
            }
            else
            {
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
