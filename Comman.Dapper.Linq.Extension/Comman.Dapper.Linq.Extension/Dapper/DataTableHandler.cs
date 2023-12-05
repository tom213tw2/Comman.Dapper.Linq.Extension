using System;
using System.Data;
using Kogel.Dapper.Extension;

#if !NETSTANDARD1_3
namespace Comman.Dapper.Linq.Extension.Dapper
{
    internal sealed class DataTableHandler : SqlMapper.ITypeHandler
    {
        public object Parse(Type destinationType, object value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(IDbDataParameter parameter, object value)
        {
            TableValuedParameter.Set(parameter, value as DataTable, null);
        }
    }
}
#endif