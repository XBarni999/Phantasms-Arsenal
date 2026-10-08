using System.Collections.Generic;

namespace PhantasmsArsenal.ITGT
{
    internal sealed class DesignationCycle<T> where T : class
    {
        readonly Dictionary<string, T> next = new Dictionary<string, T>();
        internal T Current(string key, IList<T> marks)
        {
            if (marks.Count == 0) return null;
            return next.TryGetValue(key, out var value) && marks.Contains(value) ? value : marks[0];
        }
        internal void Advance(string key, IList<T> marks, T captured)
        {
            if (marks.Count == 0 || Current(key, marks) != captured) return;
            next[key] = marks[(marks.IndexOf(captured) + 1) % marks.Count];
        }
        internal void Remove(T removed, IList<T> marks)
        {
            int index = marks.IndexOf(removed);
            if (index < 0) return;
            foreach (var key in new List<string>(next.Keys))
                if (next[key] == removed)
                {
                    if (marks.Count > 1) next[key] = marks[(index + 1) % marks.Count];
                    else next.Remove(key);
                }
        }
        internal void Reset(string key) => next.Remove(key);
        internal void Clear() => next.Clear();
    }
}
