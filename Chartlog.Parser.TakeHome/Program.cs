// See https://aka.ms/new-console-template for more information
using Chartlog.Parser.TakeHome;
using Chartlog.Parser.TakeHome.Domain.Infrastructure.Pipelines;
using Chartlog.Parser.TakeHome.Domain.ParserTwo;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var collection = new ServiceCollection();
Bootstrapper.RegisterDependencies(collection);

var serviceProvider = collection.BuildServiceProvider();

//var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Files\\1.csv";
var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Files\\2.csv";

using var str = new FileStream(filePath, FileMode.Open);

var pipelines = serviceProvider.GetService<IEnumerable<PipelineMarker>>();
var pipeline = pipelines.First(a => a.IntegrationType == Activator.CreateInstance<ParserTwoIntegrationType>().IntegrationName);
await pipeline.ExecuteAsync(new Chartlog.Parser.TakeHome.Domain.Models.ProcessFileRequest()
{
    Stream = str,
    FileExtension = Path.GetExtension(filePath),
    FileName = Path.GetFileName(filePath),
    UserId = Guid.NewGuid(),
    SessionId = Guid.NewGuid(),
    Integration = pipeline.IntegrationType,
    StartTime = DateTime.UtcNow
});
