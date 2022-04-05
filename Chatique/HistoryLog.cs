using System;
using System.Collections.Generic;

namespace Chatique
{
    public static class HistoryLog<T>
    {
        static List<T> history;

        static HistoryLog()
        {
            history = new List<T>();
        }

        public static IEnumerable<T> History
        {
            get
            {
                foreach (T item in history)
                    yield return item;
            }
        }

        public static int HistoryCount() => history.Count;

        public static void AddMessageToHistory(T message) => history.Add(message);
    }
}
