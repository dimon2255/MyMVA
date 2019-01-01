using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyMVA.Data;
using MyMVA.Requirements;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyMVA
{
    /// <summary>
    /// This Configures and Injects specified services to .NET Core pipeline
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Logger for logging
        /// </summary>
        public ILogger<Startup> Logger { get; set; }

        /// <summary>
        /// Configuration details 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="configuration">Configuration details for use</param>
        /// <param name="logger">Logger for logging</param>
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;

            Logger.LogDebug("Constructor finished running");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Service Collection, use this to add services to the DI Container</param>        
        public void ConfigureServices(IServiceCollection services)
        {

            Logger.LogDebug($"Adding Cookie Policy GDPR...");

            //Configure GDPR
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            Logger.LogDebug($"Adding Database Context with ConnectionString => {Configuration.GetConnectionString("DefaultConnection")}...");

            //Adds Database Context, Using SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));


            Logger.LogDebug($"Adding Identity -> IdentityUI -> Identity EntityFramework Stores -> Default Token Providers");

            //Adds Identity for User Management
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            Logger.LogDebug($"Adding Authorization Policy based on ClaimTypes or IAuthorizationRequirement");

            //Adds authorization policy
            services.AddAuthorization(options =>
            {
                //Adds a Canadian Only Policy
                options.AddPolicy("CanadiansOnly", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Country, "Canada");
                });

                //Adds a Canadian or Admin Policy
                options.AddPolicy("CanadiansorAdmin", policy =>
                {
                    policy.AddRequirements(new CanadianRequirement());
                });
            });


            Logger.LogDebug($"Adding OAuth 2.0 Authentication for Microsoft account...");

            //Adds Oauth 2.0 Authentication with Microsoft account
            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:Password"];
            });

            Logger.LogDebug($"Configuring Password Policy");

            //Change Password Policy
            services.Configure<IdentityOptions>(options =>
            {
                //Make really weak passwords possible
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 5;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                //Make sure unique emails are
                options.User.RequireUniqueEmail = true;
            });


            Logger.LogDebug($"Adding MVC services to the DI container");

            //Adds MVC support
            services.AddMvc()

                //Adds Authorized Access to folders and pages specified
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });


            Logger.LogDebug($"Adding IEmailSender as a Singleton instance in the DI container");


            //Add IEmailSender service for users email management
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();


            ////Create roles and add users to them
            //Task.Run(async () =>
            //{
            //    var rolemanager = new UserRoleManager(app.ApplicationServices, loggerFactory, env);

            //    await rolemanager.AddUserToRole("dimon2255@gmail.com", "password", "Admin");
            //    await rolemanager.AddUserToRole("dimon2255@inbox.com", "password", "Manager");
            //    await rolemanager.AddUserToRole("pankov.dimon2255@yandex.ru", "password", "TeamLead");

            //    //Add claims for users
            //    await rolemanager.AddClaimToUserAsync("dimon2255@gmail.com", new Claim(ClaimTypes.Country, "Canada"));
            //    await rolemanager.AddClaimToUserAsync("dimon2255@gmail.com", new Claim(ClaimTypes.DateOfBirth, "02041983"));
            //    await rolemanager.AddClaimToUserAsync("dimon2255@gmail.com", new Claim(ClaimTypes.Gender, "Male"));
            //    await rolemanager.AddClaimToUserAsync("dimon2255@gmail.com", new Claim(ClaimTypes.Surname, "Dima"));
            //});


        }
    }
}
