using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ReHackt.Queryable.Extensions
{
    public class QueryableFilter<T>
    {
        private const string TokenPattern = @"(""[^""]+""|(\d+\.\d+)|\w+|\(|\))\s*";
        private const string OpenParenthesisPattern = @"^\($";
        private const string ClosedParenthesisPattern = @"^\)$";
        private const string BooleanOperatorPattern = "^and|or$";
        private const string ComparisonOperatorPattern = "^eq|lt|gt|in$";
        private const string BooleanValuePattern = @"^true|false$";
        private const string NumberValuePattern = @"^(\d+\.\d+)|\d+$";
        private const string StringValuePattern = @"^""[^""]+""$";

        private readonly MethodInfo _containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
        private readonly ParameterExpression _item = Expression.Parameter(typeof(T));
        private readonly Expression<Func<T, bool>> _expression;

        private QueryableFilter() { }

        private QueryableFilter(Element element)
            => _expression = Expression.Lambda<Func<T, bool>>(GetExpression(element), _item);

        public static bool TryParse(string query, out QueryableFilter<T> filter)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                filter = new QueryableFilter<T>();
                return true;
            }
            try
            {
                var tokens = Regex.Matches(query, TokenPattern).Select(m => m.Groups[1].Value);
                var elements = ParseTokens(tokens, true)
                    .HandleComparisonElements()
                    .HandleBooleanElements(BooleanOperator.And)
                    .HandleBooleanElements(BooleanOperator.Or);
                if (elements.Count == 1)
                {
                    filter = new QueryableFilter<T>(elements.First().CleanGroups());
                    return true;
                }
                throw new InvalidOperationException();
            }
            catch
            {
                filter = null;
                return false;
            }
        }

        public IQueryable<T> Apply(IQueryable<T> queryable)
        {
            return queryable.WhereIf(_expression != null, _expression);
        }

        private static IList<Element> ParseTokens(IEnumerable<string> tokens, bool firstLevel = false)
        {
            var elements = new List<Element>();
            int level = 0;
            int index = 0;
            foreach (var token in tokens)
            {
                if (Regex.IsMatch(token, ClosedParenthesisPattern))
                {
                    level--;
                    if (level < 0)
                    {
                        if (firstLevel) throw new InvalidOperationException(); else break;
                    }
                }
                else if (Regex.IsMatch(token, OpenParenthesisPattern))
                {
                    if (level == 0)
                    {
                        elements.Add(new Group(ParseTokens(tokens.Skip(index + 1).ToList())));
                    }
                    level++;
                }
                else if (level == 0)
                {
                    if (Regex.IsMatch(token, BooleanOperatorPattern, RegexOptions.IgnoreCase))
                    {
                        elements.Add(new Element
                        {
                            Type = ElementType.BooleanOperator,
                            Value = token.ToLowerInvariant() switch
                            {
                                "and" => BooleanOperator.And,
                                "or" => BooleanOperator.Or,
                                _ => throw new NotImplementedException()
                            }
                        });
                    }
                    else if (Regex.IsMatch(token, ComparisonOperatorPattern, RegexOptions.IgnoreCase))
                    {
                        elements.Add(new Element
                        {
                            Type = ElementType.ComparisonOperator,
                            Value = token.ToLowerInvariant() switch
                            {
                                "eq" => ComparisonOperator.Equal,
                                "gt" => ComparisonOperator.GreaterThan,
                                "lt" => ComparisonOperator.LessThan,
                                "in" => ComparisonOperator.Contains,
                                _ => throw new NotImplementedException()
                            }
                        });
                    }
                    else if (Regex.IsMatch(token, BooleanValuePattern, RegexOptions.IgnoreCase))
                    {
                        elements.Add(new Element
                        {
                            Type = ElementType.Value,
                            Value = bool.Parse(token)
                        });
                    }
                    else if (Regex.IsMatch(token, NumberValuePattern))
                    {
                        elements.Add(new Element
                        {
                            Type = ElementType.Value,
                            Value = double.Parse(token, NumberFormatInfo.InvariantInfo)
                        });
                    }
                    else if (Regex.IsMatch(token, StringValuePattern))
                    {
                        object value = token.Trim('"');
                        if (DateTime.TryParse(value.ToString(), out DateTime dateValue)) { value = dateValue; }
                        elements.Add(new Element
                        {
                            Type = ElementType.Value,
                            Value = value
                        });
                    }
                    else
                    {
                        elements.Add(new Element
                        {
                            Type = ElementType.Property,
                            Value = token
                        });
                    }
                }
                index++;
            }
            if (level > 0)
            {
                throw new InvalidOperationException();
            }
            return elements;
        }

        private Expression GetExpression(Element element)
        {
            switch (element)
            {
                case BooleanQueryClause booleanQueryClause:
                    return booleanQueryClause.Operator switch
                    {
                        BooleanOperator.And => Expression.AndAlso(GetExpression(booleanQueryClause.Item1), GetExpression(booleanQueryClause.Item2)),
                        BooleanOperator.Or => Expression.OrElse(GetExpression(booleanQueryClause.Item1), GetExpression(booleanQueryClause.Item2)),
                        _ => throw new NotImplementedException()
                    };
                case ComparisonQueryClause comparisonQueryClause:
                    var item1Expression = GetExpression(comparisonQueryClause.Item1);
                    var item2Expression = GetExpression(comparisonQueryClause.Item2);
                    var item1IsProperty = comparisonQueryClause.Item1.Type == ElementType.Property;
                    if (item1IsProperty)
                        item2Expression = Expression.Convert(item2Expression, item1Expression.Type);
                    else
                        item1Expression = Expression.Convert(item1Expression, item2Expression.Type);
                    return comparisonQueryClause.Operator switch
                    {
                        ComparisonOperator.Equal => Expression.Equal(item1Expression, item2Expression),
                        ComparisonOperator.GreaterThan => Expression.GreaterThan(item1Expression, item2Expression),
                        ComparisonOperator.LessThan => Expression.LessThan(item1Expression, item2Expression),
                        ComparisonOperator.Contains => Expression.Call(item1IsProperty ? item1Expression : item2Expression, _containsMethod, item1IsProperty ? item2Expression : item1Expression),
                        _ => throw new NotImplementedException()
                    };
                case Element value when value.Type == ElementType.Value:
                    return Expression.Constant(value.Value);
                case Element property when property.Type == ElementType.Property:
                    return Expression.Property(_item, property.Value.ToString());
                default:
                    throw new InvalidOperationException();
            }
        }
    }



    public static class ElementExtensions
    {
        public static IList<Element> HandleComparisonElements(this IList<Element> elements)
        {
            var list = new List<Element>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Type == ElementType.Group)
                {
                    list.Add(new Group(((Group)elements[i]).Children.HandleComparisonElements()));
                }
                else if (elements[i].Type == ElementType.ComparisonOperator)
                {
                    if ((elements[i - 1].Type == ElementType.Property && elements[i + 1].Type == ElementType.Value)
                        || (elements[i - 1].Type == ElementType.Value && elements[i + 1].Type == ElementType.Property))
                    {
                        list.RemoveAt(list.Count - 1);
                        list.Add(new ComparisonQueryClause((ComparisonOperator)elements[i].Value, elements[i - 1], elements[i + 1]));
                        i++;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    list.Add(elements[i]);
                }
            }
            return list;
        }

        public static IList<Element> HandleBooleanElements(this IList<Element> elements, BooleanOperator booleanOperator)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is Group group)
                {
                    group.Children = group.Children.HandleBooleanElements(booleanOperator);
                }
                else if (elements[i] is BooleanQueryClause clause)
                {
                    clause.Item1 = clause.Item1.HandleBooleanElements(booleanOperator);
                    clause.Item2 = clause.Item2.HandleBooleanElements(booleanOperator);
                }
                else if (elements[i].Type == ElementType.BooleanOperator && elements[i].Value as BooleanOperator? == booleanOperator)
                {
                    if ((elements[i - 1].Type == ElementType.BooleanQueryClause || elements[i - 1].Type == ElementType.ComparisonQueryClause || elements[i - 1].Type == ElementType.Group)
                        && (elements[i + 1].Type == ElementType.BooleanQueryClause || elements[i + 1].Type == ElementType.ComparisonQueryClause || elements[i + 1].Type == ElementType.Group))
                    {
                        if (elements[i + 1] is Group group1)
                        {
                            group1.Children = group1.Children.HandleBooleanElements(booleanOperator);
                        }
                        else if (elements[i + 1] is BooleanQueryClause clause1)
                        {
                            clause1.Item1 = clause1.Item1.HandleBooleanElements(booleanOperator);
                            clause1.Item2 = clause1.Item2.HandleBooleanElements(booleanOperator);
                        }

                        var list = new List<Element>();
                        list.AddRange(elements.Take(i - 2));
                        list.Add(new BooleanQueryClause(booleanOperator, elements[i - 1], elements[i + 1]));
                        list.AddRange(elements.Skip(i + 2));
                        return list.HandleBooleanElements(booleanOperator);
                    }
                    else throw new InvalidOperationException();
                }
            }
            return elements;
        }

        public static Element HandleBooleanElements(this Element element, BooleanOperator booleanOperator)
        {
            return new List<Element> { element }.HandleBooleanElements(booleanOperator).First();
        }

        public static Element CleanGroups(this Element element)
        {
            if (element is BooleanQueryClause clause)
            {
                clause.Item1 = clause.Item1.CleanGroups();
                clause.Item2 = clause.Item2.CleanGroups();
            }
            else if (element is Group group)
            {
                if (group.Children.Count == 1)
                {
                    element = group.Children.First().CleanGroups();
                }
                else throw new InvalidOperationException();
            }
            return element;
        }
    }



    public class Element
    {
        public ElementType Type { get; set; }

        public object Value { get; set; }
    }

    public class Group : Element
    {
        public Group(IList<Element> children)
        {
            Type = ElementType.Group;
            Children = children;
        }

        public IList<Element> Children { get => (IList<Element>)Value; set => Value = value; }
    }

    public class BooleanQueryClause : Element
    {
        public BooleanQueryClause(BooleanOperator @operator, Element item1, Element item2)
        {
            Type = ElementType.BooleanQueryClause;
            Operator = @operator;
            Item1 = item1;
            Item2 = item2;
        }

        public BooleanOperator Operator { get => (BooleanOperator)Value; set => Value = value; }

        public Element Item1 { get; set; }

        public Element Item2 { get; set; }
    }

    public class ComparisonQueryClause : Element
    {
        public ComparisonQueryClause(ComparisonOperator @operator, Element item1, Element item2)
        {
            Type = ElementType.ComparisonQueryClause;
            Operator = @operator;
            Item1 = item1;
            Item2 = item2;
        }

        public ComparisonOperator Operator { get => (ComparisonOperator)Value; set => Value = value; }

        public Element Item1 { get; set; }

        public Element Item2 { get; set; }
    }

    public enum ElementType
    {
        BooleanOperator,
        ComparisonOperator,
        Group,
        Property,
        Value,
        BooleanQueryClause,
        ComparisonQueryClause
    }

    public enum BooleanOperator
    {
        And,
        Or
    }

    public enum ComparisonOperator
    {
        Equal,
        GreaterThan,
        LessThan,
        Contains
    }
}
