namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class MonstreStateDto
    {
        public int Pv { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int Niveau { get; set; }
        public string SpriteUrl { get; set; } = string.Empty;
    }
}