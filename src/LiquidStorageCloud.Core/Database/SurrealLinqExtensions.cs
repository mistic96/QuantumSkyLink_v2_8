using System.Linq.Expressions;
using SurrealDb.Net;

namespace LiquidStorageCloud.Core.Database
{
    /// <summary>
    /// Provides LINQ extension methods for SurrealDB queries.
    /// </summary>
    public static class SurrealLinqExtensions
    {
        /// <summary>
        /// Selects records from the specified table that match the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of the records to select.</typeparam>
        /// <param name="db">The SurrealDB client instance.</param>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="predicate">A function to test each record for a condition.</param>
        /// <returns>A collection of records that match the condition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when db, tableName, or predicate is null.</exception>
        /// <exception cref="ArgumentException">Thrown when tableName is empty or whitespace.</exception>
        public static async Task<IEnumerable<T>?> Select<T>(this ISurrealDbClient db, string tableName, Expression<Func<T, bool>> predicate)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var query = TranslateExpression(predicate, tableName);
            var result = await db.RawQuery(query);
            return result.GetValue<IEnumerable<T>>(0);
        }

        /// <summary>
        /// Selects records from the specified table that match the given predicate, with ordering and pagination.
        /// </summary>
        /// <typeparam name="T">The type of the records to select.</typeparam>
        /// <param name="db">The SurrealDB client instance.</param>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="predicate">A function to test each record for a condition.</param>
        /// <param name="orderBy">A function to order the results.</param>
        /// <param name="skip">The number of records to skip.</param>
        /// <param name="take">The number of records to take.</param>
        /// <param name="ascending">True for ascending order, false for descending.</param>
        /// <returns>A collection of records that match the condition, ordered and paginated.</returns>
        /// <exception cref="ArgumentNullException">Thrown when db, tableName, predicate, or orderBy is null.</exception>
        /// <exception cref="ArgumentException">Thrown when tableName is empty or whitespace, or when skip or take is negative.</exception>
        public static async Task<IEnumerable<T>?> Select<T>(
            this ISurrealDbClient db,
            string tableName,
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> orderBy,
            int skip = 0,
            int take = int.MaxValue,
            bool ascending = true)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));
            if (skip < 0) throw new ArgumentException("Skip cannot be negative.", nameof(skip));
            if (take < 0) throw new ArgumentException("Take cannot be negative.", nameof(take));

            var visitor = new SurrealDbExpressionVisitor();
            var whereClause = visitor.Translate(predicate.Body);
            var orderByClause = visitor.Translate(orderBy.Body);
            var orderDirection = ascending ? "ASC" : "DESC";

            var query = $"SELECT * FROM {EscapeIdentifier(tableName)} WHERE {whereClause} ORDER BY {orderByClause} {orderDirection} LIMIT {take} START {skip}";
            var result = await db.RawQuery(query);
            return result.GetValue<IEnumerable<T>>(0);
        }

        private static string TranslateExpression<T>(Expression<Func<T, bool>> predicate, string tableName)
        {
            var visitor = new SurrealDbExpressionVisitor();
            var whereClause = visitor.Translate(predicate.Body);
            return $"SELECT * FROM {EscapeIdentifier(tableName)} WHERE {whereClause}";
        }

        private static string EscapeIdentifier(string identifier)
        {
            return identifier.Contains('`') ? $"`{identifier.Replace("`", "``")}`" : identifier;
        }

        /// <summary>
        /// Deletes records from the specified table that match the given predicate.
        /// </summary>
        /// <typeparam name="T">The type of the records to delete.</typeparam>
        /// <param name="db">The SurrealDB client instance.</param>
        /// <param name="tableName">The name of the table to delete from.</param>
        /// <param name="predicate">A function to test each record for deletion.</param>
        /// <returns>A collection of deleted records.</returns>
        /// <exception cref="ArgumentNullException">Thrown when db, tableName, or predicate is null.</exception>
        /// <exception cref="ArgumentException">Thrown when tableName is empty or whitespace.</exception>
        public static async Task<IEnumerable<T>?> Delete<T>(this ISurrealDbClient db, string tableName, Expression<Func<T, bool>> predicate)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("Table name cannot be empty or whitespace.", nameof(tableName));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var visitor = new SurrealDbExpressionVisitor();
            var whereClause = visitor.Translate(predicate.Body);
            var query = $"DELETE {EscapeIdentifier(tableName)} WHERE {whereClause}";
            var result = await db.RawQuery(query);
            return result.GetValue<IEnumerable<T>>(0);
        }
    }

    internal class SurrealDbExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<string> _stack = new Stack<string>();

        public string Translate(Expression expression)
        {
            Visit(expression);
            return _stack.Pop() ?? string.Empty;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Visit(node.Right);

            var right = _stack.Pop() ?? string.Empty;
            var left = _stack.Pop() ?? string.Empty;

            string result = node.NodeType switch
            {
                ExpressionType.Equal => $"{left} = {right}",
                ExpressionType.NotEqual => $"{left} != {right}",
                ExpressionType.GreaterThan => $"{left} > {right}",
                ExpressionType.GreaterThanOrEqual => $"{left} >= {right}",
                ExpressionType.LessThan => $"{left} < {right}",
                ExpressionType.LessThanOrEqual => $"{left} <= {right}",
                ExpressionType.AndAlso => $"({left} AND {right})",
                ExpressionType.OrElse => $"({left} OR {right})",
                _ => throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported")
            };

            _stack.Push(result);

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                _stack.Push(node.Member.Name);
                return node;
            }

            // Handle property/field access on constant values
            var value = Expression.Lambda(node).Compile().DynamicInvoke();
            PushFormattedValue(value);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            PushFormattedValue(node.Value);
            return node;
        }

        private void PushFormattedValue(object? value)
        {
            string formattedValue = value switch
            {
                null => "NULL",
                string s => FormatString(s),
                bool b => b ? "true" : "false",
                DateTime dt => $"time('{dt:yyyy-MM-ddTHH:mm:ss.fffZ}')",
                Guid g => FormatString(g.ToString()),
                IFormattable f => FormatString(f.ToString(null, System.Globalization.CultureInfo.InvariantCulture)),
                _ => value.ToString() ?? "NULL"
            };
            _stack.Push(formattedValue);
        }

        private static string FormatString(string value)
        {
            // Escape single quotes and wrap in single quotes
            return $"'{value.Replace("'", "''")}'";
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                switch (node.Method.Name)
                {
                    case "Contains":
                        Visit(node.Object);
                        Visit(node.Arguments[0]);
                        var argument = _stack.Pop() ?? string.Empty;
                        var member = _stack.Pop() ?? string.Empty;
                        _stack.Push($"{member} CONTAINS {argument}");
                        return node;
                    case "StartsWith":
                        Visit(node.Object);
                        Visit(node.Arguments[0]);
                        argument = _stack.Pop() ?? string.Empty;
                        member = _stack.Pop() ?? string.Empty;
                        _stack.Push($"{member} STARTS WITH {argument}");
                        return node;
                    case "EndsWith":
                        Visit(node.Object);
                        Visit(node.Arguments[0]);
                        argument = _stack.Pop() ?? string.Empty;
                        member = _stack.Pop() ?? string.Empty;
                        _stack.Push($"{member} ENDS WITH {argument}");
                        return node;
                }
            }

            throw new NotSupportedException($"The method '{node.Method.Name}' is not supported");
        }
    }
}
