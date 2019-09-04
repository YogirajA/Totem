using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Totem.Models;

namespace Totem.Infrastructure
{
    public class TotemContext : DbContext
    {
        public TotemContext(DbContextOptions<TotemContext> options)
            : base(options)
        {
        }

        public DbSet<Contract> Contract { get; set; }
        public DbSet<ContractSchema> ContractSchema { get; set; }
    }

    public static class DbSetExtensions
    {
        public static EntityEntry<T> AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }
    }
}
