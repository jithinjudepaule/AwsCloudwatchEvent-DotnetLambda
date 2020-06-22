using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.AspNetCoreServer.Internal;
using Amazon.Lambda.Core;
using LambdaCloudWatchEvent.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace LambdaCloudWatchEvent
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// LambdaCloudWatchEvent::LambdaCloudWatchEvent.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :

        // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
        // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
        //
        // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
        // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        // 
        // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
        // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {

        protected override void MarshallRequest(InvokeFeatures features,APIGatewayProxyRequest apiGatewayRequest, ILambdaContext lambdaContext)
        {
           //string serializedRequest = JsonConvert.SerializeObject(apiGatewayRequest);
            lambdaContext.Logger.LogLine($"REQUEST: {JsonConvert.SerializeObject(apiGatewayRequest)}");
            lambdaContext.Logger.LogLine($"REQUEST Context: {JsonConvert.SerializeObject(apiGatewayRequest.RequestContext)}");

            if (apiGatewayRequest.RequestContext==null)
            {
                CallHealthCheck(lambdaContext);
            }
            else
            {
                base.MarshallRequest(features, apiGatewayRequest, lambdaContext);
            }

        }

        private void CallHealthCheck(ILambdaContext lambdaContext)
        {
            try
            {

           
            lambdaContext.Logger.Log($"Execution started for function - {lambdaContext.FunctionName} at {DateTime.Now}");
           //  //  lambdaContext.Logger.Log($"Environment {environment}");
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                //.AddJsonFile(string.Format($"appsettings.{environment}.json", environment), true, true)
                .AddEnvironmentVariables();
            var configuration = configBuilder.Build();
                lambdaContext.Logger.Log($"confi {configuration}");
                var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IHealthService, HealthService>();

            //CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => { builder.AllowAnyOrigin(); });
            });

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
                lambdaContext.Logger.Log($"GetAWSOptions after");
                services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();
                lambdaContext.Logger.Log($"Before healthcheck and after seviceprovide {serviceProvider}");
             var response=   serviceProvider.GetService<IHealthService>().HealthCheck();
                lambdaContext.Logger.Log($"response {response}");

           lambdaContext.Logger.Log($"Finished execution for function - {lambdaContext.FunctionName} at {DateTime.Now}");
            }
            catch (Exception ex)
            {

                lambdaContext.Logger.Log(ex.ToString());
            }

        }

        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
        }
    }
}
