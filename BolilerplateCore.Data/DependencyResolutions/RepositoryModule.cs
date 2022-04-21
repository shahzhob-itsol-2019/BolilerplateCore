using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Data.IRepository;
using BoilerplateCore.Data.Repository;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class RepositoryModule
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SqlServerDbContext>(
                options => options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                        msSqlServerOptions => msSqlServerOptions.MigrationsAssembly("BoilerplateCore.Data")));
            var componentOptions = services.BuildServiceProvider().GetService<Microsoft.Extensions.Options.IOptionsSnapshot<ComponentOptions>>();
            if (componentOptions.Value.Security.SecurityService == "AspnetIdentity")
            {
                services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<SqlServerDbContext>()
                .AddDefaultTokenProviders();

                // AddCors must be before AddMvc
                services.AddCors(options =>
                {
                    options.AddPolicy(
                        "CorsPolicy",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .WithExposedHeaders("x-pagination");
                        });
                });
                services.AddControllers();
                services.AddHealthChecks();

                services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
                services.AddTransient<IDbContext, SqlServerDbContext>();
            }

            services.AddScoped<IDbContext, SqlServerDbContext>();
            services.AddScoped<IUnitOfWork>(unitOfWork => new UnitOfWork(unitOfWork.GetService<IDbContext>()));

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICompanyRepository, CompanyRepository>();
            services.AddTransient<IAddressRepository, AddressRepository>();
            services.AddTransient<ICityRepository, CityRepository>();
            services.AddTransient<ICountryRepository, CountryRepository>();

            services.AddTransient<IStatusRepository, StatusRepository>();
            services.AddTransient<IStatusTypeRepository, StatusTypeRepository>();

            services.AddTransient<INotificationRepository, NotificationRepository>();
            services.AddTransient<INotificationTemplateRepository, NotificationTemplateRepository>();
            services.AddTransient<INotificationTypeRepository, NotificationTypeRepository>();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DbMigrator.Migrate(app);
            DataSeeder.Seed(app);
        }
    }
}
