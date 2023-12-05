﻿using System;
using System.Data;

namespace Comman.Dapper.Linq.Extension.Dapper
{
    /// <summary>
    ///     Handles variances in features per DBMS
    /// </summary>
    internal class FeatureSupport
    {
        private static readonly FeatureSupport
            Default = new FeatureSupport(false),
            Postgres = new FeatureSupport(true);

        private FeatureSupport(bool arrays)
        {
            Arrays = arrays;
        }

        /// <summary>
        ///     True if the db supports array columns e.g. Postgresql
        /// </summary>
        public bool Arrays { get; }

        /// <summary>
        ///     Gets the feature set based on the passed connection
        /// </summary>
        /// <param name="connection">The connection to get supported features for.</param>
        public static FeatureSupport Get(IDbConnection connection)
        {
            var name = connection?.GetType().Name;
            if (string.Equals(name, "npgsqlconnection", StringComparison.OrdinalIgnoreCase)) return Postgres;
            return Default;
        }
    }
}