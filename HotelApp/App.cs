using HotelApp.Core;
using HotelApp.Core.Handlers;
using HotelApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp
{
    public class App
    {
        private DbContextOptionsBuilder<HotelContext> options;
        public App()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection");
            options = new DbContextOptionsBuilder<HotelContext>();
            options.UseSqlServer(connectionString);
            using (var db = new HotelContext(options.Options))
            {
                // if database exists then seed first then migrate otherwise other way around
                if ((db.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
                {
                    SeedData(db);
                    db.Database.Migrate();
                }
                else
                {
                    db.Database.Migrate();
                    SeedData(db);
                }
                // activate/deactivate/archive bookings and invoices when needed
                db.Booking.Where(b => !b.IsArchived)
                    .ToList()
                    .ForEach(b =>
                    {
                        var invoice = db.Invoice.First(i => i.BookingId == b.Id);
                        if (!invoice.IsPaid && DateTime.Today >= invoice.DueDate)
                        {
                            invoice.IsArchived = true;
                            b.IsActive = false;
                            b.IsArchived = true;
                        }
                        if (DateTime.Today >= b.EndDate)
                        {
                            b.IsActive = false;
                            b.IsArchived = true;
                        }
                        else if (DateTime.Today >= b.StartDate)
                        {
                            b.IsActive = true;
                        }
                    });
                db.SaveChanges();
            }
        }
        public void Run()
        {
            Menu.MainMenu(new HotelContext(options.Options));
        }
        private void SeedData(HotelContext db)
        {
            if (!db.Guest.Any())
            {
                db.Guest.Add(new Guest
                {
                    Name = "Hossén Rahimzadegan",
                    Address = "Yesgatan 3",
                    PhoneNumber = "1234567890",
                    IsActive = true
                });
                db.Guest.Add(new Guest
                {
                    Name = "Ali Rahimzadegan",
                    Address = "Yesgatan 3",
                    PhoneNumber = "0987654321",
                    IsActive = true
                });
                db.Guest.Add(new Guest
                {
                    Name = "Vincent Wang",
                    Address = "Ligmagatan 21",
                    PhoneNumber = "8745123690",
                    IsActive = true
                });
                db.Guest.Add(new Guest
                {
                    Name = "Lars Verhulst",
                    Address = "Evergreen 742",
                    PhoneNumber = "2356987410",
                    IsActive = true
                });
            }
            if (!db.Room.Any())
            {
                db.Room.Add(new Room
                {
                    Size = 15,
                    Beds = 1,
                    Price = 50
                });
                db.Room.Add(new Room
                {
                    Size = 25,
                    Beds = 1,
                    Price = 70
                });
                db.Room.Add(new Room
                {
                    Size = 30,
                    Beds = 2,
                    ExtraBeds = 1,
                    Price = 100
                });
                db.Room.Add(new Room
                {
                    Size = 40,
                    Beds = 2,
                    ExtraBeds = 2,
                    Price = 200
                });
            }
            db.SaveChanges();
        }
    }
}