﻿using System;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Comman.Dapper.Linq.Extension.Dapper
{
    /// <summary>
    ///     Represents the key aspects of a sql operation
    /// </summary>
    public struct CommandDefinition
    {
        internal static CommandDefinition ForCallback(object parameters)
        {
            return parameters is DynamicParameters ? new CommandDefinition(parameters) : default;
        }

        internal void OnCompleted()
        {
            (Parameters as SqlMapper.IParameterCallbacks)?.OnCompleted();
        }

        /// <summary>
        ///     db command connection
        /// </summary>
        public IDbConnection Connection { get; set; }

        /// <summary>
        ///     The command (sql or a stored-procedure name) to execute
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        ///     The parameters associated with the command
        /// </summary>
        public object Parameters { get; }

        /// <summary>
        ///     The active transaction for the command
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        ///     是否进入过工作单元
        /// </summary>
        public bool IsUnitOfWork { get; set; }

        /// <summary>
        ///     是否排除在工作单元外
        /// </summary>
        public bool IsExcludeUnitOfWork { get; set; }

        /// <summary>
        ///     The effective timeout for the command
        /// </summary>
        public int? CommandTimeout { get; }

        /// <summary>
        ///     The type of command that the command-text represents
        /// </summary>
        public CommandType? CommandType { get; }

        /// <summary>
        ///     Should data be buffered before returning?
        /// </summary>
        public bool Buffered => (Flags & CommandFlags.Buffered) != 0;

        /// <summary>
        ///     Should the plan for this query be cached?
        /// </summary>
        internal bool AddToCache => (Flags & CommandFlags.NoCache) == 0;

        /// <summary>
        ///     Additional state flags against this command
        /// </summary>
        public CommandFlags Flags { get; }

        /// <summary>
        ///     Can async queries be pipelined?
        /// </summary>
        public bool Pipelined => (Flags & CommandFlags.Pipelined) != 0;

        /// <summary>
        ///     Initialize the command definition
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandText">The text for this command.</param>
        /// <param name="parameters">The parameters for this command.</param>
        /// <param name="transaction">The transaction for this command to participate in.</param>
        /// <param name="commandTimeout">The timeout (in seconds) for this command.</param>
        /// <param name="commandType">The <see cref="CommandType" /> for this command.</param>
        /// <param name="flags">The behavior flags for this command.</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <param name="isExcludeUnitOfWork"></param>
        public CommandDefinition(IDbConnection connection, string commandText, object parameters = null,
            IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null, CommandFlags flags = CommandFlags.Buffered
            , CancellationToken cancellationToken = default,
            bool isExcludeUnitOfWork = false
        )
        {
            Connection = connection;
            CommandText = commandText;
            Parameters = parameters;
            Transaction = transaction;
            CommandTimeout = commandTimeout;
            CommandType = commandType;
            Flags = flags;
            CancellationToken = cancellationToken;
            IsUnitOfWork = false;
            IsExcludeUnitOfWork = isExcludeUnitOfWork;
        }

        private CommandDefinition(object parameters) : this()
        {
            Parameters = parameters;
        }

        /// <summary>
        ///     For asynchronous operations, the cancellation-token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader)
        {
            var cmd = cnn.CreateCommand();
            var init = GetInit(cmd.GetType());
            init?.Invoke(cmd);
            if (Transaction != null)
                cmd.Transaction = Transaction;
            cmd.CommandText = CommandText;
            if (CommandTimeout.HasValue)
                cmd.CommandTimeout = CommandTimeout.Value;
            else if (SqlMapper.Settings.CommandTimeout.HasValue)
                cmd.CommandTimeout = SqlMapper.Settings.CommandTimeout.Value;
            if (CommandType.HasValue)
                cmd.CommandType = CommandType.Value;
            paramReader?.Invoke(cmd, Parameters);
            return cmd;
        }

        private static SqlMapper.Link<Type, Action<IDbCommand>> _commandInitCache;

        private static Action<IDbCommand> GetInit(Type commandType)
        {
            if (commandType == null)
                return null; // GIGO
            if (SqlMapper.Link<Type, Action<IDbCommand>>.TryGet(_commandInitCache, commandType, out var action))
                return action;
            var bindByName = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
            var initialLongFetchSize = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));

            action = null;
            if (bindByName != null || initialLongFetchSize != null)
            {
                var method = new DynamicMethod(commandType.Name + "_init", null, new[] { typeof(IDbCommand) });
                var il = method.GetILGenerator();

                if (bindByName != null)
                {
                    // .BindByName = true
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.EmitCall(OpCodes.Callvirt, bindByName, null);
                }

                if (initialLongFetchSize != null)
                {
                    // .InitialLONGFetchSize = -1
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Castclass, commandType);
                    il.Emit(OpCodes.Ldc_I4_M1);
                    il.EmitCall(OpCodes.Callvirt, initialLongFetchSize, null);
                }

                il.Emit(OpCodes.Ret);
                action = (Action<IDbCommand>)method.CreateDelegate(typeof(Action<IDbCommand>));
            }

            // cache it
            SqlMapper.Link<Type, Action<IDbCommand>>.TryAdd(ref _commandInitCache, commandType, ref action);
            return action;
        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
        {
            var prop = declaringType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop?.CanWrite == true && prop.PropertyType == expectedType && prop.GetIndexParameters().Length == 0)
                return prop.GetSetMethod();
            return null;
        }
    }
}