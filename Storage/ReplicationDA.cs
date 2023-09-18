using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public class ReplicationDA : IReplicationDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public ReplicationDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get all replications by objType
        /// </summary>
        /// <returns>TestConfigs list</returns>
        public IEnumerable<Replication> GetReplications(ReplObjects objType)
        {
            using (var db = new DaContext(_options))
            {
                return db.Replication.Where(r => r.ReplObject == objType).ToList();
            }
        }

        /// <summary>
        /// Insert replications
        /// </summary>
        /// <param name="replObject">Replication object</param>
        /// <param name="rid_lid">Replications (key=remoteId, val=localId)</param>
        public void InsertReplications(ReplObjects replObject, Dictionary<int, int> rid_lid)
        {
            if (rid_lid == null)
                throw new ArgumentNullException();
            if (!rid_lid.Any()) return;

            using (var db = new DaContext(_options))
            {
                foreach (var rid in rid_lid.Keys)
                {
                    int lid = rid_lid[rid];
                    var isFound = db.Replication.Any(r => r.ReplObject == replObject && r.LocalID == lid && r.RemoteID == rid);
                    if (!isFound)
                    {
                        db.Replication.Add(new Replication() { LocalID = lid, RemoteID = rid, ReplObject = replObject });
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete all object replications
        /// </summary>
        /// <param name="replObject">Replication object</param>
        public void DeleteReplications(ReplObjects replObject)
        {
            using (var db = new DaContext(_options))
            {
                db.Database.ExecuteSqlRaw("delete from replication where repl_object = " + ((int)replObject).ToString());
            }
        }
    }
}
