using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Utilities
{
    public static class OperationExtensions
    {
        static readonly Func<int, int, int> Sum = (a, b) => a + b;

        public static int CustomSum(this int a, int b)
        {
            return Sum(a, b);
        }
    }
}
