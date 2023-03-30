using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using Pulxer.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pulxer.Drawing
{
    /// <summary>
    /// Управление графиком
    /// </summary>
    public class ChartManager
    {
        private TickDispatcher _tickDispatcher;
        private ChartData _chartData = null;
        private Dictionary<string, PriceSource> _guid_source = new Dictionary<string, PriceSource>();
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;
        private readonly IAccountDA _accountDA;

        private DateTime _startDate;
        private DateTime _endDate;
        private List<IndicatorBase> _indicators = new List<IndicatorBase>();
        private IFactory _factory = null;
        private IDependencyManager _depManager;
        private IValueRowSourcesProvider _srcProv = new ValueRowSourceProvider();
        private Equity _equity = null;
        private EquityIndicator _eqIndic = null;
        private bool _isDynamic;

        /// <summary>
        /// Используется для динамических графиков.
        /// То есть предусмотрено динамическое изменение цен.
        /// </summary>
        /// <param name="instrumBL">Подсистема фин. инструментов</param>
        /// <param name="td">Диспетчер потока данных по сделкам</param>
        public ChartManager(IInstrumBL instrumBL, IInsStoreBL insStoreBL, IAccountDA accountDA, TickDispatcher td)
        {
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _accountDA = accountDA;
            _tickDispatcher = td;
            _depManager = new DependencyManager();
            _factory = new Factory(_srcProv, _depManager);
            _isDynamic = true;
        }

        /// <summary>
        /// Используется для статических графиков исторических данных.
        /// То есть динамическое изменение цены не предусмотрено.
        /// </summary>
        /// <param name="instrumBL">Подсистема фин. инструментов</param>
        /// <param name="startDate">Первый день отображаемых данных</param>
        /// <param name="endDate">Последний день отображаемых данных</param>
        public ChartManager(IInstrumBL instrumBL, IInsStoreBL insStoreBL, IAccountDA accountDA, DateTime startDate, DateTime endDate)
        {
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _accountDA = accountDA;
            _startDate = startDate.Date;
            _endDate = endDate.Date;
            _depManager = new DependencyManager();
            _factory = new Factory(_srcProv, _depManager);
            _isDynamic = false;
        }

        /// <summary>
        /// Фабрика индикаторов
        /// </summary>
        public IFactory Factory
        {
            get
            {
                return _factory;
            }
        }

        /// <summary>
        /// Инициализация графика
        /// </summary>
        /// <param name="xdChart">Данные для инициализации</param>
        public void Initialize(XDocument xdChart = null)
        {
            if (xdChart == null)
            {
                xdChart = new XDocument(new XElement("Chart"));
            }

            var xnSources = xdChart.Root.Element("Sources");
            if (xnSources != null)
            {
                foreach (var xnSrc in xnSources.Elements())
                {
                    if (xnSrc.Name == "BarRow")
                    {
                        int insID = 0;
                        string guid = xnSrc.Attribute("Id").Value;
                        int.TryParse(xnSrc.Attribute("InsID").Value, out insID);
                        int tf = 0;
                        int.TryParse(xnSrc.Attribute("Tf").Value, out tf);
                        if (insID == 0) continue;
                        Instrum ins = _instrumBL.GetInstrumByID(insID);
                        if (ins == null) continue;

                        CreateBarRowSource(ins, (Timeframes)tf, guid);
                    }
                }
            }

            _indicators.Clear();
            var xnIndicators = xdChart.Root.Element("Indicators");
            if (xnIndicators != null)
            {
                foreach (var xnIndic in xnIndicators.Elements())
                {
                    var indic = _factory.CreateIndicator(xnIndic.Name.ToString());
                    if (indic == null) continue;

                    indic.Initialize(new XDocument(xnIndic));
                    AddIndicator(indic);
                }
            }
        }

        private void CreateBarRowSource(Instrum ins, Timeframes tf, string guid)
        {
            BarRow bars = new BarRow(tf, ins.InsID);
            _chartData = new ChartData(bars.Dates, ins.Decimals, _isDynamic);
            _chartData.AddPrices(bars, new ChartBrush(0, 0, 0));
            _guid_source.Add(guid, new PriceSource() { Bars = bars, Instrum = ins });

            _srcProv.Initialize(new List<ValueRowSource>()
            {
                new ValueRowSource(guid + ":O", "Open", bars.Open, bars),
                new ValueRowSource(guid + ":H", "High", bars.High, bars),
                new ValueRowSource(guid + ":L", "Low", bars.Low, bars),
                new ValueRowSource(guid + ":C", "Close", bars.Close, bars),
                new ValueRowSource(guid + ":T", "Typical", bars.Typical, bars),
                new ValueRowSource(guid + ":M", "Median", bars.Median, bars)
            });
        }

        /// <summary>
        /// Загрузка истории для пустых Bars.
        /// Если Bars не пустой, то история не загружается.
        /// </summary>
        /// <returns></returns>
        public async Task LoadHistoryAsync()
        {
            DateTime start, end;
            if (_tickDispatcher != null)
            {
                end = _tickDispatcher.SessionDate.AddDays(-1);
            }
            else
            {
                end = _endDate;
            }

            foreach (var psrc in _guid_source.Values)
            {
                if (psrc.Bars.Count > 0) continue; // история уже загружена

                if (_tickDispatcher != null)
                {
                    start = _insStoreBL.GetDefaultStartHistoryDate(end, psrc.Bars.Timeframe);
                }
                else
                {
                    start = _startDate;
                }
                await _insStoreBL.LoadHistoryAsync(psrc.Bars, psrc.Instrum.InsID, start, end);
                if (_tickDispatcher != null) psrc.Bars.TickDispatcher = _tickDispatcher;
            }
        }

        /// <summary>
        /// Сериализация данных графика.
        /// </summary>
        /// <returns>xml-документ</returns>
        public XDocument Serialize()
        {
            XDocument xdChart = new XDocument(new XElement("Chart"));

            if (FirstSource != null && FirstSource.Instrum != null && FirstSource.Bars != null)
            {
                xdChart.Root.Add(new XAttribute("InsID", FirstSource.Instrum.InsID.ToString()));
                xdChart.Root.Add(new XAttribute("Tf", ((int)FirstSource.Bars.Timeframe).ToString()));
            }

            foreach (var key in _guid_source.Keys)
            {
                var cms = _guid_source[key];
                AddBarRowSource2Xml(xdChart, key, cms.Instrum.InsID, cms.Bars.Timeframe);
            }
            foreach (var indic in _indicators)
            {
                AddIndicator2Xml(xdChart, indic);
            }

            return xdChart;
        }

        private void AddBarRowSource2Xml(XDocument xdChart, string guid, int insID, Timeframes tf)
        {
            var xnSources = xdChart.Root.Element("Sources");
            if (xnSources == null)
            {
                xnSources = new XElement("Sources");
                xdChart.Root.Add(xnSources);
            }

            var xnBarRow = new XElement("BarRow",
                new XAttribute("Id", guid),
                new XAttribute("InsID", insID.ToString()),
                new XAttribute("Tf", ((int)tf).ToString()));
            xnSources.Add(xnBarRow);
        }

        private void AddIndicator2Xml(XDocument xdChart, IIndicator indicator)
        {
            var xnIndicators = xdChart.Root.Element("Indicators");
            if (xnIndicators == null)
            {
                xnIndicators = new XElement("Indicators");
                xdChart.Root.Add(xnIndicators);
            }

            var xdIndic = indicator.Serialize();
            if (xdIndic != null)
            {
                xnIndicators.Add(xdIndic.Root);
            }
        }

        /// <summary>
        /// Все источники ценовых данных
        /// </summary>
        public Dictionary<string, PriceSource> Sources
        {
            get
            {
                return _guid_source;
            }
        }

        /// <summary>
        /// Главный источник ценовых данных
        /// </summary>
        public PriceSource FirstSource
        {
            get
            {
                return _guid_source.Values.FirstOrDefault();
            }
        }

        /// <summary>
        /// Из ноды Frame выделяет Chart
        /// Если Chart-а нет, возвращает null
        /// </summary>
        /// <param name="xnFrame"></param>
        /// <returns></returns>
        public static XDocument GetChartXmlDoc(XElement xnFrame)
        {
            var xnChart = xnFrame.Element("Chart");
            if (xnChart == null) return null;

            return new XDocument(xnChart);
        }

        /// <summary>
        /// Из сериализации Simul вычленить Charts
        /// </summary>
        /// <param name="xdSimul"></param>
        /// <returns></returns>
        public static XDocument GetChartsXmlDoc(XDocument xdSimul)
        {
            if (xdSimul == null) return null;

            XDocument xd = new XDocument();
            XElement xn = xdSimul.Root.Element("Charts");
            if (xn == null)
            {
                xn = new XElement("Charts");
            }
            xd.Add(xn);

            return xd;
        }

        /// <summary>
        /// Добавить в chart график цены
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">Таймфрейм</param>
        public void CreatePrices(int insID, Timeframes tf)
        {
            var ins = _instrumBL.GetInstrumByID(insID);
            if (ins == null) return;

            string guid = Guid.NewGuid().ToString();
            CreateBarRowSource(ins, tf, guid);
        }

        /// <summary>
        /// Получить данные графика
        /// </summary>
        /// <returns>Объект данных графика</returns>
        public ChartData GetChartData()
        {
            return _chartData;
        }

        /// <summary>
        /// Добавить в график новый индикатор
        /// </summary>
        /// <param name="indic">Объект индикатора</param>
        /// <param name="isLeftAxis">true - ось Y будет слева, иначе справа</param>
        public void AddIndicator(IndicatorBase indic, bool isLeftAxis = false)
        {
            _indicators.Add(indic);
            var visuals = indic.GetVisuals();
            foreach (var vis in visuals)
            {
                _chartData.AddVisual(vis, isLeftAxis);
            }
            _srcProv.AddSources(indic.Id, indic.GetSources());
        }

        /// <summary>
        /// Добавить объект Маркер в график
        /// </summary>
        /// <param name="marker">Объект Маркер</param>
        /// <param name="isLeftAxis">true-по левой шкале Y, по умолчанию - по правой</param>
        public void AddMarker(MarkerBase marker, bool isLeftAxis = false)
        {
            _chartData.AddVisual(marker.GetVisual(), isLeftAxis);
            _markers.Add(marker);
        }

        /// <summary>
        /// Удалить объект Маркер из графика
        /// </summary>
        /// <param name="marker">Объект Маркер</param>
        public void RemoveMarker(MarkerBase marker)
        {
            var visual = marker.GetVisual();
            _chartData.RemoveVisual(visual);
            _markers.Remove(marker);
        }

        /// <summary>
        /// Получить все маркеры графика
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MarkerBase> GetMarkers()
        {
            return _markers;
        }

        private List<MarkerBase> _markers = new List<MarkerBase>();

        /// <summary>
        /// Получить список индикаторов графика
        /// </summary>
        /// <returns>Индикаторы</returns>
        public IEnumerable<IndicatorBase> GetIndicators()
        {
            return _indicators.ToList();
        }

        /// <summary>
        /// Возможность удаления индикатора (имеются ли зависимости от удаляемого объекта)
        /// </summary>
        /// <param name="indic">Индикатор</param>
        /// <returns>true - удаление возможно</returns>
        public bool CanRemoveIndicator(IndicatorBase indic)
        {
            return !_depManager.AnyDependsFrom(indic);
        }

        /// <summary>
        /// Удаление индикатора из графика
        /// </summary>
        /// <param name="indic">Индикатор</param>
        /// <returns>true - успешно, false - удаление невозможно</returns>
        public bool RemoveIndicator(IndicatorBase indic)
        {
            if (_depManager.AnyDependsFrom(indic)) return false;

            _srcProv.RemoveSources(indic.Id);

            var visuals = indic.GetVisuals().ToArray();
            foreach (var vis in visuals)
            {
                _chartData.RemoveVisual(vis);
            }

            _indicators.Remove(indic);
            _depManager.Undepend(indic);

            return true;
        }

        #region SeriesIndic
        /// <summary>
        /// Добавить индикатор Серия в график
        /// </summary>
        /// <param name="si">Индикатор Серия</param>
        public void AddSeriesIndic(SeriesIndic si)
        {
            _chartData.AddVisual(si.GetVisual(), si.Series.Axis == SeriesAxis.LeftAxis);
            _seriesIndics.Add(si);
        }

        /// <summary>
        /// Удалить индикатор Серия из графика
        /// </summary>
        /// <param name="si">Индикатор Серия</param>
        public void RemoveSeriesIndic(SeriesIndic si)
        {
            var visual = si.GetVisual();
            _chartData.RemoveVisual(visual);
            _seriesIndics.Remove(si);
        }

        /// <summary>
        /// Получить все индикаторы Серия графика
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SeriesIndic> GetSeriesIndics()
        {
            return _seriesIndics;
        }

        private List<SeriesIndic> _seriesIndics = new List<SeriesIndic>();
        #endregion

        #region Equity
        private bool _isShowEquity = false;

        public async void ShowEquity(int accountId)
        {
            if (_eqIndic == null && FirstSource != null)
            {
                _equity = new Equity(_insStoreBL, _instrumBL, _accountDA);
                _eqIndic = new EquityIndicator(_equity);
                await _equity.Generate(accountId, FirstSource.Bars.Dates);
            }
            if (_eqIndic == null || _chartData == null) return;

            var vis = _eqIndic.GetVisuals();
            foreach (var v in vis)
            {
                if (_chartData.LeftVisuals.Contains(v)) continue;
                _chartData.AddVisual(v, true);
            }

            _isShowEquity = true;
        }

        public void HideEquity()
        {
            if (_eqIndic == null) return;

            var vis = _eqIndic.GetVisuals();
            foreach (var v in vis)
            {
                _chartData.RemoveVisual(v);
            }

            _isShowEquity = false;
        }

        public bool IsShowEquity
        {
            get
            {
                return _isShowEquity;
            }
        }
        #endregion
    }

    /// <summary>
    /// Источник ценовых данных
    /// </summary>
    public class PriceSource
    {
        public BarRow Bars { get; internal set; }
        public Instrum Instrum { get; internal set; }
    }
}
