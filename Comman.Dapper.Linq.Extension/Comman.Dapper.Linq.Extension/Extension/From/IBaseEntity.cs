using Comman.Dapper.Linq.Extension.Attributes;

namespace Comman.Dapper.Linq.Extension.Extension.From
{
	/// <summary>
	///     父级实体类
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TKey"></typeparam>
	public abstract class IBaseEntity<TEntity, TKey> : IBaseEntity
    {
	    /// <summary>
	    ///     主键id
	    /// </summary>
	    [Identity]
        public virtual TKey Id { get; set; }

	    /// <summary>
	    ///     获取主键值
	    /// </summary>
	    /// <returns></returns>
	    public object GetId()
        {
            return Id;
        }
    }

    public interface IBaseEntity
    {
        object GetId();
    }

    public class IBaseEntityDto : IBaseEntity
    {
        public object GetId()
        {
            return null;
        }
    }
}