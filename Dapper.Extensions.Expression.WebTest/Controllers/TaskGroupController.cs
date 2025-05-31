using Dapper.Extensions.Expression.Queries;
using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class TaskGroupController(IConfiguration configuration) : ControllerBase
    {

        private readonly IConfiguration _configuration = configuration;

        [HttpGet("{id}")]
        public async Task<TaskGroup> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<TaskGroup>().Where(f => f.Id == id).FirstOrDefaultAsync<TaskGroup>();
        }

        /// <summary>
        /// 新增任务
        /// </summary>
        /// <param name="taskGroup">任务对象</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TaskGroup> Create([FromBody] TaskGroup taskGroup)
        {
            taskGroup.Id=ObjectId.GenerateNewId().ToString();
            taskGroup.CreateTime =DateTime.Now;
            taskGroup.Version =1;
            using IDbConnection connection = CreateConnection();
            await connection.InsertAsync(taskGroup);
            return taskGroup;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="taskGroupQuery">查询参数</param>
        /// <returns>分页列表</returns>
        [HttpPost("paging")]
        public async Task<Paging<TaskGroup>> PagingList([FromBody] TaskGroupQuery taskGroupQuery)
        {
            using IDbConnection connection = CreateConnection();
            Query<TaskGroup> query = connection.Query<TaskGroup>()
                 .WhereIf(!string.IsNullOrEmpty(taskGroupQuery.Name), f => f.Name.Contains(taskGroupQuery.Name))
                 .WhereIf(taskGroupQuery.CreateTimeStart.HasValue, f => f.CreateTime>=taskGroupQuery.CreateTimeStart)
                 .WhereIf(taskGroupQuery.CreateTimeEnd.HasValue, f => f.CreateTime<taskGroupQuery.CreateTimeEnd);
            Paging<TaskGroup> paging = new Paging<TaskGroup>();
            int total = await query.CountAsync();
            paging.Total = total;
            if (total==0)
            {
                return paging;
            }
            paging.Data = await query.TakePage(taskGroupQuery.Index, taskGroupQuery.Size).ToListAsync<TaskGroup>();
            paging.Index     = taskGroupQuery.Index;
            paging.Size = taskGroupQuery.Size;
            return paging;
        }

        private IDbConnection CreateConnection()
        {
            string connectionString = _configuration.GetConnectionString("Default");
            IDbConnection connection = new MySqlConnection(connectionString);
            return connection;
        }
    }
}

