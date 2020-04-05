namespace Varbsorb.Operations
{
    public abstract class OperationBase
    {
        protected abstract string Name { get; }
        protected IConsoleOutput Output { get; }

        protected OperationBase(IConsoleOutput output)
        {
            Output = output;
        }

        protected void StartProgress()
        {
            Output.CursorVisible = false;
        }

        protected void ReportProgress(ProgressInfo progress)
        {
            if (progress.Total > 0)
                Output.WriteAndReset($"{Name}: {progress.Processed / (float)progress.Total * 100:00}% ({progress.Processed} of {progress.Total}): {progress.Current}");
            else
                Output.WriteAndReset($"{Name}: {progress.Processed}: {progress.Current}");
        }

        protected void CompleteProgress()
        {
            Output.WriteAndReset("");
            Output.CursorVisible = true;
        }

        public class ProgressInfo
        {
            public int Processed { get; }
            public int Total { get; }
            public string Current { get; }

            public ProgressInfo(int scenesProcessed, int totalScenes, string current)
            {
                Processed = scenesProcessed;
                Total = totalScenes;
                Current = current;
            }
        }
    }
}
