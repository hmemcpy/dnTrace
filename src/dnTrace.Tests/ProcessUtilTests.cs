using System;
using System.Diagnostics;
using System.Linq;
using dnTrace.Utils;
using NUnit.Framework;

namespace dnTrace.Tests
{
    [TestFixture]
    public class ProcessUtilTests
    {
        private Process process;

        [SetUp]
        public void Setup()
        {
            process = Process.GetCurrentProcess();
        }

        [Test]
        public void ShouldGetAllRunningProcesses()
        {
            var processes = ProcessUtil.GetAllProcesses();
            Assert.That(processes.Any(info => info.ProcessId == process.Id));
        }

        [Test]
        public void ShouldGetProcessModules()
        {
            var modules = ProcessUtil.GetProcessModules(process.Id);
            Assert.That(modules.Any());
        }

        [Test]
        public void ShouldListManagedAndUnmanagedModules()
        {
            var modules = ProcessUtil.GetProcessModules(process.Id).ToList();
            var kernel = modules.First(info => info.Name.ToLowerInvariant().Contains("kernel32"));
            Assert.That(kernel.IsManaged, Is.False);
            var mscorlib = modules.First(info => info.Name.ToLowerInvariant().Contains("mscorlib"));
            Assert.That(mscorlib.IsManaged, Is.True);
        }
    }
}
