using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    /// <summary>
    /// The all ticks dispatcher 
    /// </summary>
    public interface ITickDispatcher
    {
        /// <summary>
        /// Subscribe to all ticks for instrument
        /// </summary>
        /// <param name="subscriber">Subscriber</param>
        /// <param name="insID">Instrument</param>
        /// <param name="onTick">OnTick callback</param>
        void Subscribe(object subscriber, int insID, OnTickEH onTick);

        /// <summary>
        /// Unsubscribe from instrument
        /// </summary>
        /// <param name="subscriber">Subscriber</param>
        /// <param name="insID">Instrument</param>
        void Unsubscribe(object subscriber, int insID);

        /// <summary>
        /// Unsubscribe from all instruments
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        /// Add new trade
        /// </summary>
        /// <param name="tick">Trade info</param>
        void AddTick(Tick tick);

        /// <summary>
        /// Инициализация перед началом торговой сессии.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Завершение после окончания торговой сессии.
        /// </summary>
        void Close();

        /// <summary>
        /// Список фин инструментов, для которых есть накопленные данные
        /// </summary>
        IEnumerable<int> GetInstrumIDs();

        /// <summary>
        /// Список всех сделок по указанному инструменту
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns></returns>
        IEnumerable<Tick> GetTicks(int insID);

        /// <summary>
        /// Дата текущей торговой сессии (без времени)
        /// </summary>
        DateTime CurrentDate { get; }
    }

}
