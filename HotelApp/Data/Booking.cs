using HotelApp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Data
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        public int Guests { get; set; }
        public virtual Guest Guest { get; set; }
        public int GuestId { get; set; }
        public virtual Room Room { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
    }
}