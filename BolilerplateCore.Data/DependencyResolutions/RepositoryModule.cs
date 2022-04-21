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

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class RepositoryModule
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SqlServerDbContext>(options => options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection"),
            msSqlServerOptions => msSqlServerOptions.MigrationsAssembly("BoilerplateCore.Data")));
            
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
            //services.AddMvc(option => option.EnableEndpointRouting = false)
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            //    .AddNewtonsoftJson(opt =>
            //    {
            //        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //    });
            services.AddHealthChecks();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                
                // for email confirmation
                //options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<SqlServerDbContext>()
            .AddDefaultTokenProviders();

            services.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
            services.AddScoped<IDbContext, SqlServerDbContext>();

            //services.AddTransient<IPrizeRepository, PrizeRepository>();
            //services.AddTransient<ICategoryRepository, CategoryRepository>();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DbMigrator.Migrate(app);
            if (env.IsDevelopment())
            {
                DataSeeder.Seed(app);
            }
        }
    }
}
