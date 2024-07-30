using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.RegularExpressions;
using AutoFixture;
using System.Globalization;
using Chloe.Infrastructure;
using Snowflake.Core;
using Dapper.Extensions.Expression.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Dapper.Extensions.Expression.Queries;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _cache;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("QueryTest")]
        public IEnumerable<TestEntity> QueryTest()
        {
            using IDbConnection connection = CreateConnection();
            Query<TestEntity> query = connection.Query<TestEntity>();
            query = query.Where(v => v.TestName.Contains("FD2D"));
            IList<TestEntity> data = query.ToList<TestEntity>();
            return data;
        }

        [HttpGet("TestEntity")]
        public async Task<bool> CreateTestEntity()
        {
            TestEntity testEntity = new TestEntity
            {
                Id = Guid.NewGuid(),
                Code = "123",
                Ignore = "1234",
                IsDelete = false,
                Items = new List<Item> { new Item { Id = Guid.NewGuid() } },
                Logged = DateTime.Now,
                Message = "test",
                TestName = nameof(WeatherForecastController),
                Number = "test",
                Type = TestType.Log
            };
            IDbConnection connection = CreateConnection();
            int result = await connection.InsertAsync(testEntity);
            return result > 0;
        }

        [HttpGet("TransactionAsync")]
        public async Task<int> TransactionAsync()
        {
            using (var trans = new TransactionScope())
            {
                var buyers = Enumerable.Range(0, 10000).Select(f => CreateBuyer()).ToList();
                IDbConnection connection = CreateConnection();
                int result = await connection.InsertBulkAsync(buyers);
                trans.Complete();
                return result;
            }
        }

        [HttpGet("Transaction/{total}")]
        public int Transaction(int total)
        {
            List<BusinessDocument> data = EntityMock.CreateMany<BusinessDocument>(total).Select(RepositoryTestExtensions.Fix).ToList();
            using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5)))
            {
                IDbConnection connection = CreateConnection();
                int result = connection.InsertBulk(data);
                trans.Complete();
                return result;
            }
        }

        /// <summary>
        /// 测试发送mq消息
        /// </summary>
        /// <returns></returns>
        [HttpGet("BulkCreateAsyncTest/{total}")]
        public async Task<int> BulkCreateAsyncTest(int total)
        {
            List<BusinessDocument> data = EntityMock.CreateMany<BusinessDocument>(total).Select(RepositoryTestExtensions.Fix).ToList();
            using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(5)))
            {
                IDbConnection connection = CreateConnection();
                int result = await connection.InsertBulkAsync(data);
                trans.Complete();
                return result;
            }
            //var ctx = new MySqlContext(new MySqlConnectionFactory("server=127.0.0.1;port=3306;database=dapper_extension;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8"));
            //foreach (var item in data)
            //{
            //    await ctx.InsertAsync(item);
            //}
            //await ctx.InsertRangeAsync(data);
        }

        public class MySqlConnectionFactory : IDbConnectionFactory
        {
            string _connString = null;
            public MySqlConnectionFactory(string connString)
            {
                this._connString = connString;
            }
            public IDbConnection CreateConnection()
            {
                IDbConnection conn = new MySqlConnection(this._connString);
                return conn;

            }
        }

        private static Buyer CreateBuyer()
        {
            string id = Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper();
            Buyer buyer = new Buyer
            {
                Id = Guid.NewGuid(),
                Code = id.Substring(0, 4),
                CreateTime = DateTime.Now,
                Email = id.Substring(4, 8) + "@" + id.Substring(10, 2) + ".com",
                Identity = id.Substring(10, 10),
                IsActive = true,
                IsDelete = false,
                Mobile = "13900000000",
                Name = id.Substring(20, 4),
                Type = BuyerType.Company,
            };
            return buyer;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_extension;uid=root;pwd=g~zatvcWLfm]yTa;charset=utf8");
            //IDbConnection connection = new MySqlConnection("server=192.168.1.102;port=3306;database=dapper_extension;uid=root;pwd=Q1@we34r;charset=utf8");
            //IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=dapper_exp;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }

        [HttpGet("Upload")]
        public async Task<int> Upload()
        {
            using (var connection = CreateConnection())
            {
                Attachment attachment = new Attachment
                {
                    Id = new Guid("FE995DA8-32AA-4383-9D86-53247FD5F243"),
                    OrderId = Guid.NewGuid().ToString(),
                    Name = "123.pdf",
                    Extend = ".pdf"
                };
                return await connection.UniqueInsertAsync(attachment);
            }
        }

        [HttpGet("ErrorUpload")]
        public async Task<int> ErrorUpload()
        {
            using (var connection = CreateConnection())
            {
                Attachment attachment = new Attachment
                {
                    Id = new Guid("FE995DA8-32AA-4383-9D86-53247FD5F243"),
                    OrderId = Guid.NewGuid().ToString(),
                    Name = "123.pdf",
                    Extend = ".pdf"
                };
                return await connection.InsertAsync(attachment);
            }
        }

        [HttpGet("Cache")]
        public async Task<object> Cache()
        {
            _cache.SetString("set_string_1", Guid.NewGuid().ToString());
            await _cache.SetStringAsync("set_string_async_1", Guid.NewGuid().ToString());

            string v1 = _cache.GetString("set_string_1");
            string v2 = await _cache.GetStringAsync("set_string_async_1");

            _cache.Set("set_byte_1", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1) });
            await _cache.SetAsync("set_byte_async_1", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), new DistributedCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1) });

            byte[] v3b = _cache.Get("set_byte_1");
            string v3 = Encoding.UTF8.GetString(v3b);

            byte[] v4b = await _cache.GetAsync("set_byte_async_1");
            string v4 = Encoding.UTF8.GetString(v4b);

            return new { v1, v2, v3, v4 };
        }
    }

    [Table("attachment")]
    public class Attachment : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string OrderId { get; set; }

        public string Name { get; set; }

        public string Extend { get; set; }

        public int Version { get; set; }
    }

    [Table("testentity")]
    public class TestEntity : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Column("Name")]
        public string TestName { get; set; }

        public DateTime Logged { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string Message { get; set; }

        public string Code { get; set; }

        public string Number { get; set; }

        [Computed]
        public TestType Type { get; set; }

        [NotMapped]
        public string Ignore { get; set; }

        [NotMapped]
        public IList<Item> Items { get; set; }
    }

    [Table("buyer")]
    public class Buyer : IEntity
    {
        [Key] public Guid Id { get; set; }

        public string Name { get; set; }

        public BuyerType Type { get; set; }

        public string Code { get; set; }

        public string Identity { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public bool IsDelete { get; set; }

        public bool? IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
    }


    public enum BuyerType
    {
        Person,
        Company,
        Other
    }

    public interface IEntity
    {
        Guid Id { get; set; }
    }

    public enum TestType
    {
        Log,
        Trace
    }

    public class Item : IEntity
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    ///业务单据表
    /// </summary>
    /// <remarks>具有増、删、改、查功能</remarks>
    [Table("BusinessDocuments")]
    //[Chloe.Annotations.Table("BusinessDocuments")]
    public class BusinessDocument
    {
        /// <summary>
        /// Id
        /// </summary>
        [Key]
        //[Chloe.Annotations.Column(IsPrimaryKey = true)]
        public long Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 公司代码
        /// </summary>
        public string CorporationCode { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CorporationName { get; set; }

        /// <summary>
        /// 公司税号
        /// </summary>
        public string TaxpayerNumber { get; set; }

        /// <summary>
        /// 发票类型
        /// </summary>
        public Guid InvoiceKindId { get; set; }

        /// <summary>
        /// 发票介质
        /// </summary>
        public InvoiceClass InvoiceClass { get; set; }

        /// <summary>
        /// 发票联次
        /// </summary>
        public InvoiceForm? InvoiceForm { get; set; }

        /// <summary>
        /// 购买方税号
        /// </summary>
        public string BuyerTaxpayerNumber { get; set; }

        /// <summary>
        /// 购买方名称
        /// </summary>
        public string BuyerName { get; set; }

        /// <summary>
        /// 购买方地址
        /// </summary>
        public string BuyerAddress { get; set; }

        /// <summary>
        /// 购买方省份
        /// </summary>
        public string BuyerProvince { get; set; }

        /// <summary>
        /// 购买方电话
        /// </summary>
        public string BuyerTelephone { get; set; }

        /// <summary>
        /// 购买方手机
        /// </summary>
        public string BuyerMobile { get; set; }

        /// <summary>
        /// 购买方邮件
        /// </summary>
        public string BuyerEmail { get; set; }

        /// <summary>
        /// 购买方类型
        /// </summary>
        public CustomerType BuyerCompanyClass { get; set; } = CustomerType.Enterprise;

        /// <summary>
        /// 购买方银行及账号
        /// </summary>
        public string BuyerBankAccount { get; set; }

        /// <summary>
        /// 购买方微信OpenId
        /// </summary>
        public string BuyerOpenId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string BatchNumber { get; set; }

        /// <summary>
        /// 打印编号
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 销售订单编号
        /// </summary>
        public string SalesOrderNo { get; set; }

        /// <summary>
        /// 凭证/销售订单行号
        /// </summary>
        public int? OrderItem { get; set; }

        /// <summary>
        /// 订单创建日期
        /// </summary>
        public DateTime? OrderCreateTime { get; set; }

        /// <summary>
        /// 交货单号
        /// </summary>
        public string DeliveryOrderNo { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 含税单价
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 含税折扣金额
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// 含折扣税金
        /// </summary>
        public decimal Tax { get; set; }

        /// <summary>
        /// 含税含折扣金额
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// 折扣策略
        /// </summary>
        public TaxPolicy PayTaxPolicy { get; set; } = TaxPolicy.Normal;

        /// <summary>
        /// 结算单号
        /// </summary>
        public string SettledNumber { get; set; }

        /// <summary>
        /// 系统凭证号
        /// </summary>
        public string VoucherNumber { get; set; }

        /// <summary>
        /// 凭证状态
        /// </summary>
        public VoucherStatus VoucherStatus { get; set; }

        /// <summary>
        /// 原凭证号
        /// </summary>
        public string OriginVoucher { get; set; }

        /// <summary>
        /// 凭证日期
        /// </summary>
        public DateTime? VoucherDate { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderType AccountOrderType { get; set; }

        /// <summary>
        /// 是否借票
        /// </summary>
        public bool IsBorrow { get; set; }

        /// <summary>
        /// 是否核销
        /// </summary>
        public bool IsCredit { get; set; }

        /// <summary>
        /// 核销时间
        /// </summary>
        public DateTime? CreditTime { get; set; }

        /// <summary>
        /// 预计核销时间
        /// </summary>
        public DateTime? ExpectCreditTime { get; set; }

        /// <summary>
        /// 核销凭证号
        /// </summary>
        public string CreditMemo { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Creditor { get; set; }

        /// <summary>
        /// 借票联系人
        /// </summary>
        public string BorrowLink { get; set; }

        /// <summary>
        /// 借票税金凭证
        /// </summary>
        public string BorrowVoucher { get; set; }

        /// <summary>
        /// 借票申请单号
        /// </summary>
        public string BorrowReqNo { get; set; }

        /// <summary>
        /// 还票申请单号
        /// </summary>
        public string CreditReqNo { get; set; }

        /// <summary>
        /// 开票类型
        /// </summary>
        public InvoiceDirection OperationCode { get; set; }

        /// <summary>
        /// 来源代码
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// 数据获取方式
        /// </summary>
        public DataWay DataWay { get; set; }

        /// <summary>
        /// 2019-7-16 新增冻结字段
        /// </summary>
        public bool IsFreeze { get; set; }

        /// <summary>
        /// 是否编号
        /// </summary>
        public bool IsNumber { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 非开票数据标识
        /// </summary>
        public bool InValid { get; set; }

        /// <summary>
        /// 请求单Id
        /// </summary>
        public Guid? RequestId { get; set; }

        /// <summary>
        /// 购货方银行代码
        /// </summary>
        public string BuyerBankCode { get; set; }

        /// <summary>
        /// 税收分类编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 规格型号
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 物料代码
        /// </summary>
        public string BeforeMaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        public string BeforeMaterialName { get; set; }

        /// <summary>
        /// 物料长描述
        /// </summary>
        public string BeforeMaterialDesc { get; set; }

        /// <summary>
        /// 物料规格
        /// </summary>
        public string BeforeMaterialSpec { get; set; }

        /// <summary>
        /// 业务范围代码
        /// </summary>
        public string BusinessRangCode { get; set; }

        /// <summary>
        /// 业务范围Id
        /// </summary>
        public Guid? BusinessRangId { get; set; }

        /// <summary>
        /// 购买方代码
        /// </summary>
        public string BuyerCode { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        public string UnitsNumber { get; set; }

        /// <summary>
        /// 购买方银行名称
        /// </summary>
        public string BuyerBankName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 发票代码
        /// </summary>
        public string InvoiceCode { get; set; }

        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public DateTime? PrintDate { get; set; }

        /// <summary>
        /// 发票状态
        /// </summary>
        public InvoiceStatus PrintStatus { get; set; }

        /// <summary>
        /// 初始备注
        /// </summary>
        public string InitialRemark { get; set; }

        /// <summary>
        /// 红字通知单号
        /// </summary>
        public string RedRequisitionNo { get; set; }

        /// <summary>
        /// 原发票代码
        /// </summary>
        public string OldInvoiceCode { get; set; }

        /// <summary>
        /// 原发票号码
        /// </summary>
        public string OldInvoiceNumber { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public string PurchaseOrderNo { get; set; }

        /// <summary>
        /// 提货单号
        /// </summary>
        public string LadingNo { get; set; }

        /// <summary>
        /// 云单号
        /// </summary>
        public string YunCode { get; set; }

        /// <summary>
        /// 订单是否释放(当发票状态是红冲、作废、异常、失控、红蓝合并几种状态为已释放)
        /// </summary>
        public bool IsRelease { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 录入员
        /// </summary>
        public string Creator { get; set; }
    }

    /// <summary>
    /// 红字信息表开具配置
    /// </summary>
    public static class RequisitionConfig
    {
        /// <summary>
        /// 红字信息表开据方
        /// </summary>
        public static readonly ImmutableDictionary<string, string> RequisitionIssuers = new Dictionary<string, string>
        {
            ["KJ01"] = "销售方",
            ["KJ02"] = "购货方"
        }.ToImmutableDictionary();

        /// <summary>
        /// 对应蓝字销项税金情况
        /// </summary>
        public static readonly ImmutableDictionary<string, IDictionary<string, string>> BlueStatuses = new Dictionary<string, IDictionary<string, string>>
        {
            ["KJ01"] = new Dictionary<string, string> { ["YW01"] = "因开票有误购买方拒收的", ["YW02"] = "因开票有误等原因尚未交付的" },
            ["KJ02"] = new Dictionary<string, string> { ["DK02"] = "未抵扣-无法认证", ["DK05"] = "未抵扣-所购货物或劳务、服务不属于增值税扣税范围" }
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// 纳税策略(0:含税;1:免税;2:不征税)
    /// </summary>
    public enum TaxPolicy
    {
        /// <summary>
        /// 含税
        /// </summary>
        [Description("含税")] Normal = 0,

        /// <summary>
        /// 免税
        /// </summary>
        [Description("免税")] Free = 1,

        /// <summary>
        /// 不征税
        /// </summary>
        [Description("不征税")] NoTax = 2
    }

    /// <summary>
    /// 凭证状态
    /// </summary>
    public enum VoucherStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Normal = 0,

        /// <summary>
        /// 已冲销
        /// </summary>
        [Description("已冲销")]
        WriteOff = 1
    }

    /// <summary>
    /// 开票类型 蓝字发票-金额为正  红字发票-金额为负
    /// </summary>
    public enum InvoiceDirection
    {
        /// <summary>
        /// 蓝字发票（金额为正）
        /// </summary>
        [Description("蓝字发票")]
        BlueInvoice = 10,

        /// <summary>
        /// 红字发票（金额为负）
        /// </summary>
        [Description("红字发票")]
        RedInvoice = 22
    }

    /// <summary>
    /// 数据途径
    /// </summary>
    public enum DataWay
    {
        /// <summary>
        /// 手工录入
        /// </summary>
        [Description("手工")]
        Manual = 0,

        /// <summary>
        /// 对接接口
        /// </summary>
        [Description("接口")]
        Api = 1,

        /// <summary>
        /// 导入
        /// </summary>
        [Description("导入")]
        Import = 2
    }

    /// <summary>
    /// 商品名称组合模式
    /// </summary>
    public enum NameCombineMode
    {
        /// <summary>
        /// 物料名称+物料规格
        /// </summary>
        [Description("物料名称+物料规格")] NameAndSpec,

        /// <summary>
        /// 物料长描述+物料代码
        /// </summary>
        [Description("物料长描述+物料代码")] DescAndCode
    }

    /// <summary>
    /// 执行状态
    /// </summary>
    public enum ExecuteState
    {
        /// <summary>
        /// 空闲
        /// </summary>
        [Description("空闲")] Free = 0,

        /// <summary>
        /// 执行中
        /// </summary>
        [Description("执行中")] Executing = 1
    }

    /// <summary>
    /// 百望接口类型
    /// </summary>
    public enum ExecuteType
    {
        /// <summary>
        /// 专票获取（初始化）
        /// </summary>
        [Description("专票获取（初始化）")]
        InvoiceInit = 0,

        /// <summary>
        /// 专票获取（增量）
        /// </summary>
        [Description("专票获取（增量）")]
        InvoiceIncr = 1,

        /// <summary>
        /// 专票认证
        /// </summary>
        [Description("专票认证")]
        InvoiceDeduct = 2,

        /// <summary>
        /// 同步认证结果
        /// </summary>
        [Description("同步认证结果")]
        SyncDeductResult = 3
    }

    #region 客户

    /// <summary>
    /// 客户类型
    /// </summary>
    public enum CustomerType
    {
        /// <summary>
        /// 企业
        /// </summary>
        [Description("企业")] Enterprise = 0,

        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")] Personal = 1,

        /// <summary>
        /// 非企业性单位
        /// </summary>
        [Description("非企业性单位")] NotEnterprise = 2
    }

    #endregion

    /// <summary>
    /// 发票介质
    /// </summary>
    public enum InvoiceClass
    {
        /// <summary>
        /// 电子发票
        /// </summary>
        [Description("电子发票")]
        Electronic = 0,

        /// <summary>
        /// 纸质发票
        /// </summary>
        [Description("纸质发票")]
        Paper = 1
    }

    /// <summary>
    /// 发票分类
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>
        /// 普通发票
        /// </summary>
        [Description("普通发票")]
        PlainInvoice = 0,

        /// <summary>
        /// 专用发票
        /// </summary>
        [Description("专用发票")]
        ValueAdded = 1
    }

    /// <summary>
    /// 发票种类代码
    /// </summary>
    public static class InvoiceKindCode
    {
        /// <summary>
        /// 增值税专用发票
        /// </summary>
        [Description("增值税专用发票")]
        public const string SpecialInvoice = "001";

        /// <summary>
        /// 增值税电子普通发票
        /// </summary>
        [Description("增值税电子普通发票")]
        public const string EInvoice = "002";

        /// <summary>
        /// 增值税普通发票
        /// </summary>
        [Description("增值税普通发票")]
        public const string TaxInvoice = "003";

        /// <summary>
        /// 货运运输业增值税专用发票
        /// </summary>
        [Description("货运运输业增值税专用发票")]
        public const string Transportation = "006";

        /// <summary>
        /// 机动车销售统一发票
        /// </summary>
        [Description("机动车销售统一发票")]
        public const string VehicleSale = "007";

        /// <summary>
        /// 增值税普通发票(卷式)
        /// </summary>
        [Description("增值税普通发票(卷式)")]
        public const string Volume = "008";

        /// <summary>
        /// 二手车销售统一发票
        /// </summary>
        [Description("二手车销售统一发票")]
        public const string SecondHandCar = "009";

        /// <summary>
        /// 增值税电子普通发票(通行费)
        /// </summary>
        [Description("增值税电子普通发票(通行费)")]
        public const string Toll = "010";
    }

    /// <summary>
    /// 数据源类
    /// </summary>
    public static class InvoiceSource
    {
        /// <summary>
        /// 发票云
        /// </summary>
        [Description("发票管理平台")]
        public const string Bill = "001";

        /// <summary>
        /// R3系统
        /// </summary>
        [Description("R3系统")]
        public const string R3 = "002";

        /// <summary>
        /// USO系统
        /// </summary>
        [Description("USO系统")]
        public const string Uso = "003";

        /// <summary>
        /// Ofc系统
        /// </summary>
        [Description("OFC系统")]
        public const string Ofc = "004";

        /// <summary>
        /// 百望
        /// </summary>
        [Description("百望")]
        public const string BaiWang = "005";

        /// <summary>
        /// 财务公司
        /// </summary>
        [Description("财务公司")]
        public const string Finance = "010";

        /// <summary>
        /// 发票云移动开票
        /// </summary>
        [Description("移动开票")]
        public const string BillMobile = "011";

        /// <summary>
        /// srm系统
        /// </summary>
        [Description("SRM系统")]
        public const string Srm = "017";

        /// <summary>
        /// BRM系统
        /// </summary>
        [Description("BRM系统")]
        public const string Brm = "018";

        /// <summary>
        /// SPM系统
        /// </summary>
        [Description("SPM系统")]
        public const string Spm = "019";

        /// <summary>
        /// 开放接口
        /// </summary>
        [Description("开放接口")]
        public const string Public = "020";

        /// <summary>
        /// 天新燃气
        /// </summary>
        [Description("天新燃气")]
        public const string Tx = "021";

        /// <summary>
        /// 新SRM系统
        /// </summary>
        [Description("新SRM系统")]
        public const string NewSrm = "022";

        /// <summary>
        /// 手工录入
        /// </summary>
        [Description("手工录入")]
        public const string Manual = "026";

        /// <summary >
        /// 批量导入
        /// </summary>
        [Description("批量导入")]
        public const string BulkImport = "027";

        /// <summary >
        /// 专票获取
        /// </summary>
        [Description("专票获取")]
        public const string Incr = "028";

        /// <summary >
        /// 手工查验
        /// </summary>
        [Description("手工查验")]
        public const string Web = "029";

        /// <summary >
        /// 票速开
        /// </summary>
        [Description("票速开")]
        public const string MiniApp = "030";

    }

    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// 普通开票
        /// </summary>
        [Description("普通开票")]
        Ordinary = 0,

        /// <summary>
        /// 先开票
        /// </summary>
        [Description("先开票")]
        Borrow = 1,

        /// <summary>
        /// 拆分开票
        /// </summary>
        [Description("拆分开票")]
        Split = 2,

        /// <summary>
        /// 重开开票
        /// </summary>
        [Description("重开开票")]
        Reopen = 3,

        /// <summary>
        /// 红冲开票
        /// </summary>
        [Description("红冲开票")]
        RedOpen = 4
    }

    /// <summary>
    /// 推送方式标识
    /// </summary>
    public enum PushWay
    {
        /// <summary>
        /// 短信推送
        /// </summary>
        [Description("短信推送")] Sms = 0,

        /// <summary>
        /// Email推送
        /// </summary>
        [Description("Email推送")] Email = 1,

        /// <summary>
        /// 短信和Email推送
        /// </summary>
        [Description("短信和Email推送")] SmsAndEmail = 2,

        /// <summary>
        /// 不推送
        /// </summary>
        [Description("不推送")] NotPush = 3
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 公众查询
        /// </summary>
        [Description("公众查询")] Query = 1,

        /// <summary>
        /// 发票推送
        /// </summary>
        [Description("发票推送")] Push = 2,

        /// <summary>
        /// 手工签收
        /// </summary>
        [Description("手工签收")] ManualSign = 3
    }

    /// <summary>
    /// 接口类型标识
    /// </summary>
    public enum InterfaceType
    {
        /// <summary>
        /// 组件接口
        /// </summary>
        [Description("组件接口")] Ejb = 0,

        /// <summary>
        /// TXT接口
        /// </summary>
        [Description("TXT接口")] Txt = 1,

        /// <summary>
        /// XML接口
        /// </summary>
        [Description("XML接口")] Xml = 2
    }

    /// <summary>
    /// 开票结果文件类型
    /// </summary>
    public enum ResultFileType
    {
        /// <summary>
        /// TXT接口
        /// </summary>
        [Description("TXT")] Txt = 0,

        /// <summary>
        /// XML接口
        /// </summary>
        [Description("XML接")] Xml = 1
    }

    /// <summary>
    /// 硬件类型标识
    /// </summary>
    public enum HardwareClass
    {
        /// <summary>
        /// 金税盘
        /// </summary>
        [Description("金税盘")] Jsp = 0,

        /// <summary>
        /// 税控盘
        /// </summary>
        [Description("税控盘")] Skp = 1,

        /// <summary>
        /// 百望税控服务器
        /// </summary>
        [Description("税控服务器")] BaiWang = 2
    }

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// 添加
        /// </summary>
        [Description("添加")] Add = 1,

        /// <summary>
        /// 删除
        /// </summary>
        [Description("删除")] Delete = 2,

        /// <summary>
        /// 修改
        /// </summary>
        [Description("修改")] Update = 3,

        /// <summary>
        /// 推送
        /// </summary>
        [Description("推送")] Send = 4,

        /// <summary>
        ///接收
        /// </summary>
        [Description("接收")] Receive = 5,

        /// <summary>
        ///接口
        /// </summary>
        [Description("接口")] InterFace = 6,

        /// <summary>
        ///异常
        /// </summary>
        [Description("异常")] Exception = 99
    }

    /// <summary>
    /// 流程处理类型
    /// </summary>
    public enum FlowSign
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")] Initialize = 0,

        /// <summary>
        /// 流程中
        /// </summary>
        [Description("流程中")] Processing = 1,

        /// <summary>
        /// 流程结束
        /// </summary>
        [Description("流程结束")] Resolve = 2,

        /// <summary>
        /// 流程终止
        /// </summary>
        [Description("流程终止")] Termination = 3,

        /// <summary>
        /// 已打印
        /// </summary>
        [Description("已打印")] Printed = 4,

        /// <summary>
        /// 已驳回
        /// </summary>
        [Description("已驳回")] Refuse = 5,

        /// <summary>
        /// 已冻结
        /// </summary>
        [Description("已冻结")] Freeze = 6
    }

    /// <summary>
    /// 底账库-认证状态标识
    /// </summary>
    public enum AuthStatus
    {
        /// <summary>
        /// 未认证
        /// </summary>
        [Description("未认证")]
        UnCertified = 0,

        /// <summary >
        /// 勾选认证
        /// </summary>
        [Description("预勾选")]
        PreChecked = 1,

        /// <summary>
        /// 认证中
        /// </summary>
        [Description("认证中")]
        Certifying = 2,

        /// <summary >
        /// 已认证
        /// </summary>
        [Description("已认证")]
        Certified = 3,

        /// <summary>
        /// 认证失败
        /// </summary>
        [Description("认证失败")]
        CertifyFailed = 4
    }

    /// <summary>
    /// 认证结果
    /// </summary>
    public enum AuthResults
    {
        /// <summary>
        /// 任务执行中
        /// </summary>
        [Description("任务执行中")] Executing = 0,

        /// <summary >
        /// 成功
        /// </summary>
        [Description("成功")] Success = 1,

        /// <summary >
        /// 无此票
        /// </summary>
        [Description("无此票")] NonExistent = 2,

        /// <summary >
        /// 该票异常无法认证
        /// </summary>
        [Description("该票异常无法认证")] InvoiceException = 3,

        /// <summary >
        /// 该票已经认证
        /// </summary>
        [Description("该票已经认证")] Certified = 4,

        /// <summary >
        /// 该票已经逾期无法认证
        /// </summary>
        [Description("该票已经逾期无法认证")] Overdue = 5,

        /// <summary >
        /// 该票已经申请认证
        /// </summary>
        [Description("该票已经申请认证")] Applied = 6,

        /// <summary >
        /// 申请认证月份已过期
        /// </summary>
        [Description("申请认证月份已过期")] Outdated = 7,

        /// <summary >
        /// 其它异常
        /// </summary>
        [Description("其它异常")] OtherException = 8,

        /// <summary >
        /// 发票未到期(需切换税期)
        /// </summary>
        [Description("发票未到期(需切换税期)")] BeforeTaxPeriod = 9,

        /// <summary >
        /// 该票已作废
        /// </summary>
        [Description("该票已作废")] Canceled = 11,

        /// <summary >
        /// 该发票已红冲
        /// </summary>
        [Description("该发票已红冲")] RedFlushed = 12,

        /// <summary >
        /// 未到申报期
        /// </summary>
        [Description("未到申报期")] NotDeclarePeriod = 13,

        /// <summary >
        /// 该发票税号不匹配
        /// </summary>
        [Description("该发票税号不匹配")] TaxNotMatch = 14,

        /// <summary >
        /// 红字发票不可认证
        /// </summary>
        [Description("红字发票不可认证")] CreditNote = 15,

        /// <summary>
        /// 认证类型错误
        /// </summary>
        [Description("认证类型错误")] AuthTypeError = 16,

        /// <summary>
        /// 信用等级异常
        /// </summary>
        [Description("信用等级异常")] CreditException = 17,

        /// <summary>
        /// 登陆税局失败
        /// </summary>
        [Description("登陆税局失败")] LoginFail = 18,

        /// <summary>
        /// 有效税额异常（有效税额大于实际税额）
        /// </summary>
        [Description("有效税额异常")] TaxError = 20,

        /// <summary>
        /// 统计表申请已提交，导致此税号的勾选、统计、确认操作都已锁定
        /// </summary>
        [Description("当期已锁定")] TaxPeriodLocked = 21,

        /// <summary>
        /// 发票未勾选，如取消抵扣操作的发票未勾选
        /// </summary>
        [Description("发票未勾选（确认）")] UnChecked = 23,

        /// <summary>
        /// 取消操作不匹配，如抵扣的按不抵扣的取消
        /// </summary>
        [Description("取消操作不匹配")] CancelNotMatch = 24,

        /// <summary>
        /// 等待上次申请完成
        /// </summary>
        [Description("需等待上次申请完成")] WaitLastOp = 41,

        /// <summary>
        /// 勾选时间范围有误
        /// </summary>
        [Description("勾选时间范围有误")] ErrorTaxPeriod = 42,

        /// <summary>
        /// 错误
        /// </summary>
        [Description("错误")] Error = 99
    }

    /// <summary>
    /// 时间区间类型
    /// </summary>
    public enum PeriodType
    {
        /// <summary>
        /// 0-90天
        /// </summary>
        Quarter = 0,

        /// <summary>
        /// 90-180天
        /// </summary>
        HalfYear = 1,

        /// <summary>
        /// 180-360天
        /// </summary>
        WholeYear = 2,

        /// <summary>
        /// 360天以上
        /// </summary>
        OverYear = 3
    }

    /// <summary>
    /// 认证类型
    /// </summary>
    public enum CertificationType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        [Description("未知类型")]
        None = 0,

        /// <summary>
        /// 抵扣
        /// </summary>
        [Description("抵扣")]
        Deduction = 1,

        /// <summary>
        /// 出口退税
        /// </summary>
        [Description("出口退税")]
        ExportTaxRebate = 2,

        /// <summary>
        /// 代理出口退税
        /// </summary>
        [Description("代理出口退税")]
        ExportTaxRebateAgency = 3
    }

    /// <summary>
    /// 底账库-记账状态标识
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>
        /// 未记账
        /// </summary>
        [Description("未记账")] NoAccounting = 0,

        /// <summary>
        /// 部分记账
        /// </summary>
        [Description("部分记账")] Partial = 1,

        /// <summary >
        /// 已记账
        /// </summary>
        [Description("已记账")] Completed = 2
    }

    /// <summary>
    /// 处理状态
    /// </summary>
    public enum HandleStatus
    {
        /// <summary>
        /// 未处理
        /// </summary>
        [Description("未处理")]
        UnHandled,

        /// <summary>
        /// 已处理
        /// </summary>
        [Description("已处理")]
        Handled
    }

    /// <summary>
    /// 发票状态标识
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// 未开票
        /// </summary>
        [Description("未开票")] UnMakeOut = 0,

        /// <summary>
        /// 开票中
        /// </summary>
        [Description("开票中")] MakeOuting = 1,

        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")] Normal = 2,

        /// <summary>
        /// 红冲
        /// </summary>
        [Description("红冲")] RedInvoice = 3,

        /// <summary >
        /// 作废
        /// </summary>
        [Description("作废")] Canceled = 4,

        /// <summary >
        /// 失控
        /// </summary>
        [Description("失控")] OutOfControl = 5,

        /// <summary >
        /// 异常
        /// </summary>
        [Description("异常")] Exception = 6,

        /// <summary>
        /// 红蓝合并
        /// </summary>
        [Description("红蓝合并")] RedBlue = 7,

        /// <summary>
        /// 重复开票
        /// </summary>
        [Description("重复")] Duplicate = 8
    }

    /// <summary>
    /// 签收状态标识
    /// </summary>
    public enum SignStatus
    {
        /// <summary>
        /// 未签收
        /// </summary>
        [Description("未签收")] UnSign = 0,

        /// <summary >
        /// 已签收
        /// </summary>
        [Description("已签收")] Sign = 1,

        /// <summary>
        /// 已退换
        /// </summary>
        [Description("已退换")] Exchanged = 2
    }
    /// <summary>
    /// 还票方式
    /// </summary>
    public enum CreditWay
    {
        /// <summary>
        /// 系统对接开票
        /// </summary>
        [Description("系统对接开票")]
        Docking,

        /// <summary>
        /// 系统外开票
        /// </summary>
        [Description("系统外开票")]
        Outside
    }

    /// <summary>
    /// 推送状态
    /// </summary>
    public enum SendStatus
    {
        /// <summary>
        /// 未推送
        /// </summary>
        [Description("未推送")] UnSend = 0,

        /// <summary >
        /// 已推送
        /// </summary>
        [Description("已推送")] Send = 1
    }

    /// <summary>
    /// 归档类型
    /// </summary>
    public enum ArchiveType
    {
        /// <summary>
        /// 销项
        /// </summary>
        Sales = 0,

        /// <summary>
        /// 进项
        /// </summary>
        Purchase = 1
    }

    /// <summary>
    /// 税收分类编码模式
    /// </summary>
    public enum TaxCodeMode
    {
        /// <summary>
        /// 从接口传递
        /// </summary>
        [Description("接口传递")]
        Required = 0,

        /// <summary >
        /// 系统维护
        /// </summary>
        [Description("系统维护")]
        System = 1
    }

    /// <summary>
    /// 影像件识别状态
    /// </summary>
    public enum RecognizeStatus
    {
        /// <summary>
        /// 未识别
        /// </summary>
        [Description("未识别")] Unidentified = 0,

        /// <summary>
        /// 已识别
        /// </summary>
        [Description("已识别")] Success = 1,

        /// <summary>
        /// 识别失败
        /// </summary>
        [Description("识别失败")] Failure = 2,

        /// <summary>
        /// 重复识别
        /// </summary>
        [Description("重复识别")] Duplicate = 3
    }

    /// <summary>
    /// 验证状态
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success,

        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Failed
    }

    /// <summary>
    /// 接口配置类型
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// 开票授权
        /// </summary>
        [Description("开票")]
        Invoice = 0,

        /// <summary>
        /// 查询授权
        /// </summary>
        [Description("查询")]
        Query = 1
    }

    /// <summary>
    /// 发票薄状态
    /// </summary>
    public enum BookStatus
    {
        /// <summary>
        /// 状态
        /// </summary>
        [Description("正常")]
        Normal = 0,

        /// <summary>
        /// 已注销
        /// </summary>
        [Description("已注销")]
        Canceled = 1
    }

    /// <summary>
    /// 发票联次
    /// </summary>
    public enum InvoiceForm
    {
        /// <summary>
        /// 二联
        /// </summary>
        [Description("二联")]
        Two = 2,

        /// <summary>
        /// 三联
        /// </summary>
        [Description("三联")]
        Three = 3,

        /// <summary>
        /// 五联
        /// </summary>
        [Description("五联")]
        Five = 5,

        /// <summary>
        /// 六联
        /// </summary>
        [Description("六联")]
        Six = 6
    }

    /// <summary>
    /// 限额类型
    /// </summary>
    public enum QuotaType
    {
        /// <summary>
        /// 百
        /// </summary>
        [Description("百")]
        Hundred = 1,

        /// <summary>
        /// 千
        /// </summary>
        [Description("千")]
        Thousand = 2,

        /// <summary>
        /// 万
        /// </summary>
        [Description("万")]
        TenThousand = 3,

        /// <summary>
        /// 十万
        /// </summary>
        [Description("十万")]
        OneHundredThousand = 4,

        /// <summary>
        /// 百万
        /// </summary>
        [Description("百万")]
        Million = 5,

        /// <summary>
        /// 千万
        /// </summary>
        [Description("千万")]
        TenMillion = 6
    }

    /// <summary>
    /// 发票状态(发票资源管理)
    /// </summary>
    public enum ResourceStatus
    {
        /// <summary>
        /// 空白发票
        /// </summary>
        [Description("空白")]
        Empty = 0,

        /// <summary>
        /// 正常发票
        /// </summary>
        [Description("正常")]
        Used = 1,

        /// <summary>
        /// 已作废
        /// </summary>
        [Description("作废")]
        Invalid = 2,

        /// <summary>
        /// 缴销
        /// </summary>
        [Description("缴销")]
        Canceled = 3
    }

    /// <summary>
    /// 盘存类型
    /// </summary>
    public enum InventoryType
    {
        /// <summary>
        /// 盘存范围内
        /// </summary>
        Range,

        /// <summary>
        /// 盘存范围外
        /// </summary>
        OutRange
    }

    /// <summary>
    /// 发票领用类型
    /// </summary>
    public enum ApplyType
    {
        /// <summary>
        /// 领用
        /// </summary>
        [Description("领用")]
        Use = 0,

        /// <summary>
        /// 退回
        /// </summary>
        [Description("退回")]
        GiveBack = 1
    }

    /// <summary>
    /// 发票领用-状态
    /// </summary>
    public enum ApplyStatus
    {
        /// <summary >
        /// 申请中
        /// </summary>
        [Description("申请中")] Applying = 0,

        /// <summary >
        /// 已发放
        /// </summary>
        [Description("已发放")] Granted = 1,

        /// <summary >
        /// 已签收
        /// </summary>
        [Description("已签收")] Received = 2,

        /// <summary >
        /// 已拒绝
        /// </summary>
        [Description("已拒绝")] Rejected = 3,

        /// <summary >
        /// 已退回
        /// </summary>
        [Description("已退回")] Returned = 4,

        /// <summary >
        /// 已撤回
        /// </summary>
        [Description("已撤回")] Revoked = 5
    }

    /// <summary>
    /// 拒绝类型
    /// </summary>
    public enum RejectType
    {
        /// <summary>
        /// 发放
        /// </summary>
        Approve,

        /// <summary>
        /// 退回
        /// </summary>
        Back,

        /// <summary>
        /// 签收
        /// </summary>
        Receive
    }

    /// <summary>
    /// 盘存-状态
    /// </summary>
    public enum InventoryStatus
    {
        /// <summary >
        /// 草稿
        /// </summary>
        [Description("草稿")] Draft = 0,

        /// <summary >
        /// 已完成
        /// </summary>
        [Description("已完成")] Completed = 1
    }

    internal static class EntityMock
    {


        public static T CreateOne<T>()
        {
            Fixture fixture = new Fixture();
            return fixture.Build<T>().Create();
        }

        public static IList<T> CreateMany<T>(int count = 5)
        {
            Fixture fixture = new Fixture();
            return fixture.Build<T>().CreateMany(count).ToList();
        }

        public static IList<Guid> CreateManyGuid()
        {
            return Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        }
    }

    internal static class RepositoryTestExtensions
    {
        private static IdWorker worker = new IdWorker(1, 1);
        private static Random rd = new Random();

        public static BusinessDocument Fix(this BusinessDocument f)
        {
            f.Id = worker.NextId();
            f.CorporationCode = f.CorporationCode.LeftSubString(20);
            f.TaxpayerNumber = f.TaxpayerNumber.LeftSubString(20);
            f.BuyerTaxpayerNumber = f.BuyerTaxpayerNumber.LeftSubString(20);
            f.BuyerProvince = f.BuyerProvince.LeftSubString(20);
            f.BatchNumber = f.BatchNumber.LeftSubString(20);
            f.SalesOrderNo = f.SalesOrderNo.LeftSubString(20);
            f.DeliveryOrderNo = f.DeliveryOrderNo.LeftSubString(20);
            f.MaterialCode = f.MaterialCode.LeftSubString(20);
            f.VoucherNumber = f.VoucherNumber.LeftSubString(12);
            f.SourceCode = f.SourceCode.LeftSubString(8);
            f.BuyerCode = f.BuyerCode.LeftSubString(20);
            f.UnitsNumber = f.UnitsNumber.LeftSubString(20);
            f.LadingNo = f.LadingNo.LeftSubString(20);
            f.OldInvoiceCode = f.OldInvoiceCode.LeftSubString(20);
            f.OldInvoiceNumber = f.OldInvoiceNumber.LeftSubString(20);
            f.InvoiceCode = f.OldInvoiceCode.LeftSubString(12);
            f.InvoiceNumber = f.OldInvoiceNumber.LeftSubString(12);
            f.TenantId = f.TenantId.LeftSubString(20);
            f.SettledNumber = f.SettledNumber.LeftSubString(30);
            f.OriginVoucher = f.SettledNumber.LeftSubString(30);
            f.BeforeMaterialCode = f.BeforeMaterialCode = f.BeforeMaterialCode.LeftSubString(50);
            f.BeforeMaterialDesc = f.BeforeMaterialDesc = f.BeforeMaterialDesc.LeftSubString(50);
            f.BeforeMaterialSpec = f.BeforeMaterialSpec = f.BeforeMaterialSpec.LeftSubString(50);
            f.PurchaseOrderNo = f.PurchaseOrderNo.LeftSubString(20);
            f.Creator = f.Creator.LeftSubString(10);
            f.RequestId = null;
            f.InvoiceKindId = Guid.Parse("6D5693F2-2015-41EA-B827-2D2F006030EA");
            f.Tax = rd.Next(0, 1);
            f.TaxAmount = rd.Next(10, 100);
            f.Amount = rd.Next(10, 100);
            f.Quantity = rd.Next(10, 100);
            f.DiscountAmount = rd.Next(10, 100);
            return f;
        }
    }

    /// <summary>字符串的扩展功能</summary>
    public static class StringExtensions
    {
        /// <summary>左取字符串</summary>
        /// <param name="inStr">输入字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns>输出字符串</returns>
        public static string LeftSubString(this string inStr, int length)
        {
            if (string.IsNullOrWhiteSpace(inStr))
            {
                return string.Empty;
            }
            return inStr.Length <= length ? inStr : inStr.Substring(0, length);
        }

        /// <summary>右取字符串</summary>
        /// <param name="inStr">输入字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns>输出字符串</returns>
        public static string RightSubString(this string inStr, int length)
        {
            if (string.IsNullOrWhiteSpace(inStr))
            {
                return string.Empty;
            }
            return inStr.Length <= length ? inStr : inStr.Substring(inStr.Length - length, length);
        }

        /// <summary>去掉指定的前导字符</summary>
        /// <param name="str">字符串</param>
        /// <param name="delChar">前导字符</param>
        public static string RemoveLeadingString(this string str, char delChar)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : str.TrimStart(delChar);
        }

        /// <summary>从ERP日期字符串转换为日期</summary>
        /// <param name="str">ERP日期字符串</param>
        /// <param name="defaultDate">默认日期</param>
        public static DateTime ErpStringToDate(this string str, DateTime defaultDate)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return defaultDate;
            }
            if (str.Trim().Length == 8)
            {
                str = str.Trim();
            }
            if (str.Trim().Length == 10)
            {
                str = $"{str.Substring(0, 4)}{str.Substring(5, 2)}{str.Substring(8, 2)}";
            }
            return DateTime.TryParseExact(str, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateTime) ? dateTime : defaultDate;
        }

        /// <summary>检查是否为邮件地址格式字符串</summary>
        /// <param name="emailAddress">邮件地址</param>
        /// <returns>是合法的邮件地址</returns>
        public static bool IsEmail(this string emailAddress)
        {
            return !string.IsNullOrWhiteSpace(emailAddress) && Regex.IsMatch(emailAddress, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }

        /// <summary>检查是否为邮件地址格式字符串</summary>
        /// <param name="mobile">邮件地址</param>
        /// <returns>是合法的邮件地址</returns>
        public static bool IsMobile(this string mobile)
        {
            return !string.IsNullOrWhiteSpace(mobile) && Regex.IsMatch(mobile.Trim(), @"^1[3456789]\d{9}$");
        }

        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// 判断字符串长度
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int GetLength(this string input)
        {
            return string.IsNullOrWhiteSpace(input) ? 0 : input.Length;
        }

        /// <summary>
        /// 获取字符串长度
        /// </summary>
        /// <returns></returns>
        public static int StringLength(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? 0 : value.Aggregate(0, Aggregate);
        }

        /// <summary>
        /// 计算字符串长度
        /// </summary>
        private static Func<int, char, int> Aggregate => (c, t) => t > 127 ? c + 2 : c + 1;

        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="content">截取内容</param>
        /// <param name="specificationStart">截取起始位置</param>
        /// <param name="specLength">最大长度</param>
        /// <param name="actual">本次截取长度</param>
        /// <returns></returns>
        public static string CutContent(this string content, int specificationStart, int specLength, out int actual)
        {
            return CutString(content, specificationStart, specLength, out actual);
        }

        /// <summary>
        /// 截取指定长度的字符串（规格型号）
        /// </summary>
        /// <param name="str">要截取的字符串</param>
        /// <param name="start">起始位置</param>
        /// <param name="len">要截取的长度</param>
        /// <param name="actual">实际截取的长度</param>
        /// <returns></returns>
        private static string CutString(string str, int start, int len, out int actual)
        {
            actual = 0;
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            int cutLen = 0;
            foreach (char c in str.Skip(start))
            {
                cutLen += 1;
                //遇到中文，长度再加一
                if (c > 127)
                {
                    cutLen += 1;
                }
                //如果截取长度大于目标长度，则不截取后一个字符串
                if (cutLen > len)
                {
                    break;
                }
                actual++;
            }
            return str.Substring(start, actual);
        }

        /// <summary>
        /// 判断是否是url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsUrl(this string url)
        {
            if (url.IsEmpty())
            {
                return false;
            }
            var regUrl = new Regex("^(ht|f)tp(s?)\\:\\/\\/[0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*(:(0-9)*)*(\\/?)([a-zA-Z0-9\\-\\.\\?\\,\'\\/\\\\+&amp;%\\$#_]*)?$");
            return regUrl.IsMatch(url);
        }

        /// <summary>
        /// 获取数字的汉字大写
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetCnsCapital(this decimal number)
        {
            string x = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            string y = Regex.Replace(x, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            string chs = Regex.Replace(y, ".", m => "负圆空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - '-'].ToString(CultureInfo.InvariantCulture));
            if (!chs.Contains("角"))
            {
                chs += "整";
            }
            if (number < 0)
            {
                chs = "(负数)" + chs;
            }
            return chs;
        }

        /// <summary>
        /// 换行符
        /// </summary>
        private static readonly char[] LinesChars = { '\r', '\n' };

        /// <summary>
        /// 去掉末尾换行符
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string TrimNewlines(this string content)
        {
            return content.IsEmpty() ? content : content.TrimEnd(LinesChars);
        }
    }
}
