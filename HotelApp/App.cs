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
        /* todo:
         * 1. add support for checking specific dates when making a booking so it only shows available rooms - DONE
         * 2. make sure you can't make a booking if there is already one already made for the same date(s) and room. - DONE cause of 1
         * 3. remove all bookings related when deleting a guest, this also means deleting invoices of the booking. 
         * 4. everytime the app is ran, check for any invoices that haven't been paid after the due date and archive them and remove the booking.
         * 
         */
        private DbContextOptionsBuilder<HotelContext> options;

        public App()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            options = new DbContextOptionsBuilder<HotelContext>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);

            using (var db = new HotelContext(options.Options))
            {
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
                //Console.WriteLine(BookingHandler.CheckRoomAvailability(db, 1, new DateTime(2023, 12, 24), new DateTime(2023, 12, 27)));
                //Console.ReadLine();
                //Console.WriteLine(db.Guest.Max(g => g.Id)); .Max.Contains(input)
                //var list = db.Guest.Where(g => g.Invoices.Any()).SelectMany(g => g.Invoices.Select(i => i.Id)).ToList();
                //var list2 = db.Invoice.Where(i => !i.IsArchived && !i.IsPaid).ToList();
                //    var list3 = new List<Invoice>();
                //    foreach (var i in db.Guest.ToList())
                //    {
                //        if (i.Invoices.Count > 0)
                //        {
                //            i.Invoices.ForEach(i => list3.Add(i));
                //        }
                //    }
                //    Console.ReadLine();
            }
        }
        public void Run()
        {
            Menu.MainMenu(new HotelContext(options.Options));
        }

        private void SeedData(HotelContext db)
        {
            if (!db.Guest.Any(g => g.Name.Contains("Hossén")))
            {
                db.Guest.Add(new Guest
                {
                    Name = "Hossén Rahimzadegan",
                    Address = "Yesgatan 3",
                    PhoneNumber = "1234567890",
                    IsActive = true
                });
            }
            if (!db.Guest.Any(g => g.Name.Contains("Ali")))
            {
                db.Guest.Add(new Guest
                {
                    Name = "Ali Rahimzadegan",
                    Address = "Yesgatan 3",
                    PhoneNumber = "0987654321",
                    IsActive = true
                });
            }
            if (!db.Guest.Any(g => g.Name.Contains("Vincent")))
            {
                db.Guest.Add(new Guest
                {
                    Name = "Vincent Wang",
                    Address = "Ligmagatan 21",
                    PhoneNumber = "8745123690",
                    IsActive = true
                });
            }
            if (!db.Guest.Any(g => g.Name.Contains("Lars")))
            {
                db.Guest.Add(new Guest
                {
                    Name = "Lars Verhulst",
                    Address = "Evergreen 742",
                    PhoneNumber = "2356987410",
                    IsActive = true
                });
            }
            if (!db.Room.Any(r => r.Size == 15))
            {
                db.Room.Add(new Room
                {
                    Size = 15,
                    Beds = 1,
                    GuestId = 0,
                    Price = 50,
                    IsOccupied = false
                });
            }
            if (!db.Room.Any(r => r.Size == 25))
            {
                db.Room.Add(new Room
                {
                    Size = 25,
                    Beds = 1,
                    GuestId = 0,
                    Price = 70,
                    IsOccupied = false
                });
            }
            if (!db.Room.Any(r => r.Size == 30))
            {
                db.Room.Add(new Room
                {
                    Size = 30,
                    Beds = 2,
                    GuestId = 0,
                    Price = 100,
                    IsOccupied = false
                });
            }
            if (!db.Room.Any(r => r.Size == 40))
            {
                db.Room.Add(new Room
                {
                    Size = 40,
                    Beds = 2,
                    GuestId = 0,
                    Price = 200,
                    IsOccupied = false
                });
            }
            db.SaveChanges();
        }
    }
}