using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ISyncBL
    {
        /// <summary>
        /// Получить сериализованный список инструментов, если хэши различаются
        /// </summary>
        /// <param name="hash">Хэш</param>
        /// <returns>Сериализованный список инструментов</returns>
        string GetInstrums(string hash);
    }
}
