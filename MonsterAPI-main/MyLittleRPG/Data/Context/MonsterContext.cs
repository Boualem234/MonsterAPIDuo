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
        public DbSet<Quest> Quest { get; set; }

        public MonsterContext(DbContextOptions<MonsterContext> options) : base(options) { }

        // Configuration des relations et des contraintes
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration de l'index unique sur l'email de l'utilisateur
            modelBuilder.Entity<User>()
                .HasIndex(u => u.email)
                .IsUnique();

            // Configuration de la relation entre Character et User
            modelBuilder.Entity<Character>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.utilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de la relation entre InstanceMonstre et Monster
            modelBuilder.Entity<InstanceMonstre>()
                .HasOne<Monster>()
                .WithMany()
                .HasForeignKey(m => m.monstreID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuration de la relation entre Quest et Character
            modelBuilder.Entity<Quest>()
                .HasOne<Character>()
                .WithMany()
                .HasForeignKey(q => q.idPersonnage)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // Méthode pour vérifier si un utilisateur existe et est connecté
        public (bool, User?) DoesExistAndConnected(string email)
        {
            try
            {
                return (User.Any(u => u.email == email && u.isConnected == true), User.First(u => u.email == email));
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return (false, null);
            }
        }
    }
}
