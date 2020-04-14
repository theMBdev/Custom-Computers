using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CustomComputersGU.Startup))]
namespace CustomComputersGU
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
