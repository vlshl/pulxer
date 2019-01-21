using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pulxer
{
    public interface IReplicationBL
    {
        Dictionary<int, int> GetReplications(ReplObjects ro);
        void UpdateReplications(ReplObjects ro, Dictionary<int, int> rid_lid);
    }

    public class ReplicationBL : IReplicationBL
    {
        private readonly IReplicationDA _replDA;

        public ReplicationBL(IReplicationDA replDA)
        {
            _replDA = replDA;
        }

        /// <summary>
        /// Соответствие идентификаторов
        /// </summary>
        /// <param name="ro">Объект репликации</param>
        /// <returns>Словарь соответствия (key=remoteID, val=localID)</returns>
        public Dictionary<int, int> GetReplications(ReplObjects ro)
        {
            var repls = _replDA.GetReplications(ro);
            return repls.ToDictionary(k => k.RemoteID, v => v.LocalID);
        }

        /// <summary>
        /// Обновление данных о репликации.
        /// (сначала полное удаление старых данных)
        /// </summary>
        /// <param name="ro">Объект репликации</param>
        /// <param name="rid_lid">Словарь соответствия (key=remoteID, val=localID)</param>
        public void UpdateReplications(ReplObjects ro, Dictionary<int, int> rid_lid)
        {
            _replDA.DeleteReplications(ro);
            _replDA.InsertReplications(ro, rid_lid);
        }
    }
}
