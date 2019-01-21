using System;

namespace Common.Data
{
    /// <summary>
    /// Trade account
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Broker commission percentage
        /// </summary>
        public decimal CommPerc { get; set; }

        /// <summary>
        /// Short position enable
        /// </summary>
        public bool IsShortEnable { get; set; }

        /// <summary>
        /// Account type - for testing or real trading
        /// </summary>
        public AccountTypes AccountType { get; set; }

        public Account()
        {
            Name = ""; Code = "";
        }
    }

    /// <summary>
    /// Account type: for testing, for real trading
    /// </summary>
    public enum AccountTypes : byte
    {
        Test = 0,
        Real = 1
    }

    /// <summary>
    /// Use for display account list
    /// </summary>
    public class AccountListItem
    {
        public AccountListItem(int accountID, string code, string name)
        {
            this.AccountID = accountID;
            this.Code = code;
            this.Name = name;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Positions for money
    /// </summary>
    public class Cash
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int CashID { get; set; }

        /// <summary>
        /// Initial summa
        /// </summary>
        public decimal Initial { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Current summa
        /// </summary>
        public decimal Current { get; set; }

        /// <summary>
        /// Total sell summa
        /// </summary>
        public decimal Sell { get; set; }

        /// <summary>
        /// Total buy summa
        /// </summary>
        public decimal Buy { get; set; }

        /// <summary>
        /// Total sell trades commission
        /// </summary>
        public decimal SellComm { get; set; }

        /// <summary>
        /// Total buy trades commission
        /// </summary>
        public decimal BuyComm { get; set; }
    }

    /// <summary>
    /// Positions for fin. instruments
    /// </summary>
    public class Holding
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int HoldingID { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }
    }
}
