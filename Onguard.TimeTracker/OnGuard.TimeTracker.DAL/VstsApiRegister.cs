using Microsoft.Extensions.DependencyInjection;

namespace Onguard.TimeTracker.DAL
{
    public static class VstsApiRegister
    {
        public static void AddVstsApi(this IServiceCollection services)
        {
            services.AddScoped<IVstsApi>((_) => new VstsApi(VstsApiConfiguration.Url, VstsApiConfiguration.Psa));
        }
    }
}
