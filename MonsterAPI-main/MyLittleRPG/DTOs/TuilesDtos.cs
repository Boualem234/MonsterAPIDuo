using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class TuilesDtos
    {
        public class TuileAvecMonstresDto
        {
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public TypeTuile Type { get; set; }
            public bool EstTraversable { get; set; }
            public string ImageURL { get; set; } = string.Empty;
            public MonstreDto Monstres { get; set; } = new();
        }

        public class MonstreDto
        {
            public int Id { get; set; }
            public int Niveau { get; set; }
            public int Force { get; set; }
            public int Defense { get; set; }
            public int HP { get; set; }
            public string SpriteUrl { get; set; } = string.Empty;
        }
    }
}
