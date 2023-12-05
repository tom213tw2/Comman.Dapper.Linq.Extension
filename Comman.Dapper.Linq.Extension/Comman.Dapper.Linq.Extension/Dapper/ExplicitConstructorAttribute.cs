﻿using System;

namespace Comman.Dapper.Linq.Extension.Dapper
{
    /// <summary>
    ///     Tell Dapper to use an explicit constructor, passing nulls or 0s for all parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class ExplicitConstructorAttribute : Attribute
    {
    }
}