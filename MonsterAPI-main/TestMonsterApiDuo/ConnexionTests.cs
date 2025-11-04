using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;
using Xunit;
using Xunit.Sdk;

namespace TestMonsterApiDuo
{
    public class ConnexionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly MonsterContext _context;

        public ConnexionTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            var options = new DbContextOptionsBuilder<MonsterContext>().UseMySql("Server=sql.decinfo-cchic.ca;Port=33306;Database=a25_mylittlerpg_equipe2;Uid=dev-2534056;Pwd=cruipi72failou", new MySqlServerVersion(new Version(8, 0, 36))).Options;

            _context = new MonsterContext(options);
        }

        #region Tests de succès
        [Fact]
        public async Task Connexion_Avec_Data_Valide()
        {
            await Task.Delay(2000);

            string mail = "test@gmail.com";
            string pwd = "test";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Assert.True(response.IsSuccessStatusCode, $"Connexion échouée: {await response.Content.ReadAsStringAsync()}");

            await _client.PostAsync($"/api/Users/Logout/{mail}", null);
        }

        [Fact]
        public async Task Connexion_Avec_Data_Valide_Set_Est_Connecte()
        {
            await Task.Delay(2000);

            string mail = "test@gmail.com";
            string pwd = "test";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            if (response.IsSuccessStatusCode)
            {
                Assert.True(_context.User.First(u => u.email == mail).isConnected, $"Utilisateur pas connecté: {await response.Content.ReadAsStringAsync()}");
            }
            else
            {
                Assert.True(response.IsSuccessStatusCode, $"Connexion échouée: {await response.Content.ReadAsStringAsync()}");
            }

            await _client.PostAsync($"/api/Users/Logout/{mail}", null);
        }

        [Fact]
        public async Task Connexion_Avec_Data_Valide_Et_Requete_Apres()
        {
            await Task.Delay(2000);

            string mail = "test@gmail.com";
            string pwd = "test";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            if (response.IsSuccessStatusCode)
            {
                HttpResponseMessage subsequent = await _client.GetAsync($"/api/Characters/Load/{mail}");
                Assert.True(subsequent.StatusCode == System.Net.HttpStatusCode.OK, $"Utilisateur non connecté: {await subsequent.Content.ReadAsStringAsync()}");
            }
            else
            {
                Assert.True(response.IsSuccessStatusCode, $"Connexion échouée: {await response.Content.ReadAsStringAsync()}");
            }

            await _client.PostAsync($"/api/Users/Logout/{mail}", null);
        }
        #endregion

        #region Tests d'erreur
        [Fact]
        public async Task Connexion_Avec_Data_Invalide()
        {
            await Task.Delay(2000);

            string mail = "identifiantinvalide@gmail.com";
            string pwd = "passwordinvalide";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Assert.True(response.StatusCode != System.Net.HttpStatusCode.OK, $"Connexion réussi: {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_Avec_MotDePasse_Invalide()
        {
            await Task.Delay(2000);

            string mail = "test@gmail.com";
            string pwd = "passwordinvalide";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Assert.True(response.StatusCode != System.Net.HttpStatusCode.OK, $"Connexion réussi: {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_Avec_Email_Invalide()
        {
            await Task.Delay(2000);

            string mail = "identifiantinvalide@gmail.com";
            string pwd = "test";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Assert.True(response.StatusCode != System.Net.HttpStatusCode.OK, $"Connexion réussi: {await response.Content.ReadAsStringAsync()}");
        }

        [Fact]
        public async Task Connexion_Avec_Aucun_Identifiant()
        {
            await Task.Delay(2000);

            string mail = "";
            string pwd = "";

            HttpResponseMessage response = await _client.GetAsync($"/api/Users/Login/{mail}/{pwd}");

            Assert.True(response.StatusCode != System.Net.HttpStatusCode.OK, $"Connexion réussi: {await response.Content.ReadAsStringAsync()}");
        }
        #endregion
    }
}
