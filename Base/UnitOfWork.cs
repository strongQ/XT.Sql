using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Sql.Interfaces;

namespace XT.Sql.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        public UnitOfWork(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }

        /// <summary>
        /// 获取DB，保证唯一性
        /// </summary>
        /// <returns></returns>
        public SqlSugarScope GetDbClient()
        {
            return _sqlSugarClient as SqlSugarScope;
        }

        public void BeginTran()
        {
            GetDbClient().BeginTran();
        }

        public void CommitTran()
        {
            try
            {
                GetDbClient().CommitTran();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                GetDbClient().RollbackTran();                             
            }
        }

        public void RollbackTran()
        {
            GetDbClient().RollbackTran();
        }
    }
}
