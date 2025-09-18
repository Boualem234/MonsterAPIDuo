using System.ComponentModel.DataAnnotations;

namespace MyLittleRPG_ElGuendouz.Models
{
    public class User
    {
        [Key]
        public int utilisateurId {  get; set; }
        public string email { get; set; } = string.Empty;
        public string mdp { get; set; } = string.Empty;
        public string pseudo {  get; set; } = string.Empty;
        public DateTime dateInscription { get; set; }
        public bool isConnected { get; set; } = false;

        public User(){ }

        public User(int utilisateurId, string email, string mdp, string pseudo, DateTime dateInscription)
        {
            this.utilisateurId = utilisateurId;
            this.email = email;
            this.mdp = mdp;
            this.pseudo = pseudo;
            this.dateInscription = dateInscription;
        }
    }
}
