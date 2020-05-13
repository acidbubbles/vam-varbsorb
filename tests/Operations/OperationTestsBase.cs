using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Moq;
using NUnit.Framework;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public abstract class OperationTestsBase
    {
        protected const string _vamPath = @"C:\Vam";

        protected Mock<IConsoleOutput> _consoleOutput;
        protected MockFileSystem _fs;
        protected ILogger _logger;

        private List<string> _logs;

        [SetUp]
        public void Setup()
        {
            _consoleOutput = new Mock<IConsoleOutput>(MockBehavior.Loose);
            _fs = new MockFileSystem();
            _logs = new List<string>();
            var logger = new Mock<ILogger>();
            logger.Setup(mock => mock.Enabled).Returns(true);
            logger.Setup(mock => mock.Log(It.IsAny<string>())).Callback((string message) => _logs.Add(message));
            _logger = logger.Object;
        }

        protected static IList<FreeFile> GivenFiles(params string[] localPaths)
        {
            return localPaths.Select(GivenFile).ToList();
        }

        protected static FreeFile GivenFile(string f)
        {
            return new FreeFile($@"{_vamPath}\{f}", f);
        }

        protected FreeFile MockFile(string f, string contents)
        {
            var file = GivenFile(f);
            _fs.AddFile(file.Path, new MockFileData(contents));
            return file;
        }

        protected IFilter GivenExcludes(params string[] excluded)
        {
            return Filter.From(_vamPath, null, excluded);
        }

        protected void AssertLogs()
        {
            var errors = _logs.Where(l => l.StartsWith("[ERROR]")).ToList();
            Assert.That(errors, Is.Empty);
        }
    }
}
