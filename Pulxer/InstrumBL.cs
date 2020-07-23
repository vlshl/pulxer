using Common.Interfaces;
using Storage;
using System.Collections.Generic;
using CommonData = Common.Data;

namespace Pulxer
{
    /// <summary>
    /// Подсистема фин. инструментов
    /// </summary>
    public class InstrumBL : IInstrumBL
    {
        private readonly IInstrumDA _instrumDA;
        private readonly IInsStoreDA _insStoreDA;

        public InstrumBL(IInstrumDA instrumDA, IInsStoreDA insStoreDA)
        {
            _instrumDA = instrumDA;
            _insStoreDA = insStoreDA;
        }

        /// <summary>
        /// Список всех фин. инструментов
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.InstrumListItem> GetInstrumList()
        {
            return _instrumDA.GetInstrumList();
        }

        /// <summary>
        /// Список всех фин. инструментов
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.Instrum> GetInstrums()
        {
            return _instrumDA.GetInstrums();
        }

        /// <summary>
        /// Данные фин. инструмента по ID
        /// </summary>
        /// <param name="insID">ID инструмента</param>
        /// <returns>Фин. инструмент (новый, если 0 или отрицательное)</returns>
        public CommonData.Instrum GetInstrumByID(int insID)
        {
            if (insID > 0)
                return _instrumDA.GetInstrum(insID);
            else
                return NewInstrum("", "", "", 1, 0, 0);
        }

        /// <summary>
        /// Данные фин. инструмента по ticker
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <returns>Фин. инструмент (null, если тикер не найден)</returns>
        public CommonData.Instrum GetInstrum(string ticker)
        {
            return _instrumDA.GetInstrum(0, ticker);
        }

        /// <summary>
        /// Данные фин. инструмента по тикеру
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <returns>Фин. инструмент</returns>
        public CommonData.Instrum GetInstrumByTicker(string ticker)
        {
            return _instrumDA.GetInstrum(0, ticker);
        }

        private CommonData.Instrum NewInstrum(string ticker, string shortName, string name,
            int lotSize, int decimals, decimal priceStep)
        {
            return new CommonData.Instrum()
            {
                InsID = 0,
                Ticker = ticker,
                ShortName = shortName,
                Name = name,
                LotSize = lotSize,
                Decimals = decimals,
                PriceStep = priceStep
            };
        }

        /// <summary>
        /// Сохранение данных фин. инструмента в базе
        /// </summary>
        /// <param name="ins">Фин. инструмент</param>
        public void SaveInstrum(CommonData.Instrum ins)
        {
            if (ins.InsID > 0)
            {
                _instrumDA.UpdateInstrum(ins.InsID, ins.Ticker, ins.ShortName, ins.Name, ins.LotSize, ins.Decimals, ins.PriceStep);
            }
            else
            {
                ins.InsID = _instrumDA.InsertInstrum(ins.Ticker, ins.ShortName, ins.Name, ins.LotSize, ins.Decimals, ins.PriceStep);
            }
        }

        /// <summary>
        /// Удаление фин. инструмента по ID
        /// </summary>
        /// <param name="insID">ID фин. инструмента</param>
        public void DeleteInstrumByID(int insID)
        {
            _instrumDA.DeleteInstrumByID(insID);
        }
    }
}
