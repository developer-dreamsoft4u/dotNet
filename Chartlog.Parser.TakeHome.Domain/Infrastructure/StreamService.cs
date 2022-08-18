using Chartlog.Parser.TakeHome.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure
{
    public interface IStreamService
    {
        FileContainer ConvertToText(Stream stream, Encoding encoding);
    }
    public class StreamService : IStreamService
    {
        public FileContainer ConvertToText(Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            stream.Position = 0;
            string result = string.Empty;

            using (var reader = new StreamReader(stream, encoding))
            {
                result = reader.ReadToEnd();
            }

            var binaries = encoding.GetBytes(result);

            return new FileContainer()
            {
                Binaries = binaries,
                Content = result
            };
        }
    }
}
