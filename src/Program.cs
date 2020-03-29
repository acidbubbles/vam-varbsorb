using System.IO.Abstractions;
using System.Threading.Tasks;
using Autofac;
using Varbsorb.Operations;

namespace Varbsorb
{
    class Program
    {
        static async Task Main(string vam, bool noop)
        {
            var container = Configure();
            var runtime = container.Resolve<Varbsorber>();
            await runtime.ExecuteAsync(vam, noop);
        }

        private static IContainer Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleOutput>().As<IConsoleOutput>();
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterType<ListVarPackagesOperation>().As<IListVarPackagesOperation>();
            builder.RegisterType<ListFilesOperation>().As<IListFilesOperation>();
            builder.RegisterType<OperationsFactory>().As<IOperationsFactory>();
            builder.RegisterType<Varbsorber>();
            return builder.Build();
        }
    }
}
