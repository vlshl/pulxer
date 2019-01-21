using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CommonData = Common.Data;

namespace Pulxer
{
    public interface ITickSourceBL
    {
        IEnumerable<CommonData.TickSourceItem> GetTickSourceList();
        TickSource GetTickSourceByID(int tickSourceID);
        void SaveTickSource(TickSource tickSource);
        void DeleteTickSourceByID(int tickSourceID);
    }

    /// <summary>
    /// Подсистема источников тиковых данных
    /// </summary>
    public class TickSourceBL : ITickSourceBL
    {
        private readonly ITickSourceDA _tickSourceDA = null;
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreBL _insStoreBL = null;
        private readonly ITickHistoryBL _tickHistoryBL = null;

        public TickSourceBL(IInstrumBL instrumBL, ITickSourceDA tickSource, IInsStoreBL insStoreBL, ITickHistoryBL tickHistoryBL)
        {
            _instrumBL = instrumBL;
            _tickSourceDA = tickSource;
            _insStoreBL = insStoreBL;
            _tickHistoryBL = tickHistoryBL;
        }

        /// <summary>
        /// Список всех источников
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.TickSourceItem> GetTickSourceList()
        {
            var list = _tickSourceDA.GetTickSources().ToList();
            return list.Select(r => new TickSourceItem(r.TickSourceID, r.Name));
        }

        /// <summary>
        /// Тиковый источник данных
        /// </summary>
        /// <param name="tickSourceID">ID источника</param>
        /// <returns>Тиковый источник или null</returns>
        public TickSource GetTickSourceByID(int tickSourceID)
        {
            if (tickSourceID <= 0) return null;

            try
            {
                var ts = _tickSourceDA.GetTickSourceByID(tickSourceID);
                if (ts == null || ts.DataStr == null) return null;

                var tickSource = new TickSource(_instrumBL, _insStoreBL, _tickHistoryBL);
                tickSource.TickSourceID = ts.TickSourceID;
                tickSource.Name = ts.Name;
                XDocument xd = XDocument.Parse(ts.DataStr);
                tickSource.Initialize(xd);

                return tickSource;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при извлечении тикового источника", ex);
            }
        }

        /// <summary>
        /// Сохранение тикового источника
        /// </summary>
        /// <param name="tickSource">Тиковый источник</param>
        public void SaveTickSource(TickSource tickSource)
        {
            if (tickSource == null)
                throw new ArgumentNullException("tickSource");

            string xml = "";
            var xd = tickSource.Serialize();
            if (xd != null) xml = xd.ToString(SaveOptions.DisableFormatting);

            DbTickSource dbts = new DbTickSource() { TickSourceID = tickSource.TickSourceID, Name = tickSource.Name, DataStr = xml };

            try
            {
                if (tickSource.TickSourceID > 0)
                {
                    _tickSourceDA.UpdateTickSource(dbts);
                }
                else
                {
                    tickSource.TickSourceID = _tickSourceDA.InsertTickSource(dbts);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при сохранении тикового источника", ex);
            }
        }

        /// <summary>
        /// Удаление тикового источника
        /// </summary>
        /// <param name="tickSourceID">Идентификатор</param>
        public void DeleteTickSourceByID(int tickSourceID)
        {
            try
            {
                _tickSourceDA.DeleteTickSourceByID(tickSourceID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при удалении тикового источника", ex);
            }
        }
    }
}
