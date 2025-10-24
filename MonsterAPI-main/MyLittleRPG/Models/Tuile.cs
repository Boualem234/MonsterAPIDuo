using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyLittleRPG_ElGuendouz.Models
{
    public enum TypeTuile
    {
        HERBE = 0,
        EAU = 1,
        MONTAGNE = 2,
        FORET = 3,
        VILLE = 4,
        ROUTE = 5
    }

    [PrimaryKey(nameof(PositionX), nameof(PositionY))]
    public class Tuile
    {
        private const int MAX_POS_X = 50;
        private const int MAX_POS_Y = 50;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public TypeTuile Type { get; set; }
        public bool EstTraversable { get; set; }
        public string ImageURL { get; set; }

        public Tuile() { }

        public Tuile(int positionX, int positionY, TypeTuile type, bool estTraversable, string imageURL)
        {
            if (positionX < 0 || positionX > MAX_POS_X)
                throw new ArgumentOutOfRangeException(nameof(positionX), "PositionX doit être compris entre 0 et 50.");
            if (positionY < 0 || positionY > MAX_POS_Y)
                throw new ArgumentOutOfRangeException(nameof(positionY), "PositionY doit être compris entre 0 et 50.");

            this.PositionX = positionX;
            this.PositionY = positionY;
            this.Type = type;
            this.EstTraversable = estTraversable;
            this.ImageURL = imageURL;
        }
    }
}
