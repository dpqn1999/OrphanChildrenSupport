using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using OrphanChildrenSupport.Common.Attributes;
using OrphanChildrenSupport.Common.Settings;
using OrphanChildrenSupport.Extensions;
using OrphanChildrenSupport.Infrastructure;
using OrphanChildrenSupport.Services;
using OrphanChildrenSupport.Swagger;
using OrphanChildrenSupport.IoC.Configuration.DI;

namespace OrphanChildrenSupport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _appsettingsConfigurationSection = Configuration.GetSection(nameof(AppSettingsSwagger));
            _appSettings = _appsettingsConfigurationSection.Get<AppSettingsSwagger>();
        }

        public IConfiguration Configuration { get; }

        private IConfigurationSection _appsettingsConfigurationSection;
        private AppSettingsSwagger _appSettings;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetSection("AppSettings").Bind(AppSettings.Default);

            try
            {
                if (_appSettings.IsValid())
                {
                    Log.Fatal("Startup::ConfigureServices::valid AppSettings");

                    services.Configure<AppSettingsSwagger>(_appsettingsConfigurationSection);
                    Log.Fatal("Startup::ConfigureServices::AppSettings loaded for DI");

                    services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                    services.AddControllersWithViews(opts =>
                    {
                        opts.Filters.Add<SerilogMvcLoggingAttribute>();
                    });

                    services.AddNodeServicesWithHttps(Configuration);

#pragma warning disable CS0618 // Type or member is obsolete
                    services.AddSpaPrerenderer();
#pragma warning restore CS0618 // Type or member is obsolete

                    // Add your own services here.
                    services.AddScoped<AccountService>();
                    services.AddScoped<PersonService>();

                    services.AddMvcCore().AddApiExplorer();
                    services.AddCors();
                    services.AddControllers(
                        opt =>
                        {
                            //Custom filters, if needed
                            //opt.Filters.Add(typeof(CustomFilterAttribute));
                            opt.Filters.Add(new ProducesAttribute("application/json"));
                        })
                        .AddNewtonsoftJson(
                        opt =>
                        {
                            opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        })
                        .SetCompatibilityVersion(CompatibilityVersion.Latest);

                    //API versioning
                    services.AddApiVersioning(
                        o =>
                        {
                            //o.Conventions.Controller<UserController>().HasApiVersion(1, 0);
                            o.ReportApiVersions = true;
                            o.AssumeDefaultVersionWhenUnspecified = true;
                            o.DefaultApiVersion = new ApiVersion(1, 0);
                            o.ApiVersionReader = new UrlSegmentApiVersionReader();
                        }
                        );

                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    services.AddVersionedApiExplorer(
                    options =>
                    {
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route templates
                        options.SubstituteApiVersionInUrl = true;
                    });

                    //SWAGGER
                    if (_appSettings.Swagger.Enabled)
                    {
                        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

                        services.AddSwaggerGen(options =>
                        {
                            options.OperationFilter<SwaggerDefaultValues>();

                            //1-Get all the assemblies of the project to add the related XML Comments
                            Assembly currentAssembly = Assembly.GetExecutingAssembly();
                            AssemblyName[] referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                            IEnumerable<AssemblyName> allAssemblies = null;

                            if (referencedAssemblies != null && referencedAssemblies.Any())
                                allAssemblies = referencedAssemblies.Union(new AssemblyName[] { currentAssembly.GetName() });
                            else
                                allAssemblies = new AssemblyName[] { currentAssembly.GetName() };

                            IEnumerable<string> xmlDocs = allAssemblies
                                    .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                                    .Where(f => File.Exists(f));

                            //2-Add the path to the XML comments for the assemblies having enabled the doc generation
                            if (xmlDocs != null && xmlDocs.Any())
                            {
                                foreach (var item in xmlDocs)
                                {
                                    options.IncludeXmlComments(item);
                                }
                            }
                        });
                    }

                    //Mappings
                    services.ConfigureMappings();

                    //Business settings            
                    services.ConfigureBusinessServices(Configuration);

                    //HttpClient Factory setting
                    //services.ConfigureHttpClientFactory(Configuration);

                    //Helpers settings
                    services.ConfigureHelperServices(Configuration);

                    //Repositories settings
                    services.ConfigureRepositoryServices();

                    //ADFS settings
                    services.ConfigureADFSServices(Configuration);

                    //DbContext
                    services.ConfigureDbContexts(Configuration);

                    Log.Fatal("Startup::ConfigureServices::ApiVersioning, Swagger and DI settings");

                    // In production, the React files will be served from this directory
                    services.AddSpaStaticFiles(configuration =>
                    {
                        configuration.RootPath = "ClientApp/dist";
                    });
                }
                else
                    Log.Fatal("Startup::ConfigureServices::invalid AppSettings");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            Log.Fatal("Startup::Configure");
            Log.Fatal($"Startup::Configure::Environment:{env.EnvironmentName}");

            try
            {
                app.UseMiddleware<ExceptionMiddleware>();

                // Adds an IP address to your log's context.
                app.Use(async (context, next) =>
                {
                    using (LogContext.PushProperty("IPAddress", context.Connection.RemoteIpAddress))
                    {
                        await next.Invoke();
                    }
                });

                // Build your own authorization system or use Identity.
                app.Use(async (context, next) =>
                {
                    var accountService = (AccountService)context.RequestServices.GetService(typeof(AccountService));
                    var verifyResult = accountService.Verify(context);
                    if (!verifyResult.HasErrors)
                    {
                        context.Items.Add(Constants.HttpContextServiceUserItemKey, verifyResult.Value);
                    }
                    await next.Invoke();
                    // Do logging or other work that doesn't write to the Response.
                });

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
#pragma warning disable CS0618 // Type or member is obsolete
                    WebpackDevMiddleware.UseWebpackDevMiddleware(app, new WebpackDevMiddlewareOptions
                    {
                        HotModuleReplacement = true,
                        ReactHotModuleReplacement = true
                    });
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
                    app.UseExceptionHandler("/Main/Error");
                    Log.Fatal("Setting not development exception handling settings.");

                    //Both alternatives are usable for general error handling:
                    // - middleware
                    // - UseExceptionHandler()

                    //app.UseMiddleware(typeof(ErrorHandlingMiddleware));

                    app.UseExceptionHandler(a => a.Run(async context =>
                    {
                        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = feature.Error;
                        var code = HttpStatusCode.InternalServerError;

                        if (exception is ArgumentNullException) code = HttpStatusCode.BadRequest;
                        else if (exception is ArgumentException) code = HttpStatusCode.BadRequest;
                        else if (exception is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;

                        Log.Fatal($"GLOBAL ERROR HANDLER::HTTP:{code}::{exception.Message}");

                        var result = JsonConvert.SerializeObject(exception, Formatting.Indented);

                        context.Response.Clear();
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(result);
                    }));

                    app.UseHsts();
                }

                app.UseDefaultFiles();
                app.UseStaticFiles();

                // Write streamlined request completion events, instead of the more verbose ones from the framework.
                // To use the default framework request logging instead, remove this line and set the "Microsoft"
                // level in appsettings.json to "Information".
                app.UseSerilogRequestLogging();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                //SWAGGER
                if (_appSettings.IsValid())
                {
                    if (_appSettings.Swagger.Enabled)
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI(options =>
                        {
                            foreach (var description in provider.ApiVersionDescriptions)
                            {
                                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                                //options.RoutePrefix = string.Empty;
                            }
                            options.DisplayRequestDuration();
                        });
                    }
                }

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Main}/{action=Index}/{id?}");

                    endpoints.MapFallbackToController("Index", "Main");
                });

                app.UseHttpsRedirection();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }
        }
    }
}