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
    public class InvoiceHandler : IHandler
    {
        public static void Create(HotelContext db)
        {
            return;
        }

        public static void Delete(HotelContext db)
        {
            Invoice? invoice = null;
            string? confirmInput = string.Empty;
            int invoiceIndex = -1, range;
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Deleting a invoice\n ");
            Console.WriteLine("0. Back");
            var allInvoices = db.Invoice.ToList();
            range = allInvoices.Count;
            if (range == 0) return;
            Console.WriteLine("Index - ID - Guest - Paid Date - Due Date - Total Price - Paid off? - Archived ");
            for (int i = 1; i <= range; i++)
            {
                Console.WriteLine($"{i}. {allInvoices[i - 1].Id} - {allInvoices[i - 1].GuestId}- {allInvoices[i - 1].PaidDate} - {allInvoices[i - 1].DueDate} - {allInvoices[i - 1].TotalSum} - {allInvoices[i - 1].IsPaid} - {allInvoices[i - 1].IsArchived}");
            }
            Menu.RequestEntryWithinRange("Which invoice would you like to delete? ", ref invoiceIndex, range);
            if (invoiceIndex > 0)
            {
                invoice = db.Invoice.First(cr => cr.Id == allInvoices[invoiceIndex - 1].Id);
                Menu.RequestEntry($"Are you sure you want to delete {invoice.Id}?(y/n) ", ref confirmInput);
                if (confirmInput.ToLower() == "y")
                {
                    db.Invoice.Remove(invoice);
                    db.SaveChanges();
                }
            }
        }

        public static void Update(HotelContext db)
        {
            return;
        }
        public static void ShowAll(HotelContext db)
        {
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Showing all invoices\n ");
            Console.WriteLine("Index - ID - Guest - Paid Date - Due Date - Total Sum - Paid off? - Archived");
            foreach (var i in db.Invoice.ToList())
            {
                var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                Console.WriteLine($"{i.Id}. {i.GuestId} - {paidDate} - {i.DueDate.ToShortDateString()} - {i.TotalSum} - {i.IsPaid} - {i.IsArchived}");
            }
            //db.Invoice.ToList().ForEach(i => Console.WriteLine($"{i.Id}. {i.GuestId} - {i.PaidDate.ToShortDateString()} - {i.DueDate.ToShortDateString()} - {i.TotalPrice} - {i.IsPaid}"));
            Console.ReadKey();
        }

        public static void PayInvoice(HotelContext db)
        {
            Console.Clear();
            Console.WriteLine("Hossen Hotel - Paying off an invoice\n");
            int input;
            Guest? guest = null;
            Invoice? invoice = null;
            //Console.WriteLine(db.Guest.Where(g => g.Invoices.Any()).SelectMany(g => g.Invoices.Select(i => i.Id)).ToList());
            var guestsWithInvoices = db.Guest.Where(g => db.Invoice.Any(i => i.GuestId == g.Id && !i.IsPaid && !i.IsArchived)).ToList();
            if (guestsWithInvoices.Count == 0) return;
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
            guest = db.Guest.First(g => g.Id == input);
            foreach (var i in db.Invoice.Where(i => !i.IsPaid && !i.IsArchived).ToList())
            {
                var paidDate = i.IsPaid ? i.PaidDate.ToShortDateString() : "N/A";
                Console.WriteLine($"Id:{i.Id} - Due: {i.DueDate.ToShortDateString()} - Sum: {i.TotalSum}");
            }
            Console.Write("Which invoice would you like to pay off? ");
            while (!int.TryParse(Console.ReadLine(), out input)
                || !db.Invoice.Any(i => i.Id == input))
            {
                if (input == 0) return;
                Console.WriteLine("Please enter the correct option.)");
            }
            invoice = db.Invoice.First(i => i.Id == input);
            Console.WriteLine("The invoice has been paid off.");
            invoice.IsPaid = true;
            invoice.IsArchived = true;
            invoice.PaidDate = DateTime.Today;
            db.SaveChanges();
            Console.ReadKey();
        }
    }
}
