using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrphanChildrenSupport.HttpClientFactory.Libraries;
using OrphanChildrenSupport.Infrastructure.Data;
using OrphanChildrenSupport.Services.Contracts;
using OrphanChildrenSupport.Servicess;
using OrphanChildrenSupport.Tools;
using OrphanChildrenSupport.Tools.Encryptions;
using OrphanChildrenSupport.Tools.HttpContextExtensions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OrphanChildrenSupport.IoC.Configuration.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                services.AddTransient<IPersonalProfileService, PersonalProfileService>();
            }
        }

        public static void ConfigureHelperServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                services.AddSingleton<ICryptoEncryptionHelper, CryptoEncryptionHelper>();
                services.AddScoped<IHttpContextHelper, HttpContextHelper>();
            }
        }

        public static void ConfigureRepositoryServices(this IServiceCollection services)
        {
            if (services != null)
            {
            }
        }

        public static void ConfigureADFSServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                var Wtrealm = configuration.GetValue<string>("ADFS:Wtrealm");
                var MetadataAddress = configuration.GetValue<string>("ADFS:MetadataAddress");
                services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
                    //sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }).AddWsFederation(options =>
                {
                    options.Wtrealm = Wtrealm;
                    options.MetadataAddress = MetadataAddress;

                    options.Events.OnRemoteFailure = context =>
                    {
                        Console.WriteLine(context.Failure);
                        if (context.Failure.Message.Contains("Correlation failed."))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                        }

                        return Task.FromResult(0);
                    };
                }).AddCookie();

                services.AddHttpContextAccessor();
            }
        }

        public static void ConfigureMappings(this IServiceCollection services)
        {
            if (services != null)
            {
                //Automap settings
                services.AddAutoMapper(Assembly.GetExecutingAssembly());
            }
        }

        public static void ConfigureDbContexts(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                //DbContext settings
                services.AddControllersWithViews();
                services.AddDbContext<OrphanChildrenSupportDbContext>(options =>
                       options.UseSqlServer(configuration.GetConnectionString("OrphanChildrenSupportConnection")), ServiceLifetime.Transient);

                //services.AddIdentity<IdentityUser, IdentityRole>(options =>
                //{
                //    options.Password.RequiredLength = 1;
                //    options.Password.RequireLowercase = false;
                //    options.Password.RequireUppercase = false;
                //    options.Password.RequireNonAlphanumeric = false;
                //    options.Password.RequireDigit = false;
                //})
                //.AddUserStore<OrphanChildrenSupportDbContext>()
                //.AddDefaultTokenProviders();
            }
        }
    }
}
