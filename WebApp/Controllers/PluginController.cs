using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulxer.Plugin;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PluginController : ControllerBase
    {
        private readonly PluginManager _pluginManager;

        public PluginController(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        [HttpGet("list")]
        [Authorize]
        public PxPluginInfo[] GetPlugins()
        {
            return _pluginManager.GetPlugins();
        }

        [HttpPost("{key}/load")]
        [Authorize]
        public bool LoadPlugin(string key)
        {
            return _pluginManager.LoadPlugin(key);
        }

        [HttpPost("{key}/unload")]
        [Authorize]
        public bool UnloadPlugin(string key)
        {
            return _pluginManager.UnloadPlugin(key);
        }

        [HttpPost("load-all")]
        [Authorize]
        public void LoadAllPlugins()
        {
            _pluginManager.LoadAllPlugins();
        }

        [HttpPost("unload-all")]
        [Authorize]
        public void UnloadAllPlugins()
        {
            _pluginManager.UnloadAllPlugins();
        }

        [HttpGet("{key}/cols")]
        [Authorize]
        public PxColumn[] GetColumns(string key)
        {
            var p = _pluginManager.GetLoadedPlugin(key);
            if (p == null) return null;

            return p.GetColumns();
        }

        [HttpGet("{key}/data")]
        [Authorize]
        public object[] GetData(string key)
        {
            var p = _pluginManager.GetLoadedPlugin(key);
            if (p == null) return null;

            return p.GetData();
        }
    }
}
