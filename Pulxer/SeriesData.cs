using Common;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public class SeriesData
    {
        private readonly IAccountDA _accountDA;
        private List<Series> _series;
        private Dictionary<Series, List<SeriesValue>> _series_values;
        private Dictionary<Series, Tuple<ValueRow, Timeline>> _series_vrdata;
        private int _accountID = 0;

        public SeriesData(IAccountDA accountDA)
        {
            _accountDA = accountDA;
            _series = new List<Series>();
            _series_values = new Dictionary<Series, List<SeriesValue>>();
            _series_vrdata = new Dictionary<Series, Tuple<ValueRow, Timeline>>();
        }

        /// <summary>
        /// Открыть серию.
        /// Если серия с указанным ключом уже есть, то возврящается ее временный идентификатор.
        /// Иначе создается новая серия и возвращается ее временный идентификатор.
        /// </summary>
        /// <param name="key">Уникальный ключ (уникальность в пределах счета)</param>
        /// <param name="name">Наименование для показа на экране</param>
        /// <param name="axis">Ось, к которой относится серия</param>
        /// <returns>Идентификатор серии (если меньше нуля, то временный)</returns>
        public int OpenSeries(string key, string name = "", SeriesAxis axis = SeriesAxis.AxisX)
        {
            var s = _series.FirstOrDefault(r => r.Key == key);
            if (s != null) return s.SeriesID;

            int id = -1;
            if (_series.Any())
            {
                id = _series.Select(r => r.SeriesID).Min() - 1;
                if (id >= 0) id = -1;
            }

            Series series = new Series();
            series.SeriesID = id;
            series.Key = key;
            series.AccountID = 0;
            series.Name = name;
            series.Axis = axis;
            series.Data = "";
            _series.Add(series);

            return series.SeriesID;
        }

        /// <summary>
        /// Добавление в серию нового значения
        /// </summary>
        /// <param name="seriesID">Идентификатор серии (то значение, которое вернул вызов OpenSeries)</param>
        /// <param name="time">Дата и время значения</param>
        /// <param name="val">Десятичное значение</param>
        /// <returns>true - успешно, false - не найден Series по указанному идентификатору</returns>
        public bool AddSeriesValue(int seriesID, DateTime time, decimal val)
        {
            var series = _series.FirstOrDefault(r => r.SeriesID == seriesID);
            if (series == null) return false;

            if (!_series_values.ContainsKey(series))
            {
                _series_values.Add(series, new List<SeriesValue>());
            }
            var vals = _series_values[series];

            var sv = new SeriesValue();
            sv.SeriesValueID = 0;
            sv.SeriesID = series.SeriesID;
            sv.Time = time;
            sv.Value = val;
            sv.Data = "";
            sv.EndTime = null;
            sv.EndValue = null;
            vals.Add(sv);

            return true;
        }

        /// <summary>
        /// Подписывание ValueRow. 
        /// Все новые значение ValueRow будут автоматически добавляться в серию.
        /// </summary>
        /// <param name="seriesID">Идентификатор серии</param>
        /// <param name="valueRow">Ряд значений</param>
        /// <param name="timeline">Временная ось</param>
        public void SubscribeValueRow(int seriesID, ValueRow valueRow, Timeline timeline)
        {
            var ser = _series.FirstOrDefault(s => s.SeriesID == seriesID);
            if (ser == null) return;

            if (_series_vrdata.ContainsKey(ser))
            {
                var old_vr = _series_vrdata[ser].Item1;
                old_vr.Change -= valueRow_Change;
                _series_vrdata.Remove(ser);
            }

            if (valueRow != null && timeline != null)
            {
                _series_vrdata.Add(ser, new Tuple<ValueRow, Timeline>(valueRow, timeline));
                valueRow.Change += valueRow_Change;
            }
        }

        private void valueRow_Change(ValueRow valueRow, bool isReset)
        {
            if (isReset) return; // reset мы не обрабатываем, т.е. reset означает очистку массива, а не добавление нового значения
            if (valueRow == null || valueRow.LastIndex < 0) return; // пустой список, добавлять нечего
            decimal? val = valueRow[valueRow.LastIndex];
            if (val == null) return; // пустые значения не добавляем

            var sers = _series_vrdata.Where(r => r.Value.Item1 == valueRow).Select(r => r.Key).ToList();
            foreach (var s in sers)
            {
                var tl = _series_vrdata[s].Item2;
                var time = tl.Start(valueRow.LastIndex);
                if (time == null) continue; // не смогли определить время

                AddSeriesValue(s.SeriesID, time.Value, val.Value);
            }
        }

        /// <summary>
        /// Установка идентификатора счета.
        /// Сразу после создания серии счет не установлен.
        /// Если был LoadData(), то счет устанавливается.
        /// Перед сохранением данных счет должен быть установлен.
        /// Т.е. если загрузки не было, а сохранить нужно, то надо сначала выполнить установку счета.
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        public void SetAccount(int accountID)
        {
            _accountID = accountID;
        }

        /// <summary>
        /// Сохранение данных в базе.
        /// Временный идентификатор серии (меньше нуля) после сохранения перестает быть действительным.
        /// Новый идентификатор можно узнать через процедуру OpenSeries.
        /// Перед сохранением нужно установить счет (SetAccount), если он не был установлен ранее, например, в результате выполнения LoadData()
        /// </summary>
        /// <returns>true - успешно, false - не установлен счет, сохранения не было</returns>
        public bool SaveData()
        {
            if (_accountID == 0) return false;

            foreach (var series in _series)
            {
                if (series.SeriesID <= 0)
                {
                    var s = _accountDA.CreateSeries(_accountID, series.Key, series.Name, series.Axis, series.Data);
                    series.SeriesID = s.SeriesID;
                }

                List<SeriesValue> vals = null;
                if (_series_values.ContainsKey(series))
                {
                    vals = _series_values[series];
                }
                if (vals != null)
                {
                    var newVals = vals.Where(r => r.SeriesValueID <= 0).ToList();
                    if (newVals.Any())
                    {
                        foreach (var v in newVals)
                        {
                            v.SeriesID = series.SeriesID;
                        }
                        _accountDA.CreateSeriesValues(newVals);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Загрузка данных из базы и установка счета
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        public void LoadData(int accountID)
        {
            _accountID = accountID;
            _series = _accountDA.GetSeries(accountID).ToList();

            _series_values.Clear();
            foreach (var series in _series)
            {
                var list = _accountDA.GetSeriesValues(series.SeriesID).ToList();
                _series_values.Add(series, list);
            }
        }
    }
}
