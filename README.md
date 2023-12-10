# Comman.Dapper.Linq.Extension

### 可以在Release 中找到對應的版本

## (一)Model
可以使用Release 中[工具](https://github.com/tom213tw2/Comman.Dapper.Linq.Extension/releases/tag/工具)
```
using Comman.Dapper.Linq.Extension.Attributes;
using Comman.Dapper.Linq.Extension.Extension.From;

[Display(Rename = "Users", Schema = "dbo")]
public class Users : IBaseEntity<Users, Guid>
{
    [Identity(IsIncrease = false)] public override Guid Id { get; set; }

    [Display(Rename = "org_id")] public Guid OrgId { get; set; }

    [Display(Rename = "name")] public string Name { get; set; }

    [Display(Rename = "birthday")] public DateTime? Birthday { get; set; }

    [Display(Rename = "email")] public string Email { get; set; }

    [Display(Rename = "account")] public string Account { get; set; }

    [Display(Rename = "password")] public string Password { get; set; }

    [Display(Rename = "status")] public string Status { get; set; }

    [Display(Rename = "created_at")] public DateTime? CreatedAtDateTime { get; set; }

    [Display(Rename = "updated_at")] public DateTime? UpdatedAtDateTime { get; set; }
}
```

## (二)使用情境
**可通過```var conn = new SqlConnection(connectionString)```擴展方法**

### 查詢單一筆資料
```var users = conn.QuerySet<users>().Where(x => x.Name != "1").Get();```

```var users1 = conn.QuerySet<users>().Where(x => x.Name.Contains("Y")).Get();```

### 查詢一個範圍的資料
```
int[] array = new int[] { 1, 2, 3 };
//使用In
var comment = conn.QuerySet<Comment>().Where(x => x.Id.In(array)).ToList();
//或者使用Contains
var comment = conn.QuerySet<Comment>().Where(x => array.Contains(x.Id)).ToList();
```

### 使用Sql 查詢
```
DynamicParameters param = new DynamicParameters();
                param.Add("Id", 1);
var comment = conn.QuerySet<Comment>().Where("Id=@Id", param)
                    .ToList();
```


### 使用With(NOLOCK) 查詢，避免鎖表
```
var users = conn.QuerySet<Users>().WithNoLock().ToList();
```

### 範圍尋找
```
var comment = conn.QuerySet<Comment>().Where(x => x.Id.Between(1, 10)).ToList();
```


### 修改
```
var users=new users();         
users.name = Guid.NewGuid().ToString();
users.createDate = DateTime.Now;
int result = conn.CommandSet<users>().Where(x => x.id == 4).Update(users);
```

### 自定義修改
```
int result = conn.CommandSet<Comment>()
                    .Where(x => x.Content == "test")
                    .Update(x => new Comment
                    {
                        Content = "test1"
                    });
```

### 新增
```
int result = conn.CommandSet<users>()
                 .Insert(new users() {
                       code = Guid.NewGuid().ToString(),
                       name = "test", createWay = 1,
                       createDate = DateTime.Now,
                       roleId = 2
                 });
```

### 新增返回Id
```
int result = conn.CommandSet<users>()
                 .InsertIdentity(new users() {
                       code = Guid.NewGuid().ToString(),
                       name = "test", createWay = 1,
                       createDate = DateTime.Now,
                       roleId = 2
                 });
```


### 刪除
```
int result = conn.CommandSet<users>()
              .Where(x => x.roleId == 2 && x.name == users2.name)
              .Delete();
```

### 使用交易
```
using (var conn = new SqlConnection(mysqlConnection))
{
    // 首先開啟資料庫連接
    conn.Open();

    // 建立交易處理對象
    var transaction = conn.BeginTransaction();

    // 在交易中執行資料修改
    // 這裡以更新 Comment 表中 Id 為 1 的記錄為例
    var result = conn.CommandSet<Comment>(transaction)
        .Where(x => x.Id.Equals(1))
        .Update(x => new Comment()
        {
            Content = "test"
        });

    // 提交交易事務。若需要在交易發生Exception，使用 transaction.Rollback();
    transaction.Commit();
}
```

### Join 
```
var list = conn.QuerySet<users>()
           .Where(x => x.code != "1")
           .Join<users, project_Role>(x => x.roleId, y => y.id)
           .ToList();
```

### 任何條件 Join
```
var list = conn.QuerySet<users>()
           .Where(x => x.code != "1")
           .Join<users, project_Role>((x,y)=>x.roleId==y.id)
           .ToList();
```

### 設置Join 條件
```
var list = conn.QuerySet<users>()
           .Where(x => x.code != "1")
           .Join<users, project_Role>((x,y)=>x.roleId==y.id, JoinMode.LEFT)
           .ToList();
```

### Join 返回多筆型態
```
var list = conn.QuerySet<Users>()
               .WithNoLock()
               .Join<Users, Orgs>(s => s.Org_Id,t => t.Id,JoinMode.RIGHT)
               .From<Users, Orgs>()
               .ToList((x, y) => new
               {
                   x.Id,
                   x.Org_Id,
                   x.Name1234,
                   x.Birthday,
                   x.Email,
                   x.Account,
                   x.Password,
                   x.Status,
                   x.Created_at,
                   x.Updated_at,
                   OrgId = y.Id,
                   y.title,
                   y.org_no,
                   y.created_at,
                   y.updated_at
               });
```

### Join 多表條件塞選
```
var users = conn.QuerySet<users>()
                       .Join<users, project_Role>((a, b) => a.roleId == b.id)
                       .Where<users, project_Role>((a, b) => a.id == 3 && b.id == 3)
                       .Get<dynamic>();
```

### Join 支持動態型別(dynamic)
```
var list = conn.QuerySet<users>()
           .Where(x => x.code != "1")
           .Join<users, project_Role>(x => x.roleId, y => y.id)
           .ToList<dynamic>();
```

### 分頁查詢
```
var list = conn.QuerySet<users>()
           .OrderBy(x => x.createDate)
           .PageList(1, 10);
```

### Join 分頁查詢
```
var list = conn.QuerySet<Users>().Join<Users, Orgs>(s => s.Org_Id, t => t.Id).From<Users, Orgs>()
.OrderBy<Users>(s => s.Id).PageList(1, 10, (s, t) => new
{
    s.Id,
    s.Org_Id,
    s.Name1234,
    s.Birthday,
    s.Email,
    s.Account,
    s.Password,
    s.Status,
    s.Created_at,
    s.Updated_at,
    t.org_no
});
```

### 支持自定義查詢
```
var ContentList = conn.QuerySet<Comment>()
                 .ToList(x => new CommentDto()
                 {
                     Id = x.Id,
                     ArticleIds = x.ArticleId,
                     count = conn.QuerySet<News>().Where(y => y.Id == x.ArticleId).Count(),
                     NewsList = new QuerySet<News>().Where(y => y.Id == x.ArticleId).ToList(y => new NewsDto()
                     {
                         Id = y.Id,
                         Contents = y.Content
                     }).ToList()
                 });
```

### 支持分組查詢
```
var commne = conn.QuerySet<Comment>()
                    .Where(x => x.Id > 0 && array1.Contains(x.Id) && x.Content.Replace("1", "2") == x.Content)
                    .Where(x => x.Id.In(array1))
                    .GroupBy(x => new { x.ArticleId })
                    .Having(x => Function.Sum(x.Id) >= 5)
                    .ToList(x => new
                    {
                        x.ArticleId,
                        test1 = Function.Sum(x.Id)
                    });
```



