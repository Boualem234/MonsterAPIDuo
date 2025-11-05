using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MyLittleRPG_ElGuendouz;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.DTOs;
using MyLittleRPG_ElGuendouz.Models;
using Xunit;
using Xunit.Sdk;

namespace TestMonsterApiDuo
{
    public class TuilesTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TuilesTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Tests de succès
        [Fact]
        public async Task Obtenir_Tuiles_Avec_Utilisateur_Connecte()
        {
            await Task.Delay(2000);

            string mail = "a@a.a";
            string pwd = "a";

            await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Character? character = await _client.GetAsync($"/api/Characters/Load/{mail}").Result.Content.ReadFromJsonAsync<Character>();

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/{character.posX+1}/{character.posY+1}?email={mail}");
            Assert.True(response.IsSuccessStatusCode, $"Chargement de tuile échoué: {await response.Content.ReadAsStringAsync()}");

            await _client.PostAsync($"/api/Users/Logout/{mail}", null);
        }

        [Fact]
        public async Task Obtenir_Tuiles_Avec_Utilisateur_Connecte_Position_Personnage()
        {
            await Task.Delay(2000);

            string mail = "a@a.a";
            string pwd = "a";

            await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Character? character = await _client.GetAsync($"/api/Characters/Load/{mail}").Result.Content.ReadFromJsonAsync<Character>();

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/{character.posX}/{character.posY}?email={mail}");
            Assert.True(response.IsSuccessStatusCode, $"Chargement de tuile échoué: {await response.Content.ReadAsStringAsync()}");

            await _client.PostAsync($"/api/Users/Logout/{mail}", null);
        }

        [Fact]
        public async Task Obtenir_Tuiles_Avec_Utilisateur_Connecte_Obtient_Monstre()
        {
            await Task.Delay(2000);

            IEnumerable<InstanceMonstre>? monstres = await _client.GetAsync($"/api/Monsters/GetInstances").Result.Content.ReadFromJsonAsync<IEnumerable<InstanceMonstre>>();

            string nouveauEmail = $"tuiles_{Guid.NewGuid()}@test.com";
            User newUser = new User
            {
                email = nouveauEmail,
                mdp = "password123",
                pseudo = "TuileMonstre",
                dateInscription = DateTime.Now,
                isConnected = false
            };
            HttpResponseMessage? registerResponse = await _client.PostAsJsonAsync("/api/Users/Register/", newUser);
            registerResponse.EnsureSuccessStatusCode();

            await _client.GetAsync($"/api/Users/Login/{nouveauEmail}/password123");

            // Charger le personnage fait par Register
            HttpResponseMessage? characterResponse = await _client.GetAsync($"/api/Characters/Load/{nouveauEmail}");
            Character? character = await characterResponse.Content.ReadFromJsonAsync<Character>();
            Assert.NotNull(character);

            // Trouver un monstre proche de la position du personnage (dist <= 2)
            InstanceMonstre? monstreProche = monstres.FirstOrDefault(m => 
                Math.Abs(m.PositionX - character.posX) <= 2 && 
                Math.Abs(m.PositionY - character.posY) <= 2);

            // Si aucun monstre proche, tester avec la position du personnage
            int targetX = monstreProche?.PositionX ?? character.posX;
            int targetY = monstreProche?.PositionY ?? character.posY;

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/{targetX}/{targetY}?email={nouveauEmail}");
            Assert.True(response.IsSuccessStatusCode, $"Chargement de la tuile échouée: {await response.Content.ReadAsStringAsync()}");

            if (monstreProche != null)
            {
                TuilesDtos.TuileAvecMonstresDto? monstreDto = await response.Content.ReadFromJsonAsync<TuilesDtos.TuileAvecMonstresDto>();
                Assert.True(monstreDto.Monstres is not null, "Échec du chargement de monstre");
            }

            await _client.PostAsync($"/api/Users/Logout/{nouveauEmail}", null);
        }
        #endregion

        #region Tests d'erreur
        [Fact]
        public async Task Obtenir_Tuiles_Avec_Utilisateur_Non_Existant()
        {
            await Task.Delay(2000);

            string mail = "mailnonexistant@gmail.com";

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/1/1?email={mail}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Obtenir_Tuile_Pour_Tuile_Hors_Limite()
        {
            await Task.Delay(2000);

            string mail = "a@a.a";
            string pwd = "a";

            await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/-1/-1?email={mail}");

            Assert.False(response.IsSuccessStatusCode, $"Tuile bien chargée: {response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Obtenir_Tuiles_Avec_Utilisateur_Non_Connecte()
        {
            await Task.Delay(2000);

            string mail = "a@a.a";

            HttpResponseMessage response = await _client.GetAsync($"/api/Tuiles/1/1?email={mail}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion
    }
}
