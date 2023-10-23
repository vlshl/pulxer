using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer
{
    public class TickHistory : ITickHistory
    {
        private string _tickHistoryPath;
        private ILogger<TickHistory> _logger;

        public TickHistory(IConfig config, ILogger<TickHistory> logger)
        {
            _tickHistoryPath = config.GetTickHistoryPath();
            _logger = logger;
        }

        public DateTime[] GetDates(int year = 0)
        {
            List<DateTime> dates = new List<DateTime>();

            try
            {
                if (!Directory.Exists(_tickHistoryPath)) return dates.ToArray();

                var years = Directory.EnumerateDirectories(_tickHistoryPath)
                    .Select(d => int.Parse(Path.GetFileName(d))).ToArray();

                foreach (var y in years)
                {
                    if ((year != 0) && (y != year)) continue;

                    var months = Directory.EnumerateDirectories(Path.Combine(_tickHistoryPath, y.ToString()))
                        .Select(d => int.Parse(Path.GetFileName(d))).ToArray();

                    foreach (var m in months)
                    {
                        var days = Directory.EnumerateDirectories(Path.Combine(_tickHistoryPath, y.ToString(), m.ToString("0#")))
                            .Select(d => int.Parse(Path.GetFileName(d))).ToArray();

                        foreach (var d in days)
                        {
                            dates.Add(new DateTime(y, m, d));
                        }
                    }
                }

                return dates.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка дат.");
                return dates.ToArray();
            }
        }

        public Task<Instrum[]> GetInstrumsAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastDate()
        {
            try
            {
                if (!Directory.Exists(_tickHistoryPath)) return null;

                var years = Directory.EnumerateDirectories(_tickHistoryPath)
                    .Select(d => int.Parse(Path.GetFileName(d))).ToArray();
                if (!years.Any()) return null;

                var year = years.Max();

                var months = Directory.EnumerateDirectories(Path.Combine(_tickHistoryPath, year.ToString()))
                    .Select(d => int.Parse(Path.GetFileName(d))).ToArray();
                if (!months.Any()) return null;

                var month = months.Max();

                var days = Directory.EnumerateDirectories(Path.Combine(_tickHistoryPath, year.ToString(), month.ToString("0#")))
                    .Select(d => int.Parse(Path.GetFileName(d))).ToArray();
                if (!days.Any()) return null;

                var day = days.Max();

                string tickerPath = Path.Combine(_tickHistoryPath, year.ToString(), month.ToString("0#"), day.ToString("0#"));
                if (!Directory.EnumerateFiles(tickerPath).Any()) return null;

                return new DateTime(year, month, day);
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при получении максимальной даты.");
                return null;
            }
        }

        public Task<Tick[]> GetTicksAsync(DateTime date, int insId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Сохранение тиковых данных в формате AllTrades
        /// </summary>
        /// <param name="date">Дата тиковых данных</param>
        /// <param name="ticker">Тикер</param>
        /// <param name="data">Бинарные данные</param>
        /// <returns>true-успешно, false-ошибка</returns>
        public async Task<bool> SaveTicksBlobAsync(DateTime date, string ticker, byte[] data)
        {
            string dir = Path.Combine(_tickHistoryPath,
                date.Year.ToString(),
                date.Month.ToString("0#"),
                date.Day.ToString("0#"));

            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (var fs = File.Create(Path.Combine(dir, ticker)))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                    fs.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении тиковых данных.");
                return false;
            }
        }
    }
}
