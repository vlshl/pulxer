using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Common.Interfaces
{
    /// <summary>
    /// Repository subsystem interface
    /// </summary>
    public interface IRepositoryBL
    {
        int PutXmlObject<T>(T obj, string key = "") where T : IXmlSerializable, new();
        int PutStrObject<T>(T obj, string key = "") where T : IStrSerializable, new();
        T GetXmlObject<T>(int id) where T : IXmlSerializable, new();
        T GetXmlObject<T>(string key) where T : IXmlSerializable, new();
        T GetStrObject<T>(int id) where T : IStrSerializable, new();
        T GetStrObject<T>(string key) where T : IStrSerializable, new();
        int? GetIntParam(string key);
        void SetIntParam(string key, int n);
        string GetStringParam(string key);
        void SetStringParam(string key, string str);
    }
}
