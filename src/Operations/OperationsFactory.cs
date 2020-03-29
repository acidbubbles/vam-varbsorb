using Autofac;

namespace Varbsorb.Operations
{
    public interface IOperationsFactory
    {
        T Get<T>() where T : IOperation;
    }

    public class OperationsFactory : IOperationsFactory
    {
        private readonly ILifetimeScope _scope;

        public OperationsFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Get<T>() where T : IOperation
        {
            return _scope.Resolve<T>();
        }
    }
}