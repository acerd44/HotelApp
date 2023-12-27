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

         * 3. remove all bookings related when deleting a guest, this also means deleting invoices of the booking. 
         * 4. everytime the app is ran, check for any invoices that haven't been paid after the due date and archive them and remove the booking.
         * 7. make sure showAll invoices just checks for invoices in general instead of guests with invoices?
         * 8. add isarchived checks for bookings and invoices

         * 1. add support for checking specific dates when making a booking so it only shows available rooms - DONE
         * 2. make sure you can't make a booking if there is already one already made for the same date(s) and room. - DONE cause of 1
         * 5. When checking availbility while editting booking dates or rooms, make sure to exclude the booking itself. - DONE
         * 6. when editting endDate, make sure to lower the totalSum by how many less days the booking is active for. - DONE?
         */
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
            if (!db.Booking.Any(b => b.StartDate.Date != new DateTime(2023, 12, 20)))
            {
                db.Booking.Add(new Booking
                {
                    GuestId = 3,
                    RoomId = 4,
                    StartDate = new DateTime(2023, 12, 20),
                    EndDate = new DateTime(2023, 12, 30),
                    IsActive = true,
                    IsArchived = false
                });
                db.Guest.First(g => g.Id == 3).Invoices.Add(new Invoice
                {
                    GuestId = 3,
                    BookingId = 1,
                    TotalSum = db.Room.First(r => r.Id == 4).Price * 10,
                    PaidDate = new DateTime(2023, 12, 20),
                    DueDate = new DateTime(2023, 12, 20).AddDays(10),
                    IsPaid = true
                });
            }
            if (!db.Booking.Any(b => b.StartDate.Date != new DateTime(2023, 12, 18)))
            {
                db.Booking.Add(new Booking
                {
                    GuestId = 3,
                    RoomId = 4,
                    StartDate = new DateTime(2023, 12, 18),
                    EndDate = new DateTime(2023, 12, 20),
                    IsActive = true
                });
                db.Guest.First(g => g.Id == 3).Invoices.Add(new Invoice
                {
                    GuestId = 3,
                    BookingId = 2,
                    TotalSum = db.Room.First(r => r.Id == 4).Price * 3,
                    PaidDate = new DateTime(2023, 12, 19),
                    DueDate = new DateTime(2023, 12, 18).AddDays(10),
                    IsPaid = true,
                });
            }
            db.SaveChanges();
        }
    }
}