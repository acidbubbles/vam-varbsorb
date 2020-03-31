using Autofac;

namespace Varbsorb.Operations
{
    public class OperationsFactory : IOperationsFactory
    {
        private readonly ILifetimeScope _scope;

        public OperationsFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Get<T>()
            where T : notnull, IOperation
        {
            return _scope.Resolve<T>();
        }
    }
}
