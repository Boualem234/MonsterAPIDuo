namespace MyLittleRPG_ElGuendouz.DTOs
{
    public class DeplacementResultDto
    {
        public bool Combat { get; set; }
        public CharacterStateDto Character { get; set; } = new CharacterStateDto();
    }
}