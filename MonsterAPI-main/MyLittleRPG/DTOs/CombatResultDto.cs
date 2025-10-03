namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class CombatResultDto
    {
        public bool Combat { get; set; }
        public string? Resultat { get; set; }
        public CharacterStateDto Character { get; set; } = new CharacterStateDto();
        public MonstreStateDto? Monstre { get; set; }
    }
}