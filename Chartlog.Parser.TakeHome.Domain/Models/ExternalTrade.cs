using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class ExternalTrade
    {
        public enum MonynessType
        {
            Unknown,
            InTheMoney,
            AtTheMoney,
            OutOfTheMoney
        }

        public enum UploadMethod
        {
            Importer = 0,
            Manual = 1,
            Connect = 2,
            Demo = 3,
            Manual_UI = 4,
            Unknown = 999
        }

        public enum TradeType
        {
            Equity,
            Option,
            ForEx,
            Crypto,
            Future
        }

        public enum OptionType
        {
            Unknown,
            Call,
            Put
        }

        public enum PositionEffect
        {
            Unknown,
            Opening,
            Closing
        }

        public long Id
        {
            get;
            set;
        }

        public string? Hash
        {
            get;
            set;
        }

        public string? Account
        {
            get;
            set;
        }

        public string? ObfuscatedAccountNumber
        {
            get;
            set;
        }

        public int ExistingAccountId
        {
            get;
            set;
        }

        public DateTime Timestamp
        {
            get;
            set;
        }

        public Direction Direction
        {
            get;
            set;
        }

        public string? Symbol
        {
            get;
            set;
        }
        public string? Uderlying
        {
            get;
            set;
        }
        public string? Action
        {
            get;
            set;
        }
        public decimal Quantity
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public string? OrderId
        {
            get;
            set;
        }

        public Guid UserId
        {
            get;
            set;
        }

        public TradeType Type
        {
            get;
            set;
        }

        public DateTime IngestionDate
        {
            get;
            set;
        }

        public UploadMethod Method
        {
            get;
            set;
        }

        public bool Processed
        {
            get;
            set;
        }

        public decimal Commissions
        {
            get;
            set;
        }

        public DateTime? ExpirationDate
        {
            get;
            set;
        }

        public decimal? Premium
        {
            get;
            set;
        }

        public decimal? StrikePrice
        {
            get;
            set;
        }

        public OptionType OptionOrderType
        {
            get;
            set;
        }

        public decimal? EcnFee
        {
            get;
            set;
        }

        public decimal? SecFee
        {
            get;
            set;
        }

        public decimal? TafFee
        {
            get;
            set;
        }

        public decimal? ClearingFee
        {
            get;
            set;
        }

        public decimal? StopLoss
        {
            get;
            set;
        }

        public decimal? ProfitTarget
        {
            get;
            set;
        }

        public decimal? MiscFee
        {
            get;
            set;
        }

        public PositionEffect Effect
        {
            get;
            set;
        }


        public decimal? UnderlyingAssetPrice
        {
            get;
            set;
        }

        public MonynessType Monyness
        {
            get;
            set;
        }
    }
}
