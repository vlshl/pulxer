using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class Lib
    {
        public static string Ids2Str(IEnumerable<int> ids)
        {
            return string.Join(",", ids.Select(r => r.ToString()));
        }

        public static IEnumerable<int> Str2Ids(string str)
        {
            if (str == null) return null;
            if (str == "") return new List<int>();

            var parts = str.Split(',');
            List<int> list = new List<int>();
            int n;
            foreach (var part in parts)
            {
                if (int.TryParse(part, out n))
                {
                    list.Add(n);
                }
            }

            return list;
        }

        /// <summary>
        /// Преобразование числа в виде yyyymmdd в дату.
        /// Если yyyymmdd <= 9999, считается что указан только год, а месяц и день будут 0101 или 1231 в зависимости от значения toLast (true - 1231, false - 0101)
        /// Если год <= 99, к нему прибавляется 2000
        /// Если 10000 <= yyyymmdd <= 999999, то считается что задан год и месяц, а день будет взят 1 (toLast = false) или 28-31 в зависимости от месяца/года (toLast = true)
        /// В остальных случаях считается, что заданы все компоненты даты - год, месяц, день
        /// </summary>
        /// <param name="y4md">Целое положительное число в формате ГГГГММДД
        /// Допускается также ГГ - только год в виде двух знаков, прибавляем 2000
        /// ГГГГ - только год в виде четырех знаков
        /// ГГГГММ - год и месяц
        /// ГГГГММДД - год, месяц и день
        /// Если дата неполная, то она округляется на начало или конец периода, в зависимости от toLast
        /// Например, если день не указан, то он будет 1 или 29-31, если месяц не указан, то он будет 1 или 12
        /// То есть значение 21 будет интерпретироваться как 20210101 или как 20211231
        /// </param>
        /// <param name="toLast">В случае неполной даты (задан только год, или только год и месяц) округлять в большую сторону (true), или в меньшую (false)</param>
        /// <returns>Дата или null, если преобразовать не удалось</returns>
        public static DateTime? IntToDateTime(int y4md, bool toLast = false)
        {
            if (y4md < 0) return null;

            try
            {
                int year; int month; int day;
                if (y4md <= 9999) // задан только год
                {
                    year = y4md;
                    if (year <= 99) year += 2000;
                    month = toLast ? 12 : 1;
                    day = 1;
                    if (toLast)
                    {
                        day = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
                    }
                }
                else if (y4md >= 10000 && y4md <= 999999) // задан год и месяц
                {
                    year = y4md / 100;
                    month = y4md % 100;
                    day = 1;
                    if (toLast)
                    {
                        day = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
                    }
                }
                else
                {
                    year = y4md / 10000;
                    int mmdd = y4md % 10000;
                    month = mmdd / 100;
                    day = mmdd % 100;
                }

                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }
    }
}
