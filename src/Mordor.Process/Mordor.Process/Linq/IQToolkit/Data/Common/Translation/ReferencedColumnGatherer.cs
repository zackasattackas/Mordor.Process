﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq.Expressions;
using Mordor.Process.Linq.IQToolkit.Data.Common.Expressions;

namespace Mordor.Process.Linq.IQToolkit.Data.Common.Translation
{
    /// <summary>
    /// Gathers all columns referenced by the given expression
    /// </summary>
    public class ReferencedColumnGatherer : DbExpressionVisitor
    {
        private readonly HashSet<ColumnExpression> _columns = new HashSet<ColumnExpression>();
        private bool _first = true;

        public static HashSet<ColumnExpression> Gather(Expression expression)
        {
            var visitor = new ReferencedColumnGatherer();
            visitor.Visit(expression);
            return visitor._columns;
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            _columns.Add(column);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (_first)
            {
                _first = false;
                return base.VisitSelect(select);
            }
            return select;
        }
    }
}