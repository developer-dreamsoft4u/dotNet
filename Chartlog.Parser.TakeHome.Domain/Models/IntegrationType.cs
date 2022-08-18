using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Models
{
    public abstract class IntegrationType
    {
        public abstract string IntegrationName { get; }
        public abstract string FriendlyIntegrationName { get; }
        public abstract string[] FileExtensionWhiteList { get; }
        public abstract string Delimiter { get; }
        public abstract string[] RequiredHeaders { get; }
        public abstract Dictionary<HeaderTransformation.ExpectedChartlogColumnType, string> HeaderDictionary { get; }
    }
}
