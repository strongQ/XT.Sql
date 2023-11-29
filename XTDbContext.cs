
using Mapster;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XT.Common.Config;
using XT.Common.Extensions;
using XT.Sql.Enums;
using XT.Sql.Interfaces;
using XT.Sql.Models;

namespace XT.Sql
{
    public class XTDbContext
    {
        public static event EventHandler<string> AopSqlEvent;
        private static List<DataBaseOperate> ConnectObjects => GetCurrentConnectionDb();

        /// <summary>
        /// sql事件
        /// </summary>
        /// <param name="sql"></param>
        public static void LogSql(string sql)
        {
            AopSqlEvent?.Invoke(null, sql);
        }

        /// <summary>
        /// 连接字符串 
        /// </summary>
        public static DataBaseOperate MasterDbConfig { get; set; } = ConnectObjects.FirstOrDefault();

        /// <summary>
        /// 数据库类型 
        /// </summary>
        public static DataBaseType? DbType { get; set; } = ConnectObjects.FirstOrDefault()?.DbType;


        /// <summary>
        /// 数据连接对象 
        /// </summary>
        private SqlSugarScope _db;

        public SqlSugarScope Db
        {
            get => _db;
            private set => _db = value;
        }

        public XTDbContext(ISqlSugarClient sqlSugarClient)
        {
            if (XTDbContext.MasterDbConfig==null)
                throw new ArgumentNullException("sqlSugarClient", "数据库连接字符串为空");

            _db = sqlSugarClient as SqlSugarScope;

        }

        /// <summary>
        /// 当前数据库连接字符串 
        /// </summary>
        private static List<DataBaseOperate> GetCurrentConnectionDb()
        {
            return BaseDbConfig.GetDataBaseOperate.MasterDb;
        }

        #region 实例方法

        /// <summary>
        /// 获取数据库处理对象
        /// </summary>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDb<T>() where T : class, new()
        {
            return new SimpleClient<T>(_db);

            
        }

        /// <summary>
        /// 获取数据库处理对象
        /// </summary>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDb<T>(string connID) where T : class, new()
        {
            var provider= _db.GetConnectionScope(connID);
           return provider.GetSimpleClient<T>();
        }

        /// <summary>
        /// 获取数据库处理对象
        /// </summary>
        /// <param name="db">db</param>
        /// <returns>返回值</returns>
        public SimpleClient<T> GetEntityDb<T>(SqlSugarClient db) where T : class, new()
        {
            return new SimpleClient<T>(db);
        }

        #endregion


        #region 根据实体类生成数据库表

        /// <summary>
        /// 根据实体类生成数据库表
        /// </summary>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="entityList">指定的实体</param>
        public void CreateTableByEntity<T>(bool blnBackupTable, params T[] entityList) where T : class, new()
        {
            Type[] lstTypes = null;
            if (entityList != null)
            {
                lstTypes = new Type[entityList.Length];
                for (int i = 0; i < entityList.Length; i++)
                {
                    lstTypes[i] = typeof(T);
                }
            }

            CreateTableByEntity(blnBackupTable, lstTypes);
        }

        /// <summary>
        /// 根据实体类生成数据库表
        /// </summary>
        /// <param name="blnBackupTable">是否备份表</param>
        /// <param name="entityList">指定的实体</param>
        private void CreateTableByEntity(bool blnBackupTable, params Type[] entityList)
        {
            if (blnBackupTable)
            {
                Db.CodeFirst.BackupTable().InitTables(entityList); //change entity backupTable            
            }
            else
            {
                Db.CodeFirst.InitTables(entityList);
            }
        }


        /// <summary>
        /// 自动生成表
        /// </summary>
        /// <param name="entityAssemblys">entity所在程序集名称</param>
        /// <param name="connid">数据库标识</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task InitSystemDataAsync(List<string> entityAssemblys,string connid="")
        {
            try
            {
                var db = Db.GetConnectionScope(MasterDbConfig.ConnId);
                if (connid.IsNotNullOrEmpty())
                {
                    db=Db.GetConnectionScope(connid);
                }
                //实体
                var assemblys = entityAssemblys.Select(x => Assembly.Load(x)).ToList();
                List<Type> entityTypes = new List<Type>();
                assemblys.ForEach(entityAssembly => { entityTypes.AddRange(entityAssembly.GetTypes()); });
               

                Console.WriteLine($"程序正在启动....", ConsoleColor.Green);
                Console.WriteLine($"是否开发环境: {AppSettings.IsDevelopment}");
                
               
                Console.WriteLine($"DB Type: {DbType}");
                Console.WriteLine($"DB ConnectString: {MasterDbConfig.ConnectionString}");
                Console.WriteLine("初始化数据库....");
                if (DbType != DataBaseType.Oracle)
                {
                    Db.DbMaintenance.CreateDatabase();
                }

                Console.WriteLine("初始化数据库成功。", ConsoleColor.Green);
               

                Console.WriteLine("初始化数据表....");

                var localizedTable = typeof(ILocalizedTable);
                var localizedTableArray = entityTypes
                    .Where(x => localizedTable.IsAssignableFrom(x) && x != localizedTable).ToArray();

                localizedTableArray.ForEach(entity =>
                {
                    var attr = entity.GetCustomAttribute<SugarTable>();
                    if (attr == null)
                    {
                        Console.WriteLine($"Entity:{entity.Name}-->缺少SugarTable特性");
                    }
                    else
                    {
                        if (!db.DbMaintenance.IsAnyTable(attr.TableName))
                        {
                            Console.WriteLine(
                                $"Entity:{entity.Name}-->Table:{attr.TableName}-->Desc:{attr.TableDescription}-->创建完成！");
                            db.CodeFirst.InitTables(entity);
                        }
                    }
                });

                Console.WriteLine("初始化数据表成功！", ConsoleColor.Green);
                Console.WriteLine();
              

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion
    }
}
