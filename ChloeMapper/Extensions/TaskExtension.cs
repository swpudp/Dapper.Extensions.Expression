using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.Extensions
{
    public static class TaskExtension
    {
        public static TResult GetResult<TResult>(this Task<TResult> task)
        {
            return task.GetAwaiter().GetResult();
        }
        public static void GetResult(this Task task)
        {
            task.GetAwaiter().GetResult();
        }
    }
}
