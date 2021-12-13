using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectTemplate.Extensions
{
    public static class ListExtension
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) => list == null || !list.Any();

        public static void ParallelForAsync<T>(this IEnumerable<T> list, int maxDegreeOfParallelism, Func<T, Task> run)
        {
            Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, item => run(item).Wait());
        }
    }
}
