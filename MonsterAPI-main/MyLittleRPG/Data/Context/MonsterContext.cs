using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Monster> Monsters { get; set; } // factorie

        public DbSet<Tuile> Tuiles { get; set; } 

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }
    }
}
