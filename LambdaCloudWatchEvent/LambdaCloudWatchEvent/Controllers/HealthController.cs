using Amazon.Runtime.Internal.Util;
using LambdaCloudWatchEvent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LambdaCloudWatchEvent.Controllers
{
    public class HealthController
    {
        private IHealthService _healthService;
        private ILogger<HealthController> _logger;
        public HealthController(ILogger<HealthController> logger,IHealthService healthService)
        {
            _logger = logger;
            _healthService = healthService;
        }

        [HttpGet]
        [Route("/v1/GetHealthStatus")]
        public IActionResult GetHealthStatus()
        {
            _logger.LogInformation("In Health Controller");
            
            return new OkObjectResult(_healthService.HealthCheck());
        }
    }
}
