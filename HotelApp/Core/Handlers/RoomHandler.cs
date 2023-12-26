using Azure;
using HotelApp.Core;
using HotelApp.Data;
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
            int input = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new room\n ");
            Console.WriteLine("0. Back");
            Console.Write("Write the size of the room:(15-40) ");
            while (!int.TryParse(Console.ReadLine(), out input) || (input >= 15 && input <= 40))
            {
                if (input == 0) return;
                Console.WriteLine($"Please enter a size between 15-40");
            }
            room.Size = input;
            Console.Write("Write the amount of beds in the room:(1-4) ");
            while (!int.TryParse(Console.ReadLine(), out input) || (input >= 1 && input <= 4))
            {
                if (input == 0) return;
                Console.WriteLine($"Please enter a size between 1-4");
            }
            room.Beds = input;
            Console.Write("Write the price of the room: ");
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
            int roomIndex = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a room\n ");
            Console.WriteLine("0. Back");
            var allRooms = db.Room.ToList();
            if (allRooms.Count == 0) return;
            Console.WriteLine("Index - ID - Size - Beds");
            allRooms.ForEach(r => Console.WriteLine($"{r.Id}. {r.Size} - {r.Beds} - {r.Price}"));
            Console.Write("Which room would you like to delete?");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !allRooms.Any(r => r.Id == roomIndex))
            {
                if (roomIndex == 0) return;
                Console.WriteLine($"Please enter an option.");
            }
            Room room = db.Room.First(cr => cr.Id == allRooms[roomIndex - 1].Id);
            Console.Write($"Are you sure you want to delete {room.Id}?(y/n) ");
            string? confirmInput = Console.ReadLine().ToLower();
            while (confirmInput != "y" && confirmInput != "n")
            {
                Console.WriteLine("Please enter Y/N");
                confirmInput = Console.ReadLine();
            }
            if (confirmInput.ToLower() == "y")
            {
                db.Room.Remove(room);
                db.SaveChanges();
            }
        }

        public static void Update(HotelContext db)
        {
            int roomIndex = -1, input = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a new room\n ");
            Console.WriteLine("0. Back");
            var allRooms = db.Room.ToList();
            if (allRooms.Count == 0) return;
            allRooms.ForEach(r => Console.WriteLine($"{r.Id}. {r.Size} - {r.Beds} - {r.Price}"));
            Console.Write("Which room would you like to edit?");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !allRooms.Any(r => r.Id == input))
            {
                if (roomIndex == 0) return;
                Console.WriteLine("Please enter an option");
            }
            if (roomIndex == 0) return;
            Room room = db.Room.First(cr => cr.Id == allRooms[roomIndex - 1].Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {room.Id}\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Size");
            Console.WriteLine("2. Beds");
            Console.WriteLine("3. Price");
            Console.Write("Which part would you like to edit?");
            while (!int.TryParse(Console.ReadLine(), out roomIndex) || !Enumerable.Range(0, 4).Contains(roomIndex))
            {
                if (roomIndex == 0) return;
                Console.WriteLine("Please enter an option (0-3)");
            }
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {room.Id} \n ");
            switch (roomIndex)
            {
                case 1:
                    Console.Write("What would you like to change the size to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter a value above 0kr");
                    }
                    room.Size = input;
                    break;
                case 2:
                    Console.Write("What would you like to change the amount of beds to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter a value above 0kr");
                    }
                    room.Beds = input;
                    break;
                case 3:
                    Console.Write("What would you like to change the price to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine("Please enter a value above 0kr");
                    }
                    room.Price = input;
                    break;
            }
            db.SaveChanges();
        }

        public static void ShowAll(HotelContext db)
        {
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all rooms\n ");
            Console.WriteLine("Id - Size - Beds - Price");
            db.Room.ToList().ForEach(r => Console.WriteLine($"{r.Id}. {r.Size} - {r.Beds} - {r.Price}"));
            Console.ReadKey();
        }

    }
}
