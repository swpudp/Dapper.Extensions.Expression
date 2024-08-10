# Dapper.Extensions.Expression
Dapper.Extensions.Expression is a lightweight Object/Relational Mapping(ORM) library.
The query interface is similar to LINQ. You can query data like LINQ and do any things(Join Query | Group Query | Aggregate Query | Insert | Batch Update | Batch Delete) by lambda with Dapper.Extensions.Expression.

# NuGet Install Command

|     Database         | Install Command  |
| ------------ | --------------- |
| Dapper.Extensions.Expression  | Install-Package Dapper.Extensions.Expression  |

# License
[MIT](http://opensource.org/licenses/MIT) License

# Usage
* **Entity**
```C#
public class QueryParam
{
    public DateTime? CreateTime { get; set; }

    public bool? IsDelete { get; set; }

    public string Key { get; set; }
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

    public int Version { get; set; }
}

[Table("items")]
public class Item : IEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public int Index { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }

    public decimal? Discount { get; set; }

    public decimal Amount { get; set; }

    public string Unit { get; set; }

    public int Version { get; set; }
}

[Table("order")]
public class Order : IEntity
{
    [Key] public Guid Id { get; set; }

    public Guid BuyerId { get; set; }

    [Column("Number")]
    public string SerialNo { get; set; }

    public string Remark { get; set; }

    public Status Status { get; set; }

    public SignState? SignState { get; set; }

    public decimal Amount { get; set; }
    public decimal? Freight { get; set; }

    public Guid? DocId { get; set; }

    public bool IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public bool IsEnable { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    [Computed]
    public int Index { get; set; }

    [NotMapped]
    public LogType Type { get; set; }

    [NotMapped]
    public string Ignore { get; set; }

    public int Version { get; set; }
}

public interface IEntity
{
    Guid Id { get; set; }

    int Version { get; set; }
}

public enum LogType
{
    Log,
    Trace
}

public enum Status
{
    Draft,
    Running,
    Stop
}

public enum SignState
{
    UnSign,
    Signed
}

public enum BuyerType
{
    Person,
    Company,
    Other
}
```
* **Query**
```C#
IDbConnection connection = new MsSqlConnection("ConnectionString");
Query<Order> query = connection.Query<Order>();
```
* **Query**
```C#
using IDbConnection connection = new MsSqlConnection("ConnectionString");
Query<Order> query = connection.Query<Order>();
query.Where(v => v.Remark.Contains("FD2"));
List<Order> data = query.ToList<Order>();
```
* **Join Query**
```C#
using IDbConnection connection = new MsSqlConnection("ConnectionString");
JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
query.Where((v, w) => v.SerialNo.Contains("FD2"));
IEnumerable<Order> data = query.ToList<Order>();
```
* **Group Query**
```C#
using IDbConnection connection = new MsSqlConnection("ConnectionString");
JoinQuery<Order, Item> query = connection.JoinQuery<Order, Item>().On(JoinType.Left, (a, b) => a.Id == b.OrderId);
List<Order> result = await query.Select((a, b) => new { a.CreateTime, Total = Function.Count() }).GroupBy((f, g) => f.CreateTime).Having((f, d) => f.CreateTime > new DateTime(2021, 3, 10)).ToListAsync<Order>();
```
* **Insert**
```C#
IDbConnection connection = new MsSqlConnection("ConnectionString");
Buyer buyer = CreateBuyer();
connection.Insert(buyer)
```
* **Update**
```C#
IDbConnection connection = new MsSqlConnection("ConnectionString");
 Order order = connection.Get<Order>(f => f.Id == new Guid("000c70b3-fccc-4838-a524-9b7edc4f9c9a"));
 order.UpdateTime = DateTime.Now;
 order.Amount = Convert.ToDecimal(new Random().NextDouble() * 100);
 int updated = connection.Update(order);
```
* **Delete**
```C#
IDbConnection connection = new MsSqlConnection("ConnectionString");
connection.Delete<Order>(x => x.IsDelete)
```
