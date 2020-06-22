using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LambdaCloudWatchEvent.Services
{
    public interface IHealthService
    {
        public string HealthCheck();
       
    }
    public class HealthService:IHealthService
    {
        private ILogger<HealthService> _logger;
        public HealthService(ILogger<HealthService> logger)
        {
            _logger = logger;
        }
        public string HealthCheck()
        {
            
            _logger.LogInformation($"Endpoint is healthy at {DateTime.Now}");



            return "Endpoint is healthy";
        }
    }
}
