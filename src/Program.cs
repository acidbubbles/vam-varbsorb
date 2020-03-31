using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Autofac;
using Varbsorb.Hashing;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Program
    {
        /// <summary>
        /// Varsborb: Clean your Virt-A-Mate install folder of duplicates found in var files.
        /// </summary>
        /// <param name="vam">The Virt-A-Mate install folder.</param>
        /// <param name="verbose">Prints detailed output.</param>
        /// <param name="warnings">Prints broken scene references.</param>
        /// <param name="noop">Do not actually delete or write anything, just print the result.</param>
        private static async Task<int> Main(string vam, bool verbose = false, bool warnings = false, bool noop = false)
        {
            var container = Configure();
            var runtime = container.Resolve<Varbsorber>();
            try
            {
                await runtime.ExecuteAsync(vam, verbose, warnings, noop);
                return 0;
            }
            catch (VarbsorberException exc)
            {
                Console.Error.WriteLine(exc.Message);
            }
            return 1;
        }

        private static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleOutput>().As<IConsoleOutput>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterType<ListVarPackagesOperation>().As<IListVarPackagesOperation>();
            builder.RegisterType<SHA1HashingAlgo>().As<IHashingAlgo>();
            builder.RegisterType<ListFilesOperation>().As<IListFilesOperation>();
            builder.RegisterType<MatchFilesToPackagesOperation>().As<IMatchFilesToPackagesOperation>();
            builder.RegisterType<ListScenesOperation>().As<IListScenesOperation>();
            builder.RegisterType<UpdateSceneReferencesOperation>().As<IUpdateSceneReferencesOperation>();
            builder.RegisterType<OperationsFactory>().As<IOperationsFactory>();
            builder.RegisterType<Varbsorber>();
            return builder.Build();
        }
    }
}
