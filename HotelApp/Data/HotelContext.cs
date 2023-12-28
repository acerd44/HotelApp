using HotelApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class HotelContext : DbContext
    {
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Guest> Guest { get; set; }

        public HotelContext()
        {
        }
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=localhost;Database=HossenHotel;Trusted_Connection=True;TrustServerCertificate=true");
            }
        }
    }
}