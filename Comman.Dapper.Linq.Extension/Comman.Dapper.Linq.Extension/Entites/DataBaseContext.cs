using Comman.Dapper.Linq.Extension.Core.Interfaces;
using Comman.Dapper.Linq.Extension.Core.SetC;

namespace Comman.Dapper.Linq.Extension.Entites
{
    public class DataBaseContext<T> : AbstractDataBaseContext
    {
        public Comman.Dapper.Linq.Extension.Core.SetQ.QuerySet<T> QuerySet => (Comman.Dapper.Linq.Extension.Core.SetQ.QuerySet<T>)Set;

        public CommandSet<T> CommandSet => (CommandSet<T>)Set;
    }

    public abstract class AbstractDataBaseContext
    {
        public AbstractSet Set { get; set; }

        public EOperateType OperateType { get; set; }
    }
}