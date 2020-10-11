﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.API.Service;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class ApiGrpc : IPluginBase, IApiHandler
	{
		private GrpcChannel channel;
		private ApiServices.ApiServicesClient client;

		private string serverHost;
		private int serverPort;
		private string certificatePath;
		private string certificatePassword;

		// Diagnostics
		private ILogger log;

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public Task<bool> Initialize(PluginOptions options)
		{
			try
			{
				// Initiate the configuration
				IConfigurationRoot config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName)
#if DEBUG
					.AddJsonFile("appSettings.development.json", false, true)
#else
                    .AddJsonFile("appSettings.json", false, true)
#endif
					.AddJsonFile("logSettings.json", false, true)
					.Build();

				log = new LoggerConfiguration()
					.ReadFrom.Configuration(config)
					.CreateLogger()
					.ForContext(typeof(ApiGrpc));

				// Assign config variables
				serverHost = config.GetSection("Host").Get<string>();
				serverPort = config.GetSection("Port").Get<int>();
				certificatePath = config.GetSection("CertificatePath").Get<string>();
				certificatePassword = config.GetSection("CertificatePassword").Get<string>();

				// Diagnostics
				log.Information("Plugin initialized");

				return Task.FromResult(true);
			}
			catch (Exception e)
			{
				if (log == null)
				{
					Console.WriteLine(e);
				}
				else
				{
					log.Error(e.Message);
				}

				return Task.FromResult(false);
			}
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public Task<bool> Configure()
		{
			try
			{
				// Load certificate
				X509Certificate2 cert = new X509Certificate2(certificatePath, certificatePassword);

				HttpClientHandler handler = new HttpClientHandler();
				handler.ClientCertificates.Add(cert);

				handler.ServerCertificateCustomValidationCallback =
					HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

				channel = GrpcChannel.ForAddress($"{serverHost}:{serverPort}", new GrpcChannelOptions
				{
					HttpHandler = handler
				});

				client = new ApiServices.ApiServicesClient(channel);

				return Task.FromResult(true);
			}
			catch (Exception e)
			{
				log.Error("{0}", e);
				return Task.FromResult(false);
			}
			finally
			{
				if (log == null)
				{
					Console.WriteLine("Completed Configuration stage");
				}
				else
				{
					log.Information("Completed Configuration stage");
				}
			}
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		/// <param name="loginAttemptHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterLoginAttemptHandler(IPluginBase.LoginAttempt loginAttemptHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		/// <param name="blockHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> RegisterUnblockHandler(IPluginBase.Unblock unblockHandler)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> LoginAttemptEvent(SP.Models.LoginAttempts loginAttempt)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> BlockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> UnblockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		public async Task<List<Blocks>> GetUnblock(int minutes)
		{
			// Contact the api
			//string path = $"/block/GetUnblocks?minutes={minutes}";

			//HttpResponseMessage message = await HttpClient.GetAsync(path);

			//if (message.IsSuccessStatusCode)
			//{
			//	return JsonSerializer.Deserialize<List<Blocks>>(await message.Content.ReadAsStringAsync(),
			//		new JsonSerializerOptions
			//		{
			//			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			//		});
			//}

			//log.Error(
			//	$"Invalid response code while calling {path}. Status code: {message.StatusCode}, {message.RequestMessage}");
			return null;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> AddBlock(Blocks block)
		{
			//// Contact the api
			//const string path = "/block/AddBlock";

			//// Add content
			//HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			//// Execute the request
			//HttpResponseMessage message = await PostRequest(path, content);
			//return message.IsSuccessStatusCode;

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> UpdateBlock(Blocks block)
		{
			//// Contact the api
			//const string path = "/block/UpdateBlock";

			//// Add content
			//HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			//// Execute the request
			//HttpResponseMessage message = await PostRequest(path, content);
			//return message.IsSuccessStatusCode;

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> StatisticsUpdateBlocks(Blocks block)
		{
			//// Contact the api
			//const string path = "/statistics/UpdateBlock";

			//// Add content
			//HttpContent content = new StringContent(JsonSerializer.Serialize(block), Encoding.UTF8, "application/json");

			//// Execute the request
			//HttpResponseMessage message = await PostRequest(path, content);
			//return message.IsSuccessStatusCode;

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		/// <param name="detectIPRange"></param>
		/// <param name="fromTime"></param>
		/// <returns>
		/// The number of login attempts that took place within the timespan of the current time vs the fromTime. If -1
		/// gets returned, the call failed.
		/// </returns>
		public async Task<int> GetLoginAttempts(SP.Models.LoginAttempts loginAttempt, bool detectIPRange, DateTime fromTime)
		{
			LoginAttemptsResponse response = await client.GetLoginAttemptsAsync(
				new LoginAttemptsRequest
				{
					DetectIPRange = detectIPRange,
					FromTime = Timestamp.FromDateTime(DateTime.SpecifyKind(fromTime, DateTimeKind.Utc)),
					LoginAttempts = new SP.API.Service.LoginAttempts
					{
						Id = loginAttempt.Id,
						IpAddress = loginAttempt.IpAddress,
						IpAddressRange = loginAttempt.IpAddressRange,
						Details = loginAttempt.Details,
						EventDate = Timestamp.FromDateTime(DateTime.SpecifyKind(loginAttempt.EventDate, DateTimeKind.Utc))
					}
				});

			return response.Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="loginAttempt"></param>
		/// <returns></returns>
		public async Task<bool> AddLoginAttempt(SP.Models.LoginAttempts loginAttempt)
		{
			//// Contact the api
			//const string path = "/loginAttempts/Add";

			//// Add content
			//HttpContent content =
			//	new StringContent(JsonSerializer.Serialize(loginAttempt), Encoding.UTF8, "application/json");

			//// Execute the request
			//HttpResponseMessage message = await PostRequest(path, content);
			//return message.IsSuccessStatusCode;

			return false;
		}

	}
}