namespace Comman.Dapper.Linq.Extension.Attributes
{
    /// <summary>
    /// 表示字段的長度限制。
    /// </summary>
    public class StringLength : BaseAttribute
    {
        /// <summary>
        /// 初始化 <see cref="StringLength"/> 類別的新實例。
        /// </summary>
        /// <param name="length">字段的最大長度。</param>
        public StringLength(int length)
        {
            Length = length;
        }

        /// <summary>
        /// 獲取字段的最大長度。
        /// </summary>
        public int Length { get; private set; }
    }
}