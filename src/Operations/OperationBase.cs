namespace Varbsorb.Operations
{
    public abstract class OperationBase
    {
        protected IConsoleOutput Output { get; }

        protected OperationBase(IConsoleOutput output)
        {
            Output = output;
        }

        protected void StartProgress()
        {
            Output.CursorVisible = false;
        }

        protected void CompleteProgress()
        {
            Output.WriteAndReset("");
            Output.CursorVisible = false;
        }
    }
}
