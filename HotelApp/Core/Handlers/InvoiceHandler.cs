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

        public static void Delete(HotelContext db)
        {
            var allInvoices = db.Invoice.ToList();
            if (allInvoices.Count == 0) return;
            int invoiceIndex = -1;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a invoice\n ");
            Console.WriteLine("0. Back");
            Console.WriteLine("ID - Guest - Paid Date - Due Date - Total Price - Paid off");
            allInvoices.ForEach(i =>
            {
                var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                Console.WriteLine($"{i.Id}. {i.GuestId} - {paidDate} - {i.DueDate} - {i.TotalSum}");
            });
            Console.Write("Which invoice would you like to delete? ");
            while (!int.TryParse(Console.ReadLine(), out invoiceIndex) || !allInvoices.Any(i => i.Id == invoiceIndex))
            {
                if (invoiceIndex == 0) return;
                Console.WriteLine("Please enter an option");
            }
            Invoice invoice = db.Invoice.First(cr => cr.Id == invoiceIndex);
            Console.Write($"Are you sure to delete this invoice {invoice.Id}?(y/n) "); ;
            string? confirmInput = Console.ReadLine().ToLower();
            while (!confirmInput.Equals("y") || !confirmInput.Equals("n"))
            {
                Console.WriteLine("Please enter Y/N");
                confirmInput = Console.ReadLine().ToLower();
            }
            if (confirmInput.Equals("y"))
            {
                db.Invoice.Remove(invoice);
                db.SaveChanges();
            }
        }

        public static void Update(HotelContext db)
        {
            return;
        }
        public static void ShowAll(HotelContext db)
        {
            var guestsWithInvoices = db.Guest.Include(g => g.Invoices).Where(g => g.Invoices.Count != 0).ToList();
            if (guestsWithInvoices.Count == 0) return;
            int input = -1;
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
            switch (input)
            {
                case 1:
                    Console.WriteLine("ID - Guest - Paid Date - Due Date - Total Sum - Paid off");
                    foreach (var g in guestsWithInvoices)
                    {
                        foreach (var i in g.Invoices)
                        {
                            var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                            Console.WriteLine($"{i.Id}. {g.Name} - {paidDate} - {i.DueDate.ToShortDateString()} - {i.TotalSum} - {i.IsPaid}");
                        }
                    }
                    break;
                case 2:
                    Console.WriteLine("ID - Guest - Due Date - Total Sum");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (!g.Invoices.Any(i => i.IsPaid)) continue;
                        foreach (var i in g.Invoices.Where(i => i.IsPaid))
                        {
                            Console.WriteLine($"{i.Id}. {g.Name} - {i.DueDate.ToShortDateString()} - {i.TotalSum}");
                        }
                    }
                    break;
                case 3:
                    Console.WriteLine("ID - Guest - Due Date - Total Sum");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (g.Invoices.Any(i => i.IsPaid)) continue;
                        foreach (var i in g.Invoices.Where(i => !i.IsPaid))
                        {
                            Console.WriteLine($"{i.Id}. {g.Name} - {i.DueDate.ToShortDateString()} - {i.TotalSum}");
                        }
                    }
                    break;
                case 4:
                    Console.WriteLine("ID - Guest - Paid Date - Due Date - Total Sum - Paid off");
                    foreach (var g in guestsWithInvoices)
                    {
                        if (!g.Invoices.Any(i => i.IsArchived)) continue;
                        foreach (var i in g.Invoices.Where(i => i.IsArchived))
                        {
                            var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                            Console.WriteLine($"{i.Id}. {g.Name} - {paidDate} - {i.DueDate.ToShortDateString()} - {i.TotalSum} - {i.IsPaid}");
                        }
                    }
                    break;
            }
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
            foreach (var i in guestsWithInvoices)
            {
                Console.WriteLine($"{i.Id}. {i.Name}");
            }
            Console.Write("Which guest is paying their invoice? ");
            while (!int.TryParse(Console.ReadLine(), out input) || !guestsWithInvoices.Any(g => g.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            Guest guest = db.Guest.First(g => g.Id == input);
            foreach (var i in db.Invoice.Where(i => i.GuestId == guest.Id).ToList())
            {
                var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                Console.WriteLine($"Id: {i.Id} - Due: {i.DueDate.ToShortDateString()} - Sum: {i.TotalSum}");
            }
            Console.Write("Which invoice would you like to pay off? ");
            while (!int.TryParse(Console.ReadLine(), out input)
                || !db.Invoice.Any(i => i.Id == input && i.GuestId == guest.Id))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.");
            }
            Invoice invoice = db.Invoice.First(i => i.Id == input);
            Console.WriteLine("The invoice has been paid off.");
            invoice.IsPaid = true;
            invoice.IsArchived = true;
            invoice.PaidDate = DateTime.Today;
            db.SaveChanges();
            Console.ReadKey();
        }
    }
}