using Clients;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace DPoPApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors();
            services.AddDistributedMemoryCache();

            // this API will accept any access token from the authority
            services.AddAuthentication("token")
                .AddJwtBearer("bearer", options =>
                {
                    options.Authority = Constants.Authority;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.Principal.HasClaim(x => x.Type == JwtClaimTypes.Confirmation))
                            {
                                context.Fail("Must use DPoP when using a token with a 'cnf' claim");
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddJwtBearer("dpop", options =>
                {
                    options.Authority = Constants.Authority;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

                    options.Challenge = "DPoP";
                    options.EventsType = typeof(DPoPJwtBearerEvents);
                });

            services.AddTransient<DPoPJwtBearerEvents>();
            services.AddDistributedMemoryCache();
            services.AddTransient<IReplayCache, DefaultReplayCache>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("token", policy =>
                {
                    policy.AddAuthenticationSchemes("bearer", "dpop");
                    policy.RequireAuthenticatedUser();
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                await next();
            });

            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "https://localhost:44300");

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization("token");
            });
        }
    }
}