using System;
using System.Diagnostics;
using System.Linq;
using dnTrace.ProcessData;
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
        public void GetAllRunningProcesses()
        {
            var processes = ProcessUtil.GetAllProcesses();
            Assert.That(processes.Any(info => info.ProcessId == process.Id));
        }

        [Test]
        public void GetProcessModules()
        {
            var modules = ProcessUtil.GetProcessModules(process.Id);
            Assert.That(modules.Any());
        }

        [Test]
        public void ListManagedAndUnmanagedModules()
        {
            var modules = ProcessUtil.GetProcessModules(process.Id).ToList();
            var kernel = modules.First(info => info.Name.ToLowerInvariant().Contains("kernel32"));
            Assert.That(kernel.IsManaged, Is.False);
            var mscorlib = modules.First(info => info.Name.ToLowerInvariant().Contains("mscorlib"));
            Assert.That(mscorlib.IsManaged, Is.True);
        }

        [Test]
        public void CreatesInjectionContextFromProcess()
        {
            var f = new InjectionContextCreator(process);
            var contexts = f.CreateInjectionContext("System.String", "Trim").ToArray();
            Assert.That(contexts.Length, Is.EqualTo(2));
            Assert.That(contexts[0].MethodName, Is.EqualTo("Trim"));
            Assert.That(contexts[1].MethodName, Is.EqualTo("Trim"));
        }

        [Test]
        public void ReturnsEmptyContextsForNonExistantType()
        {
            var f = new InjectionContextCreator(process);
            var contexts = f.CreateInjectionContext("System.I.Do.Not.Exist", "AtAll").ToArray();
            Assert.That(contexts, Is.Empty);
        }
    }
}
