using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DoTaskController : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<DoTask> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<DoTask>().Where(f => f.Id == id).FirstOrDefaultAsync<DoTask>();
        }

        /// <summary>
        /// 新增任务
        /// </summary>
        /// <param name="doTask">任务对象</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<DoTask> Create([FromBody] DoTask doTask)
        {
            doTask.Id=ObjectId.GenerateNewId().ToString();
            doTask.CreateTime =DateTime.Now;
            doTask.Version =1;
            using IDbConnection connection = CreateConnection();
            await connection.InsertAsync(doTask);
            return doTask;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="doTaskQuery">查询参数</param>
        /// <returns>分页列表</returns>
        [HttpPost("paging")]
        public async Task<Paging<DoTask>> PagingList([FromBody] DoTaskQuery doTaskQuery)
        {
            using IDbConnection connection = CreateConnection();
            Query<DoTask> query = connection.Query<DoTask>()
                 .WhereIf(!string.IsNullOrEmpty(doTaskQuery.Name), f => f.Name.Contains(doTaskQuery.Name))
                 .WhereIf(doTaskQuery.TaskType!=null, f => f.TaskType==doTaskQuery.TaskType)
                 .WhereIf(doTaskQuery.CreateTimeStart.HasValue, f => f.CreateTime>=doTaskQuery.CreateTimeStart)
                 .WhereIf(doTaskQuery.CreateTimeEnd.HasValue, f => f.CreateTime<doTaskQuery.CreateTimeEnd);
            Paging<DoTask> paging = new Paging<DoTask>();
            int total = await query.CountAsync();
            paging.Total = total;
            if (total==0)
            {
                return paging;
            }
            paging.Data = await query.TakePage(doTaskQuery.Index, doTaskQuery.Size).ToListAsync<DoTask>();
            paging.Index     = doTaskQuery.Index;
            paging.Size = doTaskQuery.Size;
            return paging;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

