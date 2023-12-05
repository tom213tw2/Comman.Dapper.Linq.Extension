namespace Kogel.Dapper.Extension
{
    /// <summary>
    ///     函数列表
    /// </summary>
    public static class Function
    {
        #region 聚合函数

        public static int Count<T>(T countExpression)
        {
            return default;
        }

        public static T Sum<T>(T sumExpression)
        {
            return default;
        }

        public static T Max<T>(T maxExpression)
        {
            return default;
        }

        public static T Min<T>(T minExpression)
        {
            return default;
        }

        public static T Avg<T>(T avgExpression)
        {
            return default;
        }

        #endregion

        #region 基础函数

        /// <summary>
        ///     左模糊
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool StartsWith(this string left, string right)
        {
            return default;
        }

        /// <summary>
        ///     右模糊
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool EndsWith(this string left, string right)
        {
            return default;
        }

        /// <summary>
        ///     拼接（oracle使用，mssql可以直接使用+号）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static T Concat<T>(T left, T right)
        {
            return default;
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static T IfNull<T>(T left, T right)
        {
            return default;
        }

        /// <summary>
        ///     拼接sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T ConcatSql<T>(string sql)
        {
            return default;
        }

        /// <summary>
        ///     拼接sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static T ConcatSql<T>(string sql, object param)
        {
            return default;
        }

        #endregion
    }
}