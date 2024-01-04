using HotelApp.Core.Handlers;
using HotelApp.Data;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Core
{
    public static class Menu
    {
        public static void MainMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Welcome to Hossen Hotel\n");
                Console.WriteLine("-1. Exit");
                Console.WriteLine("1. New booking");
                Console.WriteLine("2. Pay invoice");
                Console.WriteLine("3. Cancel booking");
                Console.WriteLine("\n\n4. Admin");
                RequestEntryWithinRange("", ref option, 5);
                switch (option)
                {
                    case 1:
                        NewMenu(db);
                        break;
                    case 2:
                        InvoiceHandler.PayInvoice(db);
                        break;
                    case 3:
                        BookingHandler.Delete(db);
                        break;
                    case 4:
                        CRUDMenu(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        public static void CRUDMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Hossen Hotel - Admin\n");
                Console.WriteLine("-1. Exit");
                Console.WriteLine("1. Guests");
                Console.WriteLine("2. Rooms");
                Console.WriteLine("3. Bookings");
                Console.WriteLine("4. Invoices");
                RequestEntryWithinRange("", ref option, 4);
                switch (option)
                {
                    case 1:
                        GuestMenu(db);
                        break;
                    case 2:
                        RoomMenu(db);
                        break;
                    case 3:
                        BookingMenu(db);
                        break;
                    case 4:
                        InvoiceMenu(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        public static void NewMenu(HotelContext db)
        {
            int input = 0;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - New booking\n");
            Console.WriteLine("-1. Exit");
            Console.WriteLine("1. Existing guest");
            Console.WriteLine("2. New guest");
            RequestEntryWithinRange("", ref input, 2);
            if (input == -1) return;
            else
            {
                if (input == 1) BookingHandler.Create(db);
                else if (input == 2)
                {
                    int amountOfGuests = db.Guest.ToList().Count;
                    GuestHandler.Create(db);
                    if (db.Guest.ToList().Count > amountOfGuests)
                        BookingHandler.Create(db, db.Guest.OrderByDescending(g => g.Id).First());
                    else NewMenu(db);
                }
            }
        }
        public static void GuestMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Hossen Hotel - Guests\n");
                Console.WriteLine("-1. Exit");
                Console.WriteLine("1. Add a guest");
                Console.WriteLine("2. Delete a guest");
                Console.WriteLine("3. Edit a guest");
                Console.WriteLine("4. Show all guests");
                RequestEntryWithinRange("", ref option, 4);
                switch (option)
                {
                    case 1:
                        GuestHandler.Create(db);
                        break;
                    case 2:
                        GuestHandler.Delete(db);
                        break;
                    case 3:
                        GuestHandler.Update(db);
                        break;
                    case 4:
                        GuestHandler.ShowAll(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        public static void RoomMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Hossen Hotel - Rooms\n");
                Console.WriteLine("-1. Exit");
                Console.WriteLine("1. Add a room");
                Console.WriteLine("2. Delete a room");
                Console.WriteLine("3. Edit a room");
                Console.WriteLine("4. Show all rooms");
                RequestEntryWithinRange("", ref option, 4);
                switch (option)
                {
                    case 1:
                        RoomHandler.Create(db);
                        break;
                    case 2:
                        RoomHandler.Delete(db);
                        break;
                    case 3:
                        RoomHandler.Update(db);
                        break;
                    case 4:
                        RoomHandler.ShowAll(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        public static void BookingMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Hossen Hotel - Bookings\n");
                Console.WriteLine("-1. Exit");
                Console.WriteLine("1. Add a booking");
                Console.WriteLine("2. Delete a booking");
                Console.WriteLine("3. Edit a booking");
                Console.WriteLine("4. Show all bookings");
                RequestEntryWithinRange("", ref option, 4);
                switch (option)
                {
                    case 1:
                        BookingHandler.Create(db);
                        break;
                    case 2:
                        BookingHandler.Delete(db);
                        break;
                    case 3:
                        BookingHandler.Update(db);
                        break;
                    case 4:
                        BookingHandler.ShowAll(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        public static void InvoiceMenu(HotelContext db)
        {
            int option = 0;
            bool menu = true;
            while (menu)
            {
                Console.Clear();
                Console.WriteLine("Hossen Hotel - Invoices\n");
                Console.WriteLine("-1. Exit");
                //Console.WriteLine("1. Add an invoice, does nothing");
                Console.WriteLine("1. Delete an invoice");
                //Console.WriteLine("3. Edit an invoice, does nothing");
                Console.WriteLine("2. Show all invoices");
                RequestEntryWithinRange("", ref option, 2);
                switch (option)
                {
                    case 1:
                        InvoiceHandler.Delete(db);
                        break;
                    case 2:
                        InvoiceHandler.ShowAll(db);
                        break;
                    case -1:
                        menu = false;
                        break;
                }
            }
        }
        /// <summary>
        /// Writes out <paramref name="request"/> and asks for an <paramref name="input"/> between 0 and <paramref name="range"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="input"></param>
        /// <param name="range"></param>
        public static void RequestEntryWithinRange(string request, ref int input, int range)
        {
            Console.Write(request);
            while (!int.TryParse(Console.ReadLine(), out input) || !Enumerable.Range(1, range).Contains(input))
            {
                if (input == -1) return;
                Console.WriteLine($"Please enter an option");
            }
        }
    }
}