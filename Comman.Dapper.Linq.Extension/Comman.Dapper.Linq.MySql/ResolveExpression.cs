using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Comman.Dapper.Linq.Extension;
using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Entites;
using Comman.Dapper.Linq.Extension.Exception;
using Comman.Dapper.Linq.Extension.Expressions;
using Comman.Dapper.Linq.Extension.Extension;
using Comman.Dapper.Linq.Extension.Helper.Cache;

namespace Comman.Dapper.Linq.MySql
{
    internal class ResolveExpression : IResolveExpression
    {
        public ResolveExpression(SqlProvider provider) : base(provider)
        {

        }

        public override string ResolveSelect(int? topNum)
        {
            // 添加需要連接的導航表
            var masterEntity = EntityCache.QueryEntity(abstractSet.TableType);
            var navigationList = masterEntity.Navigations;
            if (navigationList.Any())
            {
                provider.JoinList.AddRange(ExpressionExtension.Clone(navigationList));
            }
            // 查詢字段解析
            StringBuilder selectSql = new StringBuilder("SELECT");
            // 去重
            if (abstractSet.IsDistinct)
                selectSql.Append(" DISTINCT ");

            // 不是自定義返回視圖則顯示所有字段
            if (provider.Context.Set.SelectExpression == null)
            {
                var propertyBuilder = GetTableField(masterEntity);
                selectSql.Append($" {propertyBuilder} ");
            }
            else // 自定義查詢字段
            {
                var selectExp = new SelectExpression(provider.Context.Set.SelectExpression, "", provider);
                selectSql.Append($" {selectExp.SqlCmd} ");
                provider.Params.AddDynamicParams(selectExp.Param);
            }
            return selectSql.ToString();
        }


        public override string ResolveSum(LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            switch (selector.NodeType)
            {
                case ExpressionType.Lambda:
                case ExpressionType.MemberAccess:
                    {
                        EntityObject entityObject = EntityCache.QueryEntity(selector.Parameters[0].Type);
                        var memberName = selector.Body.GetCorrectPropertyName();
                        string fieldName = $"{entityObject.AsName}.{providerOption.CombineFieldName(entityObject.FieldPairs[memberName])}";
                        selectSql = $" SELECT IFNULL(SUM({fieldName}),0)  ";
                    }
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支援該表達式類型");
            }

            return selectSql;
        }

        public override string ResolveMax(LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            switch (selector.NodeType)
            {
                case ExpressionType.Lambda:
                case ExpressionType.MemberAccess:
                    {
                        EntityObject entityObject = EntityCache.QueryEntity(selector.Parameters[0].Type);
                        var memberName = selector.Body.GetCorrectPropertyName();
                        string fieldName = $"{entityObject.AsName}.{providerOption.CombineFieldName(entityObject.FieldPairs[memberName])}";
                        selectSql = $" SELECT Max({fieldName})  ";
                    }
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支援該表達式類型");
            }

            return selectSql;
        }

        public override string ResolveMin(LambdaExpression selector)
        {
            if (selector == null)
                throw new ArgumentException("selector");
            var selectSql = "";

            switch (selector.NodeType)
            {
                case ExpressionType.Lambda:
                case ExpressionType.MemberAccess:
                    {
                        EntityObject entityObject = EntityCache.QueryEntity(selector.Parameters[0].Type);
                        var memberName = selector.Body.GetCorrectPropertyName();
                        string fieldName = $"{entityObject.AsName}.{providerOption.CombineFieldName(entityObject.FieldPairs[memberName])}";
                        selectSql = $" SELECT Min({fieldName})  ";
                    }
                    break;
                case ExpressionType.MemberInit:
                    throw new DapperExtensionException("不支援該表達式類型");
            }

            return selectSql;
        }

        public override string ResolveWithNoLock(bool nolock)
        {
            return "";
        }
    }
}
