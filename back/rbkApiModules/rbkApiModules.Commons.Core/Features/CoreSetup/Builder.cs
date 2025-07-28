using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("rbkApiModules.Commons.Relational")]

namespace rbkApiModules.Commons.Core;


public class RbkApiCoreOptions
{
    #region IHttpContextAccessor 

    internal bool _useDefaultHttpContextAccessor = false;

    public RbkApiCoreOptions UseHttpContextAccessor()
    {
        _useDefaultHttpContextAccessor = true;
        return this;
    }

    #endregion

    #region Response compression

    internal bool _useDefaultCompression = false;
    internal Action<ResponseCompressionOptions> _userCompressionOptions = null;
    internal Action<GzipCompressionProviderOptions> _userProviderCompressionOptions = null;

    public RbkApiCoreOptions UseDefaultCompression()
    {
        _useDefaultCompression = true;
        return this;
    }

    public RbkApiCoreOptions UseCustomCompression(Action<ResponseCompressionOptions> compressionOptions, Action<GzipCompressionProviderOptions> providerOptions = null)
    {
        if (compressionOptions == null) throw new ArgumentNullException(nameof(compressionOptions));
        _userCompressionOptions = compressionOptions;
        _userProviderCompressionOptions = providerOptions;
        return this;
    }

    #endregion

    #region Service registration

    internal List<Assembly> _assembliesForServices = new List<Assembly>();

    public RbkApiCoreOptions RegisterServices(params Assembly[] assemblies)
    {
        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
        _assembliesForServices.AddRange(assemblies);
        return this;
    }

    #endregion

    #region Http client

    internal bool _useDefaultHttpClient = false;

    public RbkApiCoreOptions UseDefaultHttpClient()
    {
        _useDefaultHttpClient = true;
        return this;
    }

    #endregion

    #region Memory cache

    internal bool _useDefaultMemoryCache = false;
    internal Action<MemoryCacheOptions> _userMemoryCacheOptions = null;

    public RbkApiCoreOptions UseDefaultMemoryCache()
    {
        _useDefaultMemoryCache = true;
        return this;
    }

    public RbkApiCoreOptions UseCustomMemoryCache(Action<MemoryCacheOptions> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _userMemoryCacheOptions = options;
        return this;
    }

    #endregion

    #region Https redirection

    internal bool _useDefaultHttpsRedirection = false;
    internal Action<HttpsRedirectionOptions> _userHttpsRedirectionOptions = null;

    public RbkApiCoreOptions UseDefaultHttpsRedirection()
    {
        _useDefaultHttpsRedirection = true;

        return this;
    }

    public RbkApiCoreOptions UseCustomHttpsRedirection(Action<HttpsRedirectionOptions> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _userHttpsRedirectionOptions = options;
        return this;
    }

    #endregion

    #region CORS

    internal bool _useDefaultCors = false;
    internal string _defaultCorsPolicy = string.Empty;
    internal Action<CorsOptions> _userCorsOptions = null;

    public RbkApiCoreOptions UseDefaultCors()
    {
        _useDefaultCors = true;
        return this;
    }

