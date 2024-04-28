using Overlayer.WebAPI.Controllers;

namespace Overlayer.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new BinaryInputFormatter());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            MainController.HandshakeCount = File.ReadAllLines("handshakes.txt").LongLength;
            AdofaiggController.RequestCount = long.Parse(File.ReadAllText("ggreqcnt.txt"));
            TUFController.RequestCount = long.Parse(File.ReadAllText("tufreqcnt.txt"));

            new Task(async () =>
            {
                while (true)
                {
                    await File.WriteAllTextAsync("ggreqcnt.txt", AdofaiggController.RequestCount.ToString());
                    await File.WriteAllTextAsync("tufreqcnt.txt", TUFController.RequestCount.ToString());
                    await Task.Delay(10000);
                }
            }).Start();

            app.Run("http://127.0.0.1:7777");
        }
    }
}