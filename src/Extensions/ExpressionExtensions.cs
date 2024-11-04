using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace UCode.Extensions
{
    /// <summary>
    /// Provides extension methods for working with expressions in C#.
    /// This static class contains methods that enhance the functionality
    /// of expression trees, allowing for more intuitive and powerful 
    /// manipulations of expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Initializes the static members of the <see cref="ExpressionExtensions"/> class.
        /// This constructor is called once before any static members are accessed or 
        /// any instances of the class are created.
        /// </summary>
        static ExpressionExtensions()
        {
        }


        /// <summary>
        /// Represents an item containing a parameter expression and its associated index.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression associated with this item.</param>
        /// <param name="index">The index of the parameter expression.</param>
        public struct ParameterExpressionItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterExpressionItem"/> class.
            /// </summary>
            /// <param name="parameterExpression">
            /// The <see cref="ParameterExpression"/> associated with this item.
            /// </param>
            /// <param name="index">
            /// An integer representing the index of the <see cref="ParameterExpression"/>.
            /// </param>
            internal ParameterExpressionItem(ParameterExpression parameterExpression, int index)
            {
                this.ParameterExpression = parameterExpression;
                this.Index = index;
            }

            public readonly ParameterExpression ParameterExpression;
            public readonly int Index;

            /// <summary>
            /// Gets a value indicating the parameter to be replaced, if available.
            /// </summary>
            /// <value>
            /// An instance of <see cref="IReplaceParam"/>, or <c>null</c> if no parameter is set.
            /// </value>
            /// <remarks>
            /// The property is read-only from outside the class, as the set accessor is 
            public IReplaceParam? ReplaceParam {
                get;
                private set;
            }

            /// <summary>
            /// Replaces the current parameter expression with a constant value of type <typeparamref name="TConstant"/>.
            /// </summary>
            /// <typeparam name="TConstant">
            /// The type of the constant value.
            /// </typeparam>
            /// <param name="constantValue">
            /// The constant value to replace the parameter expression with. It can be null if <typeparamref name="TConstant"/> is a nullable type.
            /// </param>
            public void Constant<TConstant>(TConstant? constantValue) => this.ReplaceParam = this.ParameterExpression.ToConstant<TConstant>(constantValue);

            public static implicit operator ParameterExpression(ParameterExpressionItem source) => source.ParameterExpression;

            public static implicit operator int(ParameterExpressionItem source) => source.Index;
        }



        /// <summary>
        /// Converts a given <see cref="ParameterExpression"/> to a constant replacement parameter.
        /// </summary>
        /// <param name="parameterExpression">
        /// The <see cref="ParameterExpression"/> to be converted to a constant replacement parameter.
        /// </param>
        /// <param name="constantValue">
        /// The constant value to replace the specified parameter expression. This value can be null.
        /// </param>
        /// <returns>
        /// An instance of <see cref="IReplaceParam"/> representing the constant replacement of the parameter expression.
        /// </returns>
        public static IReplaceParam ToConstant<TConstant>(this ParameterExpression parameterExpression, TConstant? constantValue)
        {
            return new ReplaceParam<TConstant>(parameterExpression, constantValue);
        }

        /// <summary>
        /// Represents a delegate that defines a method for processing 
        /// a reference of a <see cref="ParameterExpressionItem"/> 
        /// and returning a boolean value.
        /// </summary>
        /// <param name="arg">A reference to a <see cref="ParameterExpressionItem"/> 
        /// that will be passed to the delegate's method for evaluation.</param>
        /// <returns>A boolean value that indicates the result of the evaluation 
        /// of the <see cref="ParameterExpressionItem"/>.</returns>
        public delegate bool ParameterExpressiomDelegate(ref ParameterExpressionItem arg);


        /// <summary>
        /// Filters a collection of <see cref="ParameterExpression"/> based on a specified predicate.
        /// </summary>
        /// <param name="parameterExpressions">
        /// A <see cref="ReadOnlyCollection{ParameterExpression}"/> of parameter expressions to filter.
        /// </param>
        /// <param name="where">
        /// A delegate that defines the conditions of the filter. It takes a reference to a <see cref="ParameterExpressionItem"/> 
        /// and returns a boolean indicating whether the item meets the conditions.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{IReplaceParam}"/> containing the filtered items that satisfy the predicate.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the `ReplaceParam` property of an item is null.
        /// </exception>
        public static IEnumerable<IReplaceParam> Where(this ReadOnlyCollection<ParameterExpression> parameterExpressions, ParameterExpressiomDelegate where)
        {
            for (int i = 0; i < parameterExpressions.Count; i++)
            {
                var item = new ParameterExpressionItem(parameterExpressions[i], i);

                if (where.Invoke(ref item))
                {
                    if(item.ReplaceParam == null)
                        throw new ArgumentNullException("Object ReplaceParam from item is null.");

                    yield return item.ReplaceParam!;
                }
            }
        }


        /// <summary>
        /// Replaces parameters in the given expression with constant values based on the provided selector function.
        /// </summary>
        /// <typeparam name="TFunc">The type of the delegate represented by the input expression.</typeparam>
        /// <typeparam name="TConstant">The type of the constant value used for replacement.</typeparam>
        /// <typeparam name="TResult">The type of the result of the expression after replacement.</typeparam>
        /// <param name="inputExpression">The expression in which parameters are to be replaced.</param>
        /// <param name="selectParamsToReplace">A function that selects parameters to replace.</param>
        /// <returns>An expression of type <typeparamref name="TResult"/> with the specified parameters replaced by constants.</returns>
        [return: NotNull]
        public static Expression<TResult> ReplaceToConstant<TFunc, TConstant, TResult>([NotNull] this Expression<TFunc> inputExpression, [NotNull] Func<ReadOnlyCollection<ParameterExpression>, IEnumerable<IReplaceParam>> selectParamsToReplace)
            where TFunc : Delegate
        {
            LambdaExpression ExpressionResult = inputExpression;

            var replaceParameters = selectParamsToReplace.Invoke(inputExpression.Parameters);

            foreach (var replaceParameter in replaceParameters)
            {
                var replacer = new Replacer(replaceParameter.Parameter, replaceParameter.Constant);

                var body = replacer.Visit(ExpressionResult.Body);

                var newParameters = ExpressionResult.Parameters.Where(w => w != replaceParameter.Parameter).ToArray();

                ExpressionResult = Expression.Lambda<TResult>(body, newParameters);
            }

            return (Expression<TResult>)ExpressionResult;
        }

        /// <summary>
        /// Replaces parameters in the given expression with a constant value based on the provided selection function.
        /// </summary>
        /// <typeparam name="TFunc">The type of the delegate representing the input expression.</typeparam>
        /// <typeparam name="TConstant">The type of the constant that will replace the parameters.</typeparam>
        /// <typeparam name="TResult">The type of the result of the expression.</typeparam>
        /// <param name="inputExpression">The input expression to modify.</param>
        /// <param name="selectParamToReplace">A function that selects which parameters to replace with a constant.</param>
        /// <returns>An expression of type TResult with parameters replaced by constants.</returns>
        [return: NotNull]
        public static Expression<TResult> ReplaceToConstant<TFunc, TConstant, TResult>([NotNull] this Expression<TFunc> inputExpression, [NotNull] Func<ReadOnlyCollection<ParameterExpression>, IReplaceParam> selectParamToReplace)
            where TFunc : Delegate
        {
            return ReplaceToConstant<TFunc, TConstant, TResult>(inputExpression, (col) => new IReplaceParam[1] { selectParamToReplace.Invoke(col) });
        }

        /// <summary>
        /// Represents a contract for classes that provide functionality to replace parameters.
        /// </summary>
        /// <remarks>
        /// Implementing this interface allows a class to substitute placeholder parameters
        /// with their actual values, facilitating dynamic adjustments in various contexts,
        /// such as string formatting or template processing.
        /// </remarks>
        public interface IReplaceParam
        {
            /// <summary>
            /// Represents a constant expression. The property allows retrieval of the constant expression 
            /// associated with the object, but does not allow modification since it only has a getter.
            /// </summary>
            /// <value>
            /// A <see cref="ConstantExpression"/> instance that represents the constant expression.
            /// </value>
            /// <remarks>
            /// This property is read-only and is typically implemented as part of a larger expression tree 
            /// structure, where constant values are used within various expressions.
            /// </remarks>
            public ConstantExpression Constant
            {
                get;
            }
            /// <summary>
            /// Represents a parameter expression in the expression tree.
            /// This property provides access to a ParameterExpression that is utilized 
            /// within the context of an expression tree, enabling the dynamic composition 
            /// of lambda expressions and other expressions requiring parameters.
            /// </summary>
            /// <value>
            /// A <see cref="ParameterExpression"/> that represents the parameter in the expression tree.
            /// </value>
            public ParameterExpression Parameter
            {
                get;
            }

            /// <summary>
            /// Gets a value indicating whether the current instance is valid.
            /// </summary>
            /// <value>
            /// True if the instance is valid; otherwise, false.
            /// </value>
            public bool IsValid
            {
                get;
            }
        }

        /// <summary>
        /// A readonly struct that represents a replacement parameter for expressions. 
        /// Holds a parameter expression and a constant value, and handles validation 
        /// of the types of these values.
        /// </summary>
        /// <typeparam name="TConstant">The type of the constant value associated with the parameter.</typeparam>
        private readonly struct ReplaceParam<TConstant>: IReplaceParam
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReplaceParam"/> class,
            /// setting the parameter and checking the validity of the constant value.
            /// </summary>
            /// <param name="parameterExpression">The parameter expression to be replaced.</param>
            /// <param name="constantValue">The constant value to assign to the parameter.</param>
            /// <remarks>
            /// The constructor checks if the type of the parameter matches the constant value type.
            /// If they match, it sets <see cref="IsValid"/> to true and creates a constant expression.
            /// If they do not match, it attempts to convert the constant value to the type of the parameter.
            /// If the conversion succeeds, it sets <see cref="IsValid"/> to true and creates a constant expression.
            /// If the conversion fails, it sets <see cref="IsValid"/> to false.
            /// </remarks>
            public ReplaceParam(ParameterExpression parameterExpression, TConstant? constantValue)
            {
                this.Parameter = parameterExpression;

                if (parameterExpression.Type == typeof(TConstant))
                {
                    this.IsValid = true;
                    this.Constant = Expression.Constant(constantValue, typeof(TConstant));
                }
                else if (!TryExpression.IsThrowingException(() => Convert.ChangeType(constantValue, parameterExpression.Type, System.Globalization.CultureInfo.InvariantCulture), out var newConstantValue))
                {
                    this.IsValid = true;
                    this.Constant = Expression.Constant(newConstantValue, typeof(TConstant));
                }
                else
                {
                    this.IsValid = false;
                }
            }

            /// <summary>
            /// Gets the constant expression associated with this instance.
            /// </summary>
            /// <value>
            /// A <see cref="ConstantExpression"/> that represents the constant value.
            /// </value>
            public ConstantExpression Constant
            {
                get;
            }
            /// <summary>
            /// Gets the <see cref="ParameterExpression"/> associated with this instance.
            /// </summary>
            /// <value>
            /// A <see cref="ParameterExpression"/> that represents a parameter in the expression tree.
            /// </value>
            public ParameterExpression Parameter
            {
                get;
            }

            /// <summary>
            /// Gets a value indicating whether the current instance is valid.
            /// </summary>
            /// <value>
            /// <c>true</c> if the instance is valid; otherwise, <c>false</c>.
            /// </value>
            /// <remarks>
            /// This property is typically used to determine if an object has been properly initialized 
            /// or meets certain criteria to be considered valid for use.
            /// </remarks>
            public bool IsValid
            {
                get;
            }

            public static implicit operator ConstantExpression(ReplaceParam<TConstant> source) => source.Constant;

            public static implicit operator ParameterExpression(ReplaceParam<TConstant> source) => source.Parameter;
        }

        /// <summary>
        /// The <c>Replacer</c> class is a 
        private class Replacer : ExpressionVisitor
        {
            private readonly Expression _from;
            private readonly Expression _to;

            /// <summary>
            /// Initializes a new instance of the <see cref="Replacer"/> class with specified expressions.
            /// </summary>
            /// <param name="from">
            /// The expression to be replaced. This parameter is required and cannot be null.
            /// </param>
            /// <param name="to">
            /// The expression to replace the original expression. This parameter is also required and cannot be null.
            /// </param>
            /// <returns>
            /// An instance of the <see cref="Replacer"/> class.
            /// </returns>
            public Replacer([NotNull] Expression from, [NotNull] Expression to): base()
            {
                this._from = from;
                this._to = to;
            }

            /// <summary>
            /// Visits the specified <see cref="Expression"/> node and replaces it with another expression if it matches a predefined reference.
            /// </summary>
            /// <param name="node">The <see cref="Expression"/> node to visit.</param>
            /// <returns>
            /// Returns the modified <see cref="Expression"/> node if it is the same as the reference node; otherwise, returns the result of the base visit method.
            /// </returns>
            [return: NotNull]
            public override Expression Visit([NotNull] Expression node)
            {
                return node == this._from ? this._to : base.Visit(node);
            }
        }
    }
}
