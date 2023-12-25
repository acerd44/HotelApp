using HotelApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotelApp.Core
{
    public interface IHandler
    {
        public static abstract void Create(HotelContext db);
        public static abstract void Delete(HotelContext db);
        public static abstract void Update(HotelContext db);
        public static abstract void ShowAll(HotelContext db);
    }
}
