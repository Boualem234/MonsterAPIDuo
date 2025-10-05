using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyLittleRPG_ElGuendouz.Models
{
    public class Character
    {
        [Key]
        public int idPersonnage { get; set; }
        public string nom {  get; set; } = string.Empty;
        public int niveau { get; set; }
        public int exp { get; set; }
        public int pv { get; set; }
        public int pvMax { get; set; }
        public int force { get; set; }
        public int def { get; set; }
        public int posX { get; set; }
        public int posY { get; set; }
        public int utilisateurId { get; set; }
        public DateTime dateCreation { get; set; }
        public int villeX { get; set; }
        public int villeY { get; set; }

        public Character() { }

        public Character(int idPersonnage, string nom, int niveau, int exp, int pv, int pvMax, int force, int def, int posX, int posY, int utilisateurId, DateTime dateCreation)
        {
            this.idPersonnage = idPersonnage;
            this.nom = nom;
            this.niveau = niveau;
            this.exp = exp;
            this.pv = pv;
            this.pvMax = pvMax;
            this.force = force;
            this.def = def;
            this.posX = posX;
            this.posY = posY;
            this.utilisateurId = utilisateurId;
            this.dateCreation = dateCreation;
        }
    }
}
