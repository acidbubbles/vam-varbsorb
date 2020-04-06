using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Models;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class VarbsorberTests
    {
        private const string _vam = @"C:\VaM";

        [Test]
        public async Task CanCreateFilter()
        {
            var fs = new MockFileSystem();
            fs.AddFile(@"C:\VaM\VaM.exe", new MockFileData(""));
            var opFactory = new Mock<IOperationsFactory>(MockBehavior.Strict);
            opFactory.Setup(fac => fac.Get<IScanVarPackagesOperation>()).Returns(Mock.Of<IScanVarPackagesOperation>());
            opFactory.Setup(fac => fac.Get<IScanFilesOperation>()).Returns(Mock.Of<IScanFilesOperation>());
            opFactory.Setup(fac => fac.Get<IMatchFilesToPackagesOperation>()).Returns(Mock.Of<IMatchFilesToPackagesOperation>());
            opFactory.Setup(fac => fac.Get<IScanJsonFilesOperation>()).Returns(Mock.Of<IScanJsonFilesOperation>());
            opFactory.Setup(fac => fac.Get<IUpdateJsonFileReferencesOperation>()).Returns(Mock.Of<IUpdateJsonFileReferencesOperation>());
            opFactory.Setup(fac => fac.Get<IDeleteMatchedFilesOperation>()).Returns(Mock.Of<IDeleteMatchedFilesOperation>());
            opFactory.Setup(fac => fac.Get<IDeleteOrphanMorphFilesOperation>()).Returns(Mock.Of<IDeleteOrphanMorphFilesOperation>());
            var varbsorber = new Varbsorber(Mock.Of<IConsoleOutput>(), fs, opFactory.Object);

            await varbsorber.ExecuteAsync(_vam, null, null, DeleteOptions.Permanent, VerbosityOptions.Default, ErrorReportingOptions.None, ExecutionOptions.Default);

            Assert.Pass();
        }
    }
}
