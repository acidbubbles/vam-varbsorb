using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Moq;
using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public abstract class OperationTestsBase
    {
        protected const string _vamPath = @"C:\Vam";

        protected Mock<IConsoleOutput> _consoleOutput;
        protected MockFileSystem _fs;

        [SetUp]
        public void Setup()
        {
            _consoleOutput = new Mock<IConsoleOutput>(MockBehavior.Loose);
            _fs = new MockFileSystem();
        }

        protected IList<FreeFile> GivenFiles(params string[] localPaths)
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
    }
}
