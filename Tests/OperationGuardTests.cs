using System;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class OperationGuardTests
    {
        private void DoTest(IOperationGuard guard)
        {
            int check = 0;
            int callCount = 0;
            int callAttempts = 0;
            int successCallCount = 0;
            int exceptionCount = 0;

            Action action = () =>
            {
                Interlocked.Increment(ref callCount);

                if (Interlocked.Increment(ref check) != 1)
                    throw new Exception(check.ToString());

                Thread.Sleep(100);

                if (Interlocked.Decrement(ref check) != 0)
                    throw new Exception(check.ToString());
            };

            ThreadStart callAction = () =>
            {
                for (int i = 0; i < 1000; ++i)
                {
                    Interlocked.Increment(ref callAttempts);

                    var res = guard.ExecuteIfFree(action, ex => Interlocked.Increment(ref exceptionCount));

                    if (res)
                        successCallCount++;

                    Thread.Sleep(1);
                }
            };

            //ACT
            var threads = new Thread[20];

            for (int i = 0; i < 20; ++i)
            {
                threads[i] = new Thread(callAction);
                threads[i].Start();
            }

            for (int i = 0; i < 20; ++i)
            {
                threads[i].Join();
            }

            //ASSERT
            Assert.AreEqual(0, exceptionCount);
            Assert.AreEqual(20000, callAttempts);
            Assert.IsTrue(successCallCount > 0);
            Assert.AreEqual(successCallCount, callCount);
            Assert.IsTrue(callCount < 20000);
        }

        private async Task DoTestAsync(IOperationGuard guard)
        {
            int check = 0;
            int callCount = 0;
            int callAttempts = 0;
            int successCallCount = 0;
            int exceptionCount = 0;

            Action action = () =>
            {
                Interlocked.Increment(ref callCount);

                if (Interlocked.Increment(ref check) != 1)
                    throw new Exception(check.ToString());

                Thread.Sleep(100);

                if (Interlocked.Decrement(ref check) != 0)
                    throw new Exception(check.ToString());
            };

            ThreadStart callAction = async () =>
            {
                for (int i = 0; i < 1000; ++i)
                {
                    Interlocked.Increment(ref callAttempts);

                    var res = await guard.ExecuteIfFreeAsync(() => Task.Run(action), ex => Interlocked.Increment(ref exceptionCount));

                    if (res)
                        successCallCount++;

                    Thread.Sleep(1);
                }
            };

            //ACT
            var threads = new Thread[20];

            for (int i = 0; i < 5; ++i)
            {
                threads[i] = new Thread(callAction);
                threads[i].Start();
            }

            var monitorResult = Task.Run(async () =>
                {
                    while (callAttempts != 5000)
                        await Task.Delay(500);
                }).Wait(30000);

            Assert.IsTrue(monitorResult);

            await Task.Delay(2000);

            //ASSERT
            Assert.AreEqual(0, exceptionCount);
            Assert.AreEqual(5000, callAttempts);
            Assert.IsTrue(successCallCount > 0);
            Assert.AreEqual(successCallCount, callCount);
            Assert.IsTrue(callCount < 5000);
        }

        [TestMethod]
        public void OperationGuard_InterlockedMultithread()
        {
            DoTest(new InterlockedGuard());
        }

        [TestMethod]
        public async Task OperationGuard_InterlockedAsync()
        {
            await DoTestAsync(new InterlockedGuard());
        }

        [TestMethod]
        public void OperationGuard_TimedMultithread()
        {
            DoTest(new TimedGuard(new TimeSpan(0,0,1)));
        }

        [TestMethod]
        public async Task OperationGuard_TimedAsync()
        {
            await DoTestAsync(new TimedGuard(new TimeSpan(0, 0, 1)));
        }

        [TestMethod]
        public void OperationGuard_ManualResetMultithread()
        {
            DoTest(new ManualResetGuard());
        }

        [TestMethod]
        public async Task OperationGuard_ManualResetAsync()
        {
            await DoTestAsync(new ManualResetGuard());
        }
    }
}
