// Licensed to .NET HTTP Abstractions (the "Project") under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The Project licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//  
//   http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Taco.Startup {
    public static class Coerce {
        public static object CoerceDelegate(Type neededDelegateType, object existingDelegate) {
            // simplest case - same delegate type
            if (neededDelegateType.IsAssignableFrom(existingDelegate.GetType()))
                return existingDelegate;

            var neededParameters = DelegateParameters(neededDelegateType);
            var existingParameters = DelegateParameters(existingDelegate.GetType());

            var parameters = neededParameters.Zip(existingParameters, (neededParameterInfo, existingParameterInfo) => {
                var neededParameter = Expression.Parameter(neededParameterInfo.ParameterType, neededParameterInfo.Name);
                var existingArgument = CoerceExpression(existingParameterInfo.ParameterType, neededParameter);
                return new {neededParameter, existingArgument, neededParameterInfo, existingParameterInfo};
            }).ToArray();

            // next simplest case - delegates with the same arguments types
            if (parameters.All(pi => pi.existingParameterInfo.ParameterType == pi.neededParameterInfo.ParameterType)) {
                return Delegate.CreateDelegate(neededDelegateType, existingDelegate, existingDelegate.GetType().GetMethod("Invoke"));
            }

            // tricky case - need a lightweight method to coerce some arguments
            var body = Expression.Invoke(Expression.Constant(existingDelegate), parameters.Select(p => p.existingArgument));
            var lambda = Expression.Lambda(neededDelegateType, body, parameters.Select(p => p.neededParameter));
            return lambda.Compile();
        }

        public static TNeededDelegate CoerceDelegate<TNeededDelegate>(object existingDelegate) {
            return (TNeededDelegate)CoerceDelegate(typeof(TNeededDelegate), existingDelegate);
        }

        static Expression CoerceExpression(Type neededType, Expression existingExpression) {
            if (neededType == existingExpression.Type)
                return existingExpression;

            if (typeof(Delegate).IsAssignableFrom(neededType) &&
                typeof(Delegate).IsAssignableFrom(existingExpression.Type)) {
                return CoerceDelegateExpression(neededType, existingExpression);
            }

            throw new ApplicationException("Coerce failed");
        }

        static Expression CoerceDelegateExpression(Type neededDelegateType, Expression existingDelegate) {
            if (neededDelegateType == existingDelegate.Type)
                return existingDelegate;

            var parameterEquality = DelegateParameters(neededDelegateType).Zip(DelegateParameters(existingDelegate.Type), (p1, p2) => p1.ParameterType == p2.ParameterType);
            if (parameterEquality.All(istrue => istrue))
                return CoerceDelegateExpressionWithSameArguments(neededDelegateType, existingDelegate);

            throw new ApplicationException("Coerce failed");
        }

        static Expression CoerceDelegateExpressionWithSameArguments(Type newDelegateType, Expression existingDelegate) {
            var createDelegate = Methodof((Type type, object target, MethodInfo method) => Delegate.CreateDelegate(type, target, method));

            var existingDelegateInvoke = existingDelegate.Type.GetMethod("Invoke");

            return Expression.Convert(
                Expression.Call(null,
                    createDelegate,
                    Expression.Constant(newDelegateType, typeof(Type)),
                    existingDelegate,
                    Expression.Constant(existingDelegateInvoke, typeof(MethodInfo))),
                newDelegateType);
        }

        static IEnumerable<ParameterInfo> DelegateParameters(Type delegateType) {
            return delegateType.GetMethod("Invoke").GetParameters();
        }

        static MethodInfo Methodof<T1, T2, T3, T>(Expression<Func<T1, T2, T3, T>> expr) {
            return ((MethodCallExpression)expr.Body).Method;
        }
    }
}