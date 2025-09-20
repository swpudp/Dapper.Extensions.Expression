using Dapper.Extensions.Expression.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Dapper.Extensions.Expression.WebTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            NamingUtils.SetNamingPolicy(NamingPolicy.SnakeCase);
            services.AddControllers().AddJsonOptions(c =>
            {
                c.JsonSerializerOptions.WriteIndented = true;
                c.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dapper.Extensions.Expression.WebTest", Version = "v1" });
            });
            services.AddLogging(f => f.AddLog4Net());
            services.AddSingleton<IConnectionMultiplexer>(p => ConnectionMultiplexer.Connect("localhost"));

            string connectionString = "Server=localhost;Port=5432;Database=identity;Userid=identity_admin;Password=Q1@we34r;Pooling=true;MinPoolSize=1;MaxPoolSize=20;ConnectionLifeTime=15;";
            services.AddIdentityServer()
               //.AddTestUsers(TestUsers.Users)
               .AddConfigurationStore(options =>
               {
                   options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));
               })
               .AddOperationalStore(options =>
               {
                   options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName));
               });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dapper.Extensions.Expression.WebTest v1"));
            }

            app.UseExceptionHandler(err => err.UseCustomErrors());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
