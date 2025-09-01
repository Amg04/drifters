using BLLProject.Interfaces;
using BLLProject.Repositories;
using DAL.Data;
using DAL.Models;
using DALProject.DBInitializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PL.Services.RtspPumpService;
using PL.Services.RtspUrlBuilder;
using System.Text;

namespace PL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DriftersDBContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("CS"));
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>()
               .AddEntityFrameworkStores<DriftersDBContext>()
               .AddDefaultTokenProviders();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDBInitializer, DBInitializer>();

            #region MyRegion

            // review

            builder.Services.AddSingleton<IRtspUrlBuilder, RtspUrlBuilder>();
            builder.Services.AddHostedService<RtspPumpService>();

            //builder.Services.AddCors(o =>
            //{
            //    o.AddPolicy("allow-client", p => p
            //        .AllowAnyHeader()
            //        .AllowAnyMethod()
            //        .WithOrigins("http://localhost:3000", "http://your-flutter-app-origin")); // ⁄œ¯·Â«
            //});

            #endregion

            builder.Services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(Options =>
            {
                Options.SaveToken = true;
                Options.RequireHttpsMetadata = false;
                Options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:IssuerIP"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:AudienceIP"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(builder.Configuration["JWT:SecretKey"]))
                };
            });

            #region Swagger

            builder.Services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MyApp",
                    Version = "v1",
                    Description = "API FOR SW"
                });

                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your?valid token in the text input below.Example:Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                    {
                    Reference = new OpenApiReference
                    {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                    }
                    },
                    new string[] {}
                    }
                    });
            });

            #endregion


            var app = builder.Build();

            //Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Drifters API V1");
                    // "launchUrl": "swagger" ﬂœ… „‘  "launchUrl": "" ÷Ì› «·”ÿ— œ… ·Ê «· 
                    //c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            //app.UseCors("allow-client");
            app.UseAuthentication();
            app.UseAuthorization();

            await SeedDatabaseAsync();

            app.MapControllers();

            app.Run();

            async Task SeedDatabaseAsync()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
                    await dbInitializer.Initialize();
                }
            }
        }
    }
}
