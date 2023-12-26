using Azure;
using HotelApp.Core;
using HotelApp.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class BookingHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            return;
        }

        public static void Create(HotelContext db, Guest? existingGuest)
        {
            var booking = new Booking();
            var invoice = new Invoice();
            var startDate = new DateTime();
            var endDate = new DateTime();
            int input = -1;
            string? dateInput = string.Empty;
            bool firstCheck = false;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Creating a new booking\n ");
            Console.WriteLine("0. Back");
            if (existingGuest == null)
            {
                Console.WriteLine("If you'd like to skip the prompt, just input a minus (-)\n");
                Console.WriteLine("Guest Id. Name");
                if (db.Guest.ToList().Count == 0) return;
                db.Guest.ToList().ForEach(g => Console.WriteLine($"{g.Id}. {g.Name}"));
                Console.Write("Write the guest id for the booking: ");
                while (!int.TryParse(Console.ReadLine(), out input) || !db.Guest.Any(g => g.Id == input))
                {
                    if (input == 0) return;
                    Console.WriteLine("Please enter the correct option.");
                }
                booking.GuestId = input;
            }
            else
            {
                booking.GuestId = existingGuest.Id;
            }
            while (GetAvailableRooms(db, startDate, endDate).Count == 0)
            {
                if (firstCheck)
                {
                    Console.WriteLine($"There was no available rooms between {startDate.ToShortDateString()} - {endDate.ToShortDateString()}, please try another range of dates.");
                }
                // start date
                Console.Write("Write the start date of the booking:(write 'today' for today's date, otherwise YYYY-MM-DD) ");
                dateInput = Console.ReadLine().ToLower();
                while (true)
                {
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                    {
                        if (dateInput == "0") return;
                        Console.WriteLine("Don't type empty entries");
                    }
                    else
                    {
                        if (dateInput == "today")
                        {
                            startDate = DateTime.Today;
                            break;
                        }
                        if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                        {
                            Console.WriteLine("Please use the correct format yyyy-mm-dd");
                        }
                        else
                        {
                            if (DateTime.Parse(dateInput) >= DateTime.Today)
                            {
                                startDate = DateTime.Parse(dateInput);
                                break;
                            }
                            else Console.WriteLine("Make sure the start date isn't a date before than today");
                        }
                    }
                    dateInput = Console.ReadLine();
                }
                // end date
                Console.Write("Write the end date of the booking:(YYYY-MM-DD) ");
                dateInput = Console.ReadLine();
                while (true)
                {
                    if (string.IsNullOrEmpty(dateInput) || string.IsNullOrWhiteSpace(dateInput) || dateInput == "0")
                    {
                        if (dateInput == "0") return;
                        Console.WriteLine("Don't type empty entries");
                    }
                    else
                    {
                        if (!DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.CurrentCulture, DateTimeStyles.None, out _))
                        {
                            Console.WriteLine("Please use the correct format yyyy-mm-dd");
                        }
                        else
                        {
                            if (DateTime.Parse(dateInput) > DateTime.Today)
                            {
                                endDate = DateTime.Parse(dateInput).AddDays(1).AddTicks(-1);
                                firstCheck = true;
                                break;
                            }
                            else Console.WriteLine("Make sure the end date isn't a date earlier than today");
                        }
                    }
                    dateInput = Console.ReadLine();
                }
            }
            booking.StartDate = startDate;
            booking.EndDate = endDate;
            if (startDate == DateTime.Today) booking.IsActive = true;
            Console.WriteLine("Available rooms: - Room ID. Size, Beds, Price");
            GetAvailableRooms(db, startDate, endDate).ForEach(r => Console.WriteLine($"{r.Id}. {r.Size}, {r.Beds}, {r.Price}"));
            Console.Write("Write the room id of the booking: ");
            while (!int.TryParse(Console.ReadLine(), out input) || !db.Room.Any(r => r.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            booking.RoomId = input;
            db.Booking.Add(booking);
            invoice.GuestId = booking.GuestId;
            invoice.DueDate = booking.StartDate.AddDays(10);
            invoice.IsPaid = false;
            invoice.TotalSum = (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)
                * db.Room.First(r => r.Id == booking.RoomId).Price;
            //db.Invoice.Add(invoice);
            db.Guest.First(g => g.Id == booking.GuestId).Invoices.Add(invoice);
            db.SaveChanges();
        }

        public static void Delete(HotelContext db)
        {
            Booking? booking = null;
            string? confirmInput = string.Empty;
            int bookingIndex = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a booking\n ");
            Console.WriteLine("0. Back");
            var allBookings = db.Booking.ToList();
            range = allBookings.Count;
            if (range == 0) return;
            Console.WriteLine("Index - ID - Guest ID - Room ID - Start Date");
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allBookings[i - 1].Id} - {allBookings[i - 1].GuestId} - {allBookings[i - 1].RoomId} - {allBookings[i - 1].StartDate.Date}");
            }
            Console.Write("Which booking would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !Enumerable.Range(0, range + 1).Contains(bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            booking = db.Booking.First(cr => cr.Id == allBookings[bookingIndex - 1].Id);
            Console.Write($"Are you sure you want to delete Booking {booking.Id} as a booking?(y/n/h - hard delete) ");
            confirmInput = Console.ReadLine().ToLower();
            while (string.IsNullOrWhiteSpace(confirmInput) || string.IsNullOrEmpty(confirmInput)
                || !(confirmInput == "y" || confirmInput == "n" || confirmInput == "h"))
            {
                Console.WriteLine("Please follow the instructions.");
                confirmInput = Console.ReadLine();
            }
            if (confirmInput == "y")
            {
                booking.IsActive = false;
                db.SaveChanges();
            }
            else if (confirmInput == "h")
            {
                db.Booking.Remove(booking);
                db.SaveChanges();
            }
        }

        public static void Update(HotelContext db)
        {
            Booking? booking = null;
            int bookingIndex = -1, input = -1, range;
            string? dateInput = string.Empty;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Editting a booking\n ");
            Console.WriteLine("0. Back");
            var allBookings = db.Booking.ToList();
            range = allBookings.Count;
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allBookings[i - 1].Id} - {allBookings[i - 1].GuestId} - {allBookings[i - 1].RoomId} - {allBookings[i - 1].StartDate.Date}");
            }
            Console.Write("Which booking would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !Enumerable.Range(0, range + 1).Contains(bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            booking = db.Booking.First(cr => cr.Id == allBookings[bookingIndex - 1].Id);
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting booking {booking.Id}\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("1. Guest Id");
            Console.WriteLine("2. Room Id");
            Console.WriteLine("3. Price");
            range = 3;
            Console.Write("Which part would you like to edit? ");
            while (!int.TryParse(Console.ReadLine(), out bookingIndex) || !Enumerable.Range(0, range + 1).Contains(bookingIndex))
            {
                if (bookingIndex == 0) return;
                Console.WriteLine($"Please enter an option (0-{range})");
            }
            Console.Clear();
            Console.WriteLine($"Hossen Hotel - Editting Booking {booking.Id} \n ");
            switch (bookingIndex)
            {
                case 1:
                    Menu.RequestEntry("What would you like to change the guest ID to? ", ref input);
                    if (input == 0) return;
                    booking.GuestId = input;
                    break;
                case 2:
                    Menu.RequestEntry("What would you like to change the room ID to? ", ref input);
                    if (input == 0) return;
                    booking.RoomId = input;
                    break;
                case 3:
                    Menu.RequestDateEntry("Write the new start date of the booking:(write 'today' for today's date, otherwise YYYY-MM-DD) ", ref dateInput, true);
                    if (dateInput == "today")
                    {
                        booking.StartDate = DateTime.Now;
                        booking.IsActive = true;
                    }
                    else
                    {
                        if (input == 0 || dateInput == "0") return;
                        booking.StartDate = DateTime.Parse(dateInput);
                    }
                    break;
                case 4:
                    Menu.RequestDateEntry("Write the end date of the booking:(YYYY-MM-DD) ", ref dateInput, false);
                    if (input == 0 || dateInput == "0") return;
                    booking.EndDate = DateTime.Parse(dateInput).AddDays(1).AddTicks(-1);
                    break;
            }
            db.SaveChanges();
        }
        public static void ShowAll(HotelContext db)
        {
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all bookings\n ");
            Console.WriteLine("Id - Guest ID - Room ID - Start Date");
            db.Booking.ToList().ForEach(b => Console.WriteLine($"{b.Id}. {b.GuestId} - {b.RoomId} - {b.StartDate.Date.ToShortDateString()} - active: {b.IsActive}"));
            Console.ReadKey();
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
            return !db.Booking.Where(b => b.RoomId == roomId).Any(b =>
            (startDate >= b.StartDate && startDate <= b.EndDate)
            || (endDate >= b.StartDate && endDate <= b.EndDate)
            || (startDate <= b.StartDate && endDate >= b.EndDate));
        }
        public static List<Room> GetAvailableRooms(HotelContext db, DateTime startDate, DateTime endDate)
        {
            var rooms = db.Room.ToList();
            return rooms.Where(r => CheckSpecificRoomAvailability(db, r.Id, startDate, endDate)).ToList();
        }
    }
}
