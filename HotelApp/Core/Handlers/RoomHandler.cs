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
            Menu.RequestEntry("Write the size of the room: ", ref input);
            if (input == 0) return;
            room.Size = input;
            Menu.RequestEntry("Write the amount of beds in the room: ", ref input);
            room.Beds = input;
            Menu.RequestEntry("Write the price of the room: ", ref input);
            room.Price = input;
            db.Room.Add(room);
            db.SaveChanges();
        }

        public static void Delete(HotelContext db)
        {
            Room? room = null;
            string? confirmInput = string.Empty;
            int roomIndex = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a room\n ");
            Console.WriteLine("0. Back");
            var allRooms = db.Room.ToList();
            range = allRooms.Count;
            if (range == 0) return;
            Console.WriteLine("Index - ID - Size - Beds");
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allRooms[i - 1].Id} - {allRooms[i - 1].Size} - {allRooms[i - 1].Beds}");
            }
            Menu.RequestEntryWithinRange("Which room would you like to delete? ", ref roomIndex, range);
            if (roomIndex > 0)
            {
                room = db.Room.First(cr => cr.Id == allRooms[roomIndex - 1].Id);
                Menu.RequestEntry($"Are you sure you want to delete {room.Id}?(y/n) ", ref confirmInput);
                if (confirmInput.ToLower() == "y")
                {
                    db.Room.Remove(room);
                    db.SaveChanges();
                }
            }
        }

        public static void Update(HotelContext db)
        {
            Room? room = null;
            int roomIndex = -1, input = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a new room\n ");
            Console.WriteLine("0. Back");
            var allRooms = db.Room.ToList();
            range = allRooms.Count;
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allRooms[i - 1].Id} - {allRooms[i - 1].Size} - {allRooms[i - 1].Beds} - {allRooms[i - 1].Price}");
            }
            Menu.RequestEntryWithinRange("Which room would you like to edit? ", ref roomIndex, range);
            if (roomIndex == 0) return;
            room = db.Room.First(cr => cr.Id == allRooms[roomIndex - 1].Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {room.Id}\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Size");
            Console.WriteLine("2. Beds");
            Console.WriteLine("3. Price");
            range = 3;
            Menu.RequestEntryWithinRange("Which part would you like to edit? ", ref roomIndex, range);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Room {room.Id} \n ");
            switch (roomIndex)
            {
                case 1:
                    //Menu.RequestEntry("What would you like to change the size to? ", ref input);
                    Console.Write("What would you like to change the size to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine($"Please enter a value above 0kr");
                    }
                    room.Size = input;
                    break;
                case 2:
                    //Menu.RequestEntry("What would you like to change the amount of beds to? ", ref input);
                    Console.Write("What would you like to change the amount of beds to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine($"Please enter a value above 0kr");
                    }
                    room.Beds = input;
                    break;
                case 3:
                    //Menu.RequestEntry("What would you like to change the price to? ", ref input);
                    Console.Write("What would you like to change the price to? ");
                    while (!int.TryParse(Console.ReadLine(), out input) || input <= 0)
                    {
                        if (input == 0) return;
                        Console.WriteLine($"Please enter a value above 0kr");
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
