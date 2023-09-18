using Common;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Pulxer.Plugin
{
    public class PluginManager
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private string _pluginsPath;
        private Dictionary<string, AssemblyLoadContext> _key_context;
        private Dictionary<string, IPxPlugin> _key_plugin;
        private Dictionary<string, IPluginPlatform> _key_platform;

        public PluginManager(IConfig config, ILogger<PluginManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _key_context = new Dictionary<string, AssemblyLoadContext>();
            _key_plugin = new Dictionary<string, IPxPlugin>();
            _key_platform = new Dictionary<string, IPluginPlatform>();
            _pluginsPath = config.GetPluginsPath();
            _serviceProvider = serviceProvider;
        }

        public PxPluginInfo[] GetPlugins()
        {
            if (string.IsNullOrEmpty(_pluginsPath))
            {
                _logger?.LogWarning("Config error: PluginsPath param not found");
                return null;
            }

            if (!Directory.Exists(_pluginsPath))
            {
                _logger?.LogWarning("PluginsPath directory not exists");
                return null;
            }

            var loadedKeys = GetLoadedKeys();
            List<PxPluginInfo> plugins = new List<PxPluginInfo>();
            var dirs = Directory.EnumerateDirectories(_pluginsPath).ToArray();
            foreach (var dir in dirs)
            {
                var pc = GetPluginConfig(dir);
                if (pc == null) continue;

                string key = Path.GetFileName(dir);
                plugins.Add(new PxPluginInfo() { Key = key, Name = pc.Name, State = loadedKeys.Contains(key) } );
            }

            return plugins.ToArray();
        }

        private PluginConfig GetPluginConfig(string dir)
        {
            var cfile = Path.Combine(dir, "plugin.json");
            if (!File.Exists(cfile))
            {
                _logger?.LogWarning("File not found: plugin.json");
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<PluginConfig>(File.ReadAllText(cfile));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Invalid plugin.json file");
                return null;
            }
        }

        public void LoadAllPlugins()
        {
            _logger?.LogInformation("Load all plugins");

            if (string.IsNullOrEmpty(_pluginsPath))
            {
                _logger?.LogWarning("Config error: PluginsPath param not found");
                return;
            }

            if (!Directory.Exists(_pluginsPath))
            {
                _logger?.LogWarning("PluginsPath directory not exists");
                return;
            }

            var dirs = Directory.EnumerateDirectories(_pluginsPath).ToArray();
            foreach (var dir in dirs)
            {
                var pc = GetPluginConfig(dir);
                if (pc == null) continue;

                bool isSuccess = LoadAssembly(dir, pc.Assembly);
                if (isSuccess)
                {
                    _logger?.LogInformation("Success load assembly: " + pc.Assembly);
                }
                else
                {
                    _logger?.LogInformation("Failed load assembly: " + pc.Assembly);
                }
            }
        }

        public bool LoadPlugin(string key)
        {
            _logger?.LogInformation("Loading plugin: " + key);

            if (string.IsNullOrEmpty(_pluginsPath))
            {
                _logger?.LogWarning("Config error: PluginsPath param not found");
                return false;
            }

            if (!Directory.Exists(_pluginsPath))
            {
                _logger?.LogWarning("PluginsPath directory not exists");
                return false;
            }

            string dir = Path.Combine(_pluginsPath, key);
            var pc = GetPluginConfig(dir);
            if (pc == null) return false;

            bool isSuccess = LoadAssembly(dir, pc.Assembly);
            if (isSuccess)
            {
                _logger?.LogInformation("Success load assembly: " + pc.Assembly);
            }
            else
            {
                _logger?.LogInformation("Failed load assembly: " + pc.Assembly);
            }

            return true;
        }

        private string[] GetLoadedKeys()
        {
            lock (this)
            {
                return _key_plugin.Keys.ToArray();
            }
        }

        public IPxPlugin GetLoadedPlugin(string key)
        {
            lock (this)
            {
                if (!_key_plugin.ContainsKey(key)) return null;

                return _key_plugin[key];
            }
        }

        public bool UnloadPlugin(string key)
        {
            _logger?.LogInformation("Unloading plugin: " + key);

            lock (this)
            {
                if (!_key_context.ContainsKey(key))
                {
                    _logger?.LogInformation("Plugin not found: " + key);
                    return false;
                }
            }

            bool isSuccess = UnloadAssemblyByKey(key);
            if (isSuccess)
            {
                _logger?.LogInformation("Success unload assembly: " + key);
            }
            else
            {
                _logger?.LogInformation("Failed unload assembly: " + key);
            }

            return true;
        }

        public void UnloadAllPlugins()
        {
            _logger?.LogInformation("Unload all plugins");

            string[] keys;
            lock (this)
            { 
                keys = _key_context.Keys.ToArray();
            }

            foreach (var key in keys)
            {
                bool isSuccess = UnloadAssemblyByKey(key);
                if (isSuccess)
                {
                    _logger?.LogInformation("Success unload assembly: " + key);
                }
                else
                {
                    _logger?.LogInformation("Failed unload assembly: " + key);
                }
            }
        }

        private bool LoadAssembly(string dir, string assem)
        {
            var path = Path.Combine(dir, assem);
            if (!File.Exists(path))
            {
                _logger?.LogWarning("Plugin assembly not exists");
                return false;
            }

            string key = Path.GetRelativePath(_pluginsPath, dir);

            lock (this)
            {
                if (_key_context.ContainsKey(key) || _key_plugin.ContainsKey(key))
                    return false;

                AssemblyLoadContext context = new AssemblyLoadContext(key, true);

                context.Resolving += Context_Resolving;

                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open);
                    var assembly = context.LoadFromStream(fs);
                    fs.Close();
                    if (assembly == null) return false;

                    var plugin = ActivatePlugin(key, assembly);
                    if (plugin == null)
                    {
                        context.Unload();
                        context.Resolving -= Context_Resolving;
                        return false;
                    }

                    _key_context.Add(key, context);
                    _key_plugin.Add(key, plugin);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed load assembly: " + path);
                    return false;
                }
            }
        }

        private Assembly Context_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            try
            {
                string path = Path.Combine(_pluginsPath, arg1.Name, arg2.Name + ".dll");
                //var asm = arg1.LoadFromAssemblyPath(path);
                FileStream fs = new FileStream(path, FileMode.Open);
                var asm = arg1.LoadFromStream(fs);
                fs.Close();

                if (asm != null)
                {
                    _logger?.LogInformation("Context resolving successfully: " + path);
                }
                else
                {
                    _logger?.LogError("Context resolving error: " + path);
                }

                return asm;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Context resolving error");
                return null;
            }
        }

        public bool UnloadAssemblyByKey(string key)
        {
            try
            {
                lock (this)
                {
                    if (_key_plugin.ContainsKey(key))
                    {
                        try
                        {
                            _key_plugin[key].OnDestroy();
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Plugin OnDestroy error:" + key);
                        }
                        _key_plugin.Remove(key);
                    }

                    if (_key_platform.ContainsKey(key))
                    {
                        _key_platform[key].Close();
                        _key_platform.Remove(key);
                    }

                    if (_key_context.ContainsKey(key))
                    {
                        var ctx = _key_context[key];
                        ctx.Unload();
                        ctx.Resolving -= Context_Resolving;
                        _key_context.Remove(key);
                    }
                }
                
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed unload assembly: " + key);
                return false;
            }
        }

        private IPxPlugin ActivatePlugin(string key, Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (!typeof(IPxPlugin).IsAssignableFrom(type)) continue;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var platform = scope.ServiceProvider.GetRequiredService<IPluginPlatform>();
                        var ctxMan = scope.ServiceProvider.GetRequiredService<ITgContextManager>();

                        IPxPlugin p = null;
                        try
                        {
                            string path = Path.Combine(_pluginsPath, key);
                            p = Activator.CreateInstance(type, platform, path, ctxMan) as IPxPlugin;
                            if (p == null)
                            {
                                _logger?.LogInformation("Create instance error: " + key);
                                return null;
                            }

                            p.OnLoad();
                            _logger?.LogInformation("Success activate: " + key);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Plugin activate error: " + key);
                            return null;
                        }
                        _key_platform.Add(key, platform);

                        return p;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed activate: " + key);
                return null;
            }

            return null;
        }
    }
}
