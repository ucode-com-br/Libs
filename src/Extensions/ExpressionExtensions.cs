using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static UCode.Extensions.ExpressionExtensions;

namespace UCode.Extensions
{
    public static class ExpressionExtensions
    {
        static ExpressionExtensions()
        {
        }

        //public static Expression<Func<TElement, TResult>> ReplaceParamToConstant<TElement, TResult>(this Expression<Func<TElement, TElement, TResult>> inputExpression, int index, TElement elementValue) => ReplaceParamToConstant(inputExpression, p => p[index], elementValue);
        //{
        //var replacer = new Replacer(inputExpression.Parameters[index], Expression.Constant(elementValue, typeof(TElement)));
        //var body = replacer.Visit(inputExpression.Body);
        //return Expression.Lambda<Func<TElement, TResult>>(body,
        //    inputExpression.Parameters[]);
        //}
        public struct ParameterExpressionItem
        {
            internal ParameterExpressionItem(ParameterExpression parameterExpression, int index)
            {
                this.ParameterExpression = parameterExpression;
                this.Index = index;
            }

            public readonly ParameterExpression ParameterExpression;
            public readonly int Index;

            public IReplaceParam? ReplaceParam {
                get;
                private set;
            }

            public void Constant<TConstant>(TConstant? constantValue) => this.ReplaceParam = this.ParameterExpression.ToConstant<TConstant>(constantValue);

            public static implicit operator ParameterExpression(ParameterExpressionItem source) => source.ParameterExpression;

            public static implicit operator int(ParameterExpressionItem source) => source.Index;
        }



        public static IReplaceParam ToConstant<TConstant>(this ParameterExpression parameterExpression, TConstant? constantValue)
        {
            return new ReplaceParam<TConstant>(parameterExpression, constantValue);
        }

        public delegate bool ParameterExpressiomDelegate(ref ParameterExpressionItem arg);


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
        /// Replace Expression parameters to constraint
        /// </summary>
        /// <typeparam name="TFunc"></typeparam>
        /// <typeparam name="TConstant"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="inputExpression"></param>
        /// <param name="selectParamsToReplace">
        /// Function to replace param to constraint, see example to parse all parameters to constrain null
        /// ex:
        ///     (readOnlyCollectionParameterExpression) => readOnlyCollectionParameterExpression.Where(parameterExpression => parameterExpression.ToConstant(null));
        /// </param>
        /// <returns></returns>
        [return: NotNull]
        public static Expression<TResult> ReplaceToConstant<TFunc, TConstant, TResult>([NotNull] this Expression<TFunc> inputExpression, [NotNull] Func<ReadOnlyCollection<ParameterExpression>, IEnumerable<IReplaceParam>> selectParamsToReplace)
            where TFunc : Delegate
        {
            LambdaExpression ExpressionResult = inputExpression;

            var replaceParameters = selectParamsToReplace.Invoke(inputExpression.Parameters);

            foreach (var replaceParameter in replaceParameters)
            {
                Replacer replacer = new Replacer(replaceParameter.Parameter, replaceParameter.Constant);

                var body = replacer.Visit(ExpressionResult.Body);

                var newParameters = ExpressionResult.Parameters.Where(w => w != replaceParameter.Parameter).ToArray();

                ExpressionResult = Expression.Lambda<TResult>(body, newParameters);
            }

            return (Expression<TResult>)ExpressionResult;
            //Replacer replacer;
            //if (parameterSelected.Type == typeof(TConstant))
            //{
            //    replacer = new Replacer(parameterSelected, Expression.Constant(elementValue, typeof(TConstant)));
            //}
            //else
            //{
            //    var newElementValue = Convert.ChangeType(elementValue, parameterSelected.Type, System.Globalization.CultureInfo.InvariantCulture);

            //    replacer = new Replacer(parameterSelected, Expression.Constant(newElementValue, typeof(TConstant)));
            //}

            //var body = replacer.Visit(inputExpression.Body);

            //var newParameters = inputExpression.Parameters.Where(w => w != parameterSelected).ToArray();

            //return Expression.Lambda<TResult>(body, newParameters);
        }

        [return: NotNull]
        public static Expression<TResult> ReplaceToConstant<TFunc, TConstant, TResult>([NotNull] this Expression<TFunc> inputExpression, [NotNull] Func<ReadOnlyCollection<ParameterExpression>, IReplaceParam> selectParamToReplace)
            where TFunc : Delegate
        {
            return ReplaceToConstant<TFunc, TConstant, TResult>(inputExpression, (col) => new IReplaceParam[1] { selectParamToReplace.Invoke(col) });
        }

        public interface IReplaceParam
        {
            public ConstantExpression Constant
            {
                get;
            }
            public ParameterExpression Parameter
            {
                get;
            }

            public bool IsValid
            {
                get;
            }
        }

        private readonly struct ReplaceParam<TConstant>: IReplaceParam
        {
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

            public ConstantExpression Constant
            {
                get;
            }
            public ParameterExpression Parameter
            {
                get;
            }

            public bool IsValid
            {
                get;
            }

            public static implicit operator ConstantExpression(ReplaceParam<TConstant> source) => source.Constant;

            public static implicit operator ParameterExpression(ReplaceParam<TConstant> source) => source.Parameter;
        }

        private class Replacer : ExpressionVisitor
        {
            private readonly Expression _from;
            private readonly Expression _to;

            public Replacer([NotNull] Expression from, [NotNull] Expression to): base()
            {
                this._from = from;
                this._to = to;
            }

            [return: NotNull]
            public override Expression Visit([NotNull] Expression node)
            {
                return node == this._from ? this._to : base.Visit(node);
            }
        }
    }
}
