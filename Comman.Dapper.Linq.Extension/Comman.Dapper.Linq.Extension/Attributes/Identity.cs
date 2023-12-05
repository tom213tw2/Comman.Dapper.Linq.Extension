namespace Comman.Dapper.Linq.Extension.Attributes
{
    /// <summary>
    /// 指定屬性是否為身份欄位以及是否自動遞增。
    /// </summary>
    public class Identity : BaseAttribute
    {
        /// <summary>
        /// 初始化 <see cref="Identity"/> 屬性類別的新實例。
        /// </summary>
        /// <param name="isIncrease">指示欄位是否自動遞增。</param>
        public Identity(bool isIncrease = true)
        {
            IsIncrease = isIncrease;
        }

        /// <summary>
        /// 獲取或設置一個值，指示欄位是否自動遞增。
        /// </summary>
        public bool IsIncrease { get; set; }
    }
}