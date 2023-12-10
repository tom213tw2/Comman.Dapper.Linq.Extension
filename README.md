# Comman.Dapper.Linq.Extension


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



