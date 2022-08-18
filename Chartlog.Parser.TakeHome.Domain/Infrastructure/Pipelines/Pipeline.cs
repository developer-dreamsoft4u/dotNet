using Chartlog.Parser.TakeHome.Domain.Infrastructure.Links;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chartlog.Parser.TakeHome.Domain.Infrastructure.Pipelines
{
    public class Pipeline
    {
        public IPipelineRegistrationProcess StartWith<T>() where T : PipelineMarker
        {
            var reg = new PipelineRegistrationProcess();
            reg.ThenWith<T>();
            return reg;
        }

        public interface IPipelineRegistrationProcess
        {
            IPipelineRegistrationProcess ThenWith<T>() where T : Link;
            void BootstrapTo(IServiceCollection collection);
        }

        private class PipelineRegistrationProcess : IPipelineRegistrationProcess
        {
            private readonly List<Type> _registrations = new List<Type>();

            public IPipelineRegistrationProcess ThenWith<T>() where T : Link
            {
                _registrations.Add(typeof(T));
                return this;
            }

            public void BootstrapTo(IServiceCollection collection)
            {
                if (!_registrations.Any())
                    throw new Exception("No links added to pipeline");

                collection.AddScoped(typeof(PipelineMarker), a =>
                {
                    Type lastType = null;
                    object lastInstance = null;

                    for (var i = _registrations.Count - 1; i >= 0; i--)
                    {
                        var type = _registrations[i];
                        bool end = typeof(PipelineMarker).IsAssignableFrom(type);

                        var constructors = type.GetConstructors();
                        var targetConstructor = constructors.First();
                        var @params = targetConstructor.GetParameters();
                        Type copyLastType = lastType;



                        List<object> args = new List<object>();

                        foreach (var p in @params)
                        {
                            //if this is the last link in the chain and the parameter we are iterating derive from a `Link` type then insert a terminator `NoOpLink`
                            if (copyLastType == null && p.ParameterType.IsAssignableFrom(typeof(Link)))
                                args.Add(new NoOpLink());
                            //if this is not the last link and the parameter we are iterating derives from a `Link` then use the last constructed `Link` child from the previous iteration
                            //and pass it to the args variable
                            else if (copyLastType != null && p.ParameterType.IsAssignableFrom(typeof(Link)))
                            {
                                args.Add(lastInstance);
                            }
                            //if the paramter we are cycling does not derive from `Link` it is a service class, we need DI to resolve that.
                            else
                            {
                                args.Add(a.GetService(p.ParameterType));
                            }
                        }

                        var instance = Activator.CreateInstance(type, args.ToArray());
                        lastInstance = instance;
                        lastType = type;

                        if (end)
                            return instance;
                    }

                    return null;
                });
            }
        }
    }
}
