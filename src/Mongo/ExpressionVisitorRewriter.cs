using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace UCode.Mongo
{
    /// <summary>
    /// Custom expression visitor that rewrites LINQ expressions using <c>Any</c>, <c>All</c>,
    /// and common string methods into an equivalent form that can be correctly translated by the MongoDB driver.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This visitor intercepts method calls to <c>Any</c> and <c>All</c> in the expression tree and rewrites them.
    /// For instance, an expression such as:
    /// </para>
    /// <code>
    /// collection.Any(y => y == x.Ref)
    /// </code>
    /// <para>
    /// is transformed into:
    /// </para>
    /// <code>
    /// collection.Contains(x.Ref)
    /// </code>
    /// <para>
    /// Similarly, an expression like:
    /// </para>
    /// <code>
    /// collection.All(r => predicate(r))
    /// </code>
    /// <para>
    /// will be rewritten as:
    /// </para>
    /// <code>
    /// !collection.Any(r => !predicate(r))
    /// </code>
    /// <para>
    /// In addition, common string methods such as <c>Equals</c>, <c>StartsWith</c>, and <c>EndsWith</c>
    /// (with or without <see cref="StringComparison.InvariantCultureIgnoreCase"/>) are rewritten.
    /// When an ignore‑case comparison is requested, the method call is transformed into a regex comparison,
    /// e.g.:
    /// </para>
    /// <code>
    /// x.Name.Equals("John", StringComparison.InvariantCultureIgnoreCase)
    /// // becomes something like:
    /// x.Name == new BsonRegularExpression("^John$", "i")
    /// </code>
    /// <para>
    /// Negative comparisons (prefixed with <c>!</c>) are preserved.
    /// </para>
    /// </remarks>
    public class ExpressionVisitorRewriter : ExpressionVisitor
    {
        /// <summary>
        /// Applies the expression visitor rewriter to the provided expression query, supporting dynamic expressions,
        /// as well as rewriting calls to <c>Any</c>, <c>All</c>, and common string methods.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TResult">Generic result type (not used in the query, but kept for compatibility).</typeparam>
        /// <param name="expressionQuery">The original expression query.</param>
        /// <returns>
        /// A new expression query with the applicable rewrites applied.
        /// </returns>
        [return: NotNullIfNotNull(nameof(expressionQuery))]
        public Expression<Func<TDocument, TResult>>? Rewrite<TDocument, TResult>(Expression<Func<TDocument, TResult>>? expressionQuery)
        {
            if(expressionQuery == null)
                return null;

            // First, apply the dynamic expression visitor to process any dynamic expressions.
            var dynamicVisitor = new DynamicExpressionVisitorRewriter();
            var dynamicExpression = (Expression<Func<TDocument, TResult>>)dynamicVisitor.Visit(expressionQuery);

            // Then, apply this visitor to rewrite Any, All, and string method calls.
            var rewrittenExpression = (Expression<Func<TDocument, TResult>>)this.Visit(dynamicExpression);
            return rewrittenExpression;
        }

        /// <summary>
        /// Applies the expression visitor rewriter to the provided expression query, supporting dynamic expressions,
        /// as well as rewriting calls to <c>Any</c>, <c>All</c>, and common string methods.
        /// </summary>
        /// <typeparam name="TDocumentLeft">The type of the document.</typeparam>
        /// <typeparam name="TDocumentRight">The type of the document.</typeparam>
        /// <typeparam name="TResult">Generic result type (not used in the query, but kept for compatibility).</typeparam>
        /// <param name="expressionQuery">The original expression query.</param>
        /// <returns>
        /// A new expression query with the applicable rewrites applied.
        /// </returns>
        [return: NotNullIfNotNull(nameof(expressionQuery))]
        public Expression<Func<TDocumentLeft, TDocumentRight, TResult>>? Rewrite<TDocumentLeft, TDocumentRight, TResult>(Expression<Func<TDocumentLeft, TDocumentRight, TResult>>? expressionQuery)
        {
            if (expressionQuery == null)
                return null;

            // First, apply the dynamic expression visitor to process any dynamic expressions.
            var dynamicVisitor = new DynamicExpressionVisitorRewriter();
            var dynamicExpression = (Expression<Func<TDocumentLeft, TDocumentRight, TResult>>)dynamicVisitor.Visit(expressionQuery);

            // Then, apply this visitor to rewrite Any, All, and string method calls.
            var rewrittenExpression = (Expression<Func<TDocumentLeft, TDocumentRight, TResult>>) this.Visit(dynamicExpression)!;
            return rewrittenExpression;
        }

        /// <summary>
        /// Visits the method call expressions and rewrites calls to <c>Any</c>, <c>All</c>,
        /// and common string methods (<c>Equals</c>, <c>StartsWith</c>, <c>EndsWith</c>).
        /// </summary>
        /// <param name="node">The method call expression node to visit.</param>
        /// <returns>
        /// The rewritten expression if a rewrite was applicable; otherwise, the original expression is returned.
        /// </returns>
        /// <remarks>
        /// <para>
        /// For <c>Any</c> calls, if the lambda expression is of the form <c>y => y == value</c>,
        /// it is rewritten into a call to <c>Enumerable.Contains(collection, value)</c>.
        /// </para>
        /// <para>
        /// For <c>All</c> calls, the expression is rewritten into the negation of an <c>Any</c> call with the predicate negated.
        /// </para>
        /// <para>
        /// For string methods:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <c>Equals(string, StringComparison.InvariantCultureIgnoreCase)</c> is rewritten using a regex pattern matching the entire string.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>Equals(string)</c> is rewritten as a normal equality comparison.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>StartsWith(string, StringComparison.InvariantCultureIgnoreCase)</c> and <c>StartsWith(string)</c> are rewritten using a regex pattern anchored at the start.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <c>EndsWith(string, StringComparison.InvariantCultureIgnoreCase)</c> and <c>EndsWith(string)</c> are rewritten using a regex pattern anchored at the end.
        /// </description>
        /// </item>
        /// </list>
        /// Negated comparisons (prefixed with <c>!</c>) are preserved.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the corresponding <c>Contains</c> or <c>Any</c> method cannot be uniquely determined.
        /// </exception>
        [return: NotNullIfNotNull(nameof(node))]
        protected override Expression? VisitMethodCall(MethodCallExpression? node)
        {
            if (node == null)
                return null;

            /*
            // Handle string method calls: Equals, StartsWith, EndsWith.
            if (node.Method.DeclaringType == typeof(string))
            {
                if (node.Method.Name == "Equals")
                {
                    if (node.Object != null)
                    {
                        // Check for overload with one argument: instance.Equals(value)
                        if (node.Arguments.Count == 1)
                        {
                            // Simple equality comparison: object == value
                            return Expression.Equal(this.Visit(node.Object), this.Visit(node.Arguments[0]));
                        }
                        // Check for overload with two arguments: instance.Equals(value, StringComparison)
                        else if (node.Arguments.Count == 2)
                        {
                            var valueArg = node.Arguments[0];
                            var comparisonArg = node.Arguments[1];
                            var ignoreCase = false;
                            if (comparisonArg is ConstantExpression ce && ce.Value is StringComparison sc)
                            {
                                ignoreCase = sc is StringComparison.InvariantCultureIgnoreCase
                                             or StringComparison.CurrentCultureIgnoreCase
                                             or StringComparison.OrdinalIgnoreCase;
                            }

                            if (valueArg is ConstantExpression constValue && constValue.Value is string strValue)
                            {
                                if (ignoreCase)
                                {
                                    // Create regex pattern for full string match (equals) with ignore case.
                                    var pattern = $"^{Regex.Escape(strValue)}$";
                                    var regex = new BsonRegularExpression(pattern, "i");
                                    return Expression.Equal(this.Visit(node.Object), Expression.Constant(regex));
                                }
                                else
                                {
                                    // Normal equality comparison.
                                    return Expression.Equal(this.Visit(node.Object), this.Visit(valueArg));
                                }
                            }
                        }
                    }
                }
                else if (node.Method.Name == "StartsWith")
                {
                    if (node.Object != null && node.Arguments.Count >= 1)
                    {
                        var valueArg = node.Arguments[0];
                        var ignoreCase = false;
                        if (node.Arguments.Count == 2)
                        {
                            var comparisonArg = node.Arguments[1];
                            if (comparisonArg is ConstantExpression ce && ce.Value is StringComparison sc)
                            {
                                ignoreCase = sc is StringComparison.InvariantCultureIgnoreCase
                                             or StringComparison.CurrentCultureIgnoreCase
                                             or StringComparison.OrdinalIgnoreCase;
                            }
                        }
                        if (valueArg is ConstantExpression constValue && constValue.Value is string strValue)
                        {
                            // Create regex pattern anchored at the start.
                            var pattern = $"^{Regex.Escape(strValue)}";
                            var options = ignoreCase ? "i" : "";
                            var regex = new BsonRegularExpression(pattern, options);
                            return Expression.Equal(this.Visit(node.Object), Expression.Constant(regex));
                        }
                    }
                }
                else if (node.Method.Name == "EndsWith")
                {
                    if (node.Object != null && node.Arguments.Count >= 1)
                    {
                        var valueArg = node.Arguments[0];
                        var ignoreCase = false;
                        if (node.Arguments.Count == 2)
                        {
                            var comparisonArg = node.Arguments[1];
                            if (comparisonArg is ConstantExpression ce && ce.Value is StringComparison sc)
                            {
                                ignoreCase = sc is StringComparison.InvariantCultureIgnoreCase
                                             or StringComparison.CurrentCultureIgnoreCase
                                             or StringComparison.OrdinalIgnoreCase;
                            }
                        }
                        if (valueArg is ConstantExpression constValue && constValue.Value is string strValue)
                        {
                            // Create regex pattern anchored at the end.
                            var pattern = $"{Regex.Escape(strValue)}$";
                            var options = ignoreCase ? "i" : "";
                            var regex = new BsonRegularExpression(pattern, options);
                            return Expression.Equal(this.Visit(node.Object), Expression.Constant(regex));
                        }
                    }
                }
            }*/

            // Handle Any method calls.
            if (node.Method.Name == "Any" && node.Arguments.Count == 2)
            {
                // Extract the collection expression (assumed to be an in-memory collection)
                var collectionExpression = node.Arguments[0];

                // Retrieve the lambda expression by stripping any quote nodes.
                var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);

                // Check if the lambda body is a binary expression representing an equality comparison.
                // Example: y => y == value
                if (lambda.Body is BinaryExpression binary &&
                    binary.NodeType == ExpressionType.Equal)
                {
                    // The value to compare is expected to be on the right-hand side of the equality.
                    var valueToCompare = binary.Right;

                    // Attempt to determine the element type of the collection.
                    var elementType = collectionExpression.Type.GetGenericArguments().FirstOrDefault()
                                      ?? collectionExpression.Type.GetElementType();
                    if (elementType == null)
                    {
                        return base.VisitMethodCall(node);
                    }

                    // Retrieve the Enumerable.Contains<T> method for the determined element type.
                    var containsMethod = typeof(Enumerable).GetMethods()
                        .Where(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                        .Single()
                        .MakeGenericMethod(elementType);

                    // Create the new method call expression: collection.Contains(valueToCompare)
                    var newCall = Expression.Call(
                        containsMethod,
                        this.Visit(collectionExpression),
                        this.Visit(valueToCompare)
                    );

                    // Recursively visit the new expression in case further rewriting is needed.
                    return this.Visit(newCall);
                }
            }
            // Handle All method calls.
            else if (node.Method.Name == "All" && node.Arguments.Count == 2)
            {
                // Extract the collection expression and the lambda expression from the method call.
                var collectionExpression = node.Arguments[0];
                var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);

                // Create a new lambda expression that negates the original predicate.
                // For example, if the original predicate is r => predicate(r),
                // the negated lambda will be r => !predicate(r).
                var parameter = lambda.Parameters[0];
                Expression negatedBody = Expression.Not(lambda.Body);
                var negatedLambda = Expression.Lambda(negatedBody, parameter);

                // Retrieve the appropriate Any method (either Queryable.Any or Enumerable.Any) based on the source type.
                var anyMethod = GetAnyMethod(parameter.Type, collectionExpression.Type);
                if (anyMethod == null)
                {
                    return base.VisitMethodCall(node);
                }

                // Create the method call expression for: collection.Any(r => !predicate(r))
                Expression anyCall = Expression.Call(
                    anyMethod,
                    this.Visit(collectionExpression),
                    Expression.Quote(negatedLambda)
                );

                // Negate the result of the Any call, effectively representing: !collection.Any(r => !predicate(r))
                return Expression.Not(anyCall);
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Retrieves the appropriate <c>Any</c> method for a given element type and source type.
        /// </summary>
        /// <param name="elementType">The type of the elements in the collection.</param>
        /// <param name="sourceType">
        /// The type of the collection source, which may implement either <see cref="IQueryable"/> or <see cref="IEnumerable"/>.
        /// </param>
        /// <returns>
        /// A <see cref="MethodInfo"/> representing the generic <c>Any</c> method that matches the source type.
        /// </returns>
        /// <remarks>
        /// If the source type implements <see cref="IQueryable"/>, the method from <see cref="Queryable"/> is returned;
        /// otherwise, the method from <see cref="Enumerable"/> is used.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the corresponding <c>Any</c> method cannot be uniquely determined.
        /// </exception>
        private static MethodInfo GetAnyMethod(Type elementType, Type sourceType)
        {
            if (typeof(IQueryable).IsAssignableFrom(sourceType))
            {
                return typeof(Queryable).GetMethods()
                    .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(elementType);
            }
            else
            {
                return typeof(Enumerable).GetMethods()
                    .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(elementType);
            }
        }

        /// <summary>
        /// Recursively removes any <c>Quote</c> expressions from the given expression.
        /// </summary>
        /// <param name="e">The expression from which to remove quotes.</param>
        /// <returns>The unquoted expression.</returns>
        /// <example>
        /// Given an expression of type <c>UnaryExpression</c> wrapping a lambda expression,
        /// this method will return the underlying lambda expression.
        /// </example>
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }

    /// <summary>
    /// A dynamic expression visitor for handling dynamic expressions.
    /// This implementation can be extended to support additional dynamic rewriting if necessary.
    /// </summary>
    public class DynamicExpressionVisitorRewriter : DynamicExpressionVisitor
    {

        /// <summary>
        /// Visits a DynamicExpression node and rewrites its arguments.
        /// </summary>
        /// <param name="node">The dynamic expression node.</param>
        /// <returns>
        /// A rewritten dynamic expression if any arguments changed; otherwise, the original node.
        /// </returns>
        [return: NotNullIfNotNull(nameof(node))]
        protected override Expression? VisitDynamic(DynamicExpression? node)
        {
            if (node == null)
                return null;

            // Visit each argument of the dynamic expression.
            var newArgs = node.Arguments.Select(arg => this.Visit(arg)).ToArray();

            // If any argument has changed, update the dynamic expression.
            if (!newArgs.SequenceEqual(node.Arguments))
            {
                // Assuming the DynamicExpression type provides an Update method.
                return node.Update(newArgs);
            }
            return node;
        }

        /// <summary>
        /// Visits an expression. If the expression is a DynamicExpression, it is processed via VisitDynamic;
        /// otherwise, the base implementation is used.
        /// </summary>
        /// <param name="node">The expression node to visit.</param>
        /// <returns>The visited (and possibly rewritten) expression.</returns>
        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node == null)
                return null;

            if (node is DynamicExpression dynamicExpression)
            {
                return this.VisitDynamic(dynamicExpression);
            }
            return base.Visit(node);
        }
    }
}
