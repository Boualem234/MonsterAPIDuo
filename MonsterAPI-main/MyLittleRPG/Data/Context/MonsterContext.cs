using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Data.Context
{
    public class MonsterContext : DbContext
    {
        public DbSet<Monster> Monsters { get; set; }

        public DbSet<Tuile> Tuiles { get; set; }
        public DbSet<Character> Character { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<InstanceMonstre> InstanceMonstre { get; set; }

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<Character>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.utilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InstanceMonstre>()
                .HasOne<Monster>()
                .WithMany()
                .HasForeignKey(m => m.monstreID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public (bool, User) DoesExistAndConnected(string email)
        {
            return (User.Any(u => u.email == email && u.isConnected == true), User.First(u => u.email == email));
        }
    }
}
