using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Expression.Core.Visitors
{
    public class InitMemberExtractor : ExpressionVisitor<Dictionary<MemberInfo, System.Linq.Expressions.Expression>>
    {
        static readonly InitMemberExtractor _extractor = new InitMemberExtractor();
        InitMemberExtractor()
        {
        }
        public static Dictionary<MemberInfo, System.Linq.Expressions.Expression> Extract(System.Linq.Expressions.Expression exp)
        {
            return _extractor.Visit(exp);
        }
        public override Dictionary<MemberInfo, System.Linq.Expressions.Expression> Visit(System.Linq.Expressions.Expression exp)
        {
            if (exp == null)
                return null;

            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }
        protected override Dictionary<MemberInfo, System.Linq.Expressions.Expression> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override Dictionary<MemberInfo, System.Linq.Expressions.Expression> VisitMemberInit(MemberInitExpression exp)
        {
            Dictionary<MemberInfo, System.Linq.Expressions.Expression> ret = new Dictionary<MemberInfo, System.Linq.Expressions.Expression>(exp.Bindings.Count);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;

                ret.Add(member, memberAssignment.Expression);
            }

            return ret;
        }
    }
}
