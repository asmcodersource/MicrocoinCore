using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.MockNetworkTesting
{
    internal class Helpers
    {
        public static async Task RunWithTimeout(Action testAction, int timeoutMilliseconds, string timeoutUserMessage)
        {
            var communicationTask = Task.Run(testAction);
            var timeoutTask = Task.Delay(timeoutMilliseconds);
            var finishedTask = await Task.WhenAny(timeoutTask, communicationTask);
            Assert.True(finishedTask == communicationTask, timeoutUserMessage);
        }

        public static async Task RunWithTimeout(Func<Task> testAction, int timeoutMilliseconds, string timeoutUserMessage)
        {
            var communicationTask = testAction.Invoke();
            var timeoutTask = Task.Delay(timeoutMilliseconds);
            var finishedTask = await Task.WhenAny(timeoutTask, communicationTask);
            Assert.True(finishedTask == communicationTask, timeoutUserMessage);
        }
    }
}
