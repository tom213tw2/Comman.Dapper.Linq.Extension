using System.Collections.Generic;

namespace Comman.Dapper.Linq.Extension.Entites
{
    /// <summary>
    /// 表示分頁資料的實體類別。
    /// </summary>
    /// <typeparam name="T">資料項目的類型。</typeparam>
    public class PageList<T>
    {
        /// <summary>
        /// 初始化分頁實體。
        /// </summary>
        /// <param name="pageIndex">頁碼，表示當前頁面的編號。</param>
        /// <param name="pageSize">頁面大小，表示每頁顯示的項目數量。</param>
        /// <param name="totalCount">總項目數，表示所有項目的總數。</param>
        /// <param name="items">頁面項目，表示當前頁面上的項目列表。</param>
        public PageList(int pageIndex, int pageSize, int totalCount, List<T> items)
        {
            Total = totalCount;
            PageSize = pageSize;
            PageIndex = pageIndex;
            Items = items;
            TotalPage = Total % PageSize == 0 ? Total / PageSize : Total / PageSize + 1;
        }

        /// <summary>
        /// 總項目數。
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// 當前頁面上的項目列表。
        /// </summary>
        public List<T> Items { get; }

        /// <summary>
        /// 頁面大小，每頁顯示的項目數量。
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 頁碼，當前頁面的編號。
        /// </summary>
        public int PageIndex { get; }

        /// <summary>
        /// 總頁數，根據總項目數和頁面大小計算得出。
        /// </summary>
        public int TotalPage { get; }

        /// <summary>
        /// 是否有上一頁，當頁碼大於1時為真。
        /// </summary>
        public bool HasPrev => PageIndex > 1;

        /// <summary>
        /// 是否有下一頁，當頁碼小於總頁數時為真。
        /// </summary>
        public bool HasNext => PageIndex < TotalPage;
    }
}