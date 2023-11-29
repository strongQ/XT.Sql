using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XT.Sql.Enums;

namespace XT.Sql.Models
{
    /// <summary>
    /// 数据库操作
    /// </summary>
    public class DataBaseOperate
    {
        public string ConnId { get; set; }
        public int HitRate { get; set; }
        /// <summary>
        /// 是否主库，false为从库
        /// </summary>
        public bool IsMain { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }
        public string ConnectionString { get; set; }
        public DataBaseType DbType { get; set; }
    }
}
