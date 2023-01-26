using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Platform;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class DeviceDA : IDeviceDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public DeviceDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get device by uid
        /// </summary>
        /// <returns>Device or null</returns>
        public Device GetDevice(string devUid)
        {
            using (var db = new DaContext(_options))
            {
                return db.Device.FirstOrDefault(r => r.Uid == devUid);
            }
        }

        /// <summary>
        /// Create new device
        /// </summary>
        /// <param name="devUid">Device uid</param>
        /// <param name="code">Pin code</param>
        /// <param name="userId">UserId</param>
        /// <returns>New device</returns>
        public Device CreateDevice(string devUid, string code, int userId)
        {
            Device dev = new Device()
            {
                DeviceId = 0,
                Code = code,
                UserId = userId,
                Uid = devUid
            };

            using (var db = new DaContext(_options))
            {
                db.Device.Add(dev);
                db.SaveChanges();
            }

            return dev;
        }

        /// <summary>
        /// Delete device
        /// </summary>
        /// <param name="devUid">Device uid</param>
        /// <returns>true- device was deleted, false-device not found</returns>
        public bool DeleteDevice(string devUid)
        {
            using (var db = new DaContext(_options))
            {
                var dev = db.Device.FirstOrDefault(r => r.Uid == devUid);
                if (dev == null) return false;

                db.Remove<Device>(dev);
                db.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// Удаление всех устройств по пользователю
        /// </summary>
        /// <param name="userId">Пользователь</param>
        public void DeleteDevicesByUser(int userId)
        {
            using (var db = new DaContext(_options))
            {
                var devs = db.Device.Where(r => r.UserId == userId).ToList();
                db.RemoveRange(devs);
                db.SaveChanges();
            }
        }
    }
}
