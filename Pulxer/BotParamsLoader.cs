using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Platform;

namespace Pulxer
{
    public class BotParamsLoader
    {
        private Exception _exception;

        public BotParamsLoader()
        {
            _exception = null;
        }

        public BotParams Load(string assemblyFullPath, string classFullName)
        {
            _exception = null;

            string file = GetParamsFile(assemblyFullPath, classFullName);
            if (string.IsNullOrEmpty(file)) return null; // конфиг. файла не существует

            string json = "";
            try
            {
                json = File.ReadAllText(file);
                return Load(json);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }

            return null;
        }

        public BotParams Load(string json)
        {
            BotParams botParams = null;
            _exception = null;

            try
            {
                var prms = JsonConvert.DeserializeObject<BotParam[]>(json);
                if (prms != null)
                {
                    botParams = new BotParams(prms);
                }
            }
            catch (Exception ex)
            {
                _exception = ex;
            }

            return botParams;
        }

        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        private string GetParamsFile(string assemblyFullPath, string classFullName)
        {
            classFullName = classFullName.ToLower();
            string dir = Path.GetDirectoryName(assemblyFullPath);
            string bot = Path.GetFileNameWithoutExtension(assemblyFullPath).ToLower();

            string file = Path.Combine(dir, string.Join('.', bot, classFullName, "json"));
            if (File.Exists(file))
            {
                return file;
            }

            if (classFullName.StartsWith(bot))
            {
                file = Path.Combine(dir, string.Join('.', classFullName, "json"));
                if (File.Exists(file))
                {
                    return file;
                }
            }

            file = Path.Combine(dir, string.Join('.', bot, "json"));
            if (File.Exists(file))
            {
                return file;
            }

            return "";
        }
    }
}
