using BlogProject.Mvc.AutoMapper.Profiles;
using BlogProject.Mvc.Helpers.Abstract;
using BlogProject.Mvc.Helpers.Concrete;
using BlogProject.Services.AutoMapper.Profiles;
using BlogProject.Services.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlogProject.Mvc
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve; //nested(i�i�e) objelerde �evrimi sa�lar.
            }); //mvc uygulamas� olarak �al��mas�n� sa�lar. Razor runtime ile view de�i�iklikleri taray�c�ya an�nda yans�r.
            services.AddSession();
            services.AddAutoMapper(typeof(CategoryProfile), typeof(ArticleProfile), typeof(UserProfile)); //derlenme s�ras�nda Automapper bu projedeki s�n�flar� tarar.
            services.LoadMyServices();
            services.AddScoped<IImageHelper, ImageHelper>();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Admin/User/Login");
                options.LogoutPath = new PathString("/Admin/User/Logout");
                options.Cookie = new CookieBuilder()
                {
                    Name = "BlogProject",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict, //bu �ekilde kullan�lmal�d�r.
                    SecurePolicy = CookieSecurePolicy.SameAsRequest //ger�ek i� s�ras�nda always se�ilmelidir.
                };
                options.SlidingExpiration = true; //kullan�c� ayn� cookie ayarlar� ile belirtilen s�re i�inde giri� yapabilir.
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.AccessDeniedPath = new PathString("/Admin/User/AccessDenied"); //giri� yapan fakat yetkisi olmayan kullan�c�lar� ilgili adrese y�nlendirir.
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages(); //projede olmayan bir sayfa istenildi�inde 404 Not Found uyar�s�na y�nlendirir.
            }

            app.UseSession();

            app.UseStaticFiles(); //tema dosyalar�(resim, css veya js)
            app.UseRouting();
            app.UseAuthentication(); //authentication ve authorization, routing ile endpoints aras�nda olmal�d�rlar.
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Admin",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
                    ); //MVC'de Admin area's�na eri�im sa�lan�r.
                endpoints.MapDefaultControllerRoute(); //ilk a��l��ta default olarak HomeController ve Index.cshtml'e y�nlendirir.
            });
        }
    }
}
