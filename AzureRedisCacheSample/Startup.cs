using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzureRedisCacheSample.Startup))]
namespace AzureRedisCacheSample
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
