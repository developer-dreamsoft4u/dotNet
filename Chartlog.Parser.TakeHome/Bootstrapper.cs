using Chartlog.Parser.TakeHome.Domain.Infrastructure;
using Chartlog.Parser.TakeHome.Domain.Infrastructure.Links;
using Chartlog.Parser.TakeHome.Domain.Infrastructure.Pipelines;
using Chartlog.Parser.TakeHome.Domain.ParserTwo;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome
{
    internal static class Bootstrapper
    {
        internal static void RegisterDependencies(IServiceCollection c)
        {
            c.AddScoped<IContentNormalizer, ContentNormalizer>();
            c.AddScoped<IFileDelimiterValidator, FileDelimiterValidator>();
            c.AddScoped<IColumnHeaderValidator, ColumnHeaderValidator>();
            c.AddScoped<IStreamService, StreamService>();

            var logConfig = new LoggerConfiguration()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                .MinimumLevel.Debug();

            c.AddScoped<ILogger>(a => logConfig.CreateLogger());

            c.AddSingleton(typeof(IHeaderValidator<>), typeof(HeaderValidator<>));
            c.AddSingleton<IEnumerable<IHeaderValidator>>(a =>
            {

               return new IHeaderValidator[]
               {
                    a.GetService<IHeaderValidator<ParserTwoIntegrationType>>()
               };
            });


            var parserOnePipeline = new Pipeline();
            parserOnePipeline
                .StartWith<PipelineMarker<ParserTwoIntegrationType>>()
                .ThenWith<SessionRecordingLink>()
                .ThenWith<FileExtensionValidationLink<ParserTwoIntegrationType>>()
                .ThenWith<EncodingValidationLink>()
                .ThenWith<StreamConversionLink>()
                .ThenWith<BackupFileLink>()
                .ThenWith<ContentNormalizationLink>()
                .ThenWith<RequiredColumnHeadersValidationLink<ParserTwoIntegrationType>>()
                .ThenWith<FileDelimiterValidationLink<ParserTwoIntegrationType>>()
                .ThenWith<ParserTwoTradeParser>()
                .ThenWith<QueueTradesForProcessingLink>()
                .BootstrapTo(c);

        }
    }
}
