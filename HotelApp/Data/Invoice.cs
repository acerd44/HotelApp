using HotelApp.Core;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class Invoice : IHotel
    {
        [Key]
        public int Id { get; set; }

        public DateTime PaidDate { get; set; }
        public DateTime DueDate { get; set; }
        public int GuestId { get; set; }
        public int BookingId { get; set; }
        public int TotalSum { get; set; }
        public bool IsPaid { get; set; }
        public bool IsArchived { get; set; }
    }
}