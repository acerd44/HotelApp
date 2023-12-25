using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using HotelApp.Data;
using System.Data;

namespace HotelApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Run();
        }
    }
}
