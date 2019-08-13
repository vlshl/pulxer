using Common.Data;
using Common.Interfaces;
using Platform;
using System.Collections.Generic;
using System.Linq;
using CommonData = Common.Data;

namespace Pulxer
{
    /// <summary>
    /// Подсистема торговых счетов
    /// </summary>
    public class AccountBL : IAccountBL
    {
        private IAccountDA _accountDA;

        /// <summary>
        /// Конструктор подсистемы
        /// </summary>
        /// <param name="accountDA"></param>
        public AccountBL(IAccountDA accountDA)
        {
            _accountDA = accountDA;
        }

        #region Account
        /// <summary>
        /// Получить список торговых счетов
        /// </summary>
        /// <returns>Список торговых счетов</returns>
        public IEnumerable<CommonData.AccountListItem> GetAccountList()
        {
            var accs = _accountDA.GetAccounts();
            return (from acc in accs select new CommonData.AccountListItem(acc.AccountID, acc.Code, acc.Name)).ToList();
        }

        /// <summary>
        /// Создать новый торговый счет
        /// </summary>
        /// <param name="code">Код счета</param>
        /// <param name="name">Наименование счета</param>
        /// <param name="commPerc">Процент комиссии по операциям</param>
        /// <param name="isShortEnable">Разрешены ли короткие позиции</param>
        /// <returns></returns>
        public Account CreateTestAccount(string code, string name, decimal commPerc, bool isShortEnable)
        {
            return _accountDA.CreateAccount(code, name, commPerc, isShortEnable, AccountTypes.Test);
        }

        /// <summary>
        /// Создать cash для торгового счета
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="initialSumma">Начальная сумма</param>
        /// <param name="currSumma">Текущая сумма</param>
        /// <param name="sellSumma">Сумма продаж</param>
        /// <param name="buySumma">Сумма покупок</param>
        /// <param name="sellCommSumma">Сумма ком. при продажах</param>
        /// <param name="buyCommSumma">Сумма ком. при покупках</param>
        /// <returns></returns>
        public Cash CreateCash(int accountID, decimal initialSumma, decimal currSumma, decimal sellSumma, decimal buySumma, decimal sellCommSumma, decimal buyCommSumma)
        {
            return _accountDA.CreateCash(accountID, initialSumma, currSumma, sellSumma, buySumma, sellCommSumma, buyCommSumma);
        }

        /// <summary>
        /// Получить данные по торговому счету
        /// </summary>
        /// <param name="accountID">ID счета</param>
        /// <returns></returns>
        public CommonData.Account GetAccountByID(int accountID)
        {
            return _accountDA.GetAccountByID(accountID);
        }

        /// <summary>
        /// Обновить данные по счету
        /// </summary>
        /// <param name="account"></param>
        public void UpdateAccount(CommonData.Account account)
        {
            _accountDA.UpdateAccount(account);
        }

        /// <summary>
        /// Удаление тестового счета вместе со всеми данными
        /// </summary>
        /// <param name="accountID">ID счета</param>
        /// <param name="fullDelete">true-удаление и данных, и самого счета, false-удаление только данных по счету (заявки, сделки и т.д.)</param>
        /// <returns>true-успешно, false-удаления не произошло (или уже был удален ранее, или счет не тестовый)</returns>
        public bool DeleteTestAccountData(int accountID, bool fullDelete)
        {
            var acc = GetAccountByID(accountID);
            if (acc == null || acc.AccountType != AccountTypes.Test) return false;

            _accountDA.DeleteAccountData(accountID);
            if (fullDelete)
            {
                _accountDA.DeleteAccount(accountID);
            }

            return true;
        }
        #endregion

        #region Cash
        /// <summary>
        /// Получить запись "Позиции по деньгам"
        /// </summary>
        /// <param name="accountID">ID счета</param>
        /// <returns>Позиция по деньгам</returns>
        public CommonData.Cash GetCash(int accountID)
        {
            return _accountDA.GetCash(accountID);
        }
        #endregion

        #region Holdings
        /// <summary>
        /// Получить список владения бумагами для торгового счета
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <returns>Список владения бумагами</returns>
        public IEnumerable<CommonData.Holding> GetHoldings(int accountID)
        {
            return _accountDA.GetHoldings(accountID);
        }
        #endregion

        #region Orders
        /// <summary>
        /// Получить список заявок по счету
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="fromID">Начиная с указанного ID или все (null)</param>
        /// <returns>Список заявок</returns>
        public IEnumerable<Order> GetOrders(int accountID, int? fromID = null)
        {
            return _accountDA.GetOrders(accountID, fromID);
        }

        /// <summary>
        /// Получить список заявок по указанным id
        /// </summary>
        /// <param name="ids">Список ID</param>
        /// <returns>Список заявок</returns>
        public IEnumerable<Order> GetOrders(IEnumerable<int> ids)
        {
            return _accountDA.GetOrders(ids);
        }
        #endregion

        #region StopOrders
        /// <summary>
        /// Получить список стоп-заявок по счету
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="fromID">Начиная с указанного ID или все (null)</param>
        /// <returns>Список заявок</returns>
        public IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID = null)
        {
            return _accountDA.GetStopOrders(accountID, fromID);
        }

        /// <summary>
        /// Получить список стоп-заявок по указанным id
        /// </summary>
        /// <param name="ids">Список ID</param>
        /// <returns>Список стоп-заявок</returns>
        public IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids)
        {
            return _accountDA.GetStopOrders(ids);
        }
        #endregion

        #region Trades
        /// <summary>
        /// Получить список сделок по счету
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="fromID">Начиная с указанного ID или все (null)</param>
        /// <returns>Список сделок</returns>
        public IEnumerable<Trade> GetTrades(int accountID, int? fromID = null)
        {
            return _accountDA.GetTrades(accountID, fromID);
        }
        #endregion

        #region Series
        /// <summary>
        /// Получить список кастомных рядов данных по счету
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <returns>Список кастомных рядов</returns>
        public IEnumerable<Series> GetSeries(int accountID)
        {
            return _accountDA.GetSeries(accountID);
        }

        /// <summary>
        /// Получить кастомные значения для указанного ряда в порядке возрастания ID
        /// </summary>
        /// <param name="seriesID">Ряд кастомных значений</param>
        /// <param name="skipCount">Пропустить указанное количество значений</param>
        /// <param name="takeCount">Взять указанное количество значений после пропуска (null - все значения после пропуска)</param>
        /// <returns>Список значений в порядке возрастания ID</returns>
        public IEnumerable<SeriesValue> GetValues(int seriesID, int skipCount = 0, int? takeCount = null)
        {
            return _accountDA.GetValues(seriesID, skipCount, takeCount);
        }

        /// <summary>
        /// Получить общее количество значений в указанном ряду
        /// </summary>
        /// <param name="seriesID">Ряд кастомных значений</param>
        /// <returns>Кол-во значений в ряду</returns>
        public int GetValuesCount(int seriesID)
        {
            return _accountDA.GetValuesCount(seriesID);
        }
        #endregion
    }
}
