# sqlsugar封装库，包含雪花id创建器
1. 使用前需要添加配置，配置对象为 XTDbConfig （支持多库切换）
2. 使用时要初始化 
```
Services.AddSingleton(new AppSettings(builder.Environment.IsDevelopment()));
Services.AddXTDbSetup();
```

3. 使用方式
- 注入XTDbContext，通过SimpleClient来进行操作
- 继承ISugarHandler
```
 public interface IPatCellRepository: ISugarHandler<PatCell>, IDependencyRepository
    {
    }
```
- 继承SugarHandler
```
public class PatCellRepository : SugarHandler<PatCell>, IPatCellRepository
    {
        public PatCellRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
```
4. 数据库连接串
- Oracle:
```
DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.0.1)(PORT=21)))(CONNECT_DATA=(SERVICE_NAME=XX)));USER ID=XX;Password=XX
```
- PostgreSql:
```
Host=192.168.0.1 Port=20; Database=XX; Username=XX; Password=XX;
```
- Sqlite
```
DataSource=./test.db
```