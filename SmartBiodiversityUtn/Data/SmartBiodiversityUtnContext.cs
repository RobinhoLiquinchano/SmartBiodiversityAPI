using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Data
{
    public class SmartBiodiversityUtnContext : DbContext
    {
        public SmartBiodiversityUtnContext(DbContextOptions<SmartBiodiversityUtnContext> options)
           : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Bitacora> Bitacora { get; set; } = default!;
        public DbSet<Aporte> Aportes { get; set; } = default!;
        public DbSet<HistorialContrasena> HistorialContra { get; set; } = default!;
        public DbSet<Aviso> Avisos { get; set; } = default!;
        public DbSet<Token> Tokens { get; set; } = default!;
        public DbSet<Categoria> Categorias { get; set; } = default!;
        public DbSet<Especie> Especies { get; set; } = default!;
        public DbSet<Multimedia> Multimedia { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapear enums como int (igual que en SQL Server)
            modelBuilder.Entity<Aporte>()
                .Property(a => a.EstadoApo)
                .HasConversion<int>();

            modelBuilder.Entity<Especie>()
                .Property(e => e.EstadoEsp)
                .HasConversion<int>();

            // ---------- ROLES (IdRoles es string) ----------
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasIndex(r => r.NombreRol).IsUnique();
            });

            // ---------- USUARIOS (IdUsuario es string) ----------
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Correo).IsUnique();
                entity.HasOne(u => u.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(u => u.IdRolesU) // string
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- BITACORA (FKs: string) ----------
            modelBuilder.Entity<Bitacora>(entity =>
            {
                entity.HasOne(b => b.Usuario)
                      .WithMany(u => u.Bitacoras)
                      .HasForeignKey(b => b.IdUsuarioBit) // string
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Rol)
                      .WithMany(r => r.Bitacoras)
                      .HasForeignKey(b => b.IdRolesBit)   // string
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- APORTES (FK: string) ----------
            modelBuilder.Entity<Aporte>(entity =>
            {
                entity.HasOne(a => a.Usuario)
                      .WithMany(u => u.Aportes)
                      .HasForeignKey(a => a.IdUsuarioApo) // string
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- HISTORIALCONTRA (FK: string) ----------
            modelBuilder.Entity<HistorialContrasena>(entity =>
            {
                entity.HasOne(h => h.Usuario)
                      .WithMany(u => u.HistorialContrasenas)
                      .HasForeignKey(h => h.IdUsuarioHco) // string
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- AVISOS (FK: string) ----------
            modelBuilder.Entity<Aviso>(entity =>
            {
                entity.HasOne(a => a.Rol)
                      .WithMany(r => r.Avisos)
                      .HasForeignKey(a => a.IdRolesAvi)   // string
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- TOKENS (FK: string) ----------
            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasIndex(t => t.CodigoTok);

                entity.HasIndex(t => t.CorreoTok);

                entity.Property(t => t.CodigoTok)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(t => t.CorreoTok)
                      .HasMaxLength(100);

                entity.HasOne(t => t.Usuario)
                      .WithMany(u => u.Tokens)
                      .HasForeignKey(t => t.IdUsuarioTok)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);
            });

            // ---------- CATEGORIAS (IdCategorias es string) ----------
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasIndex(c => c.NombreCat).IsUnique();
            });

            // ---------- ESPECIES (FK: string) ----------
            modelBuilder.Entity<Especie>(entity =>
            {
                entity.HasOne(e => e.Categoria)
                      .WithMany(c => c.Especies)
                      .HasForeignKey(e => e.IdCategoriaEsp) // string
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- MULTIMEDIA (FK: string) ----------
            modelBuilder.Entity<Multimedia>(entity =>
            {
                entity.HasOne(m => m.Especie)
                      .WithMany(e => e.MultimediaArchivos)
                      .HasForeignKey(m => m.IdEspeciesMul)  // string
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
