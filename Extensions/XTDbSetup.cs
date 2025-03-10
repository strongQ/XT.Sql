﻿using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using XT.Sql.Interfaces;
using XT.Sql.Models;
using XT.Sql.Base;
using XT.Common.Config;
using XT.Common.Extensions;
using XT.Common.Helpers;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XT.Sql.Extensions
{
    /// <summary>
    /// SqlSugar 启动器
    /// </summary>
    public static class XTDbSetup
    {
        /// <summary>
        /// 添加db支持
        /// </summary>
        /// <param name="services"></param>
        /// <param name="isscope"></param>
        /// <param name="xtconfig">配置项</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddXTDbSetup(this IServiceCollection services,bool isscope=false,XTDbConfig xtconfig=null)
        {
            if (xtconfig == null)
            {
                xtconfig = AppSettings.GetObjData<XTDbConfig>("XTDbConfig");
            }

           
            if(xtconfig==null)
            {
                //根据环境读取响应的appsettings
                string appsettingsFile = AppSettings.IsDevelopment ? "appsettings.Development.json" : "appsettings.json";

                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), appsettingsFile);

              
                using var file = new StreamReader(path);
                using var reader = new JsonTextReader(file);
                var jObj = (JObject)JToken.ReadFrom(reader);
                
                 var secJt = jObj["XTDbConfig"];

                xtconfig = JsonConvert.DeserializeObject<XTDbConfig>(secJt.ToString());

            }


            BaseDbConfig.XTConfig= xtconfig;

            if (services.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(services));
            if (BaseDbConfig.GetDataBaseOperate.MasterDb.IsNullOrEmpty() || BaseDbConfig.GetDataBaseOperate.MasterDb.Count==0)
                throw new ArgumentNullException(nameof(XTDbConfig));

            var masterDbs = BaseDbConfig.GetDataBaseOperate.MasterDb;
            ConnectionConfig masterDb = null; //主库
            var slaveDbs = new List<SlaveConnectionConfig>(); //从库列表
            BaseDbConfig.GetDataBaseOperate.SlaveDbs.ForEach(db =>
            {
                slaveDbs.Add(new SlaveConnectionConfig
                {
                    HitRate = db.HitRate,
                    ConnectionString = db.ConnectionString
                });
            });
            List<ConnectionConfig> connections = new List<ConnectionConfig>();

            masterDb = new ConnectionConfig
            {
                ConfigId = BaseDbConfig.GetDataBaseOperate.MasterDb[0].ConnId,
                ConnectionString = BaseDbConfig.GetDataBaseOperate.MasterDb[0].ConnectionString,
                DbType = (DbType)BaseDbConfig.GetDataBaseOperate.MasterDb[0].DbType,
                LanguageType = LanguageType.Chinese,
                IsAutoCloseConnection = true,
                //IsShardSameThread = false,
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true
                },
                // 从库
                SlaveConnectionConfigs = slaveDbs
            };
            connections.Add(masterDb);

            for (int i = 0; i < masterDbs.Count; i++)
            {
                if (i == 0) continue;
                var config = new ConnectionConfig
                {
                    ConfigId = masterDbs[i].ConnId.ToLower(),
                    ConnectionString = masterDbs[i].ConnectionString,
                    DbType = (DbType)masterDbs[i].DbType,
                    LanguageType = LanguageType.Chinese,
                    IsAutoCloseConnection = true,
                    MoreSettings = new ConnMoreSettings
                    {
                        IsAutoRemoveDataCache = true
                    }
                };
                connections.Add(config);

            }



            var sugar = new SqlSugarScope(connections,
                config =>
                {
                #region 条件过滤器

                //config.QueryFilter.Add(new TableFilterItem<User>(it => !it.IsDeleted));
                //config.QueryFilter.Add(new TableFilterItem<UserJobs>(it => !it.IsDeleted));

                #endregion

                #region 执行的SQL

                config.Aop.OnLogExecuting = (sql, pars) =>
                {
                   

                        if (xtconfig.IsSqlAOP)
                        {
                        var pa = GetParams(pars);
                        var sqlLog= $"SqlLog{DateTime.Now:yyyy-MM-dd},【SQL参数】：\n {pa} 【SQL语句】：\n {sql}";
                        var log = UtilMethods.GetSqlString(config.CurrentConnectionConfig.DbType, sql, pars);
                        XTDbContext.LogSql(log);
                        }
                    };

                    #endregion


                    // 数据审计
                    config.Aop.DataExecuting = (oldValue, entityInfo) =>
                    {
                        if (entityInfo.OperationType == DataFilterType.InsertByObject)
                        {
                            // 主键(long类型)且没有值的---赋值雪花Id
                            if (entityInfo.EntityColumnInfo.IsPrimarykey && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                            {
                                var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                                if (id == null || (long)id == 0)
                                    entityInfo.SetValue(IdHelper.GetLongId());
                            }
                            if (entityInfo.PropertyName == "CreateTime")
                                entityInfo.SetValue(DateTime.Now);
                          
                        }
                        if (entityInfo.OperationType == DataFilterType.UpdateByObject)
                        {
                            if (entityInfo.PropertyName == "UpdateTime")
                                entityInfo.SetValue(DateTime.Now);
                           
                        }
                    };

                    //OnLogExecuted = //执行完毕
                }


                
                
                );

        




            services.AddSingleton<ISqlSugarClient>(sugar);
            if(isscope){
                 services.AddScoped<IUnitOfWork,UnitOfWork>();
                 services.AddScoped<XTDbContext>();

            }
            else{
                 services.AddSingleton<IUnitOfWork,UnitOfWork>();
                 services.AddSingleton<XTDbContext>();

            }
           
            //雪花ID器
            new IdHelperBootstrapper().SetWorkderId(1).Boot();

        

            return services;
        }


        public static IServiceCollection AddXTDbRepositories(this IServiceCollection services,List<Type> types)
        {
           foreach(var type in types)
            {
                services.AddServiceInjects<IDependencyRepository>(type);
            }
           return services;
        }

        /// <summary>
        /// 参数拼接字符串
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        private static string GetParams(SugarParameter[] pars)
        {
            return pars.Aggregate("", (current, p) => current + $"{p.ParameterName}:{p.Value}\n");
        }
    }
}
