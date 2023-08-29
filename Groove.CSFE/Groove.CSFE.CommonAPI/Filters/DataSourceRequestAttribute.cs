using Groove.CSFE.Core.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Groove.CSFE.CommonAPI.Filters
{
    public class DataSourceRequestAttribute : ModelBinderAttribute
    {
        public DataSourceRequestAttribute()
        {
            this.BinderType = typeof(DataSourceRequestModelBinder);
        }
    }

    public class DataSourceRequestModelBinder : IModelBinder
    {
        public virtual Task BindModelAsync(ModelBindingContext bindingContext)
        {
            DataSourceRequest dataSourceRequest = CreateDataSourceRequest(bindingContext.ModelMetadata, bindingContext.ValueProvider, bindingContext.ModelName);
            bindingContext.Result = ModelBindingResult.Success((object)dataSourceRequest);
            return Task.CompletedTask;
        }

        private static void TryGetValue<T>(ModelMetadata modelMetadata, IValueProvider valueProvider, string modelName, string key, Action<T> action)
        {
            if (modelMetadata.BinderModelName.HasValue())
            {
                key = modelName + "-" + key;
            }
            ValueProviderResult value = valueProvider.GetValue(key);
            if (value.FirstValue != null)
            {
                object obj = value.ConvertValueTo(typeof(T));
                if (obj != null)
                {
                    action((T)obj);
                }
            }
        }

        public static DataSourceRequest CreateDataSourceRequest(ModelMetadata modelMetadata, IValueProvider valueProvider, string modelName)
        {
            DataSourceRequest request = new DataSourceRequest();
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.Sort, delegate (string sort)
            {
                request.Sorts = DataSourceDescriptorSerializer.Deserialize<SortDescriptor>(sort);
            });
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.Page, delegate (string currentPage)
            {
                if (int.TryParse(currentPage, out int result))
                {
                    request.Page = result;
                }
            });
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.PageSize, delegate (string pageSize)
            {
                request.PageSize = int.Parse(pageSize);
            });
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.Skip, delegate (string skip)
            {
                request.Skip = int.Parse(skip);
            });
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.Take, delegate (string take)
            {
                request.Take = int.Parse(take);
            });
            TryGetValue(modelMetadata, valueProvider, modelName, DataSourceRequestUrlParameters.Filter, delegate (string filter)
            {
                request.Filters = FilterDescriptorFactory.Create(filter);
            });
            return request;
        }
    }

    public static class ValueProviderResultExtensions
    {
        public static object ConvertValueTo(this ValueProviderResult result, Type type)
        {
            if (!(type == (Type)null))
            {
                object value = null;
                StringValues values = result.Values;
                if (values.Count == 1)
                {
                    value = result.FirstValue;
                }
                else
                {
                    values = result.Values;
                    if (values.Count > 1)
                    {
                        values = result.Values;
                        value = values.ToArray();
                    }
                }
                object result2 = null;
                try
                {
                    result2 = value;
                    return result2;
                }
                catch
                {
                    return result2;
                }
            }
            throw new ArgumentNullException("type");
        }
    }

    public static class FilterDescriptorFactory
    {
        public static IList<IFilterDescriptor> Create(string input)
        {
            IList<IFilterDescriptor> list = new List<IFilterDescriptor>();
            IFilterNode filterNode = new FilterParser(input).Parse();
            if (filterNode == null)
            {
                return list;
            }
            FilterNodeVisitor filterNodeVisitor = new FilterNodeVisitor();
            filterNode.Accept(filterNodeVisitor);
            list.Add(filterNodeVisitor.Result);
            return list;
        }
    }

    public class FilterParser
    {
        private readonly IList<FilterToken> tokens;

        private int currentTokenIndex;

        public FilterParser(string input)
        {
            FilterLexer filterLexer = new FilterLexer(input);
            tokens = filterLexer.Tokenize();
        }

        public IFilterNode Parse()
        {
            if (tokens.Count > 0)
            {
                return Expression();
            }
            return null;
        }

        private IFilterNode Expression()
        {
            return OrExpression();
        }

        private IFilterNode OrExpression()
        {
            IFilterNode filterNode = AndExpression();
            if (Is(FilterTokenType.Or))
            {
                return ParseOrExpression(filterNode);
            }
            if (Is(FilterTokenType.And))
            {
                Expect(FilterTokenType.And);
                return new AndNode
                {
                    First = filterNode,
                    Second = OrExpression()
                };
            }
            return filterNode;
        }

        private IFilterNode AndExpression()
        {
            IFilterNode filterNode = ComparisonExpression();
            if (Is(FilterTokenType.And))
            {
                return ParseAndExpression(filterNode);
            }
            return filterNode;
        }

        private IFilterNode ComparisonExpression()
        {
            IFilterNode filterNode = PrimaryExpression();
            if (Is(FilterTokenType.ComparisonOperator) || Is(FilterTokenType.Function))
            {
                return ParseComparisonExpression(filterNode);
            }
            return filterNode;
        }

        private IFilterNode PrimaryExpression()
        {
            if (Is(FilterTokenType.LeftParenthesis))
            {
                return ParseNestedExpression();
            }
            if (Is(FilterTokenType.Function))
            {
                return ParseFunctionExpression();
            }
            if (Is(FilterTokenType.Boolean))
            {
                return ParseBoolean();
            }
            if (Is(FilterTokenType.DateTime))
            {
                return ParseDateTimeExpression();
            }
            if (Is(FilterTokenType.Property))
            {
                return ParsePropertyExpression();
            }
            if (Is(FilterTokenType.Number))
            {
                return ParseNumberExpression();
            }
            if (Is(FilterTokenType.String))
            {
                return ParseStringExpression();
            }
            if (Is(FilterTokenType.Null))
            {
                return ParseNullExpression();
            }
            throw new FilterParserException("Expected primaryExpression");
        }

        private IFilterNode ParseOrExpression(IFilterNode firstArgument)
        {
            Expect(FilterTokenType.Or);
            IFilterNode second = OrExpression();
            return new OrNode
            {
                First = firstArgument,
                Second = second
            };
        }

        private IFilterNode ParseComparisonExpression(IFilterNode firstArgument)
        {
            if (Is(FilterTokenType.ComparisonOperator))
            {
                FilterToken token = Expect(FilterTokenType.ComparisonOperator);
                IFilterNode second = PrimaryExpression();
                return new ComparisonNode
                {
                    First = firstArgument,
                    FilterOperator = token.ToFilterOperator(),
                    Second = second
                };
            }
            FilterToken token2 = Expect(FilterTokenType.Function);
            return new FunctionNode
            {
                FilterOperator = token2.ToFilterOperator(),
                Arguments =
            {
                firstArgument,
                PrimaryExpression()
            }
            };
        }

        private IFilterNode ParseAndExpression(IFilterNode firstArgument)
        {
            Expect(FilterTokenType.And);
            IFilterNode second = ComparisonExpression();
            return new AndNode
            {
                First = firstArgument,
                Second = second
            };
        }

        private IFilterNode ParseNullExpression()
        {
            FilterToken filterToken = Expect(FilterTokenType.Null);
            return new NullNode
            {
                Value = filterToken.Value
            };
        }

        private IFilterNode ParseStringExpression()
        {
            FilterToken filterToken = Expect(FilterTokenType.String);
            return new StringNode
            {
                Value = filterToken.Value
            };
        }

        private IFilterNode ParseBoolean()
        {
            FilterToken filterToken = Expect(FilterTokenType.Boolean);
            return new BooleanNode
            {
                Value = (object)Convert.ToBoolean(filterToken.Value)
            };
        }

        private IFilterNode ParseNumberExpression()
        {
            FilterToken filterToken = Expect(FilterTokenType.Number);
            return new NumberNode
            {
                Value = (object)Convert.ToDouble(filterToken.Value, CultureInfo.InvariantCulture)
            };
        }

        private IFilterNode ParsePropertyExpression()
        {
            FilterToken filterToken = Expect(FilterTokenType.Property);
            return new PropertyNode
            {
                Name = filterToken.Value
            };
        }

        private IFilterNode ParseDateTimeExpression()
        {
            FilterToken filterToken = Expect(FilterTokenType.DateTime);
            return new DateTimeNode
            {
                Value = (object)DateTime.ParseExact(filterToken.Value, "yyyy-MM-ddTHH-mm-ss", null)
            };
        }

        private IFilterNode ParseNestedExpression()
        {
            Expect(FilterTokenType.LeftParenthesis);
            IFilterNode result = Expression();
            Expect(FilterTokenType.RightParenthesis);
            return result;
        }

        private IFilterNode ParseFunctionExpression()
        {
            FilterToken token = Expect(FilterTokenType.Function);
            FunctionNode functionNode = new FunctionNode
            {
                FilterOperator = token.ToFilterOperator()
            };
            Expect(FilterTokenType.LeftParenthesis);
            functionNode.Arguments.Add(Expression());
            while (Is(FilterTokenType.Comma))
            {
                Expect(FilterTokenType.Comma);
                functionNode.Arguments.Add(Expression());
            }
            Expect(FilterTokenType.RightParenthesis);
            return functionNode;
        }

        private FilterToken Expect(FilterTokenType tokenType)
        {
            if (!Is(tokenType))
            {
                throw new FilterParserException("Expected " + tokenType.ToString());
            }
            FilterToken result = Peek();
            currentTokenIndex++;
            return result;
        }

        private bool Is(FilterTokenType tokenType)
        {
            FilterToken filterToken = Peek();
            if (filterToken != null)
            {
                return filterToken.TokenType == tokenType;
            }
            return false;
        }

        private FilterToken Peek()
        {
            if (currentTokenIndex < tokens.Count)
            {
                return tokens[currentTokenIndex];
            }
            return null;
        }
    }

    public class FilterLexer
    {
        private const char Separator = '~';

        private static readonly string[] ComparisonOperators = new string[6]
        {
        "eq",
        "neq",
        "lt",
        "lte",
        "gt",
        "gte"
        };

        private static readonly string[] LogicalOperators = new string[3]
        {
        "and",
        "or",
        "not"
        };

        private static readonly string[] Booleans = new string[2]
        {
        "true",
        "false"
        };

        private static readonly string[] Functions = new string[11]
        {
        "contains",
        "endswith",
        "startswith",
        "notsubstringof",
        "doesnotcontain",
        "isnull",
        "isnotnull",
        "isempty",
        "isnotempty",
        "isnullorempty",
        "isnotnullorempty"
        };

        private int currentCharacterIndex;

        private readonly string input;

        public FilterLexer(string input)
        {
            input = (input ?? string.Empty);
            this.input = input.Trim(new char[1]
            {
            '~'
            });
        }

        public IList<FilterToken> Tokenize()
        {
            List<FilterToken> list = new List<FilterToken>();
            while (currentCharacterIndex < input.Length)
            {
                if (TryParseIdentifier(out string identifier))
                {
                    list.Add(Identifier(identifier));
                }
                else if (TryParseNumber(out identifier))
                {
                    list.Add(Number(identifier));
                }
                else if (TryParseString(out identifier))
                {
                    list.Add(String(identifier));
                }
                else if (TryParseCharacter(out identifier, '('))
                {
                    list.Add(LeftParenthesis(identifier));
                }
                else if (TryParseCharacter(out identifier, ')'))
                {
                    list.Add(RightParenthesis(identifier));
                }
                else
                {
                    if (!TryParseCharacter(out identifier, ','))
                    {
                        throw new FilterParserException("Expected token");
                    }
                    list.Add(Comma(identifier));
                }
            }
            return list;
        }

        private static bool IsComparisonOperator(string value)
        {
            return Array.IndexOf(ComparisonOperators, value) > -1;
        }

        private static bool IsLogicalOperator(string value)
        {
            return Array.IndexOf(LogicalOperators, value) > -1;
        }

        private static bool IsBoolean(string value)
        {
            return Array.IndexOf(Booleans, value) > -1;
        }

        private static bool IsFunction(string value)
        {
            return Array.IndexOf(Functions, value) > -1;
        }

        private static FilterToken Comma(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Comma,
                Value = result
            };
        }

        private static FilterToken Boolean(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Boolean,
                Value = result
            };
        }

        private static FilterToken RightParenthesis(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.RightParenthesis,
                Value = result
            };
        }

        private static FilterToken LeftParenthesis(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.LeftParenthesis,
                Value = result
            };
        }

        private static FilterToken String(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.String,
                Value = result
            };
        }

        private static FilterToken Number(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Number,
                Value = result
            };
        }

        private FilterToken Date(string result)
        {
            TryParseString(out result);
            return new FilterToken
            {
                TokenType = FilterTokenType.DateTime,
                Value = result
            };
        }

        private FilterToken Null(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Null,
                Value = null
            };
        }

        private static FilterToken ComparisonOperator(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.ComparisonOperator,
                Value = result
            };
        }

        private static FilterToken LogicalOperator(string result)
        {
            if (result == "or")
            {
                return new FilterToken
                {
                    TokenType = FilterTokenType.Or,
                    Value = result
                };
            }
            if (result == "and")
            {
                return new FilterToken
                {
                    TokenType = FilterTokenType.And,
                    Value = result
                };
            }
            return new FilterToken
            {
                TokenType = FilterTokenType.Not,
                Value = result
            };
        }

        private static FilterToken Function(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Function,
                Value = result
            };
        }

        private static FilterToken Property(string result)
        {
            return new FilterToken
            {
                TokenType = FilterTokenType.Property,
                Value = result
            };
        }

        private FilterToken Identifier(string result)
        {
            if (result == "datetime")
            {
                return Date(result);
            }
            if (IsComparisonOperator(result))
            {
                return ComparisonOperator(result);
            }
            if (IsLogicalOperator(result))
            {
                return LogicalOperator(result);
            }
            if (IsBoolean(result))
            {
                return Boolean(result);
            }
            if (IsFunction(result))
            {
                return Function(result);
            }
            if (result == "null" || result == "undefined")
            {
                return Null(result);
            }
            return Property(result);
        }

        private void SkipSeparators()
        {
            for (char c = Peek(); c == '~'; c = Next())
            {
            }
        }

        private bool TryParseCharacter(out string character, char whatCharacter)
        {
            SkipSeparators();
            char c = Peek();
            if (c != whatCharacter)
            {
                character = null;
                return false;
            }
            Next();
            character = c.ToString();
            return true;
        }

        private bool TryParseString(out string @string)
        {
            SkipSeparators();
            if (Peek() != '\'')
            {
                @string = null;
                return false;
            }
            Next();
            StringBuilder result = new StringBuilder();
            @string = Read(delegate (char character)
            {
                switch (character)
                {
                    case '\uffff':
                        throw new FilterParserException("Unterminated string");
                    case '\'':
                        if (Peek(1) == '\'')
                        {
                            Next();
                            return true;
                        }
                        break;
                }
                return character != '\'';
            }, result);
            Next();
            return true;
        }

        private bool TryParseNumber(out string number)
        {
            SkipSeparators();
            char c = Peek();
            StringBuilder stringBuilder = new StringBuilder();
            int decimalSymbols = 0;
            if (c == '-' || c == '+')
            {
                stringBuilder.Append(c);
                c = Next();
            }
            if (c == '.')
            {
                decimalSymbols++;
                stringBuilder.Append(c);
                c = Next();
            }
            if (!char.IsDigit(c))
            {
                number = null;
                return false;
            }
            number = Read(delegate (char character)
            {
                if (character == '.')
                {
                    if (decimalSymbols < 1)
                    {
                        decimalSymbols++;
                        return true;
                    }
                    throw new FilterParserException("A number cannot have more than one decimal symbol");
                }
                return char.IsDigit(character);
            }, stringBuilder);
            return true;
        }

        private bool TryParseIdentifier(out string identifier)
        {
            SkipSeparators();
            char c = Peek();
            StringBuilder stringBuilder = new StringBuilder();
            if (!IsIdentifierStart(c))
            {
                identifier = null;
                return false;
            }
            stringBuilder.Append(c);
            Next();
            identifier = Read(delegate (char character)
            {
                if (!IsIdentifierPart(character))
                {
                    return character == '.';
                }
                return true;
            }, stringBuilder);
            return true;
        }

        private static bool IsIdentifierPart(char character)
        {
            if (!char.IsLetter(character) && !char.IsDigit(character) && character != '_')
            {
                return character == '$';
            }
            return true;
        }

        private static bool IsIdentifierStart(char character)
        {
            if (!char.IsLetter(character) && character != '_' && character != '$')
            {
                return character == '@';
            }
            return true;
        }

        private string Read(Func<char, bool> predicate, StringBuilder result)
        {
            char c = Peek();
            while (predicate(c))
            {
                result.Append(c);
                c = Next();
            }
            return result.ToString();
        }

        private char Peek()
        {
            return Peek(0);
        }

        private char Peek(int chars)
        {
            if (currentCharacterIndex + chars < input.Length)
            {
                return input[currentCharacterIndex + chars];
            }
            return '\uffff';
        }

        private char Next()
        {
            currentCharacterIndex++;
            return Peek();
        }
    }

    public interface IFilterNode
    {
        void Accept(IFilterNodeVisitor visitor);
    }

    public interface IFilterNodeVisitor
    {
        void Visit(PropertyNode propertyNode);

        void Visit(IValueNode valueNode);

        void StartVisit(ILogicalNode logicalNode);

        void StartVisit(IOperatorNode operatorNode);

        void EndVisit();
    }

    public interface IOperatorNode
    {
        FilterOperator FilterOperator
        {
            get;
        }
    }

    public interface ILogicalNode
    {
        FilterCompositionLogicalOperator LogicalOperator
        {
            get;
        }
    }


    public interface IValueNode
    {
        object Value
        {
            get;
        }
    }

    public class PropertyNode : IFilterNode
    {
        public string Name
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FilterNodeVisitor : IFilterNodeVisitor
    {
        private Stack<IFilterDescriptor> context;

        public IFilterDescriptor Result => context.Pop();

        private IFilterDescriptor CurrentDescriptor
        {
            get
            {
                if (context.Count > 0)
                {
                    return context.Peek();
                }
                return null;
            }
        }

        public FilterNodeVisitor()
        {
            context = new Stack<IFilterDescriptor>();
        }

        public void StartVisit(IOperatorNode operatorNode)
        {
            FilterDescriptor item = new FilterDescriptor
            {
                Operator = operatorNode.FilterOperator
            };
            (CurrentDescriptor as CompositeFilterDescriptor)?.FilterDescriptors.Add(item);
            context.Push(item);
        }

        public void StartVisit(ILogicalNode logicalNode)
        {
            CompositeFilterDescriptor item = new CompositeFilterDescriptor
            {
                LogicalOperator = logicalNode.LogicalOperator
            };
            (CurrentDescriptor as CompositeFilterDescriptor)?.FilterDescriptors.Add(item);
            context.Push(item);
        }

        public void Visit(PropertyNode propertyNode)
        {
            ((FilterDescriptor)CurrentDescriptor).Member = propertyNode.Name;
        }

        public void EndVisit()
        {
            if (context.Count > 1)
            {
                context.Pop();
            }
        }

        public void Visit(IValueNode valueNode)
        {
            ((FilterDescriptor)CurrentDescriptor).Value = valueNode.Value;
        }
    }

    public class DataSourceDescriptorSerializer
    {
        private const string ColumnDelimiter = "~";

        public static string Serialize<T>(IEnumerable<T> descriptors) where T : IDescriptor
        {
            if (!descriptors.Any())
            {
                return "~";
            }
            string[] value = (from d in descriptors
                              select d.Serialize()).ToArray();
            return string.Join("~", value);
        }

        public static IList<T> Deserialize<T>(string from) where T : IDescriptor, new()
        {
            List<T> list = new List<T>();
            if (!from.HasValue())
            {
                return list;
            }
            string[] array = from.Split("~".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string source in array)
            {
                T item = new T();
                item.Deserialize(source);
                list.Add(item);
            }
            return list;
        }
    }

    public static class DataSourceRequestUrlParameters
    {
        public static string Aggregates
        {
            get;
            set;
        }

        public static string Filter
        {
            get;
            set;
        }

        public static string Page
        {
            get;
            set;
        }

        public static string PageSize
        {
            get;
            set;
        }

        public static string Sort
        {
            get;
            set;
        }

        public static string Group
        {
            get;
            set;
        }

        public static string Mode
        {
            get;
            set;
        }

        public static string GroupPaging
        {
            get;
            set;
        }

        public static string IncludeSubGroupCount
        {
            get;
            set;
        }

        public static string Skip
        {
            get;
            set;
        }

        public static string Take
        {
            get;
            set;
        }

        static DataSourceRequestUrlParameters()
        {
            Sort = "sort";
            Group = "group";
            Page = "page";
            PageSize = "pageSize";
            Filter = "filter";
            Mode = "mode";
            Aggregates = "aggregate";
            GroupPaging = "groupPaging";
            Skip = "skip";
            Take = "take";
            IncludeSubGroupCount = "includeSubGroupCount";
        }

        public static IDictionary<string, string> ToDictionary(string prefix)
        {
            return new Dictionary<string, string>
            {
                [Group] = prefix + Group,
                [Sort] = prefix + Sort,
                [Page] = prefix + Page,
                [PageSize] = prefix + PageSize,
                [Filter] = prefix + Filter,
                [Mode] = prefix + Mode,
                [GroupPaging] = prefix + GroupPaging,
                [Skip] = prefix + Skip,
                [Take] = prefix + Take,
                [IncludeSubGroupCount] = prefix + IncludeSubGroupCount
            };
        }
    }

    public class FilterParserException : Exception
    {
        public FilterParserException()
        {
        }

        public FilterParserException(string message)
            : base(message)
        {
        }

        public FilterParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class AndNode : IFilterNode, ILogicalNode
    {
        public IFilterNode First
        {
            get;
            set;
        }

        public IFilterNode Second
        {
            get;
            set;
        }

        public FilterCompositionLogicalOperator LogicalOperator => FilterCompositionLogicalOperator.And;

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.StartVisit(this);
            First.Accept(visitor);
            Second.Accept(visitor);
            visitor.EndVisit();
        }
    }

    public class OrNode : IFilterNode, ILogicalNode
    {
        public IFilterNode First
        {
            get;
            set;
        }

        public IFilterNode Second
        {
            get;
            set;
        }

        public FilterCompositionLogicalOperator LogicalOperator => FilterCompositionLogicalOperator.Or;

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.StartVisit(this);
            First.Accept(visitor);
            Second.Accept(visitor);
            visitor.EndVisit();
        }
    }

    public class ComparisonNode : IFilterNode, IOperatorNode
    {
        public FilterOperator FilterOperator
        {
            get;
            set;
        }

        public virtual IFilterNode First
        {
            get;
            set;
        }

        public virtual IFilterNode Second
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.StartVisit(this);
            First.Accept(visitor);
            Second.Accept(visitor);
            visitor.EndVisit();
        }
    }

    public class FunctionNode : IFilterNode, IOperatorNode
    {
        public FilterOperator FilterOperator
        {
            get;
            set;
        }

        public IList<IFilterNode> Arguments
        {
            get;
            private set;
        }

        public FunctionNode()
        {
            Arguments = new List<IFilterNode>();
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.StartVisit(this);
            foreach (IFilterNode argument in Arguments)
            {
                argument.Accept(visitor);
            }
            visitor.EndVisit();
        }
    }


    public class NullNode : IFilterNode, IValueNode
    {
        public object Value
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StringNode : IFilterNode, IValueNode
    {
        public object Value
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BooleanNode : IFilterNode, IValueNode
    {
        public object Value
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NumberNode : IFilterNode, IValueNode
    {
        public object Value
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class DateTimeNode : IFilterNode, IValueNode
    {
        public object Value
        {
            get;
            set;
        }

        public void Accept(IFilterNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
