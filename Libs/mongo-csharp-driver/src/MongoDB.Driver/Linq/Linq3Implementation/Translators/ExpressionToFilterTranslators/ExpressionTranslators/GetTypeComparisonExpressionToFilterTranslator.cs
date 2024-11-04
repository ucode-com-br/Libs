﻿/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.ExtensionMethods;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;
using MongoDB.Driver.Linq.Linq3Implementation.Reflection;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ToFilterFieldTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToFilterTranslators.ExpressionTranslators
{
    internal static class GetTypeComparisonExpressionToFilterTranslator
    {
        // caller is responsible for ensuring constant is on the right
        public static bool CanTranslate(Expression leftExpression, Expression rightExpression)
        {
            return
                leftExpression is MethodCallExpression methodCallExpression &&
                methodCallExpression.Method.Is(ObjectMethod.GetType) &&
                rightExpression is ConstantExpression;
        }

        public static AstFilter Translate(TranslationContext context, BinaryExpression expression, MethodCallExpression getTypeExpression, Expression typeConstantExpression)
        {
            var field = ExpressionToFilterFieldTranslator.Translate(context, getTypeExpression.Object);
            var nominalType = field.Serializer.ValueType;
            var actualType = typeConstantExpression.GetConstantValue<Type>(expression);

            var discriminatorConvention = field.Serializer.GetDiscriminatorConvention();
            var discriminatorField = field.SubField(discriminatorConvention.ElementName, BsonValueSerializer.Instance);

            return discriminatorConvention switch
            {
                IHierarchicalDiscriminatorConvention hierarchicalDiscriminatorConvention => DiscriminatorAstFilter.TypeEquals(discriminatorField, hierarchicalDiscriminatorConvention, nominalType, actualType),
                _ => DiscriminatorAstFilter.TypeEquals(discriminatorField, discriminatorConvention, nominalType, actualType),
            };
        }
    }
}