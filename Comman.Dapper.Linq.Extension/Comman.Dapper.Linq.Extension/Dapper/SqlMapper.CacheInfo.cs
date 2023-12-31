﻿using System;
using System.Data;
using System.Threading;

namespace Comman.Dapper.Linq.Extension.Dapper
{
    public static partial class SqlMapper
    {
        internal class CacheInfo
        {
            private int hitCount;
            public SqlMapper.DeserializerState Deserializer { get; set; }
            public Func<IDataReader, object>[] OtherDeserializers { get; set; }
            public Action<IDbCommand, object> ParamReader { get; set; }

            public int GetHitCount()
            {
                return Interlocked.CompareExchange(ref hitCount, 0, 0);
            }

            public void RecordHit()
            {
                Interlocked.Increment(ref hitCount);
            }
        }
    }
}