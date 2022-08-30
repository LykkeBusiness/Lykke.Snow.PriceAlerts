using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Lykke.Snow.PriceAlerts.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Snow.PriceAlerts.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : ControllerBase
    {
        private static readonly object Version =
            new
            {
                Title = typeof(Program).Assembly.Attribute<AssemblyTitleAttribute>(attribute => attribute.Title),
                Version = typeof(Program).Assembly.Attribute<AssemblyInformationalVersionAttribute>(attribute =>
                    attribute.InformationalVersion),
                OS = RuntimeInformation.OSDescription.TrimEnd(),
                ProcessId = Process.GetCurrentProcess().Id
            };

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(Version);
        }
    }
}