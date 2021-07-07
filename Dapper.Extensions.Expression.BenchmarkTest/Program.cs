using System;
using System.Reflection;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;

namespace Dapper.Extensions.Expression.BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
        }
    }
}
