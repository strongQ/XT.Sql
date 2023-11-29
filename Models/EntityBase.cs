using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace XT.Sql.Models
{
    /// <summary>
    /// 框架实体基类Id
    /// </summary>
    public abstract class EntityBaseId
    {
        /// <summary>
        /// 雪花Id
        /// </summary>
        [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true, IsIdentity = false)]
        public virtual long Id { get; set; }
    }

    /// <summary>
    /// 框架实体基类
    /// </summary>
    public abstract class EntityBase : EntityBaseId
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间",IsNullable =true, IsOnlyIgnoreUpdate = true)]
        public virtual DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "更新时间",IsNullable =true, IsOnlyIgnoreInsert = true)]
        public virtual DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 创建者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "创建者Id",IsNullable =true, IsOnlyIgnoreUpdate = true)]
        public virtual long? CreateUserId { get; set; }

        /// <summary>
        /// 修改者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "修改者Id",IsNullable =true, IsOnlyIgnoreInsert = true)]
        public virtual long? UpdateUserId { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [SugarColumn(ColumnDescription = "软删除",IsNullable =true)]
        public virtual bool IsDelete { get; set; } = false;
    }
}
