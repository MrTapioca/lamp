using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lamp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Lamp.Services;
using Lamp.Policies;
using Microsoft.AspNetCore.Authorization;
using Lamp.Interfaces;
using System;
using Microsoft.AspNetCore.DataProtection;

namespace Lamp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                if (Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>();

            //services.AddIdentity<IdentityUser, IdentityRole>()
            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.ExpireTimeSpan = TimeSpan.FromDays(30);
            //});

            // Application services
            services.AddTransient<IEmailSender, EmailSender>()
                .Configure<AuthMessageSenderOptions>(Configuration);
            services.AddTransient<IKeyGenerator, KeyGenerator>();
            services.AddTransient<ShiftService>();
            services.AddTransient<UserService>();
            services.AddTransient<MemberService>();
            services.AddTransient<LocationService>();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasGroupAccess", policy =>
                    policy.AddRequirements(new GroupAccessRequirement("groupId")));
                options.AddPolicy("HasGroupManagementAccess", policy =>
                    policy.AddRequirements(new GroupAccessRequirement(
                        "groupId", GroupRole.Administrator, GroupRole.Assistant)));
            });

            services.AddTransient<IAuthorizationHandler, GroupAccessHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
