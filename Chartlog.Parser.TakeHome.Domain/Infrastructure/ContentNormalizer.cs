using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public interface IContentNormalizer
    {
        string Normalize(string content);
    }

    public class ContentNormalizer : IContentNormalizer
    {
        public string Normalize(string content)
        {
            return content
                .ToLower()
                .Trim();
        }
    }
}
