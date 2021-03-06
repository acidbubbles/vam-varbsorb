namespace Varbsorb.Operations
{
    public interface IOperationsFactory
    {
        T Get<T>()
            where T : notnull, IOperation;
    }
}
