using DistSysACW.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DistSysACW.Data
{
    public class UserContext : DbContext
    {
        public UserContext() : base()
        {

        }

        public DbSet<User> Users { get; set; }

        //TODO: Task13

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistSysACW;");
        }
    }
}
