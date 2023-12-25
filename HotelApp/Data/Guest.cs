using HotelApp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class Guest : IHotel
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(60)]
        public string Name { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}