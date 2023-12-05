using System.Threading;
using Comman.Dapper.Linq.Extension.Dapper;
#if NET45 || NET451
using System.Runtime.Remoting.Messaging;
#endif

namespace Comman.Dapper.Linq.Extension
{
    /// <summary>
    /// 提供 AOP 功能的提供者。
    /// </summary>
    public class AopProvider
    {
        /// <summary>
        /// 事件模型定義。
        /// </summary>
        /// <param name="command">命令定義引用。</param>
        public delegate void EventHandler(ref CommandDefinition command);

        /// <summary>
        /// 執行前事件。
        /// </summary>
        public event EventHandler OnExecuting;

        /// <summary>
        /// 執行後事件。
        /// </summary>
        public event EventHandler OnExecuted;

        /// <summary>
        /// 觸發執行前事件。
        /// </summary>
        /// <param name="definition">命令定義引用。</param>
        internal void InvokeExecuting(ref CommandDefinition definition)
        {
            OnExecuting?.Invoke(ref definition);
        }

        /// <summary>
        /// 觸發執行後事件。
        /// </summary>
        /// <param name="definition">命令定義引用。</param>
        internal void InvokeExecuted(ref CommandDefinition definition)
        {
            OnExecuted?.Invoke(ref definition);
        }

#if NET45 || NET451
        // .NET Framework 版本的 AopProvider 實現。
#else
        // .NET Core 及以上版本的 AopProvider 實現。
        private static readonly AsyncLocal<AopProvider> _aop = new AsyncLocal<AopProvider>();
#endif

        /// <summary>
        /// 獲取當前線程唯一的 Aop 實例。
        /// </summary>
        /// <returns>AopProvider 實例。</returns>
        public static AopProvider Get()
        {
#if NET45 || NET451
            string contextKey = typeof(AopProvider).FullName;
            var _aop = CallContext.LogicalGetData(contextKey);
            if (_aop == null)
            {
                _aop = new AopProvider();
                CallContext.LogicalSetData(contextKey, _aop);
            }
            return _aop as AopProvider;
#else
            lock (_aop)
            {
                return _aop.Value ?? (_aop.Value = new AopProvider());
            }
#endif
        }
    }
}
