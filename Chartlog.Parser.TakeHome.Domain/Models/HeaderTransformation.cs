using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public class HeaderTransformation
    {
        public readonly string Value;
        public readonly int Index;
        public readonly ExpectedChartlogColumnType ColumnType;

        public HeaderTransformation(string value,
            int index,
            ExpectedChartlogColumnType columnType)
        {
            Value = value;
            Index = index;
            ColumnType = columnType;
        }

        public enum ExpectedChartlogColumnType
        {
            Hash,
            Account,
            Timestamp,
            Direction,
            Symbol,
            Shares,
            Price,
            OrderId,
            Commissions,
            Underlying,
            Action,
            Quantity
        }
    }
}
