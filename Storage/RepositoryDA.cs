using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CommonData = Common.Data;

namespace Storage
{
    /// <summary>
    /// DA-layer for Repository
    /// </summary>
    public class RepositoryDA : IRepositoryDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public RepositoryDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Create repository object
        /// </summary>
        /// <param name="key">Unique string key</param>
        /// <param name="data">Serialized data</param>
        /// <returns>Repository object</returns>
        public ReposObject Create(string key, string data)
        {
            var ro = new ReposObject()
            {
                ReposID = 0,
                Key = key != null ? key : "",
                Data = data != null ? data : ""
            };

            using (var db = new DaContext(_options))
            {
                db.Repository.Add(ro);
                db.SaveChanges();
            }

            return ro;
        }

        /// <summary>
        /// Get repository object by Id or Key
        /// </summary>
        /// <param name="reposID">Id or 0 (0 - ignore Id)</param>
        /// <param name="key">Key or null (null - ignore Key)</param>
        /// <returns></returns>
        public CommonData.ReposObject Select(int reposID = 0, string key = null)
        {
            using (var db = new DaContext(_options))
            {
                var repos = db.Repository.FirstOrDefault(s =>
                    (s.ReposID == (reposID == 0 ? s.ReposID : reposID))
                    && (s.Key == ( key == null ? s.Key : key)));

                return repos;
            }
        }

        /// <summary>
        /// Update repository object
        /// </summary>
        /// <param name="ro"></param>
        public void Update(CommonData.ReposObject ro)
        {
            using (var db = new DaContext(_options))
            {
                db.Update<ReposObject>(ro);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete from repository by Id
        /// </summary>
        /// <param name="reposID"></param>
        public void Delete(int reposID)
        {
            using (var db = new DaContext(_options))
            {
                var ro = db.Repository.Find(reposID);
                if (ro == null) return;

                db.Remove<ReposObject>(ro);
                db.SaveChanges();
            }
        }
    }
}
