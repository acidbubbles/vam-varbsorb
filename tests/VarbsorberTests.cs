using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Varbsorb
{
    public class VarbsorberTests
    {
        private const string VamPath = @"C:\Vam";
        private Mock<IConsoleOutput> _consoleOutput;
        private List<string> _output;

        [SetUp]
        public void Setup()
        {
            _output = new List<string>();
            _consoleOutput = new Mock<IConsoleOutput>(MockBehavior.Strict);
            _consoleOutput
                .Setup(mock => mock.WriteLine(It.IsAny<string>()))
                .Callback((string text) => _output.Add(text));
        }

        [Test]
        public async Task CanExecute()
        {
            var varbsorber = new Varbsorber(_consoleOutput.Object, VamPath, false);

            await varbsorber.ExecuteAsync();

            Assert.That(_output, Is.EqualTo(new[]{
                @"Processing C:\Vam"
            }));
        }
    }
}