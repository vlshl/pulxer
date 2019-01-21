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
        /// <param name="initialSumma">Начальная сумма на счете</param>
        /// <param name="isTestAccount">Тестовый счет</param>
        /// <returns></returns>
        public Account CreateAccount(string code, string name, decimal commPerc, bool isShortEnable, 
            decimal initialSumma, bool isTestAccount)
        {
            var account = _accountDA.CreateAccount(code, name, commPerc, isShortEnable,
                isTestAccount ? AccountTypes.Test : AccountTypes.Real);
            _accountDA.CreateCash(account.AccountID, initialSumma, initialSumma, 0, 0, 0, 0);

            return account;
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
        public IEnumerable<Order> GetOrders(int accountID, int? fromID)
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
        public IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID)
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
        public IEnumerable<Trade> GetTrades(int accountID, int? fromID)
        {
            return _accountDA.GetTrades(accountID, fromID);
        }
        #endregion
    }
}
