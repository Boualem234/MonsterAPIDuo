using Microsoft.EntityFrameworkCore;

namespace MyLittleRPG_ElGuendouz.Models
{
    [PrimaryKey(nameof(PositionX), nameof(PositionY))]
    public class InstanceMonstre
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int monstreID { get; set; }
        public int niveau { get; set; }
        public int pointsVieMax { get; set; }
        public int pointsVieActuels { get; set; }

        public InstanceMonstre() { }

        public InstanceMonstre(int positionX, int positionY, int monstreID, int niveau, int pointsVieMax, int pointsVieActuels)
        {
            PositionX = positionX;
            PositionY = positionY;
            this.monstreID = monstreID;
            this.niveau = niveau;
            this.pointsVieMax = pointsVieMax;
            this.pointsVieActuels = pointsVieActuels;

        }
    }
}
