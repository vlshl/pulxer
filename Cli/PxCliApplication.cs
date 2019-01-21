using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cli
{
    public class PxCliApplication
    {
        private IServiceProvider _serviceProvider = null;
        private IServiceCollection _serviceCollection = null;
        private IConsole _console = null;
        private CmdExecutor _cmdExecutor = null;
        private IExecutor _currentExecutor = null;
        private IConfigurationRoot _config = null;

        public PxCliApplication()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);
            _config = builder.Build();

            _serviceCollection = new ServiceCollection();
            ConfigureServices(_serviceCollection);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
            _console = _serviceProvider.GetRequiredService<IConsole>();
            _cmdExecutor = ActivatorUtilities.CreateInstance<CmdExecutor>(_serviceProvider);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var confSection = _config.GetSection("Config");
            services.AddSingleton<IConfig>(new Config(confSection));
            services.AddSingleton<IConsole, PxConsole>();
            Pulxer.BL.ConfigureServices(services, 
                DataProtect.TryUnProtect(_config.GetConnectionString("Pulxer")), 
                DataProtect.TryUnProtect(_config.GetConnectionString("Leech")));
        }

        public void Run()
        {
            _currentExecutor = _cmdExecutor;

            try
            {
                _serviceProvider.GetRequiredService<IHistoryProvider>().Initialize();
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }

            const string quoted = @"([""'])(.*?)\1";
            const string word = @"\S+";

            while (true)
            {
                _console.Write(string.Format("\n{0}>", _currentExecutor.GetPrefix()));
                string line = _console.ReadLine();
                if (line.ToLower().Trim() == "exit") break;

                try
                {
                    var matches = Regex.Matches(line, quoted);
                    List<string> qs = matches.Select(m => m.Value).ToList();
                    var line1 = Regex.Replace(line, quoted, "'");
                    var matches1 = Regex.Matches(line1, word);
                    List<string> ags = matches1.Select(m => m.Value).ToList();
                    int idx = 0;
                    for (int i = 0; i < ags.Count; i++)
                    {
                        if (ags[i] == "'") ags[i] = qs[idx++].Trim(new char[] { '"', '\'' });
                    }
                    if (ags.Count == 0) continue;

                    string cmd = ags[0].ToLower().Trim();
                    ags.RemoveAt(0);

                    var cur = _currentExecutor.Execute(cmd, ags);
                    if (cur != null) _currentExecutor = cur;
                }
                catch (Exception ex)
                {
                    _console.WriteError(ex.ToString());
                }
            }
        }

    }
}
