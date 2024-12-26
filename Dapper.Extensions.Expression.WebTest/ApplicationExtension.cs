using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest;

/// <summary>
/// 应用程序扩展
/// </summary>
public static class ApplicationExtension
{
    /// <summary>
    /// 使用自定义错误处理
    /// </summary>
    /// <param name="app">程序构建器</param>
    public static void UseCustomErrors(this IApplicationBuilder app)
    {
        app.Use(WriteResponse);
    }

    /// <summary>
    /// 写入response
    /// </summary>
    /// <param name="httpContext">http上下文</param>
    /// <param name="next">下一个任务</param>
    /// <remarks>
    /// 在Mircroservices API设计中,为了让错误信息更具有描述性，采用Problem+json的方式输出错误。
    /// 在problem+json之前,这么返回错误：
    /// 采用Http Status Codes + Empty Response来返回错误数据:
    /// HTTP/1.1 400 Bad request
    /// Content-Type: application/json
    /// Response Body:
    /// problem+json中这么定义错误输出
    /// type: 提供一个描述问题的连接(required)
    /// title: 对错误做一个简短的描述(required)
    /// status: HTTP status code(required)
    /// detail: 详细描述错误信息(optional)
    /// instance: 返回错误产生的URL, 绝对地址(optional)
    /// </remarks>
    /// <returns></returns>
    private static async Task WriteResponse(HttpContext httpContext, Func<Task> next)
    {
        ILogger logger = null;
        ILoggerFactory loggerFactory = httpContext.RequestServices?.GetService<ILoggerFactory>();
        if (loggerFactory != null)
        {
            logger = loggerFactory.CreateLogger(typeof(ApplicationExtension));
        }
        IExceptionHandlerFeature exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
        Exception? ex = exceptionDetails?.Error;
        if (ex == null)
        {
            return;
        }
        const string title = "An error occured";
        ProblemDetails problem = new ProblemDetails
        {
            Type = "https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html",
            Title = title,
            Status = 500
        };
        string traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (traceId != null)
        {
            problem.Extensions["traceId"] = traceId;
        }
        logger?.LogError(ex, title);
        if (httpContext == null)
        {
            return;
        }
        httpContext.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(httpContext.Response.Body, problem);
    }
}
