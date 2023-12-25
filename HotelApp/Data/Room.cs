using HotelApp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class Room : IHotel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Range(15, 40)]
        public int Size { get; set; }
        [Required]
        [Range(1, 4)]
        public int Beds { get; set; }
        [Required]
        public int Price { get; set; }
        public int GuestId { get; set; }
        public bool IsOccupied { get; set; }
    }
}