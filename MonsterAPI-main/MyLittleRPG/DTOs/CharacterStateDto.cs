namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class CharacterStateDto
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Pv { get; set; }
        public int PvMax { get; set; }
        public int Niveau { get; set; }
        public int Exp { get; set; }
        public int Force { get; set; }
        public int Def { get; set; }
        public string Nom { get; set; } = string.Empty;
    }
}