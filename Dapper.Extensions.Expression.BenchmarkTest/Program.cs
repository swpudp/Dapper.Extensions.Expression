using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;

namespace Dapper.Extensions.Expression.BenchmarkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<GenerateCommandText>();
            Console.ReadLine();
        }
    }
}
