using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhiFailureDetector.UnitTest
{
    [TestClass]
    public class FixedSizeQueueWithStatistics_UnitTest
    {
        [TestMethod]
        public void Test_Baseline()
        {
            var queue = new FixedSizeQueueWithStatistics<int, long, double>(5, (s, v) => s + v, (s, v) => s - v, (s, c) => s / c);

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Enqueue(4);
            queue.Enqueue(5);
            queue.Enqueue(6);
            queue.Enqueue(7);

            var queueArray = queue.ToArray();
            Array.Sort(queueArray);

            CollectionAssert.AreEqual(
                new[] { 3, 4, 5, 6, 7 },
                queueArray
            );

            Assert.AreEqual(25, queue.Sum);
            Assert.AreEqual(5, queue.Avg);
        }
    }
}
