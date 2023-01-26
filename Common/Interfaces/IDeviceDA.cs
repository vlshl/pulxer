using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IDeviceDA
    {
        Device GetDevice(string uid);
        Device CreateDevice(string devUid, string code, int userId);
        bool DeleteDevice(string devUid);
        void DeleteDevicesByUser(int userId);
    }
}
