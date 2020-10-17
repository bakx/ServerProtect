﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SP.Models;
using SP.Plugins;

namespace Plugins
{
	public class AbuseIP : IPluginBase
	{
		private static readonly HttpClient HttpClient = new HttpClient();
		private string apiKey;
		private string apiUrl;

		// Diagnostics
		private ILogger log;

		// Handlers
		public IPluginBase.Block BlockHandler { get; set; }

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
					.ForContext(typeof(AbuseIP));

				// Assign config variables
				apiUrl = config["Url"];
				apiKey = config["Key"];

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
		public async Task<bool> Configure()
		{
			try
			{
				// Set up http client for AbuseIP
				HttpClient.DefaultRequestHeaders.Add("Key", apiKey);
				HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				return await Task.FromResult(true);
			}
			catch (Exception e)
			{
				log.Error("{0}", e);
				return await Task.FromResult(false);
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
		/// </summary>
		/// <param name="blockHandler"></param>
		/// <returns></returns>
		public async Task<bool> RegisterBlockHandler(IPluginBase.Block blockHandler)
		{
			BlockHandler = blockHandler;
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
		public async Task<bool> LoginAttemptEvent(LoginAttempts loginAttempt)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// Report the ip to AbuseIP
		/// </summary>
		public async Task<bool> BlockEvent(Blocks block)
		{
			return await ReportIP(block);
		}

		/// <summary>
		/// Not used by this plugin
		/// </summary>
		public async Task<bool> UnblockEvent(Blocks block)
		{
			return await Task.FromResult(true);
		}

		/// <summary>
		/// </summary>
		/// <param name="block"></param>
		/// <returns></returns>
		public async Task<bool> ReportIP(Blocks block)
		{
			try
			{
				Dictionary<string, string> values = new Dictionary<string, string>
				{
					{"ip", block.IpAddress},
					{"categories", "18"},
					{"comment", block.Details}
				};

				FormUrlEncodedContent content = new FormUrlEncodedContent(values);

				HttpResponseMessage response = await HttpClient.PostAsync(apiUrl, content);

				log.Debug(await response.Content.ReadAsStringAsync());
				return await Task.FromResult(true);
			}
			catch (Exception e)
			{
				log.Error(e.Message);
				return await Task.FromResult(false);
			}
		}
	}
}