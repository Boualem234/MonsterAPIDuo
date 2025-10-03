namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class CombatResultDto
    {
        public bool Combat { get; set; }
        public bool Resultat { get; set; }
        public string Message { get; set; } = string.Empty;
        public CharacterStateDto Character { get; set; } = new CharacterStateDto();
        public MonstreStateDto? Monstre { get; set; }
    }
}