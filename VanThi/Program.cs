using Microsoft.EntityFrameworkCore;
using VanThi.Data;

namespace VanThi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Thêm cấu hình CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            builder.Services.AddSignalR();

            builder.Services.AddControllers()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Luôn bật Swagger để dễ dàng kiểm tra sau khi deploy (có thể tắt sau)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VanThi API V1");
                c.RoutePrefix = string.Empty; // Để Swagger là trang chủ
            });

            // Re-enable HTTPS redirection if needed, but often Render handles SSL at load balancer
            // app.UseHttpsRedirection(); 
            
            
            app.UseCors("AllowAll"); // Áp dụng CORS
            app.UseAuthorization();
            app.MapControllers();

            var port = Environment.GetEnvironmentVariable("PORT");
            if (string.IsNullOrEmpty(port))
            {
                app.Run();
            }
            else
            {
                app.Run($"http://0.0.0.0:{port}");
            }
        }
    }
}