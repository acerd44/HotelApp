using Azure;
using ConsoleTables;
using HotelApp.Core;
using HotelApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class RoomHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            var room = new Room();
            int input;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new room\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("\nSize to bed(+extra beds) conversion:\n 40: 2+2 - 30-39: 2+1 - 15-29: 1+0\n");
            Console.Write("Write the size of the room:(15-40) ");
            while (!int.TryParse(Console.ReadLine(), out input) || !(input >= 15 && input <= 40))
            {
                if (input == 0) return;
                Console.WriteLine($"Please enter a size between 15-40");
            }
            room.Size = input;
            SetBeds(ref input, ref room); // Adjust the beds and extra beds that can be in the room
            Console.WriteLine($"The room will have {room.Beds} beds and can have {room.ExtraBeds} extra beds.");
            Console.Write("Write the price(per day) of the room: ");
            while (!int.TryParse(Console.ReadLine(), out input) || input == 0)
            {
                if (input == 0) return;
                Console.WriteLine($"Please enter a number.");
            }
            room.Price = input;
            db.Room.Add(room);
            db.SaveChanges();
        }
        public static void Delete(HotelContext db)
        {
            var allRooms = db.Room.ToList();
            if (allRooms.Count == 0) return;
            int roomIndex;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a room\n ");
            Console.WriteLine("0. Back");
            var table = new ConsoleTable("Id", "Size", "Beds+Extra Beds", "Price per day");
            table.Options.EnableCount = false;
            allRooms.ForEach(r => table.AddRow(r.Id, r.Size + "m^2", r.Beds + "+" + r.ExtraBeds, r.Price + "kr"));
            table.Write();
            Console.Write("Which room would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !allRooms.Any(r => r.Id == roomIndex))
            {
                if (roomIndex == 0) return;
                Console.WriteLine($"Please enter an option.");
            }
            Room selectedRoom = db.Room.First(cr => cr.Id == roomIndex);
            var bookingsWithRoom = db.Booking.Include(b => b.Room).Where(b => b.Room == selectedRoom).ToList(); // Get a list of bookings of the room
            if (bookingsWithRoom.Count > 0)
            {
                Console.WriteLine("\nBEWARE: There are bookings with this room, deleting the room will also HARD DELETE the bookings and invoices connected to the room\n");
            }
            Console.Write($"Are you sure you want to delete room {selectedRoom.Id}?(y/n) ");
            string? confirmInput = Console.ReadLine().ToLower();
            while (!(confirmInput.Equals("y") || confirmInput.Equals("n")))
            {
                Console.WriteLine("Please enter Y/N");
                confirmInput = Console.ReadLine().ToLower();
            }
            if (confirmInput.Equals("y"))
            {
                if (bookingsWithRoom.Count > 0)
                {
                    bookingsWithRoom.ForEach(b =>
                    {
                        var invoice = db.Invoice.Include(i => i.Booking).First(i => i.Booking == b);
                        db.Invoice.Remove(invoice);
                        db.Booking.Remove(b);
                    });
                }
                db.Room.Remove(selectedRoom);
                db.SaveChanges();
            }
        }
        public static void Update(HotelContext db)
        {
            var allRooms = db.Room.ToList();
            if (allRooms.Count == 0) return;
            int roomIndex, input;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a room\n ");
            Console.WriteLine("0. Back");
            var table = new ConsoleTable("Id", "Size", "Beds+Extra Beds", "Price per day");
            table.Options.EnableCount = false;
            allRooms.ForEach(r => table.AddRow(r.Id, r.Size + "m^2", r.Beds + "+" + r.ExtraBeds, r.Price + "kr"));
            table.Write();
            Console.Write("Which room would you like to edit?");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !allRooms.Any(r => r.Id == roomIndex))
            {
                if (roomIndex == 0) return;
                Console.WriteLine("Please enter an option");
            }
            if (roomIndex == 0) return;
            Room selectedRoom = db.Room.First(cr => cr.Id == roomIndex);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {selectedRoom.Id}\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Size (affects amount of beds)");
            Console.WriteLine("2. Price per day");
            Console.Write("Which part would you like to edit?");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !Enumerable.Range(0, 3).Contains(roomIndex))
            {
                if (roomIndex == 0) return;
                Console.WriteLine("Please enter an option (0-2)");
            }
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {selectedRoom.Id} \n ");
            switch (roomIndex)
            {
                case 1:
                    Console.WriteLine("Size to bed(+extra beds) conversion:\n 40: 2+2 - 30-39: 2+1 - 15-29: 1+0\n");
                    Console.Write("What would you like to change the size to?(15-40) ");
                    while (!int.TryParse(Console.ReadLine(), out input) || !(input >= 15 && input <= 40))
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please follow the instructions.");
                    }
                    selectedRoom.Size = input;
                    SetBeds(ref input, ref selectedRoom);
                    break;
                case 2:
                    Console.Write("What would you like to change the price to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter a value above 0kr");
                    }
                    selectedRoom.Price = input;
                    break;
            }
            db.SaveChanges();
        }
        public static void ShowAll(HotelContext db)
        {
            var allRooms = db.Room.ToList();
            if (allRooms.Count == 0) return;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all rooms\n ");
            var table = new ConsoleTable("Id", "Size", "Beds+Extra Beds", "Price per day");
            allRooms.ForEach(r => table.AddRow(r.Id, r.Size + "m^2", r.Beds + "+" + r.ExtraBeds, r.Price + "kr"));
            table.Write();
            Console.WriteLine("\nPress any button to continue.");
            Console.ReadKey();
        }
        /// <summary>
        /// Adjusts the amount of beds of a <paramref name="room"/> based on its <paramref name="size"/>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="room"></param>
        private static void SetBeds(ref int size, ref Room room)
        {
            if (size == 40)
            {
                room.Beds = 2;
                room.ExtraBeds = 2;
            }
            else if (size >= 30 && size < 40)
            {
                room.Beds = 2;
                room.ExtraBeds = 1;
            }
            else if (size < 30) room.Beds = 1;
        }
        /// <summary>
        /// Gets the available rooms based on the amount of guests and the two dates. for example if there are 3 guests, it'll give you rooms that can take at least 3 guests
        /// and are available between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="guests"></param>
        /// <returns>a List of Room</returns>
        public static List<Room> GetAvailableRooms(HotelContext db, DateTime startDate, DateTime endDate, int guests)
        {
            var recommendedRooms = new List<Room>();
            var allRooms = db.Room.ToList();
            if (guests == 1)
            {
                allRooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate))
                    .ToList()
                    .ForEach(recommendedRooms.Add);
            }
            else
            {
                if (guests == 4)
                {
                    allRooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate) && r.ExtraBeds == 2)
                        .ToList()
                        .ForEach(recommendedRooms.Add);
                }
                else
                {
                    allRooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate) && r.ExtraBeds >= 1)
                        .ToList()
                        .ForEach(recommendedRooms.Add);
                }
            }
            return recommendedRooms;
        }
        public static List<Room> GetAvailableRooms(HotelContext db, DateTime startDate, DateTime endDate)
        {
            var rooms = db.Room.ToList();
            return rooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate)).ToList();
        }
        /// <summary>
        /// Checks if the room related to the <paramref name="roomId"/> is available between <paramref name="startDate"/> and <paramref name="endDate"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="roomId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool CheckSpecificRoomAvailability(HotelContext db, int roomId, DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue) return false; // Return false in case there is no given startDate or endDate
            return !db.Booking
                .Where(b => b.RoomId == roomId)
                .Any(b =>
                (startDate >= b.StartDate && startDate <= b.EndDate)
                || (endDate >= b.StartDate && endDate <= b.EndDate)
                || (startDate <= b.StartDate && endDate >= b.EndDate));
        }
        public static bool CheckSpecificRoomAvailability(HotelContext db, int roomId, DateTime startDate, DateTime endDate, int bookingToExlude)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue) return false; // Return false in case there is no given startDate or endDate
            return !db.Booking
                .Where(b => b.RoomId == roomId && b.Id != bookingToExlude)
                .Any(b => (startDate >= b.StartDate && startDate <= b.EndDate)
                || (endDate >= b.StartDate && endDate <= b.EndDate)
                || (startDate <= b.StartDate && endDate >= b.EndDate));
        }

    }
}