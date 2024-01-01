using ConsoleTables;
using HotelApp.Core;
using HotelApp.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HotelApp.Core.Handlers
{
    public class InvoiceHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            return;
        }
        public static void Update(HotelContext db)
        {
            return;
        }
        public static void Delete(HotelContext db)
        {
            var allInvoices = db.Invoice.Include(i => i.Booking).ThenInclude(b => b.Guest).ToList();
            if (allInvoices.Count == 0) return;
            int invoiceIndex;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a invoice\n ");
            Console.WriteLine("0. Back");
            var table = new ConsoleTable("Id", "Guest", "Paid Date", "Due Date", "Total Sum", "Archived");
            table.Options.EnableCount = false;
            allInvoices.ForEach(i =>
            {
                var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "Not Paid";
                table.AddRow(i.Id, i.Booking.Guest.Name, paidDate, i.DueDate.ToShortDateString(), i.TotalSum, i.IsArchived);
            });
            table.Write();
            Console.Write("Which invoice would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out invoiceIndex) || !allInvoices.Any(i => i.Id == invoiceIndex))
            {
                if (invoiceIndex == 0) return;
                Console.WriteLine("Please enter an option");
            }
            Invoice invoice = allInvoices.First(i => i.Id == invoiceIndex);
            Booking booking = invoice.Booking;
            Console.Write($"Are you sure to delete invoice {invoice.Id}?(y/n/h hard delete) "); ;
            string? confirmInput = Console.ReadLine().ToLower();
            while (!(confirmInput.Equals("y") || confirmInput.Equals("n") || confirmInput.Equals("h")))
            {
                Console.WriteLine("Please enter Y/N/H");
                confirmInput = Console.ReadLine().ToLower();
            }
            if (confirmInput.Equals("y"))
            {
                invoice.IsArchived = true;
                booking.IsArchived = true;
                booking.IsActive = false;
            }
            else if (confirmInput.Equals("h"))
            {
                db.Invoice.Remove(invoice);
                db.Booking.Remove(booking);
            }
            db.SaveChanges();
        }
        public static void ShowAll(HotelContext db)
        {
            var guestsWithInvoices = db.Guest.Include(g => g.Invoices).Where(g => g.Invoices.Count != 0).ToList();
            if (guestsWithInvoices.Count == 0) return;
            int input;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all invoices\n ");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Show all invoices");
            Console.WriteLine("2. Only show paid invoices");
            Console.WriteLine("3. Only show unpaid invoices");
            Console.WriteLine("4. Only show archived invoices");
            while (!int.TryParse(Console.ReadLine(), out input) || !Enumerable.Range(0, 5).Contains(input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter an option (0-4)");
            }
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all invoices\n ");
            var table = new ConsoleTable();
            switch (input)
            {
                case 1:
                    table = new ConsoleTable("Id", "Guest", "Paid Date", "Due Date", "Total Sum", "Paid Off", "Archived");
                    foreach (var g in guestsWithInvoices)
                    {
                        foreach (var i in g.Invoices)
                        {
                            var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                            table.AddRow(i.Id, g.Name, paidDate, i.DueDate.ToShortDateString(), i.TotalSum, i.IsPaid, i.IsArchived);
                        }
                    }
                    break;
                case 2:
                    table = new ConsoleTable("Id", "Guest", "Paid Date", "Total Sum");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (!g.Invoices.Any(i => i.IsPaid)) continue;
                        foreach (var i in g.Invoices.Where(i => i.IsPaid))
                        {
                            table.AddRow(i.Id, g.Name, i.PaidDate.ToShortDateString(), i.TotalSum);
                        }
                    }
                    break;
                case 3:
                    table = new ConsoleTable("Id", "Guest", "Due Date", "Total Sum");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (g.Invoices.Any(i => i.IsPaid)) continue;
                        foreach (var i in g.Invoices.Where(i => !i.IsPaid))
                        {
                            table.AddRow(i.Id, g.Name, i.DueDate.ToShortDateString(), i.TotalSum);
                        }
                    }
                    break;
                case 4:
                    table = new ConsoleTable("Id", "Guest", "Paid Date", "Due Date", "Total Sum", "Paid Off");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (!g.Invoices.Any(i => i.IsArchived)) continue;
                        foreach (var i in g.Invoices.Where(i => i.IsArchived))
                        {
                            var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                            table.AddRow(i.Id, g.Name, paidDate, i.DueDate.ToShortDateString(), i.TotalSum, i.IsPaid);
                        }
                    }
                    break;
            }
            table.Write();
            Console.WriteLine("\nPress any button to continue.");
            Console.ReadKey();
        }
        public static void PayInvoice(HotelContext db)
        {
            var guestsWithInvoices = db.Guest
                .Include(g => g.Invoices)
                .Where(g => g.Invoices.Any(i => !i.IsPaid))
                .ToList();
            if (guestsWithInvoices.Count == 0) return;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Paying off an invoice\n");
            int input;
            var table = new ConsoleTable("Id", "Name");
            table.Options.EnableCount = false;
            foreach (var g in guestsWithInvoices)
            {
                table.AddRow(g.Id, g.Name);
            }
            table.Write();
            Console.Write("Which guest is paying their invoice? ");
            while (!int.TryParse(Console.ReadLine(), out input) || !guestsWithInvoices.Any(i => i.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            Guest guest = guestsWithInvoices.First(g => g.Id == input);
            table = new ConsoleTable("Id", "Due Date", "Total Sum");
            table.Options.EnableCount = false;
            foreach (var i in guest.Invoices)
            {
                table.AddRow(i.Id, i.DueDate.ToShortDateString(), i.TotalSum);
            }
            table.Write();
            Console.Write("Which invoice would you like to pay off? ");
            while (!int.TryParse(Console.ReadLine(), out input)
                || !guest.Invoices.Any(i => i.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            Invoice invoice = guest.Invoices.First(i => i.Id == input);
            Console.WriteLine("The invoice has been paid off, press any button to continue.");
            invoice.IsPaid = true;
            invoice.IsArchived = true;
            invoice.PaidDate = DateTime.Today;
            db.SaveChanges();
            Console.ReadKey();
        }
    }
}