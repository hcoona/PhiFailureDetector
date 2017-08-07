using System;
using System.Collections.Generic;
using System.Text;

namespace PhiFailureDetector
{
    public class PhiFailureDetector
    {
        public delegate double PhiFunc(long timestamp, long sum, double mean, IEnumerable<long> queue);

        private readonly FixedSizeQueueWithStatistics<long, long, double> m_queue;
        private readonly PhiFunc m_phiFunc;

        private long m_last;

        public PhiFailureDetector(int capacity, PhiFunc phiFunc)
        {
            m_queue = new FixedSizeQueueWithStatistics<long, long, double>(
                capacity,
                (s, v) => s + v,
                (s, v) => s - v,
                (s, c) => s / c
            );
            m_phiFunc = phiFunc;
        }

        public double Phi()
        {
            return m_phiFunc(DateTime.UtcNow.ToFileTimeUtc(), m_queue.Sum, m_queue.Avg, m_queue);
        }

        public void Report()
        {
            var now = DateTime.UtcNow.ToFileTimeUtc();
            m_queue.Enqueue(now);
            m_last = now;
        }

        /**
         * https://issues.apache.org/jira/browse/CASSANDRA-2597
         * Regular message transmissions experiencing typical random jitter will follow a normal distribution,
         * but since gossip messages from endpoint A to endpoint B are sent at random intervals,
         * they likely make up a Poisson process, making the exponential distribution appropriate.
         * 
         * P_later(t) = 1 - F(t)
         * P_later(t) = 1 - (1 - e^(-Lt))
         * 
         * The maximum likelihood estimation for the rate parameter L is given by 1/mean
         * 
         * P_later(t) = 1 - (1 - e^(-t/mean))
         * P_later(t) = e^(-t/mean)
         * phi(t) = -log10(P_later(t))
         * phi(t) = -log10(e^(-t/mean))
         * phi(t) = -log(e^(-t/mean)) / log(10)
         * phi(t) = (t/mean) / log(10)
         * phi(t) = 0.4342945 * t/mean
         */
        public static double Exponential(long nowTimestamp, long lastTimestamp, long sum, double mean, IEnumerable<long> queue)
        {
            var duration = nowTimestamp - lastTimestamp;
            return duration / mean;
        }

        /**
         * https://github.com/akka/akka/blob/master/akka-remote/src/main/scala/akka/remote/PhiAccrualFailureDetector.scala
         * Calculation of phi, derived from the Cumulative distribution function for
         * N(mean, stdDeviation) normal distribution, given by
         * 1.0 / (1.0 + math.exp(-y * (1.5976 + 0.070566 * y * y)))
         * where y = (x - mean) / standard_deviation
         * This is an approximation defined in β Mathematics Handbook (Logistic approximation).
         * Error is 0.00014 at +- 3.16
         * The calculated value is equivalent to -log10(1 - CDF(y))
         */
        public static double Normal(long nowTimestamp, long lastTimestamp, long sum, double mean, IEnumerable<long> queue)
        {
            var duration = nowTimestamp - lastTimestamp;
            var exp = Math.Exp(-duration * (1.5976 + 0.070566 * duration * duration));
            if (duration > mean)
            {
                return -Math.Log10(exp / (1 + exp));
            }
            else
            {
                return -Math.Log10(1 - 1 / (1 + exp));
            }
        }
    }
}
