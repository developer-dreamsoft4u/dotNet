using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.ParserTwo
{
    public class ParserTwoIntegrationType : IntegrationType
    {
        public override string IntegrationName => "ParserTwo";

        public override string FriendlyIntegrationName => "Typically these will hold the name of the broker or trading platform, for now we just call it 'ParserOne'";

        public override string[] FileExtensionWhiteList => new[] { ".csv", ".txt" };

        public override string Delimiter => ",";

        //public override string[] RequiredHeaders => new[] { "underlying", "action", "quantity", "price", "time","date", "exch.", "account", "order ref.", "clearing" };
        public override string[] RequiredHeaders => new[] { "underlying", "action", "quantity", "price" };



        public override Dictionary<HeaderTransformation.ExpectedChartlogColumnType, string> HeaderDictionary => null;
    }
}
