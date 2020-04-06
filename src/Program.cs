using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Autofac;
using Varbsorb.Hashing;
using Varbsorb.Logging;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Program
    {
        /// <summary>
        /// Varsborb: Clean your Virt-A-Mate install folder of duplicates found in var files.
        /// </summary>
        /// <param name="vam">The Virt-A-Mate install folder.</param>
        /// <param name="include">Filter which files will be included. You can specify partial paths starting from the vam root.</param>
        /// <param name="exclude">Filter which files will be excluded. You can specify partial paths starting from the vam root.</param>
        /// <param name="permanent">Whether to permanently delete files. By default, deleted files will be sent to the recycle bin.</param>
        /// <param name="verbose">Prints detailed output.</param>
        /// <param name="warnings">Prints broken scene references.</param>
        /// <param name="noop">Do not actually delete or write anything, just print the result.</param>
        /// <param name="log">Log the deleted and modified files list to a file path.</param>
        private static async Task<int> Main(
            string vam,
            string[]? include = null,
            string[]? exclude = null,
            bool permanent = false,
            bool verbose = false,
            bool warnings = false,
            bool noop = false,
            string? log = null)
        {
            var container = Configure(log);
            var runtime = container.Resolve<Varbsorber>();
            try
            {
                await runtime.ExecuteAsync(
                    vam,
                    include,
                    exclude,
                    permanent ? DeleteOptions.Permanent : DeleteOptions.RecycleBin,
                    verbose ? VerbosityOptions.Verbose : VerbosityOptions.Default,
                    warnings ? ErrorReportingOptions.ShowWarnings : ErrorReportingOptions.None,
                    noop ? ExecutionOptions.Noop : ExecutionOptions.Default);
                return 0;
            }
            catch (VarbsorberException exc)
            {
                Console.Error.WriteLine(exc.Message);
                return 1;
            }
            finally
            {
                var logger = container.Resolve<ILogger>();
                if (logger.Enabled)
                {
                    container.Resolve<IConsoleOutput>().WriteLine("Writing log file.");
                    await logger.DumpAsync();
                }
            }
        }

        private static IContainer Configure(string? log)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ConsoleOutput>().As<IConsoleOutput>().SingleInstance();
            builder.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance();
            if (string.IsNullOrEmpty(log))
                builder.RegisterType<NullLogger>().As<ILogger>().SingleInstance();
            else
                builder.Register(ctx => new WriteOnExitLogger(ctx.Resolve<IFileSystem>(), log)).As<ILogger>().SingleInstance();
            builder.RegisterType<SHA1HashingAlgo>().As<IHashingAlgo>().SingleInstance();
            builder.RegisterType<RecycleBin>().As<IRecycleBin>().SingleInstance();

            builder.RegisterType<ScanVarPackagesOperation>().As<IScanVarPackagesOperation>();
            builder.RegisterType<ScanFilesOperation>().As<IScanFilesOperation>();
            builder.RegisterType<MatchFilesToPackagesOperation>().As<IMatchFilesToPackagesOperation>();
            builder.RegisterType<ScanJsonFilesOperation>().As<IScanJsonFilesOperation>();
            builder.RegisterType<DeleteMatchedFilesOperation>().As<IDeleteMatchedFilesOperation>();
            builder.RegisterType<UpdateJsonFileReferencesOperation>().As<IUpdateJsonFileReferencesOperation>();
            builder.RegisterType<DeleteOrphanMorphFilesOperation>().As<IDeleteOrphanMorphFilesOperation>();

            builder.RegisterType<OperationsFactory>().As<IOperationsFactory>();
            builder.RegisterType<Varbsorber>();

            return builder.Build();
        }
    }
}
