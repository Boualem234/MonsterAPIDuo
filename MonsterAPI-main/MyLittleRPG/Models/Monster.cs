using System.ComponentModel.DataAnnotations;

namespace MyLittleRPG_ElGuendouz.Models
{
    public class Monster 
    {
        [Key]
        public int idMonster {  get; set; }
        public int pokemonId { get; set; }
        public string nom {  get; set; }
        public int pointVieBase { get; set; }
        public int forceBase { get; set; }
        public int defenseBase { get; set; }
        public int experienceBase { get; set; }
        public string spriteUrl { get; set; }
        public string type1 { get; set; }
        public string type2 { get; set; }

        public Monster() { }

        public Monster(int idMonster, int pokemonId, string nom, int pointVieBase, int forceBase, int defenseBase, int experienceBase, string spriteUrl, string type1, string type2)
        {
            this.idMonster = idMonster;
            this.pokemonId = pokemonId;
            this.nom = nom;
            this.pointVieBase = pointVieBase;
            this.forceBase = forceBase;
            this.defenseBase = defenseBase;
            this.experienceBase = experienceBase;
            this.spriteUrl = spriteUrl;
            this.type1 = type1;
            this.type2 = type2;
        }
    }
}