    public RbkApiCoreOptions UseCustomCors(string policyName, Action<CorsOptions> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(policyName)) throw new ArgumentNullException(nameof(policyName));
        _userCorsOptions = options;
        _defaultCorsPolicy = policyName;
        return this;
    }

    #endregion

    #region Swagger

    internal bool _useDefaultSwaggerOptions = false;
    // internal Action<SwaggerGenOptions> _userSwaggerOptions = null;
    internal string _applicationName = "Default";
    internal string _forceSwaggerBaseUrl = null;

    public RbkApiCoreOptions UseDefaultSwagger(string applicationName)
    {
        _useDefaultSwaggerOptions = true;
        _applicationName = applicationName;
        return this;
    }

    //public RbkApiCoreOptions UseDefaultSwagger(string applicationName, string forceSwaggerBaseUrl)
    //{
    //    _forceSwaggerBaseUrl = forceSwaggerBaseUrl;
    //    return UseDefaultSwagger(applicationName);
    //}

    //public RbkApiCoreOptions UseCustomSwagger(Action<SwaggerGenOptions> options)
    //{
    //    if (options == null) throw new ArgumentNullException(nameof(options));
    //    _userSwaggerOptions = options;
    //    return this;
    //}

    #endregion

    #region HSTS

    internal bool _isDevelopment = false;
    internal bool _useDefaultHsts = false;
    internal Action<HstsOptions> _userHstsOptions = null;

    public RbkApiCoreOptions UseDefaultHsts(bool isDevelopment)
    {
        _isDevelopment = isDevelopment;
        _useDefaultHsts = true;
        return this;
    }

    public RbkApiCoreOptions UseCustomHsts(Action<HstsOptions> options, bool isDevelopment)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        _isDevelopment = isDevelopment;
        _userHstsOptions = options;
        return this;
    }

    #endregion

    #region Basic authentication handler

    internal bool _enableBasicAuthenticationHandler = false;

    public RbkApiCoreOptions EnableBasicAuthenticationHandler()
    {
        _enableBasicAuthenticationHandler = true;
        return this;
    }

    #endregion

    #region Custom validators assemblies

    internal List<Assembly> _assembliesForCustomValidators = new List<Assembly>();

    public RbkApiCoreOptions RegisterAdditionalValidators(params Assembly[] assemblies)
    {
        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
        _assembliesForCustomValidators.AddRange(assemblies);
        return this;
    }

    #endregion

    #region SignalR hubs

    internal Action<IEndpointRouteBuilder> _hubMappings = null;

    public RbkApiCoreOptions MapSignalR(Action<IEndpointRouteBuilder> mappings)
    {
        if (mappings == null) throw new ArgumentNullException(nameof(mappings));
        _hubMappings = mappings;
        return this;
    }

    #endregion

    #region Static files (when not using SPA on root)

    internal bool _useStaticFilesForApi = false;

    public RbkApiCoreOptions UseStaticFiles()
    {
        _useStaticFilesForApi = true;
        return this;
    }

    #endregion

    #region SPA routes

    internal List<(string Route, string FallbackFile)> _spaRoutes = new List<(string, string)>();
    internal bool _useSpaOnRoot = false;

    public RbkApiCoreOptions MapSpas(params string[] routes)
    {
        if (routes == null) throw new ArgumentNullException(nameof(routes));
        foreach (var route in routes)
        {
            var sanitizedRoute = $"/{route.Trim('/').ToLower()}";
            _spaRoutes.Add(new(sanitizedRoute, $"{sanitizedRoute}/index.html"));
        }
        return this;
    }
    public RbkApiCoreOptions UseSpaOnRoot()
    {
        _useSpaOnRoot = true;
        return this;
    }

    #endregion

    #region Localization

    internal string _defaultLocalization = "en-us";

    public RbkApiCoreOptions UseDefaultLocalizationLanguage(string code)
    {
        if (code == null) throw new ArgumentNullException(nameof(code));

        _defaultLocalization = code;
        return this;
    }

    #endregion

    #region PathBase

    internal string _pathBase = null;

    public RbkApiCoreOptions UsePathBase(string pathBase)
    {
        if (pathBase == null) throw new ArgumentNullException(nameof(pathBase));

        _pathBase = "/" + pathBase.Trim('/');
        return this;
    }

    #endregion


    #region DbContexts to be registerd in the DI container

    internal List<Type> _dbcontexts = [];

    public RbkApiCoreOptions RegisterDbContext<TDbContext>()
    {
        _dbcontexts.Add(typeof(TDbContext));
        return this;
    }

    #endregion
}

public static class CommonsCoreBuilder
{
    public static void AddRbkApiCoreSetup(this IServiceCollection services, Action<RbkApiCoreOptions> optionsConfig)
    {
        var options = new RbkApiCoreOptions();
        optionsConfig(options);

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;

        services.AddSingleton(options);

        services.AddMessaging();

        #region Response Compression

        if (options._useDefaultCompression)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        }

