using HotelApp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class Guest : IHotel
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(60)]
        public string Name { get; set; }
        [MaxLength(60)]
        public string? Address { get; set; }
        [MaxLength(12)]
        [MinLength(6)]
        public string PhoneNumber { get; set; }
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public bool IsActive { get; set; }
    }
}