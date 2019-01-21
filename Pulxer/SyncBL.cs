using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pulxer
{
    public class SyncBL : ISyncBL
    {
        private const string DEC_FORMAT = "0.########";
        private readonly IInstrumDA _instrumDA;

        public SyncBL(IInstrumDA instrumDA)
        {
            _instrumDA = instrumDA;
        }

        /// <summary>
        /// Получить сериализованный список инструментов, если хэши различаются
        /// </summary>
        /// <param name="hash">Хэш</param>
        /// <returns>Сериализованный список инструментов</returns>
        public string GetInstrums(string hash)
        {
            var instrums = _instrumDA.GetInstrums().OrderBy(r => r.Ticker);
            string ser = InstrumSerialize(instrums, true);
            string h = CalcHash(ser);
            if (h == hash) return "";

            return InstrumSerialize(instrums, false);
        }

        /// <summary>
        /// Сериализация списка инструментов
        /// </summary>
        /// <param name="instrums">Список инструментов</param>
        /// <param name="idIgnore">true-игнорировать ID (заменять нулем) при сериализации, false - использовать ID</param>
        /// <returns>Сериализованная строка</returns>
        private string InstrumSerialize(IEnumerable<Instrum> instrums, bool idIgnore = false)
        {
            StringBuilder sb = new StringBuilder();
            string format = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\n";
            foreach (var ins in instrums)
            {
                sb.AppendFormat(format,
                    idIgnore ? "0" : ins.InsID.ToString(),
                    ins.Ticker,
                    ins.ShortName,
                    ins.Name,
                    ins.LotSize.ToString(),
                    ins.Decimals.ToString(),
                    ins.PriceStep.ToString(DEC_FORMAT, CultureInfo.InvariantCulture));
            }

            return sb.ToString().TrimEnd('\n');
        }

        /// <summary>
        /// Вычислить хэш для строки в виде base64
        /// </summary>
        /// <param name="data">Входная строка</param>
        /// <returns>Хэш в виде base64-строки</returns>
        private string CalcHash(string data)
        {
            if (data == null) data = "";

            var bytes = Encoding.UTF8.GetBytes(data);
            MD5 md5 = MD5.Create();
            var hash = md5.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}
