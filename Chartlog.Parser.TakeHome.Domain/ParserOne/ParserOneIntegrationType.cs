using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.ParserOne
{
    public class ParserTwoIntegrationType : IntegrationType
    {
        public override string IntegrationName => "ParserOne";

        public override string FriendlyIntegrationName => "Typically these will hold the name of the broker or trading platform, for now we just call it 'ParserOne'";

        public override string[] FileExtensionWhiteList => new[] { ".csv", ".txt" };

        public override string Delimiter => ",";

        public override string[] RequiredHeaders => new[] { "symb", "time", "qty", "price", "account", "side" };

        public override Dictionary<HeaderTransformation.ExpectedChartlogColumnType, string> HeaderDictionary => null;
    }
}
