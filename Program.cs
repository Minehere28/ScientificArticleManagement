using System;
using Microsoft.EntityFrameworkCore;
using ScientificArticleManagement.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ScientificArticleManagement.Models;

//var builder = WebApplication.CreateBuilder(args);

//// Cấu hình DbContext với chuỗi kết nối từ appsettings.json
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Cấu hình các middleware cho HTTP request pipeline
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();

namespace ScientificArticleManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Initialize a new instance of web app
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            //    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
            //});

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //builder.Services.AddSession();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });


            //build
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCookiePolicy();

            app.UseHttpsRedirection();

            app.UseStaticFiles(); //static file
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}