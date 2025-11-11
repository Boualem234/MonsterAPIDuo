using Microsoft.EntityFrameworkCore;

namespace MyLittleRPG_ElGuendouz.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int? NvRequis {  get; set; }
        public int? NbMonstresATuer { get; set; }
        public int? NbMonstresTues { get; set; }
        public string? TypeMonstre { get; set; }
        public int? TuileASeRendreX {  get; set; }
        public int? TuileASeRendreY { get; set; }
        public bool Termine {  get; set; }
        public int idPersonnage { get; set; }

        public Quest()
        {
        }

        public Quest(int Id, string Type, int? NvRequis, int? NbMonstresATuer, int? NbMonstresTues, string? TypeMonstre, int? TuileASeRendreX, int? TuileASeRendreY, bool Termine, int idPersonnage)
        {
            this.Id = Id;
            this.Type = Type;
            this.NvRequis = NvRequis;
            this.NbMonstresATuer = NbMonstresATuer;
            this.NbMonstresTues = NbMonstresTues;
            this.TypeMonstre = TypeMonstre;
            this.TuileASeRendreX = TuileASeRendreX;
            this.TuileASeRendreY = TuileASeRendreY;
            this.Termine = Termine;
            this.idPersonnage = idPersonnage;
        }
    }
}
