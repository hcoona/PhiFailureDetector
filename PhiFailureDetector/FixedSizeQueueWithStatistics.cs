using System;
using System.Collections;
using System.Collections.Generic;

namespace PhiFailureDetector
{
    public class FixedSizeQueueWithStatistics<T, S, V> : IEnumerable<T>, ICollection, IEnumerable
    {
        public delegate S AdditionOperator(S s, T t);
        public delegate V MultiplicationOperator(S s, int count);

        private readonly Queue<T> m_queue;
        private readonly int m_capacity;

        private readonly AdditionOperator m_addBy;
        private readonly AdditionOperator m_subtractBy;
        private readonly MultiplicationOperator m_divideBy;

        private S m_sum;
        private V m_avg;

        public FixedSizeQueueWithStatistics(int capacity,
            AdditionOperator addBy,
            AdditionOperator subtractBy,
            MultiplicationOperator divideBy)
        {
            this.m_capacity = capacity;
            this.m_queue = new Queue<T>(capacity);

            m_addBy = addBy;
            m_subtractBy = subtractBy;
            m_divideBy = divideBy;
        }

        public S Sum => m_sum;

        public V Avg => m_avg;

        public T Dequeue()
        {
            var value = m_queue.Dequeue();
            m_sum = m_subtractBy(m_sum, value);
            m_avg = m_divideBy(m_sum, Count);
            return value;
        }

        public void Enqueue(T item)
        {
            if (m_queue.Count == m_capacity)
            {
                var value = m_queue.Dequeue();
                m_sum = m_subtractBy(m_sum, value);
            }
            m_queue.Enqueue(item);
            m_sum = m_addBy(m_sum, item);
            m_avg = m_divideBy(m_sum, Count);
        }

        public void Clear() => this.m_queue.Clear();

        public bool Contains(T item) => m_queue.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => m_queue.CopyTo(array, arrayIndex);

        public T Peek() => m_queue.Peek();

        public T[] ToArray() => m_queue.ToArray();

        public int Count => this.m_queue.Count;

        public object SyncRoot => ((ICollection)this.m_queue).SyncRoot;

        public bool IsSynchronized => ((ICollection)this.m_queue).IsSynchronized;

        public void CopyTo(Array array, int index)
        {
            ((ICollection)this.m_queue).CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.m_queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this.m_queue).GetEnumerator();
        }
    }
}
