
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using XT.Sql.Models;
using XT.Sql.Enums;
using XT.Common.Config;

namespace XT.Sql
{
    public static class BaseDbConfig
    {
        
        public static (List<DataBaseOperate> MasterDb, List<DataBaseOperate> SlaveDbs) GetDataBaseOperate =>
            InitDataBaseConn();


  //      "DBS": [
  //  {
  //    "ConnId": "oracle_now",
  //    "HitRate": 20,
  //    "DBType": 3,
  //    "Enabled": true,
  //    "ConnectionString": "DATA SOURCE=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=100.100.100.100)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=DataBase)));USER ID=user;Password=pwd;Connection Timeout=600;"
  //  },
  //  {
  //    "ConnId": "mysql_slvae",
  //    "HitRate": 10,
  //    "DBType": 0,
  //    "Enabled": false,
  //    "ConnectionString": "Server=100.100.55.12; Port=3306;Stmt=; Database=apevoloDB; Uid=root; Pwd=123456;"
  //  },
  //  {
  //  "ConnId": "sqlserver",
  //    "HitRate": 20,
  //    "DBType": 1,
  //    "Enabled": false,
  //    "ConnectionString": "Data Source=localhost;User Id = sa;Password = 123456;Initial Catalog=apevoloDB;MultipleActiveResultSets=True;"
  //  },
  //  {
  //  "ConnId": "sqlite",
  //    "HitRate": 20,
  //    "DBType": 2,
  //    "Enabled": false,
  //    //sqlite数据库只需要添加数据库名称，路径系统默认在ApeVolo.Api
  //    "ConnectionString": "apevoloDB.db"
  //  }

  //],
        private static (List<DataBaseOperate>, List<DataBaseOperate>) InitDataBaseConn()
        {
            List<DataBaseOperate> masterDbs = new List<DataBaseOperate>();
            var slaveDbs = new List<DataBaseOperate>();
            var allDbs = new List<DataBaseOperate>();
            try
            {
              
                string path = AppSettings.IsDevelopment ? "appsettings.Development.json" : "appsettings.json";
                using var file = new StreamReader(path);

                var xtconfig = AppSettings.GetObjData<XTDbConfig>("XTDbConfig");

                if (xtconfig == null || xtconfig.Dbs.Count < 1)
                {
                    throw new System.Exception("请确保appsettings.json中配置连接字符串,并设置Enabled为true;");
                }

                allDbs = xtconfig.Dbs;

                masterDbs = allDbs.Where(x => x.IsMain && x.Enabled).ToList();
                if (masterDbs.Count == 0)
                {
                    throw new System.Exception($"请确保存在IsMain的数据库;");
                }



                //如果开启读写分离
                if (xtconfig.IsReadAndWrite)
                {
                    slaveDbs = allDbs.Where(x => x.DbType == masterDbs[0].DbType && x.IsMain == false)
                        .ToList();
                    if (slaveDbs.Count < 1)
                    {
                        throw new System.Exception($"请确保存在IsMain=false DbType相同的从库;");
                    }
                }


                return (masterDbs, slaveDbs);
            }
            catch(System.Exception ex)
            {
                return (masterDbs, slaveDbs);
            }
        }

        
    }
}