        if (options._userCompressionOptions != null)
        {
            if (options._useDefaultCompression)
            {
                throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseCustomCompression)} cannot be used with {nameof(RbkApiCoreOptions.UseDefaultCompression)}, choose either one or another");
            }

            services.AddResponseCompression(options._userCompressionOptions);

            if (options._userProviderCompressionOptions != null)
            {
                services.Configure<GzipCompressionProviderOptions>(options._userProviderCompressionOptions);
            }
            else
            {
                services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            }
        }

        #endregion

        #region Application services

        services.RegisterApplicationServices(Assembly.GetAssembly(typeof(CommonsCoreBuilder)));

        foreach (var assembly in options._assembliesForServices)
        {
            services.RegisterApplicationServices(options._assembliesForServices.ToArray());
        }

        #endregion

        #region Basic HttpClient

        if (options._useDefaultHttpClient)
        {
            services.AddHttpClient();
        }

        #endregion

        #region Memory cache

        if (options._useDefaultMemoryCache)
        {
            services.AddMemoryCache();
        }

        if (options._userMemoryCacheOptions != null)
        {
            if (options._useDefaultMemoryCache)
            {
                throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseCustomMemoryCache)} cannot be used with {nameof(RbkApiCoreOptions.UseDefaultMemoryCache)}, choose either one or another");
            }

            services.AddMemoryCache(options._userMemoryCacheOptions);
        }

        #endregion

        #region IHttpContextAccessor registration

        if (options._useDefaultHttpClient)
        {
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }

        #endregion

        #region HTTPS redirection

        if (options._useDefaultHttpsRedirection)
        {
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443;
            });
        }

        if (options._userHttpsRedirectionOptions != null)
        {
            if (options._useDefaultHttpsRedirection)
            {
                throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseDefaultHttpsRedirection)} cannot be used with {nameof(RbkApiCoreOptions.UseCustomHttpsRedirection)}, choose either one or another");
            }

            services.AddHttpsRedirection(options._userHttpsRedirectionOptions);
        }

        #endregion

        #region CORS

        if (options._useDefaultCors)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyOrigin()
                        .WithExposedHeaders("Content-Disposition");
                    });
            });
        }

        if (options._userCorsOptions != null)
        {
            if (options._useDefaultCors)
            {
                throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseCustomCors)} cannot be used with {nameof(RbkApiCoreOptions.UseDefaultCors)}, choose either one or another");
            }

            services.AddCors(options._userCorsOptions);
        }

        #endregion

        #region Swagger

        if (options._useDefaultSwaggerOptions)
        {
            // services.AddEndpointsApiExplorer();

            // services.AddOpenApi();
            //services.ConfigureSwaggerGen(config =>
            //{
            //    config.CustomSchemaIds(x => x.FullName.Replace("+", "."));

            //    config.SwaggerDoc("identity", new OpenApiInfo { Title = "Identity API", Version = "v1" });

            //    config.DocInclusionPredicate((docName, apiDesc) =>
            //    {
            //        var tags = apiDesc.ActionDescriptor?.EndpointMetadata?
            //                .OfType<TagsAttribute>()
            //                .SelectMany(x => x.Tags)
            //                .Where(x => !string.IsNullOrEmpty(x))
            //                .ToList();

            //        if (tags.Contains("Roles") || tags.Contains("Tenants") || tags.Contains("Claims") || tags.Contains("Authentication") || tags.Contains("Authorization"))
            //        {
            //            return docName == "identity";
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    });

            //    config.AddSecurityDefinition("Api-Key", new OpenApiSecurityScheme
            //    {
            //        Type = SecuritySchemeType.ApiKey,
            //        In = ParameterLocation.Header,
            //        Name = "Api-Key",
            //        Description = "Please inser the API key in the 'value' field",
            //    });

            //    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Type = SecuritySchemeType.ApiKey,
            //        In = ParameterLocation.Header,
            //        Name = "Authorization",
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        Description = "Please insert the JWT token prefixed with 'Bearer' in the 'value' field",
            //    });

            //    config.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //          {
            //              new OpenApiSecurityScheme
            //              {
            //                  Reference = new OpenApiReference
            //                  {
            //                      Type = ReferenceType.SecurityScheme,
            //                      Id = "Api-Key"
            //                  }
            //              },
            //              Array.Empty<string>()
            //          },
            //          {
            //              new OpenApiSecurityScheme
            //              {
            //                  Reference = new OpenApiReference
            //                  {
            //                      Type = ReferenceType.SecurityScheme,
            //                      Id = "Bearer"
            //                  }
            //              },
            //              Array.Empty<string>()
            //          },
            //    });


            //    config.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            //            },
            //            new string[] {}
            //        }
            //    });

            //});
        }

        //if (options._userSwaggerOptions != null)
        //{
        //    //if (options._useDefaultSwaggerOptions)
        //    //{
        //    //    throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseCustomSwagger)} cannot be used with {nameof(RbkApiCoreOptions.UseDefaultSwagger)}, choose either one or another");
        //    //}

        //    //services.AddSwaggerGen(options._userSwaggerOptions);
        //}

        #endregion

        #region Basic authentication (usually for SharedUI)

        if (options._enableBasicAuthenticationHandler)
        {
            services.AddAuthentication(BasicAuthenticationHandler.Basic)
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(BasicAuthenticationHandler.Basic, null);
        }

        #endregion

        #region HSTS

        if (options._useDefaultHsts)
        {
            if (!options._isDevelopment)
            {
                // https://aka.ms/aspnetcore-hsts
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });
            }
        }

        if (options._userHstsOptions != null)
        {
            if (options._useDefaultHsts)
            {
                throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseCustomHsts)} cannot be used with {nameof(RbkApiCoreOptions.UseDefaultHsts)}, choose either one or another");
            }

            if (!options._isDevelopment)
            {
                // https://aka.ms/aspnetcore-hsts
                services.AddHsts(options._userHstsOptions);
            }
        }

        #endregion

        #region SignalR

        if (options._hubMappings != null)
        {
            services.AddSignalR();
        }

        #endregion

        #region HttpContextAccessor

        if (options._useDefaultHttpContextAccessor)
        {
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
        }

        #endregion

        #region Additional custom validators

        foreach (var assembly in options._assembliesForCustomValidators)
        {
            services.RegisterFluentValidators(assembly);
        }

        #endregion

        #region Localization

        services.AddSingleton(serviceProvicer =>
        {
            var logger = serviceProvicer.GetRequiredService<ILogger<LocalizationCache>>();

            return new LocalizationCache(logger);
        });

        #endregion

        #region DbContexts to be registerd in the DI container

        foreach (var context in options._dbcontexts)
        {
            services.AddScoped(typeof(DbContext), context);
        }

        #endregion  
    }

    public static IApplicationBuilder UseRbkApiCoreSetup(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var options = scope.ServiceProvider.GetService<RbkApiCoreOptions>();

            #region Global error handler

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            #endregion

            #region Response compression

            if (options._useDefaultCompression || options._userCompressionOptions != null)
            {
                app.UseResponseCompression();
            }

            #endregion

            #region HSTS

            if (options._useDefaultHsts || options._userHstsOptions != null)
            {
                app.UseHttpsRedirection();

                if (!options._isDevelopment)
                {
                    app.UseHsts();
                }
            }

            #endregion

            #region Swagger


            //if (options._useDefaultSwaggerOptions || options._userSwaggerOptions != null)
            //{
            //    // app.MapOpenApi();

            //    //if (options._useDefaultSwaggerOptions || options._userSwaggerOptions != null)
            //    //{
            //    //    if (options._forceSwaggerBaseUrl != null)
            //    //    {
            //    //        app.UseSwagger(x =>
            //    //        {
            //    //            x.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            //    //            {
            //    //                swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = options._forceSwaggerBaseUrl } };
            //    //            });
            //    //        });
            //    //    }
            //    //    else if (options._pathBase != null)
            //    //    {
            //    //        var basePath = options._pathBase;
            //    //        app.UseSwagger(x =>
            //    //        {
            //    //            x.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            //    //            {
            //    //                swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{basePath}" } };
            //    //            });
            //    //        });
            //    //    }
            //    //    else
            //    //    {
            //    //        app.UseSwagger();
            //    //    }
            //    //}

            //    //app.UseSwaggerUI(c =>
            //    //{
            //    //    // c.SwaggerEndpoint("/openapi/v1.json", options._applicationName);
            //    //    c.SwaggerEndpoint("/swagger/identity/swagger.json", "Identity API v1");
            //    //    c.RoutePrefix = "swagger";
            //    //    c.DocExpansion(DocExpansion.Full);
            //    //});
            //}

            #endregion

            #region SPA routes

            // TODO: Needs to be tested with minimal APIs
            //foreach (var route in options._spaRoutes)
            //{
            //    app.MapWhen((context) => context.Request.Path.StartsWithSegments(route.Route), (localAppBuilder) =>
            //    {
            //        localAppBuilder.UseStaticFiles();

            //        localAppBuilder.UseRouting();

            //        localAppBuilder.UseEndpoints(endpoints =>
            //        {
            //            endpoints.MapFallbackToFile(route.FallbackFile);
            //        });
            //    });
            //}

            #endregion

            #region Authorization and Authentication

            app.UseAuthentication();
            app.UseAuthorization();

            #endregion

            #region SPA on root

            // TODO: Needs to be tested with minimal APIs
            //if (options._useSpaOnRoot)
            //{
            //    app.MapWhen((context) =>
            //    {
            //        var isApi = context.Request.Path.StartsWithSegments("/api");
            //        var isSharedUi = context.Request.Path.StartsWithSegments("/shared-ui");
            //        var hasOtherSpaRoutes = false;

            //        foreach (var route in options._spaRoutes)
            //        {
            //            hasOtherSpaRoutes = hasOtherSpaRoutes || context.Request.Path.StartsWithSegments(route.FallbackFile);
            //        }

            //        return !isApi && !hasOtherSpaRoutes && !isSharedUi;
            //    }, (appBuilder) =>
            //    {
            //        Log.Logger.Debug($"Enabling static files for landing page");
            //        app.UseStaticFiles();

            //        Log.Logger.Debug($"Enabling routing for landing page");
            //        appBuilder.UseRouting();

            //        Log.Logger.Debug($"Enabling endpoint fallback for landing page");
            //        appBuilder.UseEndpoints(endpoints =>
            //        {
            //            endpoints.MapFallbackToFile("/index.html");
            //        });
            //    });
            //}

            if (options._useStaticFilesForApi)
            {
                if (options._useSpaOnRoot) throw new InvalidOperationException($"{nameof(RbkApiCoreOptions.UseStaticFiles)} cannot be used with {nameof(RbkApiCoreOptions.UseSpaOnRoot)}. Static files are automatically enabled for SPAs");

                app.UseStaticFiles();
            }

            #endregion

            #region PathBase

            if (options._pathBase != null)
            {
                app.UsePathBase(options._pathBase);
            }

            #endregion
        }
        return app;
    }
}

