using Azure.Core;
using HotelApp.Core;
using HotelApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class GuestHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            var guest = new Guest();
            string? input = string.Empty;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new guest\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("If you'd like to skip the prompt, just input a minus (-)\n");
            //Menu.RequestEntry("Write the name of the guest: ", ref input);
            Console.Write("Enter the name of the guest: ");
            input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input) || input == "0")
            {
                if (input == "0") return;
                Console.WriteLine("Please follow the instructions.");
                input = Console.ReadLine();
            }
            guest.Name = input;
            //Menu.RequestEntry("Write the address of the guest: ", ref input);
            Console.Write("Enter the address of the guest: ");
            input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input) || input == "0")
            {
                if (input == "0") return;
                Console.WriteLine("Please follow the instructions.");
                input = Console.ReadLine();
            }
            guest.Address = input;
            //Menu.RequestEntry("Write the e-mail of the guest: ", ref input);
            //guest.Email = input;
            //Menu.RequestEntry("Write the phone number of the guest: ", ref input);
            Console.Write("Enter the phone number of the guest: ");
            input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input) || input == "0")
            {
                if (input == "0") return;
                Console.WriteLine("Please follow the instructions.");
                input = Console.ReadLine();
            }
            guest.PhoneNumber = input;
            guest.IsActive = true;
            db.Guest.Add(guest);
            db.SaveChanges();
        }

        public static void Delete(HotelContext db)
        {
            Guest? guest = null;
            string? confirmInput = string.Empty;
            int guestIndex = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a guest\n ");
            Console.WriteLine("0. Back");
            var allGuests = db.Guest.ToList();
            range = allGuests.Count;
            if (range == 0) return;
            Console.WriteLine("Index - ID - Name");
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allGuests[i - 1].Id} - {allGuests[i - 1].Name}");
            }
            //Menu.RequestEntryWithinRange("Which guest would you like to delete? ", ref guestIndex, range);
            Console.Write("Which guest would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out guestIndex) || !Enumerable.Range(0, range + 1).Contains(guestIndex))
            {
                if (guestIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            if (guestIndex > 0)
            {
                guest = db.Guest.First(cr => cr.Id == allGuests[guestIndex - 1].Id);
                //Menu.RequestEntry($"Are you sure you want to delete {guest.Name} as a guest?(y/n/h for hard delete) ", ref confirmInput);
                Console.Write($"Are you sure you want to delete {guest.Name} as a guest?(y/n/h for hard delete) ");
                while (string.IsNullOrWhiteSpace(confirmInput) || string.IsNullOrEmpty(confirmInput)
                    || !(confirmInput.ToLower() == "y" || confirmInput.ToLower() == "n" || confirmInput == "h"))
                {
                    Console.WriteLine("Please follow the instructions.");
                    confirmInput = Console.ReadLine();
                }
                if (confirmInput.ToLower() == "y")
                {
                    guest.IsActive = false;
                }
                else if (confirmInput.ToLower() == "h")
                {
                    db.Guest.Remove(guest);
                }
                db.SaveChanges();
            }
        }

        public static void Update(HotelContext db)
        {
            Guest? guest = null;
            string? input = string.Empty;
            int guestIndex = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a new guest\n ");
            Console.WriteLine("0. Back");
            var allGuests = db.Guest.ToList();
            range = allGuests.Count;
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allGuests[i - 1].Id} - {allGuests[i - 1].Name} - active: {allGuests[i - 1].IsActive.ToString().ToLower()}");
            }
            //Menu.RequestEntryWithinRange("Which guest would you like to edit? ", ref guestIndex, range);
            Console.Write("Which guest would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out guestIndex) || !Enumerable.Range(0, range + 1).Contains(guestIndex))
            {
                if (guestIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            guest = db.Guest.First(cr => cr.Id == allGuests[guestIndex - 1].Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting {guest.Name}r\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Name");
            Console.WriteLine("2. Address");
            Console.WriteLine("3. Phone number");
            range = 3;
            if (!guest.IsActive)
            {
                Console.WriteLine("4. Activate");
                range = 4;
            }
            //Menu.RequestEntryWithinRange("Which part would you like to edit? ", ref guestIndex, range);
            Console.Write("Which part would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out guestIndex) || !Enumerable.Range(0, range + 1).Contains(guestIndex))
            {
                if (guestIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting {guest.Name}\n ");
            switch (guestIndex)
            {
                case 1:
                    Menu.RequestEntry("What would you like to change their name to? ", ref input);
                    if (input == "0") return;
                    guest.Name = input;
                    break;
                case 2:
                    Menu.RequestEntry("What would you like to change their address to? ", ref input);
                    if (input == "0") return;
                    guest.Address = input;
                    break;
                case 3:
                    Menu.RequestEntry("What would you like to change their phone number to? ", ref input);
                    if (input == "0") return;
                    guest.PhoneNumber = input;
                    break;
                case 4:
                    Menu.RequestConfirmation("Would you like to activate this guest?(y/n) ", ref input);
                    if (input.ToLower() == "y" && !guest.IsActive)
                        guest.IsActive = true;
                    break;
            }
            db.SaveChanges();
        }

        public static void ShowAll(HotelContext db)
        {
            int input = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all guests\n ");
            var allGuests = db.Guest.ToList();
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Show all guests");
            Console.WriteLine("2. Only show active guests");
            Console.WriteLine("3. Only show inactive guests");
            Menu.RequestEntryWithinRange("", ref input, 3);
            Console.WriteLine("\nId - Name");
            switch (input)
            {
                case 1:
                    allGuests.ForEach(g => Console.WriteLine($"{g.Id}. {g.Name} - active: {g.IsActive.ToString().ToLower()}"));
                    break;
                case 2:
                    allGuests.Where(g => g.IsActive).ToList().ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
                    break;
                case 3:
                    allGuests.Where(g => !g.IsActive).ToList().ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
                    break;
                case 0:
                    return;
            }
            Console.ReadKey();
        }
    }
}
