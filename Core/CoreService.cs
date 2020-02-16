using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFwTypeLib;
using SP.Core.Models;
using SP.Core.Tools;
using SP.Plugins;

namespace SP.Core
{
    public class CoreService : BackgroundService
    {
        // Configuration object
        private readonly IConfigurationRoot config;

        // Diagnostics
        private readonly ILogger<CoreService> log;

        // Contains all plugins that are loaded.
        private readonly List<IPluginBase> plugins = new List<IPluginBase>();

        // Keep top 10 of last blocks
        private readonly Stack<string> lastBlocks = new Stack<string>();

        // Configuration items
        private List<string> configPlugins;
        private string ipDataKey;

        private string ipDataUrl;

        /// <summary>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="config"></param>
        /// <param name="firewall"></param>
        /// <param name="protectHandler"></param>
        public CoreService(ILogger<CoreService> log, IConfigurationRoot config, IFirewall firewall,
            IProtectHandler protectHandler)
        {
            this.log = log;
            this.config = config;

            // Handle events
            //

            // Login Attempts
            LoginAttemptEvent += async (sender, args) =>
            {
                PluginEventArgs pluginEventArgs = args as PluginEventArgs;


                // Sanity check
                if (pluginEventArgs == null)
                {
                    log.LogError($"{nameof(BlockEvent)} was called but {nameof(PluginEventArgs)} should not be null");
                    return;
                }

                // Pass the event to the protect handler
                bool block = await protectHandler.AnalyzeAttempt(pluginEventArgs);

                // Initial attempt on caching the last blocks to prevent duplicate reports/blocks
                if (lastBlocks.Count > 10)
                {
                    // Clear out top item
                    lastBlocks.Pop();
                }

                // If an attack is happening, it's possible that 100s of events fire in seconds, this logic
                // prevents that duplicate firewall rules or reports are being made.
                if (lastBlocks.Contains(pluginEventArgs.IPAddress))
                {
                    log.LogDebug($"{pluginEventArgs.IPAddress} was recently blocked. Ignoring.");
                    return;
                }

                // Block IP?
                if (!block)
                {
                    log.LogDebug($"{pluginEventArgs.IPAddress} will not be blocked.");
                    return;
                }

                // Add IP to list of last 10 blocks
                lastBlocks.Push(pluginEventArgs.IPAddress);

                // Signal block
                BlockEvent?.Invoke(this, args);
            };

            // Block events
            BlockEvent += async (sender, args) =>
            {
                PluginEventArgs pluginEventArgs = args as PluginEventArgs;

                // Create EventData object
                Blocks block = new Blocks();

                // Sanity check
                if (pluginEventArgs == null)
                {
                    log.LogError($"{nameof(BlockEvent)} was called but {nameof(PluginEventArgs)} should not be null");
                    return;
                }

                //
                block.IpAddress = pluginEventArgs.IPAddress;
                block.Hostname = "";
                block.Date = pluginEventArgs.DateTime;

                // Use IPData to get additional information about the IP
                try
                {
                    DataModel dataModel = await IPData.GetDetails(ipDataUrl, ipDataKey, pluginEventArgs.IPAddress);
                    block.Country = dataModel.Country;
                    block.City = dataModel.City;
                    block.ISP = dataModel.ISP;
                }
                catch(Exception e)
                {
                    log.LogError($"Unable to get additional details about ip {block.IpAddress}. Service response: {e.Message}");
                }

                // Attempt to get the hostname
                try
                {
                    IPHostEntry hostInfo = Dns.GetHostEntry(block.IpAddress);
                    block.Hostname = hostInfo.HostName;
                }
                catch
                {
                    log.LogDebug($"Unable to get host name for {block.IpAddress}");
                }

                // Block IP in Firewall
                firewall.Block(NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY, block);

                // Notify plug-ins of block
                foreach (IPluginBase pluginBase in plugins)
                {
                    await Task.Run(() => { pluginBase.BlockedEvent(pluginEventArgs); });
                }
            };
        }

        // Events
        public event EventHandler LoginAttemptEvent;
        public event EventHandler BlockEvent;

        /// <summary>
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure()
            Configure();

            // Load plugins
            LoadPlugins();

            // Plugin options
            PluginOptions options = new PluginOptions();

            // Start plugins
            foreach (IPluginBase plugin in plugins)
            {
                // Initialize plugin
                await plugin.Initialize(options);

                // Initial configuration of plug
                await plugin.Configure();

                // Register handlers
                await plugin.RegisterLoginAttemptHandler(LoginAttemptEvent);
                await plugin.RegisterBlockHandler(BlockEvent);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(-1, stoppingToken);
            }
        }

        /// <summary>
        /// </summary>
        private void Configure()
        {
            configPlugins = config.GetSection("Plugins").Get<List<string>>();

            // Get IPData configuration items
            ipDataUrl = config.GetSection("Tools:IPData:Url").Get<string>();
            ipDataKey = config.GetSection("Tools:IPData:Key").Get<string>();
        }

        /// <summary>
        /// </summary>
        private void LoadPlugins()
        {
            Plugins<IPluginBase> pluginLoader = new Plugins<IPluginBase>(log);

            // Paths that contain plugins.
            string[] pluginPaths =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins")
            };

            // Loop through all plugin paths and retrieve all plugins that match the naming requirements.
            foreach (string pluginPath in pluginPaths)
            {
                // Diagnostics.
                log.LogInformation("Browsing folder {0} for plugins", pluginPath);

                // Read all subdirectories of the plugin folder being processed.
                foreach (DirectoryInfo directoryInfo in new DirectoryInfo(pluginPath).GetDirectories())
                {
                    // From all subdirectories, retrieve all plugins that are compliant with the naming standards plugins.<name>.dll
                    IEnumerable<FileInfo> plugin = directoryInfo
                        .GetFiles("*.dll")
                        .Where(d =>
                            d.Name.ToLowerInvariant().StartsWith("plugins.") &&
                            !d.Name.ToLowerInvariant().StartsWith("plugins.base"))
                        .ToList();

                    // Loop through all plugins found in the directory.
                    foreach (FileInfo fileInfo in plugin)
                    {
                        int prefixLength = "plugins.".Length;
                        int suffixLength = fileInfo.Extension.Length;

                        // Get the name of the plugin.
                        string pluginName =
                            fileInfo.Name
                                .Remove(0, prefixLength)
                                .Remove(fileInfo.Name.Length - prefixLength - suffixLength, suffixLength)
                                .ToLowerInvariant();

                        if (!configPlugins.Contains(pluginName))
                        {
                            log.LogInformation($"Plugin {pluginName} is not enabled");
                            continue;
                        }

                        // One or more command line arguments mention the plugin name.
                        IEnumerable<IPluginBase> currentPlugin = plugin
                            .Select(file => pluginLoader.LoadPlugin(file.FullName))
                            .Select(pluginLoader.CreateCommands)
                            .SelectMany(d => d);

                        // Add all plugin matches to the main holder.
                        plugins.AddRange(currentPlugin);
                    }
                }
            }
        }
    }
}