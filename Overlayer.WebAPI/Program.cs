using JSON;
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
            JsonNode node = JsonNode.Parse(File.ReadAllText("counts.json"));
            MainController.PlayCount = node["PlayCount"].AsLong;
            AdofaiggController.RequestCount = node["GGRequestCount"].AsLong;
            AdofaiggController.GetRequestCount = node["GetGGRequestCount"].AsLong;
            TUFController.RequestCount = node["TUFRequestCount"].AsLong;
            TUFController.GetRequestCount = node["GetTUFRequestCount"].AsLong;

            new Task(async () =>
            {
                while (true)
                {
                    JsonNode node = JsonNode.Empty;
                    node["PlayCount"] = MainController.PlayCount;
                    node["GGRequestCount"] = AdofaiggController.RequestCount;
                    node["GetGGRequestCount"] = AdofaiggController.GetRequestCount;
                    node["TUFRequestCount"] = TUFController.RequestCount;
                    node["GetTUFRequestCount"] = TUFController.GetRequestCount;
                    await File.WriteAllTextAsync("counts.json", node.ToString(4));
                    await Task.Delay(10000);
                }
            }).Start();

            app.Run("http://127.0.0.1:7777");
        }
    }
}