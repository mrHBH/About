using Microsoft.EntityFrameworkCore;
using HBHplayground.Abstractions;
using Stl.Fusion.EntityFramework;

namespace HBHplayground.Server.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<ChatUser> ChatUsers { get; protected set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; protected set; } = null!;
        public DbSet<DbOperation> Operations { get; protected set; } = null!;

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}
