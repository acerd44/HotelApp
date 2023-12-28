using Azure;
using Azure.Core;
using HotelApp.Core;
using HotelApp.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class GuestHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            var guest = new Guest();
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new guest\n ");
            Console.WriteLine("0. Back");
            Console.Write("Enter the name of the guest: ");
            string? input = Console.ReadLine();
            int numberInput;
            while (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input) || input.Equals("0"))
            {
                if (input.Equals("0")) return;
                Console.WriteLine("Please follow the instructions.");
                input = Console.ReadLine();
            }
            // Removes white space and makes sure spaces between words aren't being removed. Also capitalizes first letter of the word(s)
            guest.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Trim().ToLower());
            Console.Write("Enter the address of the guest:(optional, enter a - if you do not wish to provide an address) ");
            input = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(input) || string.IsNullOrEmpty(input) || input.Equals("0"))
            {
                if (input.Equals("0")) return;
                Console.WriteLine("Please follow the instructions.");
                input = Console.ReadLine();
            }
            guest.Address = input;
            Console.Write("Enter the phone number of the guest (between 6 and 12 digits): ");
            while (!int.TryParse(Console.ReadLine(), out numberInput) || numberInput == 0 || !Regex.IsMatch(numberInput.ToString(), @"^\d{6,12}$"))
            {
                if (numberInput == 0) return;
                Console.WriteLine("Please follow the instructions. ");
            }
            guest.PhoneNumber = numberInput.ToString();
            guest.IsActive = true;
            db.Guest.Add(guest);
            db.SaveChanges();
        }

        public static void Delete(HotelContext db)
        {
            var allGuests = db.Guest.ToList();
            if (allGuests.Count == 0) return;
            int guestIndex;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a guest\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("ID - Name");
            allGuests.ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
            Console.Write("Which guest would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out guestIndex) || !allGuests.Any(g => g.Id == guestIndex))
            {
                if (guestIndex == 0) return;
                Console.WriteLine($"Please enter an option");
            }
            Guest guest = db.Guest.First(cr => cr.Id == allGuests[guestIndex - 1].Id);
            Console.WriteLine("\nKeep in mind this will delete bookings/invoices related to the guest.\n");
            Console.Write($"Are you sure you want to delete {guest.Name} as a guest?(y/n/h for hard delete) ");
            string? confirmInput = Console.ReadLine().ToLower();
            while (string.IsNullOrWhiteSpace(confirmInput) || string.IsNullOrEmpty(confirmInput)
                || !(confirmInput.Equals("y") || confirmInput.Equals("n") || confirmInput.Equals("h")))
            {
                Console.WriteLine("Please follow the instructions.");
                confirmInput = Console.ReadLine();
            }
            var guestBookings = db.Booking.Where(b => b.Id == guest.Id).ToList();
            var guestInvoices = db.Invoice.Where(i => i.Id == guest.Id).ToList();
            if (confirmInput.Equals("y"))
            {
                guestBookings.ForEach(b =>
                {
                    b.IsActive = false;
                    b.IsArchived = true;
                });
                guestInvoices.ForEach(i => i.IsArchived = true);
                guest.IsActive = false;
            }
            if (confirmInput.Equals("h"))
            {
                guestInvoices.ForEach(i => db.Invoice.Remove(i));
                guestBookings.ForEach(b => db.Booking.Remove(b));
                db.Guest.Remove(guest);
            }
            db.SaveChanges();
        }

        public static void Update(HotelContext db)
        {
            var allGuests = db.Guest.ToList();
            if (allGuests.Count == 0) return;
            string? input = string.Empty;
            int guestIndex;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a new guest\n ");
            Console.WriteLine("0. Back");
            allGuests.ForEach(g => Console.WriteLine($"{g.Id}. {g.Name} - active: {g.IsActive.ToString().ToLower()}"));
            Console.Write("Which guest would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out guestIndex) || !allGuests.Any(g => g.Id == guestIndex))
            {
                if (guestIndex == 0) return;
                Console.WriteLine($"Please enter an option");
            }
            Guest guest = db.Guest.First(cr => cr.Id == allGuests[guestIndex - 1].Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting {guest.Name}r\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Name");
            Console.WriteLine("2. Address");
            Console.WriteLine("3. Phone number");
            int range = 3;
            if (!guest.IsActive)
            {
                Console.WriteLine("4. Activate");
                range = 4;
            }
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
                    Console.Write("What would you like to change their name to? ");
                    while (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input) || input.Any(char.IsDigit))
                    {
                        if (input.Equals("0")) return;
                        Console.WriteLine("Please enter a name without numbers.");
                    }
                    guest.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Trim().ToLower());
                    break;
                case 2:
                    Console.Write("What would you like to change their address to? ");
                    while (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input) || input.Equals("0"))
                    {
                        if (input.Equals("0")) return;
                        Console.WriteLine("Please enter an address.");
                    }
                    guest.Address = input;
                    break;
                case 3:
                    Console.Write("What would you like to change their phone number to?(between 6 and 12 digits) ");
                    int numberInput;
                    Console.Write("Enter the phone number of the guest (between 6 and 12 digits): ");
                    while (!int.TryParse(Console.ReadLine(), out numberInput) || numberInput == 0 || !Regex.IsMatch(numberInput.ToString(), @"^\d{6,12}$"))
                    {
                        if (numberInput == 0) return;
                        Console.WriteLine("Please follow the instructions.");
                    }
                    guest.PhoneNumber = numberInput.ToString();
                    break;
                case 4:
                    Console.Write("Would you like to activate this guest?(y/n) ");
                    input = Console.ReadLine().ToLower();
                    while (!input.Equals("y") || !input.Equals("n"))
                    {
                        Console.WriteLine("Please enter Y/N");
                        input = Console.ReadLine();
                    }
                    if (input.Equals("y") || !guest.IsActive)
                    {
                        guest.IsActive = true;
                    }
                    break;
            }
            db.SaveChanges();
        }

        public static void ShowAll(HotelContext db)
        {
            var allGuests = db.Guest.ToList();
            if (allGuests.Count == 0) return;
            int input;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all guests\n ");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Show all guests");
            Console.WriteLine("2. Only show active guests");
            Console.WriteLine("3. Only show inactive guests");
            while (!int.TryParse(Console.ReadLine(), out input) || !Enumerable.Range(0, 4).Contains(input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter an option (0-3)");
            }
            Console.Clear();
            Console.WriteLine("\nId - Name");
            switch (input)
            {
                case 1:
                    allGuests.ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}, {g.Address}, {g.PhoneNumber} - active: {g.IsActive.ToString().ToLower()}"));
                    break;
                case 2:
                    allGuests.Where(g => g.IsActive)
                        .ToList()
                        .ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}, {g.Address}, {g.PhoneNumber}"));
                    break;
                case 3:
                    allGuests.Where(g => !g.IsActive)
                        .ToList()
                        .ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}, {g.Address}, {g.PhoneNumber}"));
                    break;
                case 0:
                    return;
            }
            Console.ReadKey();
        }
    }
}