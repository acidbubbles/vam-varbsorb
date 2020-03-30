namespace Varbsorb.Operations
{
    public abstract class OperationBase
    {
        protected readonly IConsoleOutput _output;

        protected OperationBase(IConsoleOutput output)
        {
            _output = output;
        }

        protected void StartProgress()
        {
            _output.CursorVisible = false;
        }

        protected void CompleteProgress()
        {
            _output.WriteAndReset("");
            _output.CursorVisible = false;
        }
    }
}