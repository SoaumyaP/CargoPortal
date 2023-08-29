using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Groove.SP.Core.Data
{
	public static class GridDataSourceQuery
    {
		/// <summary>
		/// To get IQueryable that is correctponding to grid data request.
		/// </summary>
		/// <param name="enumerable">Query</param>
		/// <param name="request">Grid data request</param>
		/// <returns></returns>
		public static IQueryable GetDataSouceQueryable(this IEnumerable enumerable, DataSourceRequest request)
		{
			IQueryable queryable = enumerable.AsQueryable();
			IQueryable result = queryable;
			List<IFilterDescriptor> list = new List<IFilterDescriptor>();
			if (request.Filters != null)
			{
				list.AddRange(request.Filters);
			}
			if (list.Any())
			{
				result = result.Where(list);
			}
			List<SortDescriptor> sort = new List<SortDescriptor>();
			if (request.Sorts != null)
			{
				sort.AddRange(request.Sorts);
			}
			List<SortDescriptor> temporarySortDescriptors = new List<SortDescriptor>();

			if (!sort.Any() && queryable.Provider.IsEntityFrameworkProvider())
			{
				SortDescriptor item = new SortDescriptor
				{
					Member = queryable.ElementType.FirstSortableProperty()
				};
				sort.Add(item);
				temporarySortDescriptors.Add(item);
			}

			if (sort.Any())
			{
				result = result.Sort(sort);
			}
			IQueryable notPagedData = result;
			if (!request.GroupPaging || !request.Groups.Any())
			{
				result = result.Page(request.Page - 1, request.PageSize);
			}
			else if (request.GroupPaging && !request.Groups.Any())
			{
				result = result.Skip(request.Skip).Take(request.Take);
			}

			return result;
		}


		/// <summary>
		/// Get data for grid/list by 2 requests: count and data fetching.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		public static Task<DataSourceResult> ToDataSourceResultAsync(this IEnumerable enumerable, DataSourceRequest request)
        {
            return CreateDataSourceResultAsync(() => enumerable.ToDataSourceResult(request));
        }

		private static Task<DataSourceResult> CreateDataSourceResultAsync(Func<DataSourceResult> expression)
		{
			return Task.Run(expression);
		}

		public static DataSourceResult ToDataSourceResult(this IEnumerable enumerable, DataSourceRequest request)
		{
			return enumerable.AsQueryable().ToDataSourceResult(request);
		}

		public static DataSourceResult ToDataSourceResult(this IQueryable queryable, DataSourceRequest request)
		{
			return queryable.CreateDataSourceResult<object, object>(request, null);
		}


		private static DataSourceResult CreateDataSourceResult<TModel, TResult>(this IQueryable queryable, DataSourceRequest request, Func<TModel, TResult> selector)
		{
			DataSourceResult dataSourceResult = new DataSourceResult();
			IQueryable queryable2 = queryable;
			List<IFilterDescriptor> list = new List<IFilterDescriptor>();
			if (request.Filters != null)
			{
				list.AddRange(request.Filters);
			}
			if (list.Any())
			{
				queryable2 = queryable2.Where(list);
			}
			List<SortDescriptor> sort = new List<SortDescriptor>();
			if (request.Sorts != null)
			{
				sort.AddRange(request.Sorts);
			}
			List<SortDescriptor> temporarySortDescriptors = new List<SortDescriptor>();

			// Disable Aggregate

            //IList<GroupDescriptor> list2 = new List<GroupDescriptor>();
            //if (request.Groups != null)
            //{
            //    list2.AddRange(request.Groups);
            //}
            //List<AggregateDescriptor> aggregates = new List<AggregateDescriptor>();
            //if (request.Aggregates != null)
            //{
            //    aggregates.AddRange(request.Aggregates);
            //}
            //if (aggregates.Any() && !request.IncludeSubGroupCount)
            //{
            //    IQueryable queryable3 = queryable2.AsQueryable();
            //    IQueryable source = queryable3;
            //    if (list.Any())
            //    {
            //        source = queryable3.Where(list);
            //    }
            //    //dataSourceResult.AggregateResults = source.Aggregate(aggregates.SelectMany((AggregateDescriptor a) => a.Aggregates));
            //    if (list2.Any() && aggregates.Any())
            //    {
            //        list2.Each(delegate (GroupDescriptor g)
            //        {
            //            g.AggregateFunctions.AddRange(aggregates.SelectMany((AggregateDescriptor a) => a.Aggregates));
            //        });
            //    }
            //}
            if (!request.GroupPaging || !request.Groups.Any())
			{
				dataSourceResult.Total = queryable2.Count();
			}
			if (!sort.Any() && queryable.Provider.IsEntityFrameworkProvider())
			{
				SortDescriptor item = new SortDescriptor
				{
					Member = queryable.ElementType.FirstSortableProperty()
				};
				sort.Add(item);
				temporarySortDescriptors.Add(item);
			}
			//if (list2.Any())
			//{
			//	list2.Reverse().Each(delegate (GroupDescriptor groupDescriptor)
			//	{
			//		SortDescriptor item2 = new SortDescriptor
			//		{
			//			Member = groupDescriptor.Member,
			//			SortDirection = groupDescriptor.SortDirection
			//		};
			//		sort.Insert(0, item2);
			//		temporarySortDescriptors.Add(item2);
			//	});
			//}
			if (sort.Any())
			{
				queryable2 = queryable2.Sort(sort);
			}
			IQueryable notPagedData = queryable2;
			if (!request.GroupPaging || !request.Groups.Any())
			{
				queryable2 = queryable2.Page(request.Page - 1, request.PageSize);
			}
			else if (request.GroupPaging && !request.Groups.Any())
			{
				queryable2 = queryable2.Skip(request.Skip).Take(request.Take);
			}
            //if (list2.Any())
            //{
            //	//queryable2 = queryable2.GroupBy(notPagedData, list2, !request.GroupPaging);
            //	if (request.GroupPaging)
            //	{
            //		dataSourceResult.Total = queryable2.Count();
            //		queryable2 = queryable2.Skip(request.Skip).Take(request.Take);
            //	}
            //}
            if (!request.IncludeSubGroupCount)
            {
                dataSourceResult.Data = queryable2.Execute(selector);
            }
            //if (modelState != null && !modelState.IsValid)
            //{
            //	dataSourceResult.Errors = modelState.SerializeErrors();
            //}
            temporarySortDescriptors.Each(delegate (SortDescriptor sortDescriptor)
			{
				sort.Remove(sortDescriptor);
			});
			return dataSourceResult;
		}

		private static IEnumerable Execute<TModel, TResult>(this IQueryable source, Func<TModel, TResult> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			Type elementType = source.ElementType;
			if (selector != null)
			{
				List<TResult> list2 = new List<TResult>();
				{
					foreach (TModel item2 in source)
					{
						list2.Add(selector(item2));
					}
					return list2;
				}
			}
			IList list3 = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
			foreach (object item3 in source)
			{
				list3.Add(item3);
			}
			return list3;
		}

		public static int Count(this IQueryable source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return source.Provider.Execute<int>(Expression.Call(typeof(Queryable), "Count", new Type[1] { source.ElementType }, source.Expression));
		}

		public static IQueryable Page(this IQueryable source, int pageIndex, int pageSize)
		{
			IQueryable source2 = source;
			source2 = source2.Skip(pageIndex * pageSize);
			if (pageSize > 0)
			{
				source2 = source2.Take(pageSize);
			}
			return source2;
		}

		public static IQueryable Take(this IQueryable source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Take", new Type[1] { source.ElementType }, source.Expression, Expression.Constant(count)));
		}


		public static IQueryable Skip(this IQueryable source, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Skip", new Type[1] { source.ElementType }, source.Expression, Expression.Constant(count)));
		}


		public static IQueryable Sort(this IQueryable source, IEnumerable<SortDescriptor> sortDescriptors)
		{
			return new SortDescriptorCollectionExpressionBuilder(source, sortDescriptors).Sort();
		}


		internal class SortDescriptorCollectionExpressionBuilder
		{
			private readonly IEnumerable<SortDescriptor> sortDescriptors;

			private readonly IQueryable queryable;

			public SortDescriptorCollectionExpressionBuilder(IQueryable queryable, IEnumerable<SortDescriptor> sortDescriptors)
			{
				this.queryable = queryable;
				this.sortDescriptors = sortDescriptors;
			}

			public IQueryable Sort()
			{
				IQueryable queryable = this.queryable;
				bool flag = true;
				foreach (SortDescriptor sortDescriptor in sortDescriptors)
				{
					Type typeFromHandle = typeof(object);
					LambdaExpression lambdaExpression = ExpressionBuilderFactory.MemberAccess(this.queryable, typeFromHandle, sortDescriptor.Member).CreateLambdaExpression();
					string text = "";
					if (flag)
					{
						text = ((sortDescriptor.SortDirection == ListSortDirection.Ascending) ? "OrderBy" : "OrderByDescending");
						flag = false;
					}
					else
					{
						text = ((sortDescriptor.SortDirection == ListSortDirection.Ascending) ? "ThenBy" : "ThenByDescending");
					}
					queryable = queryable.Provider.CreateQuery(Expression.Call(typeof(Queryable), text, new Type[2]
					{
				queryable.ElementType,
				lambdaExpression.Body.Type
					}, queryable.Expression, Expression.Quote(lambdaExpression)));
				}
				return queryable;
			}
		}



		public static IQueryable Where(this IQueryable source, IEnumerable<IFilterDescriptor> filterDescriptors)
		{
			if (filterDescriptors.Any())
			{
				FilterDescriptorCollectionExpressionBuilder filterDescriptorCollectionExpressionBuilder = new FilterDescriptorCollectionExpressionBuilder(Expression.Parameter(source.ElementType, "item"), filterDescriptors);
				filterDescriptorCollectionExpressionBuilder.Options.LiftMemberAccessToNull = source.Provider.IsLinqToObjectsProvider();
				LambdaExpression predicate = filterDescriptorCollectionExpressionBuilder.CreateFilterExpression();
				return source.Where(predicate);
			}
			return source;
		}


		public static IQueryable Where(this IQueryable source, Expression predicate)
		{
			return source.Provider.CreateQuery(Expression.Call(typeof(Queryable), "Where", new Type[1] { source.ElementType }, source.Expression, Expression.Quote(predicate)));
		}


	}

	public class FilterDescriptorCollectionExpressionBuilder : FilterExpressionBuilder
	{
		private readonly IEnumerable<IFilterDescriptor> filterDescriptors;

		private readonly FilterCompositionLogicalOperator logicalOperator;

		public FilterDescriptorCollectionExpressionBuilder(ParameterExpression parameterExpression, IEnumerable<IFilterDescriptor> filterDescriptors)
			: this(parameterExpression, filterDescriptors, FilterCompositionLogicalOperator.And)
		{
		}

		public FilterDescriptorCollectionExpressionBuilder(ParameterExpression parameterExpression, IEnumerable<IFilterDescriptor> filterDescriptors, FilterCompositionLogicalOperator logicalOperator)
			: base(parameterExpression)
		{
			this.filterDescriptors = filterDescriptors;
			this.logicalOperator = logicalOperator;
		}

		public override Expression CreateBodyExpression()
		{
			Expression expression = null;
			foreach (IFilterDescriptor filterDescriptor in filterDescriptors)
			{
				InitilializeExpressionBuilderOptions(filterDescriptor);
				Expression expression2 = filterDescriptor.CreateFilterExpression(base.ParameterExpression);
				expression = ((expression != null) ? ComposeExpressions(expression, expression2, logicalOperator) : expression2);
			}
			if (expression == null)
			{
				return ExpressionConstants.TrueLiteral;
			}
			return expression;
		}

		private static Expression ComposeExpressions(Expression left, Expression right, FilterCompositionLogicalOperator logicalOperator)
		{
			if (logicalOperator != 0 && logicalOperator == FilterCompositionLogicalOperator.Or)
			{
				return Expression.OrElse(left, right);
			}
			return Expression.AndAlso(left, right);
		}

		private void InitilializeExpressionBuilderOptions(IFilterDescriptor filterDescriptor)
		{
			(filterDescriptor as FilterDescriptorBase)?.ExpressionBuilderOptions.CopyFrom(base.Options);
		}
	}


	public class FilterDescriptorBase : JsonObject, IFilterDescriptor
	{
		private ExpressionBuilderOptions options;

		internal ExpressionBuilderOptions ExpressionBuilderOptions
		{
			get
			{
				if (options == null)
				{
					options = new ExpressionBuilderOptions();
				}
				return options;
			}
		}

		public virtual Expression CreateFilterExpression(Expression instance)
		{
			ParameterExpression parameterExpression = instance as ParameterExpression;
			if (parameterExpression == null)
			{
				throw new ArgumentException("Parameter should be of type ParameterExpression", "instance");
			}
			return CreateFilterExpression(parameterExpression);
		}

		protected virtual Expression CreateFilterExpression(ParameterExpression parameterExpression)
		{
			return parameterExpression;
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
		}
	}


	public enum FilterCompositionLogicalOperator
	{
		And,
		Or
	}

	internal class ExpressionConstants
	{
		internal static Expression TrueLiteral => Expression.Constant(true);

		internal static Expression FalseLiteral => Expression.Constant(false);

		internal static Expression NullLiteral => Expression.Constant(null);
	}


	public abstract class FilterExpressionBuilder : ExpressionBuilderBase
	{
		protected FilterExpressionBuilder(ParameterExpression parameterExpression)
			: base(parameterExpression.Type)
		{
			base.ParameterExpression = parameterExpression;
		}

		public abstract Expression CreateBodyExpression();

		public LambdaExpression CreateFilterExpression()
		{
			return Expression.Lambda(CreateBodyExpression(), base.ParameterExpression);
		}
	}



	public class DataSourceResult
    {
        public IEnumerable Data { get; set; }

        public int Total { get; set; }

        public IEnumerable ViewSettings { get; set; }

        public IEnumerable<AggregateResult> AggregateResults { get; set; }

        public object Errors { get; set; }
    }

	public class AggregateResult
	{
		private object aggregateValue;

		private int itemCount;

		private readonly AggregateFunction function;

		public object Value
		{
			get
			{
				return aggregateValue;
			}
			internal set
			{
				aggregateValue = value;
			}
		}

		public string Member => function.SourceField;

		public object FormattedValue
		{
			get
			{
				if (string.IsNullOrEmpty(function.ResultFormatString))
				{
					return aggregateValue;
				}
				return string.Format(CultureInfo.CurrentCulture, function.ResultFormatString, aggregateValue);
			}
		}

		public int ItemCount
		{
			get
			{
				return itemCount;
			}
			set
			{
				itemCount = value;
			}
		}

		public string Caption => function.Caption;

		public string FunctionName => function.FunctionName;

		public string AggregateMethodName => function.AggregateMethodName;

		public AggregateResult(object value, int count, AggregateFunction function)
		{
			if (function == null)
			{
				throw new ArgumentNullException("function");
			}
			aggregateValue = value;
			itemCount = count;
			this.function = function;
		}

		public AggregateResult(AggregateFunction function)
			: this(null, function)
		{
		}

		public AggregateResult(object value, AggregateFunction function)
			: this(value, 0, function)
		{
		}

		public override string ToString()
		{
			if (Value != null)
			{
				return Value.ToString();
			}
			return base.ToString();
		}

		public string Format(string format)
		{
			if (Value != null)
			{
				return string.Format(format, Value);
			}
			return ToString();
		}
	}

	public abstract class AggregateFunction : JsonObject
	{
		private string functionName;

		public abstract string AggregateMethodName { get; }

		public string Caption { get; set; }

		public virtual string SourceField { get; set; }

		public virtual string FunctionName
		{
			get
			{
				if (string.IsNullOrEmpty(functionName))
				{
					functionName = GenerateFunctionName();
				}
				return functionName;
			}
			set
			{
				functionName = value;
			}
		}

		public Type MemberType { get; set; }

		public virtual string ResultFormatString { get; set; }

		public abstract Expression CreateAggregateExpression(Expression enumerableExpression, bool liftMemberAccessToNull);

		protected virtual string GenerateFunctionName()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", GetType().Name);
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
			json["field"] = SourceField;
			json["aggregate"] = FunctionName.Split('_')[0].ToLowerInvariant();
		}
	}

	public abstract class JsonObject
	{
		public IDictionary<string, object> ToJson()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Serialize(dictionary);
			return dictionary;
		}

		protected abstract void Serialize(IDictionary<string, object> json);
	}

	public class DataSourceRequest
	{
		public int Page { get; set; }

		public int PageSize { get; set; }

		public IList<SortDescriptor> Sorts { get; set; }

		public IList<IFilterDescriptor> Filters { get; set; }

		public IList<GroupDescriptor> Groups { get; set; }

		public IList<AggregateDescriptor> Aggregates { get; set; }

		public bool GroupPaging { get; set; }

		public bool IncludeSubGroupCount { get; set; }

		public int Skip { get; set; }

		public int Take { get; set; }

        public string ViewSettingModuleId { get; set; }

        public DataSourceRequest()
		{
			Page = 1;
			Aggregates = new List<AggregateDescriptor>();
		}

		/// <summary>
		/// To detect if provided column is in sorting or filtering. Comparing column name with OrdinalIgnoreCase mode.
		/// </summary>
		/// <param name="columnName">Column name. Ex: activitycode.</param>
		/// <returns>True or false.</returns>
		public bool IsInteractToColumn(string columnName)
		{
			var request = this;
			if (request == null || string.IsNullOrEmpty(columnName))
				return false;
			if ((request.Filters == null || !request.Filters.Any()) && (request.Sorts == null || !request.Sorts.Any()))
			{
				return false;
			}
			var isSorting = request.Sorts?.Any(x => columnName.Equals(x.Member, StringComparison.OrdinalIgnoreCase)) ?? false;
			var isFiltering = false;
			if (request.Filters != null && request.Filters.Any())
			{
				var result = new List<FilterDescriptor>();
				GetFilterDescriptors(result, request.Filters);
				isFiltering = result.Any(x => columnName.Equals(x.Member, StringComparison.OrdinalIgnoreCase));

			}
			return isFiltering || isSorting;

		}

		/// <summary>
		/// To get filter value on provided column name. Comparing column name with OrdinalIgnoreCase mode.
		/// </summary>
		/// <param name="columnName">Column name. Ex: activitycode.</param>
		/// <returns>List of filter values.</returns>
		public IEnumerable<object> FilterValuesOnColumn(string columnName)
		{
			var request = this;
			if (request == null || string.IsNullOrEmpty(columnName))
				return null;
			if (request.Filters == null || !request.Filters.Any())
			{
				return null;
			}
			if (request.Filters != null && request.Filters.Any())
			{
				var filterDescriptors = new List<FilterDescriptor>();
				GetFilterDescriptors(filterDescriptors, request.Filters);
				return filterDescriptors.Where(x => columnName.Equals(x.Member, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value);
			}
			return null;
		}

		/// <summary>
		/// To get list of all filter sets.
		/// </summary>
		/// <param name="result">Result. List of filter sets.</param>
		/// <param name="filters">Filter from data source request.</param>
		private void GetFilterDescriptors(List<FilterDescriptor> result, IList<IFilterDescriptor> filters)
		{
			if (filters == null || !filters.Any())
			{
				return;
			}
			foreach (var filter in filters)
			{
				var filterType = filter.GetType();

				if (filterType == typeof(FilterDescriptor))
				{
					result.Add((FilterDescriptor)filter);
				}
				else if (filterType == typeof(CompositeFilterDescriptor))
				{
					GetFilterDescriptors(result, ((CompositeFilterDescriptor)filter).FilterDescriptors);
				}
			}

		}
	}

	public class SortDescriptor : JsonObject, IDescriptor
	{
		public string Member { get; set; }

		public ListSortDirection SortDirection { get; set; }

		public ClientHandlerDescriptor SortCompare { get; set; }

		public SortDescriptor()
			: this(null, ListSortDirection.Ascending)
		{
		}

		public SortDescriptor(string member, ListSortDirection order)
		{
			Member = member;
			SortDirection = order;
		}

		public void Deserialize(string source)
		{
			string[] array = source.Split(new char[1] { '-' });
			if (array.Length > 1)
			{
				Member = array[0];
			}
			string text = array.Last();
			SortDirection = ((text == "desc") ? ListSortDirection.Descending : ListSortDirection.Ascending);
		}

		public string Serialize()
		{
			return "{0}-{1}".FormatWith(Member, (SortDirection == ListSortDirection.Ascending) ? "asc" : "desc");
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
			json["field"] = Member;
			json["dir"] = ((SortDirection == ListSortDirection.Ascending) ? "asc" : "desc");
			if (SortCompare != null && SortCompare.HasValue())
			{
				json["compare"] = SortCompare;
			}
		}
	}

	public interface IDescriptor
	{
		void Deserialize(string source);

		string Serialize();
	}

	public enum ListSortDirection
	{
		Ascending,
		Descending
	}


	public class ClientHandlerDescriptor
	{
		public Func<object, object> TemplateDelegate { get; set; }

		public string HandlerName { get; set; }

		public bool HasValue()
		{
			if (!HandlerName.HasValue())
			{
				return TemplateDelegate != null;
			}
			return true;
		}
	}

	public static class StringExtensions
	{
		public static bool HasValue(this string value)
		{
			return !string.IsNullOrEmpty(value);
		}

		public static string FormatWith(this string instance, params object[] args)
		{
			return string.Format(CultureInfo.CurrentCulture, instance, args);
		}

		public static bool IsCaseInsensitiveEqual(this string instance, string comparing)
		{
			return string.Compare(instance, comparing, StringComparison.OrdinalIgnoreCase) == 0;
		}

	}

	public interface IFilterDescriptor
	{
		Expression CreateFilterExpression(Expression instance);
	}

	public class GroupDescriptor : SortDescriptor
	{
		private object displayContent;

		private AggregateFunctionCollection aggregateFunctions;

		public Type MemberType { get; set; }

		public object DisplayContent
		{
			get
			{
				if (displayContent == null)
				{
					return base.Member;
				}
				return displayContent;
			}
			set
			{
				displayContent = value;
			}
		}

		public AggregateFunctionCollection AggregateFunctions
		{
			get
			{
				AggregateFunctionCollection obj = aggregateFunctions ?? new AggregateFunctionCollection();
				AggregateFunctionCollection result = obj;
				aggregateFunctions = obj;
				return result;
			}
		}

		public void CycleSortDirection()
		{
			base.SortDirection = GetNextSortDirection(base.SortDirection);
		}

		private static ListSortDirection GetNextSortDirection(ListSortDirection? sortDirection)
		{
			if (sortDirection.HasValue && sortDirection.GetValueOrDefault() == ListSortDirection.Ascending)
			{
				return ListSortDirection.Descending;
			}
			return ListSortDirection.Ascending;
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
			base.Serialize(json);
			if (AggregateFunctions.Any())
			{
				json["aggregates"] = AggregateFunctions.ToJson();
			}
		}
	}

	public class AggregateFunctionCollection : Collection<AggregateFunction>
	{
		public AggregateFunction this[string functionName] => this.FirstOrDefault((AggregateFunction f) => f.FunctionName == functionName);
	}

	public static class JsonObjectExtensions
	{
		public static IEnumerable<IDictionary<string, object>> ToJson(this IEnumerable<JsonObject> items)
		{
			return items.Select((JsonObject i) => i.ToJson());
		}
	}

	public class AggregateDescriptor : IDescriptor
	{
		private readonly IDictionary<string, Func<AggregateFunction>> aggregateFactories;

		public ICollection<AggregateFunction> Aggregates { get; private set; }

		public string Member { get; set; }

		public AggregateDescriptor()
		{
			Aggregates = new List<AggregateFunction>();
			aggregateFactories = new Dictionary<string, Func<AggregateFunction>>
		{
			{
				"sum",
				() => new SumFunction
				{
					SourceField = Member
				}
			},
			{
				"count",
				() => new CountFunction
				{
					SourceField = Member
				}
			},
			{
				"average",
				() => new AverageFunction
				{
					SourceField = Member
				}
			},
			{
				"min",
				() => new MinFunction
				{
					SourceField = Member
				}
			},
			{
				"max",
				() => new MaxFunction
				{
					SourceField = Member
				}
			}
		};
		}

		public void Deserialize(string source)
		{
			string[] array = source.Split('-');
			if (array.Any())
			{
				Member = array[0];
				for (int i = 1; i < array.Length; i++)
				{
					DeserializeAggregate(array[i]);
				}
			}
		}

		private void DeserializeAggregate(string aggregate)
		{
			if (aggregateFactories.TryGetValue(aggregate, out var value))
			{
				Aggregates.Add(value());
			}
		}

		public string Serialize()
		{
			StringBuilder stringBuilder = new StringBuilder(Member);
			foreach (string item in Aggregates.Select((AggregateFunction aggregate) => aggregate.FunctionName.Split('_')[0].ToLowerInvariant()))
			{
				stringBuilder.Append("-");
				stringBuilder.Append(item);
			}
			return stringBuilder.ToString();
		}
	}

	public class SumFunction : EnumerableSelectorAggregateFunction
	{
		public override string AggregateMethodName => "Sum";
	}

	public class CountFunction : EnumerableAggregateFunction
	{
		public override string AggregateMethodName => "Count";
	}

	public class AverageFunction : EnumerableSelectorAggregateFunction
	{
		public override string AggregateMethodName => "Average";
	}

	public class MinFunction : EnumerableSelectorAggregateFunction
	{
		public override string AggregateMethodName => "Min";
	}


	public class MaxFunction : EnumerableSelectorAggregateFunction
	{
		public override string AggregateMethodName => "Max";
	}




	internal class EnumerableAggregateFunctionExpressionBuilder : AggregateFunctionExpressionBuilderBase
	{
		protected new EnumerableAggregateFunction Function => (EnumerableAggregateFunction)base.Function;

		public EnumerableAggregateFunctionExpressionBuilder(Expression enumerableExpression, EnumerableAggregateFunction function)
			: base(enumerableExpression, function)
		{
		}

		public override Expression CreateAggregateExpression()
		{
			return Expression.Call(Function.ExtensionMethodsType, Function.AggregateMethodName, new Type[1] { base.ItemType }, base.EnumerableExpression);
		}
	}

	public abstract class EnumerableAggregateFunction : EnumerableAggregateFunctionBase
	{
		public override Expression CreateAggregateExpression(Expression enumerableExpression, bool liftMemberAccessToNull)
		{
			EnumerableAggregateFunctionExpressionBuilder enumerableAggregateFunctionExpressionBuilder = new EnumerableAggregateFunctionExpressionBuilder(enumerableExpression, this);
			enumerableAggregateFunctionExpressionBuilder.Options.LiftMemberAccessToNull = liftMemberAccessToNull;
			return enumerableAggregateFunctionExpressionBuilder.CreateAggregateExpression();
		}
	}



	public abstract class EnumerableAggregateFunctionBase : AggregateFunction
	{
		protected internal virtual Type ExtensionMethodsType => typeof(Enumerable);

		protected override string GenerateFunctionName()
		{
			string text = SourceField;
			if (text.HasValue())
			{
				text = text.Replace(".", "-");
			}
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", AggregateMethodName, text);
		}
	}

	public abstract class EnumerableSelectorAggregateFunction : EnumerableAggregateFunctionBase
	{
		public override Expression CreateAggregateExpression(Expression enumerableExpression, bool liftMemberAccessToNull)
		{
			EnumerableSelectorAggregateFunctionExpressionBuilder enumerableSelectorAggregateFunctionExpressionBuilder = new EnumerableSelectorAggregateFunctionExpressionBuilder(enumerableExpression, this);
			enumerableSelectorAggregateFunctionExpressionBuilder.Options.LiftMemberAccessToNull = liftMemberAccessToNull;
			return enumerableSelectorAggregateFunctionExpressionBuilder.CreateAggregateExpression();
		}
	}

	internal class EnumerableSelectorAggregateFunctionExpressionBuilder : AggregateFunctionExpressionBuilderBase
	{
		protected new EnumerableSelectorAggregateFunction Function => (EnumerableSelectorAggregateFunction)base.Function;

		public EnumerableSelectorAggregateFunctionExpressionBuilder(Expression enumerableExpression, EnumerableSelectorAggregateFunction function)
			: base(enumerableExpression, function)
		{
		}

		public override Expression CreateAggregateExpression()
		{
			LambdaExpression memberSelectorExpression = CreateMemberSelectorExpression();
			return CreateMethodCallExpression(memberSelectorExpression);
		}

		private LambdaExpression CreateMemberSelectorExpression()
		{
			MemberAccessExpressionBuilderBase memberAccessExpressionBuilderBase = ExpressionBuilderFactory.MemberAccess(base.ItemType, null, Function.SourceField);
			memberAccessExpressionBuilderBase.Options.CopyFrom(base.Options);
			Expression memberExpression = memberAccessExpressionBuilderBase.CreateMemberAccessExpression();
			memberExpression = ConvertMemberAccessExpression(memberExpression);
			return Expression.Lambda(memberExpression, memberAccessExpressionBuilderBase.ParameterExpression);
		}

		private Expression CreateMethodCallExpression(LambdaExpression memberSelectorExpression)
		{
			IEnumerable<Type> methodArgumentsTypes = GetMethodArgumentsTypes(memberSelectorExpression);
			return Expression.Call(Function.ExtensionMethodsType, Function.AggregateMethodName, methodArgumentsTypes.ToArray(), base.EnumerableExpression, memberSelectorExpression);
		}

		private IEnumerable<Type> GetMethodArgumentsTypes(LambdaExpression memberSelectorExpression)
		{
			yield return base.ItemType;
			if (!memberSelectorExpression.Body.Type.IsNumericType())
			{
				yield return memberSelectorExpression.Body.Type;
			}
		}

		private Expression ConvertMemberAccessExpression(Expression memberExpression)
		{
			if (base.ItemType.IsDynamicObject() && Function.MemberType != null)
			{
				memberExpression = Expression.Convert(memberExpression, Function.MemberType);
			}
			if (base.ItemType == typeof(DataRowView) && Function.MemberType != null)
			{
				memberExpression = Expression.Convert(memberExpression, Function.MemberType);
			}
			if (ShouldConvertTypeToInteger(memberExpression.Type.GetNonNullableType()))
			{
				memberExpression = ConvertMemberExpressionToInteger(memberExpression);
			}
			return memberExpression;
		}

		private static Expression ConvertMemberExpressionToInteger(Expression expression)
		{
			Type type = (expression.Type.IsNullableType() ? typeof(int?) : typeof(int));
			return Expression.Convert(expression, type);
		}

		private static bool ShouldConvertTypeToInteger(Type type)
		{
			if (!(type == typeof(sbyte)) && !(type == typeof(short)) && !(type == typeof(byte)))
			{
				return type == typeof(ushort);
			}
			return true;
		}
	}

	internal abstract class AggregateFunctionExpressionBuilderBase : ExpressionBuilderBase
	{
		private readonly AggregateFunction function;

		private readonly Expression enumerableExpression;

		protected AggregateFunction Function => function;

		protected Expression EnumerableExpression => enumerableExpression;

		protected AggregateFunctionExpressionBuilderBase(Expression enumerableExpression, AggregateFunction function)
			: base(ExtractItemTypeFromEnumerableType(enumerableExpression.Type))
		{
			this.enumerableExpression = enumerableExpression;
			this.function = function;
		}

		public abstract Expression CreateAggregateExpression();

		private static Type ExtractItemTypeFromEnumerableType(Type type)
		{
			Type type2 = type.FindGenericType(typeof(IEnumerable<>));
			if (type2 == null)
			{
				throw new ArgumentException("Provided type is not IEnumerable<>", "type");
			}
			return type2.GenericTypeArguments.First();
		}
	}


	public abstract class ExpressionBuilderBase
	{
		private readonly ExpressionBuilderOptions options;

		private readonly Type itemType;

		private ParameterExpression parameterExpression;

		public ExpressionBuilderOptions Options => options;

		protected internal Type ItemType => itemType;

		public ParameterExpression ParameterExpression
		{
			get
			{
				if (parameterExpression == null)
				{
					parameterExpression = Expression.Parameter(ItemType, "item");
				}
				return parameterExpression;
			}
			set
			{
				parameterExpression = value;
			}
		}

		protected ExpressionBuilderBase(Type itemType)
		{
			this.itemType = itemType;
			options = new ExpressionBuilderOptions();
		}
	}


	public class ExpressionBuilderOptions
	{
		public bool LiftMemberAccessToNull { get; set; }

		public ExpressionBuilderOptions()
		{
			LiftMemberAccessToNull = true;
		}

		public void CopyFrom(ExpressionBuilderOptions other)
		{
			LiftMemberAccessToNull = other.LiftMemberAccessToNull;
		}
	}

	internal static class TypeExtensions
	{
		internal static readonly Type[] PredefinedTypes = new Type[20]
		{
		typeof(object),
		typeof(bool),
		typeof(char),
		typeof(string),
		typeof(sbyte),
		typeof(byte),
		typeof(short),
		typeof(ushort),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(float),
		typeof(double),
		typeof(decimal),
		typeof(DateTime),
		typeof(TimeSpan),
		typeof(Guid),
		typeof(Math),
		typeof(Convert)
		};

		internal static bool IsPredefinedType(this Type type)
		{
			Type[] predefinedTypes = PredefinedTypes;
			for (int i = 0; i < predefinedTypes.Length; i++)
			{
				if (predefinedTypes[i] == type)
				{
					return true;
				}
			}
			return false;
		}

		internal static bool IsGenericType(this Type type)
		{
			return type.GetTypeInfo().IsGenericType;
		}

		internal static bool IsInterface(this Type type)
		{
			return type.GetTypeInfo().IsInterface;
		}

		internal static bool IsDynamicObject(this Type type)
		{
			if (!(type == typeof(object)))
			{
				return type.IsCompatibleWith(typeof(IDynamicMetaObjectProvider));
			}
			return true;
		}

		internal static bool IsNullableType(this Type type)
		{
			if (type.IsGenericType())
			{
				return type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}

		internal static Type GetNonNullableType(this Type type)
		{
			if (!type.IsNullableType())
			{
				return type;
			}
			return type.GetGenericArguments()[0];
		}

		internal static bool IsValueType(this Type type)
		{
			return type.GetTypeInfo().IsValueType;
		}

		internal static Type FindGenericType(this Type type, Type genericType)
		{
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType() && type.GetGenericTypeDefinition() == genericType)
				{
					return type;
				}
				if (genericType.IsInterface())
				{
					Type[] interfaces = type.GetInterfaces();
					for (int i = 0; i < interfaces.Length; i++)
					{
						Type type2 = interfaces[i].FindGenericType(genericType);
						if (type2 != null)
						{
							return type2;
						}
					}
				}
				type = type.GetTypeInfo().BaseType;
			}
			return null;
		}

		internal static MemberInfo FindPropertyOrField(this Type type, string memberName)
		{
			MemberInfo memberInfo = type.FindPropertyOrField(memberName, staticAccess: false);
			if (memberInfo == null)
			{
				memberInfo = type.FindPropertyOrField(memberName, staticAccess: true);
			}
			return memberInfo;
		}

		internal static MemberInfo FindPropertyOrField(this Type type, string memberName, bool staticAccess)
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
			foreach (Type item in type.SelfAndBaseTypes())
			{
				MemberInfo[] array = (from m in item.GetProperties(bindingAttr).OfType<MemberInfo>().Concat(item.GetFields(bindingAttr).OfType<MemberInfo>())
									  where m.Name.IsCaseInsensitiveEqual(memberName)
									  select m).ToArray();
				if (array.Length != 0)
				{
					return array[0];
				}
			}
			return null;
		}

		internal static IEnumerable<Type> SelfAndBaseTypes(this Type type)
		{
			if (type.IsInterface())
			{
				List<Type> list = new List<Type>();
				AddInterface(list, type);
				return list;
			}
			return type.SelfAndBaseClasses();
		}

		internal static IEnumerable<Type> SelfAndBaseClasses(this Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.GetTypeInfo().BaseType;
			}
		}

		private static void AddInterface(List<Type> types, Type type)
		{
			if (!types.Contains(type))
			{
				types.Add(type);
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					AddInterface(types, type2);
				}
			}
		}

		internal static PropertyInfo GetIndexerPropertyInfo(this Type type, params Type[] indexerArguments)
		{
			return (from p in type.GetProperties()
					where AreArgumentsApplicable(indexerArguments, p.GetIndexParameters())
					select p).FirstOrDefault();
		}

		private static bool AreArgumentsApplicable(IEnumerable<Type> arguments, IEnumerable<ParameterInfo> parameters)
		{
			List<Type> list = arguments.ToList();
			List<ParameterInfo> list2 = parameters.ToList();
			if (list.Count != list2.Count)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list2[i].ParameterType != list[i])
				{
					return false;
				}
			}
			return true;
		}

		internal static string GetTypeName(this Type type)
		{
			Type nonNullableType = type.GetNonNullableType();
			string text = nonNullableType.Name;
			if (type != nonNullableType)
			{
				text += "?";
			}
			return text;
		}

		internal static bool IsCompatibleWith(this Type source, Type target)
		{
			if (source == target)
			{
				return true;
			}
			if (!target.IsValueType())
			{
				return target.IsAssignableFrom(source);
			}
			Type nonNullableType = source.GetNonNullableType();
			Type nonNullableType2 = target.GetNonNullableType();
			if (nonNullableType != source && nonNullableType2 == target)
			{
				return false;
			}
			if (nonNullableType.IsEnumType() || nonNullableType2.IsEnumType())
			{
				return nonNullableType == nonNullableType2;
			}
			if (nonNullableType == typeof(sbyte))
			{
				if (!(nonNullableType2 == typeof(sbyte)) && !(nonNullableType2 == typeof(short)) && !(nonNullableType2 == typeof(int)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(byte))
			{
				if (!(nonNullableType2 == typeof(byte)) && !(nonNullableType2 == typeof(short)) && !(nonNullableType2 == typeof(ushort)) && !(nonNullableType2 == typeof(int)) && !(nonNullableType2 == typeof(uint)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(ulong)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(short))
			{
				if (!(nonNullableType2 == typeof(short)) && !(nonNullableType2 == typeof(int)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(ushort))
			{
				if (!(nonNullableType2 == typeof(ushort)) && !(nonNullableType2 == typeof(int)) && !(nonNullableType2 == typeof(uint)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(ulong)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(int))
			{
				if (!(nonNullableType2 == typeof(int)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(uint))
			{
				if (!(nonNullableType2 == typeof(uint)) && !(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(ulong)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(long))
			{
				if (!(nonNullableType2 == typeof(long)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(ulong))
			{
				if (!(nonNullableType2 == typeof(ulong)) && !(nonNullableType2 == typeof(float)) && !(nonNullableType2 == typeof(double)))
				{
					return nonNullableType2 == typeof(decimal);
				}
				return true;
			}
			if (nonNullableType == typeof(float))
			{
				if (!(nonNullableType2 == typeof(float)))
				{
					return nonNullableType2 == typeof(double);
				}
				return true;
			}
			return false;
		}

		internal static string FirstSortableProperty(this Type type)
		{
			PropertyInfo propertyInfo = (from property in type.GetProperties()
										 where property.PropertyType.IsPredefinedType()
										 select property).FirstOrDefault();
			if (propertyInfo == null)
			{
				throw new NotSupportedException(Exceptions.CannotFindPropertyToSortBy);
			}
			return propertyInfo.Name;
		}

		internal static object DefaultValue(this Type type)
		{
			if (type.IsValueType())
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		internal static bool IsEnumType(this Type type)
		{
			return type.GetNonNullableType().GetTypeInfo().IsEnum;
		}

		internal static bool IsNumericType(this Type type)
		{
			return type.GetNumericTypeKind() != 0;
		}

		internal static int GetNumericTypeKind(this Type type)
		{
			if (type == null)
			{
				return 0;
			}
			type = type.GetNonNullableType();
			if (type.IsEnumType())
			{
				return 0;
			}
			if (type == typeof(char) || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
			{
				return 1;
			}
			if (type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long))
			{
				return 2;
			}
			if (type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
			{
				return 3;
			}
			return 0;
		}

		internal static string ToJavaScriptType(this Type type)
		{
			if (type == null)
			{
				return "Object";
			}
			if (type == typeof(char) || type == typeof(char?))
			{
				return "String";
			}
			if (type.IsNumericType())
			{
				return "Number";
			}
			if (type == typeof(DateTime) || type == typeof(DateTime?))
			{
				return "Date";
			}
			if (type == typeof(string))
			{
				return "String";
			}
			if (type == typeof(bool) || type == typeof(bool?))
			{
				return "Boolean";
			}
			if (type.IsEnumType())
			{
				return "Number";
			}
			if (type.GetNonNullableType() == typeof(Guid))
			{
				return "String";
			}
			return "Object";
		}

		internal static bool IsPlainType(this Type type)
		{
			return !type.IsDynamicObject();
		}
	}

	public abstract class MemberAccessExpressionBuilderBase : ExpressionBuilderBase
	{
		private readonly string memberName;

		public string MemberName => memberName;

		protected MemberAccessExpressionBuilderBase(Type itemType, string memberName)
			: base(itemType)
		{
			this.memberName = memberName;
		}

		public abstract Expression CreateMemberAccessExpression();

		public LambdaExpression CreateLambdaExpression()
		{
			return Expression.Lambda(CreateMemberAccessExpression(), base.ParameterExpression);
		}
	}

	public static class ExpressionBuilderFactory
	{
		public static MemberAccessExpressionBuilderBase MemberAccess(Type elementType, Type memberType, string memberName)
		{
			memberType = memberType ?? typeof(object);
			if (elementType.IsCompatibleWith(typeof(XmlNode)))
			{
				return new XmlNodeChildElementAccessExpressionBuilder(memberName);
			}
			if (elementType.IsCompatibleWith(typeof(ICustomTypeDescriptor)))
			{
				return new CustomTypeDescriptorPropertyAccessExpressionBuilder(elementType, memberType, memberName);
			}
			if (elementType == typeof(object) || elementType.IsCompatibleWith(typeof(IDynamicMetaObjectProvider)))
			{
				return new DynamicPropertyAccessExpressionBuilder(elementType, memberName);
			}
			return new PropertyAccessExpressionBuilder(elementType, memberName);
		}

		public static MemberAccessExpressionBuilderBase MemberAccess(Type elementType, string memberName, bool liftMemberAccess)
		{
			MemberAccessExpressionBuilderBase memberAccessExpressionBuilderBase = MemberAccess(elementType, null, memberName);
			memberAccessExpressionBuilderBase.Options.LiftMemberAccessToNull = liftMemberAccess;
			return memberAccessExpressionBuilderBase;
		}

		public static MemberAccessExpressionBuilderBase MemberAccess(Type elementType, Type memberType, string memberName, bool liftMemberAccess)
		{
			MemberAccessExpressionBuilderBase memberAccessExpressionBuilderBase = MemberAccess(elementType, memberType, memberName);
			memberAccessExpressionBuilderBase.Options.LiftMemberAccessToNull = liftMemberAccess;
			return memberAccessExpressionBuilderBase;
		}

		public static MemberAccessExpressionBuilderBase MemberAccess(IQueryable source, Type memberType, string memberName)
		{
			MemberAccessExpressionBuilderBase memberAccessExpressionBuilderBase = MemberAccess(source.ElementType, memberType, memberName);
			memberAccessExpressionBuilderBase.Options.LiftMemberAccessToNull = source.Provider.IsLinqToObjectsProvider();
			return memberAccessExpressionBuilderBase;
		}
	}

	internal class XmlNodeChildElementAccessExpressionBuilder : MemberAccessExpressionBuilderBase
	{
		private static readonly MethodInfo ChildElementInnerTextMethod = typeof(XmlNodeExtensions).GetMethod("ChildElementInnerText", new Type[2]
		{
		typeof(XmlNode),
		typeof(string)
		});

		public XmlNodeChildElementAccessExpressionBuilder(string memberName)
			: base(typeof(XmlNode), memberName)
		{
		}

		public override Expression CreateMemberAccessExpression()
		{
			ConstantExpression arg = Expression.Constant(base.MemberName);
			return Expression.Call(ChildElementInnerTextMethod, base.ParameterExpression, arg);
		}
	}


	internal class CustomTypeDescriptorPropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
	{
		private static readonly MethodInfo PropertyMethod = typeof(CustomTypeDescriptorExtensions).GetMethod("Property");

		private readonly Type propertyType;

		public Type PropertyType => propertyType;

		public CustomTypeDescriptorPropertyAccessExpressionBuilder(Type elementType, Type memberType, string memberName)
			: base(elementType, memberName)
		{
			if (!elementType.IsCompatibleWith(typeof(ICustomTypeDescriptor)))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "ElementType: {0} did not implement {1}", elementType, typeof(ICustomTypeDescriptor)), "elementType");
			}
			propertyType = GetPropertyType(memberType);
		}

		private Type GetPropertyType(Type memberType)
		{
			Type propertyTypeFromTypeDescriptorProvider = GetPropertyTypeFromTypeDescriptorProvider();
			if (propertyTypeFromTypeDescriptorProvider != null)
			{
				memberType = propertyTypeFromTypeDescriptorProvider;
			}
			if (memberType.IsValueType && !memberType.IsNullableType())
			{
				return typeof(Nullable<>).MakeGenericType(memberType);
			}
			return memberType;
		}

		private Type GetPropertyTypeFromTypeDescriptorProvider()
		{
			return TypeDescriptor.GetProperties(base.ItemType)[base.MemberName]?.PropertyType;
		}

		public override Expression CreateMemberAccessExpression()
		{
			ConstantExpression arg = Expression.Constant(base.MemberName);
			return Expression.Call(PropertyMethod.MakeGenericMethod(propertyType), base.ParameterExpression, arg);
		}
	}


	internal static class CustomTypeDescriptorExtensions
	{
		public static T Property<T>(this ICustomTypeDescriptor typeDescriptor, string propertyName)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(typeDescriptor)[propertyName];
			if (propertyDescriptor == null)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Property with specified name: {0} cannot be found on type: {1}", propertyName, typeDescriptor.GetType()), "propertyName");
			}
			return UnboxT<T>.Unbox(propertyDescriptor.GetValue(typeDescriptor));
		}
	}

	internal static class UnboxT<T>
	{
		internal static readonly Func<object, T> Unbox = Create(typeof(T));

		private static Func<object, T> Create(Type type)
		{
			if (!type.IsValueType())
			{
				return ReferenceField;
			}
			if (type.IsGenericType() && !type.GetTypeInfo().IsGenericTypeDefinition && typeof(Nullable<>) == type.GetGenericTypeDefinition())
			{
				return (Func<object, T>)typeof(UnboxT<T>).GetMethod("NullableField", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type.GetGenericArguments()[0]).CreateDelegate(typeof(Func<object, T>));
			}
			return ValueField;
		}

		private static TElem? NullableField<TElem>(object value) where TElem : struct
		{
			if (DBNull.Value == value)
			{
				return null;
			}
			return (TElem?)value;
		}

		private static T ReferenceField(object value)
		{
			if (DBNull.Value != value)
			{
				return (T)value;
			}
			return default(T);
		}

		private static T ValueField(object value)
		{
			if (DBNull.Value == value)
			{
				throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, "Type: {0} cannot be casted to Nullable type", typeof(T)));
			}
			return (T)value;
		}
	}

	public static class XmlNodeExtensions
	{
		public static string ChildElementInnerText(this XmlNode node, string childName)
		{
			XmlElement xmlElement = node[childName];
			if (xmlElement == null)
			{
				string.Format(CultureInfo.CurrentCulture, "Child element with specified name: {0} cannot be found.", childName);
				return null;
			}
			return xmlElement.InnerText;
		}
	}


	public class DynamicPropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
	{
		public DynamicPropertyAccessExpressionBuilder(Type itemType, string memberName)
			: base(itemType, memberName)
		{
		}

		public override Expression CreateMemberAccessExpression()
		{
			if (string.IsNullOrEmpty(base.MemberName))
			{
				return base.ParameterExpression;
			}
			Expression expression = base.ParameterExpression;
			foreach (IMemberAccessToken token in MemberAccessTokenizer.GetTokens(base.MemberName))
			{
				if (token is PropertyToken)
				{
					string propertyName = ((PropertyToken)token).PropertyName;
					expression = CreatePropertyAccessExpression(expression, propertyName);
				}
				else if (token is IndexerToken)
				{
					expression = CreateIndexerAccessExpression(expression, (IndexerToken)token);
				}
			}
			return expression;
		}

		private Expression CreateIndexerAccessExpression(Expression instance, IndexerToken indexerToken)
		{
			return DynamicExpression.Dynamic(Binder.GetIndex(CSharpBinderFlags.None, typeof(DynamicPropertyAccessExpressionBuilder), new CSharpArgumentInfo[2]
			{
			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
			}), typeof(object), new Expression[2]
			{
			instance,
			indexerToken.Arguments.Select(Expression.Constant).First()
			});
		}

		private Expression CreatePropertyAccessExpression(Expression instance, string propertyName)
		{
			return DynamicExpression.Dynamic(Binder.GetMember(CSharpBinderFlags.None, propertyName, typeof(DynamicPropertyAccessExpressionBuilder), new CSharpArgumentInfo[1] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }), typeof(object), new Expression[1] { instance });
		}
	}

	internal interface IMemberAccessToken
	{
	}


	internal static class MemberAccessTokenizer
	{
		public static IEnumerable<IMemberAccessToken> GetTokens(string memberPath)
		{
			string[] array = memberPath.Split(new char[2] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (TryParseIndexerToken(text, out var token))
				{
					yield return token;
				}
				else
				{
					yield return new PropertyToken(text);
				}
			}
		}

		private static bool TryParseIndexerToken(string member, out IndexerToken token)
		{
			token = null;
			if (!IsValidIndexer(member))
			{
				return false;
			}
			List<object> list = new List<object>();
			list.AddRange(from a in ExtractIndexerArguments(member)
						  select ConvertIndexerArgument(a));
			token = new IndexerToken(list);
			return true;
		}

		private static bool IsValidIndexer(string member)
		{
			return member.EndsWith("]", StringComparison.Ordinal);
		}

		private static IEnumerable<string> ExtractIndexerArguments(string member)
		{
			string text = member.TrimEnd(']');
			string[] array = text.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}

		private static object ConvertIndexerArgument(string argument)
		{
			if (int.TryParse(argument, out var result))
			{
				return result;
			}
			if (argument.StartsWith("\"", StringComparison.Ordinal))
			{
				return argument.Trim('"');
			}
			if (argument.StartsWith("'", StringComparison.Ordinal))
			{
				string text = argument.Trim('\'');
				if (text.Length == 1)
				{
					return text[0];
				}
				return text;
			}
			return argument;
		}
	}

	internal class PropertyToken : IMemberAccessToken
	{
		private readonly string propertyName;

		public string PropertyName => propertyName;

		public PropertyToken(string propertyName)
		{
			this.propertyName = propertyName;
		}
	}


	internal class IndexerToken : IMemberAccessToken
	{
		private readonly ReadOnlyCollection<object> arguments;

		public ReadOnlyCollection<object> Arguments => arguments;

		public IndexerToken(IEnumerable<object> arguments)
		{
			this.arguments = arguments.ToReadOnlyCollection();
		}

		public IndexerToken(params object[] arguments)
			: this((IEnumerable<object>)arguments)
		{
		}
	}


	public static class EnumerableExtensions
	{
		private class GenericEnumerable<T> : IEnumerable<T>, IEnumerable
		{
			private readonly IEnumerable source;

			public GenericEnumerable(IEnumerable source)
			{
				this.source = source;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return source.GetEnumerator();
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				foreach (T item in source)
				{
					yield return item;
				}
			}
		}

		private static class DefaultReadOnlyCollection<T>
		{
			private static ReadOnlyCollection<T> defaultCollection;

			internal static ReadOnlyCollection<T> Empty
			{
				get
				{
					if (defaultCollection == null)
					{
						defaultCollection = new ReadOnlyCollection<T>(new T[0]);
					}
					return defaultCollection;
				}
			}
		}

		public static void Each<T>(this IEnumerable<T> instance, Action<T, int> action)
		{
			int num = 0;
			foreach (T item in instance)
			{
				action(item, num++);
			}
		}

		public static void Each<T>(this IEnumerable<T> instance, Action<T> action)
		{
			foreach (T item in instance)
			{
				action(item);
			}
		}

		public static IEnumerable AsGenericEnumerable(this IEnumerable source)
		{
			Type type = typeof(object);
			if (source.GetType().FindGenericType(typeof(IEnumerable<>)) != null)
			{
				return source;
			}
			IEnumerator enumerator = source.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current != null)
				{
					type = enumerator.Current.GetType();
					try
					{
						enumerator.Reset();
					}
					catch
					{
					}
					break;
				}
			}
			Type type2 = typeof(GenericEnumerable<>).MakeGenericType(type);
			object[] args = new object[1] { source };
			return (IEnumerable)Activator.CreateInstance(type2, args);
		}

		public static int IndexOf(this IEnumerable source, object item)
		{
			int num = 0;
			foreach (object item2 in source)
			{
				if (object.Equals(item2, item))
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		internal static object ElementAt(this IEnumerable source, int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			IList list = source as IList;
			if (list != null && list.Count > 0)
			{
				return list[index];
			}
			foreach (object item in source)
			{
				if (index == 0)
				{
					return item;
				}
				index--;
			}
			return null;
		}

		public static IEnumerable<TSource> SelectRecursive<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> recursiveSelector)
		{
			Stack<IEnumerator<TSource>> stack = new Stack<IEnumerator<TSource>>();
			stack.Push(source.GetEnumerator());
			try
			{
				while (stack.Count > 0)
				{
					if (stack.Peek().MoveNext())
					{
						TSource current = stack.Peek().Current;
						yield return current;
						IEnumerable<TSource> enumerable = recursiveSelector(current);
						if (enumerable != null)
						{
							stack.Push(enumerable.GetEnumerator());
						}
					}
					else
					{
						stack.Pop().Dispose();
					}
				}
			}
			finally
			{
				while (stack.Count > 0)
				{
					stack.Pop().Dispose();
				}
			}
		}

		internal static IEnumerable<TResult> Consolidate<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			if (first == null)
			{
				throw new ArgumentNullException("first");
			}
			if (second == null)
			{
				throw new ArgumentNullException("second");
			}
			if (resultSelector == null)
			{
				throw new ArgumentNullException("resultSelector");
			}
			return ZipIterator(first, second, resultSelector);
		}

		private static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			using (IEnumerator<TFirst> e1 = first.GetEnumerator())
			{
				using (IEnumerator<TSecond> e2 = second.GetEnumerator())
				{
					while (e1.MoveNext() && e2.MoveNext())
					{
						yield return resultSelector(e1.Current, e2.Current);
					}
				}
			}
		}

		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
		{
			if (sequence == null)
			{
				return DefaultReadOnlyCollection<T>.Empty;
			}
			ReadOnlyCollection<T> readOnlyCollection = sequence as ReadOnlyCollection<T>;
			if (readOnlyCollection != null)
			{
				return readOnlyCollection;
			}
			return new ReadOnlyCollection<T>(sequence.ToArray());
		}
	}

	internal class PropertyAccessExpressionBuilder : MemberAccessExpressionBuilderBase
	{
		public PropertyAccessExpressionBuilder(Type itemType, string memberName)
			: base(itemType, memberName)
		{
		}

		public override Expression CreateMemberAccessExpression()
		{
			if (string.IsNullOrEmpty(base.MemberName))
			{
				return base.ParameterExpression;
			}
			return ExpressionFactory.MakeMemberAccess(base.ParameterExpression, base.MemberName, base.Options.LiftMemberAccessToNull);
		}
	}

	public static class ExpressionFactory
	{
		public static ConstantExpression ZeroExpression => Expression.Constant(0);

		public static ConstantExpression EmptyStringExpression => Expression.Constant(string.Empty);

		public static Expression DefaltValueExpression(Type type)
		{
			return Expression.Constant(type.DefaultValue(), type);
		}

		public static Expression MakeMemberAccess(Expression instance, string memberName)
		{
			foreach (IMemberAccessToken token in MemberAccessTokenizer.GetTokens(memberName))
			{
				instance = token.CreateMemberAccessExpression(instance);
			}
			return instance;
		}

		public static Expression MakeMemberAccess(Expression instance, string memberName, bool liftMemberAccessToNull)
		{
			Expression expression = MakeMemberAccess(instance, memberName);
			if (liftMemberAccessToNull)
			{
				return LiftMemberAccessToNull(expression);
			}
			return expression;
		}

		public static Expression LiftMemberAccessToNull(Expression memberAccess)
		{
			Expression defaultValue = DefaltValueExpression(memberAccess.Type);
			return LiftMemberAccessToNullRecursive(memberAccess, memberAccess, defaultValue);
		}

		public static Expression LiftMethodCallToNull(Expression instance, MethodInfo method, params Expression[] arguments)
		{
			return LiftMemberAccessToNull(Expression.Call(ExtractMemberAccessExpressionFromLiftedExpression(instance), method, arguments));
		}

		private static Expression LiftMemberAccessToNullRecursive(Expression memberAccess, Expression conditionalExpression, Expression defaultValue)
		{
			Expression instanceExpressionFromExpression = GetInstanceExpressionFromExpression(memberAccess);
			if (instanceExpressionFromExpression == null)
			{
				return conditionalExpression;
			}
			conditionalExpression = CreateIfNullExpression(instanceExpressionFromExpression, conditionalExpression, defaultValue);
			return LiftMemberAccessToNullRecursive(instanceExpressionFromExpression, conditionalExpression, defaultValue);
		}

		private static Expression GetInstanceExpressionFromExpression(Expression memberAccess)
		{
			MemberExpression memberExpression = memberAccess as MemberExpression;
			if (memberExpression != null)
			{
				return memberExpression.Expression;
			}
			return (memberAccess as MethodCallExpression)?.Object;
		}

		private static Expression CreateIfNullExpression(Expression instance, Expression memberAccess, Expression defaultValue)
		{
			if (ShouldGenerateCondition(instance.Type))
			{
				return CreateConditionExpression(instance, memberAccess, defaultValue);
			}
			return memberAccess;
		}

		private static bool ShouldGenerateCondition(Type type)
		{
			if (type.IsValueType())
			{
				return type.IsNullableType();
			}
			return true;
		}

		private static Expression CreateConditionExpression(Expression instance, Expression memberAccess, Expression defaultValue)
		{
			Expression right = DefaltValueExpression(instance.Type);
			return Expression.Condition(Expression.NotEqual(instance, right), memberAccess, defaultValue);
		}

		private static Expression ExtractMemberAccessExpressionFromLiftedExpression(Expression liftedToNullExpression)
		{
			while (liftedToNullExpression.NodeType == ExpressionType.Conditional)
			{
				ConditionalExpression conditionalExpression = (ConditionalExpression)liftedToNullExpression;
				liftedToNullExpression = ((conditionalExpression.Test.NodeType != ExpressionType.NotEqual) ? conditionalExpression.IfFalse : conditionalExpression.IfTrue);
			}
			return liftedToNullExpression;
		}

		internal static Expression LiftStringExpressionToEmpty(Expression stringExpression)
		{
			if (stringExpression.Type != typeof(string))
			{
				throw new ArgumentException("Provided expression should have string type", "stringExpression");
			}
			if (IsNotNullConstantExpression(stringExpression))
			{
				return stringExpression;
			}
			return Expression.Coalesce(stringExpression, EmptyStringExpression);
		}

		internal static bool IsNotNullConstantExpression(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Constant)
			{
				return ((ConstantExpression)expression).Value != null;
			}
			return false;
		}
	}

	internal static class MemberAccessTokenExtensions
	{
		public static Expression CreateMemberAccessExpression(this IMemberAccessToken token, Expression instance)
		{
			MemberInfo memberInfoForType = token.GetMemberInfoForType(instance.Type);
			if (memberInfoForType == null)
			{
				throw new ArgumentException(FormatInvalidTokenErrorMessage(token, instance.Type));
			}
			IndexerToken indexerToken = token as IndexerToken;
			if (indexerToken != null)
			{
				IEnumerable<Expression> indexerArguments = indexerToken.GetIndexerArguments();
				return Expression.Call(instance, (MethodInfo)memberInfoForType, indexerArguments);
			}
			return Expression.MakeMemberAccess(instance, memberInfoForType);
		}

		private static string FormatInvalidTokenErrorMessage(IMemberAccessToken token, Type type)
		{
			PropertyToken propertyToken = token as PropertyToken;
			string arg;
			string arg2;
			if (propertyToken != null)
			{
				arg = "property or field";
				arg2 = propertyToken.PropertyName;
			}
			else
			{
				arg = "indexer with arguments";
				IEnumerable<string> source = from a in ((IndexerToken)token).Arguments
											 where a != null
											 select a.ToString();
				arg2 = string.Join(",", source.ToArray());
			}
			return string.Format(CultureInfo.CurrentCulture, "Invalid {0} - '{1}' for type: {2}", arg, arg2, type.GetTypeName());
		}

		private static IEnumerable<Expression> GetIndexerArguments(this IndexerToken indexerToken)
		{
			return ((IEnumerable<object>)indexerToken.Arguments).Select((Func<object, Expression>)((object a) => Expression.Constant(a)));
		}

		private static MemberInfo GetMemberInfoForType(this IMemberAccessToken token, Type targetType)
		{
			PropertyToken propertyToken = token as PropertyToken;
			if (propertyToken != null)
			{
				return GetMemberInfoFromPropertyToken(propertyToken, targetType);
			}
			IndexerToken indexerToken = token as IndexerToken;
			if (indexerToken != null)
			{
				return GetMemberInfoFromIndexerToken(indexerToken, targetType);
			}
			throw new InvalidOperationException(token.GetType().GetTypeName() + " is not supported");
		}

		private static MemberInfo GetMemberInfoFromPropertyToken(PropertyToken token, Type targetType)
		{
			return targetType.FindPropertyOrField(token.PropertyName);
		}

		private static MemberInfo GetMemberInfoFromIndexerToken(IndexerToken token, Type targetType)
		{
			PropertyInfo indexerPropertyInfo = targetType.GetIndexerPropertyInfo(token.Arguments.Select((object a) => a.GetType()).ToArray());
			if (indexerPropertyInfo != null)
			{
				return indexerPropertyInfo.GetGetMethod();
			}
			return null;
		}
	}

	internal static class QueryProviderExtensions
	{
		public static bool IsEntityFrameworkProvider(this IQueryProvider provider)
		{
			string fullName = provider.GetType().FullName;
			if (!(fullName == "System.Data.Objects.ELinq.ObjectQueryProvider") && !(fullName == "System.Data.Entity.Core.Objects.ELinq.ObjectQueryProvider") && !fullName.StartsWith("LinqKit.ExpandableQueryProvider") && !fullName.StartsWith("Microsoft.Data.Entity.Query.EntityQueryProvider"))
			{
				return fullName.StartsWith("System.Data.Entity.Internal.Linq");
			}
			return true;
		}

		public static bool IsLinqToObjectsProvider(this IQueryProvider provider)
		{
			return provider.GetType().FullName.Contains("EnumerableQuery");
		}
	}

	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Exceptions
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("Kendo.Mvc.Resources.Exceptions", typeof(Exceptions).GetTypeInfo().Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string ArrayCannotBeEmpty => ResourceManager.GetString("ArrayCannotBeEmpty", resourceCulture);

		internal static string BatchUpdatesRequireInCellMode => ResourceManager.GetString("BatchUpdatesRequireInCellMode", resourceCulture);

		internal static string BatchUpdatesRequireUpdate => ResourceManager.GetString("BatchUpdatesRequireUpdate", resourceCulture);

		internal static string CannotBeNegative => ResourceManager.GetString("CannotBeNegative", resourceCulture);

		internal static string CannotBeNegativeOrZero => ResourceManager.GetString("CannotBeNegativeOrZero", resourceCulture);

		internal static string CannotBeNull => ResourceManager.GetString("CannotBeNull", resourceCulture);

		internal static string CannotBeNullOrEmpty => ResourceManager.GetString("CannotBeNullOrEmpty", resourceCulture);

		internal static string CannotFindPropertyToSortBy => ResourceManager.GetString("CannotFindPropertyToSortBy", resourceCulture);

		internal static string CannotHaveMoreOneColumnInOrderWhenSortModeIsSetToSingleColumn => ResourceManager.GetString("CannotHaveMoreOneColumnInOrderWhenSortModeIsSetToSingleColumn", resourceCulture);

		internal static string CannotRouteToClassNamedController => ResourceManager.GetString("CannotRouteToClassNamedController", resourceCulture);

		internal static string CannotSetAutoBindIfBoundDuringInitialization => ResourceManager.GetString("CannotSetAutoBindIfBoundDuringInitialization", resourceCulture);

		internal static string CannotUseAjaxAndWebServiceAtTheSameTime => ResourceManager.GetString("CannotUseAjaxAndWebServiceAtTheSameTime", resourceCulture);

		internal static string CannotUseDetailTemplateAndLockedColumns => ResourceManager.GetString("CannotUseDetailTemplateAndLockedColumns", resourceCulture);

		internal static string CannotUseLockedColumnsAndServerBinding => ResourceManager.GetString("CannotUseLockedColumnsAndServerBinding", resourceCulture);

		internal static string CannotUsePushStateWithServerNavigation => ResourceManager.GetString("CannotUsePushStateWithServerNavigation", resourceCulture);

		internal static string CannotUseRowTemplateAndLockedColumns => ResourceManager.GetString("CannotUseRowTemplateAndLockedColumns", resourceCulture);

		internal static string CannotUseTemplatesInAjaxOrWebService => ResourceManager.GetString("CannotUseTemplatesInAjaxOrWebService", resourceCulture);

		internal static string CannotUseVirtualScrollWithServerBinding => ResourceManager.GetString("CannotUseVirtualScrollWithServerBinding", resourceCulture);

		internal static string CollectionCannotBeEmpty => ResourceManager.GetString("CollectionCannotBeEmpty", resourceCulture);

		internal static string ControllerNameAmbiguousWithoutRouteUrl => ResourceManager.GetString("ControllerNameAmbiguousWithoutRouteUrl", resourceCulture);

		internal static string ControllerNameAmbiguousWithRouteUrl => ResourceManager.GetString("ControllerNameAmbiguousWithRouteUrl", resourceCulture);

		internal static string ControllerNameMustEndWithController => ResourceManager.GetString("ControllerNameMustEndWithController", resourceCulture);

		internal static string CustomCommandRoutesWithAjaxBinding => ResourceManager.GetString("CustomCommandRoutesWithAjaxBinding", resourceCulture);

		internal static string DataKeysEmpty => ResourceManager.GetString("DataKeysEmpty", resourceCulture);

		internal static string DataTableInLineEditingWithCustomEditorTemplates => ResourceManager.GetString("DataTableInLineEditingWithCustomEditorTemplates", resourceCulture);

		internal static string DataTablePopUpTemplate => ResourceManager.GetString("DataTablePopUpTemplate", resourceCulture);

		internal static string DeleteCommandRequiresDelete => ResourceManager.GetString("DeleteCommandRequiresDelete", resourceCulture);

		internal static string EditCommandRequiresUpdate => ResourceManager.GetString("EditCommandRequiresUpdate", resourceCulture);

		internal static string ExcelExportNotSupportedInServerBinding => ResourceManager.GetString("ExcelExportNotSupportedInServerBinding", resourceCulture);

		internal static string FirstPropertyShouldNotBeBiggerThenSecondProperty => ResourceManager.GetString("FirstPropertyShouldNotBeBiggerThenSecondProperty", resourceCulture);

		internal static string GroupWithSpecifiedNameAlreadyExists => ResourceManager.GetString("GroupWithSpecifiedNameAlreadyExists", resourceCulture);

		internal static string GroupWithSpecifiedNameAlreadyExistsPleaseSpecifyADifferentName => ResourceManager.GetString("GroupWithSpecifiedNameAlreadyExistsPleaseSpecifyADifferentName", resourceCulture);

		internal static string GroupWithSpecifiedNameDoesNotExistInAssetTypeOfSharedWebAssets => ResourceManager.GetString("GroupWithSpecifiedNameDoesNotExistInAssetTypeOfSharedWebAssets", resourceCulture);

		internal static string GroupWithSpecifiedNameDoesNotExistPleaseMakeSureYouHaveSpecifiedACorrectName => ResourceManager.GetString("GroupWithSpecifiedNameDoesNotExistPleaseMakeSureYouHaveSpecifiedACorrectName", resourceCulture);

		internal static string InCellModeNotSupportedInServerBinding => ResourceManager.GetString("InCellModeNotSupportedInServerBinding", resourceCulture);

		internal static string InCellModeNotSupportedWithRowTemplate => ResourceManager.GetString("InCellModeNotSupportedWithRowTemplate", resourceCulture);

		internal static string IndexOutOfRange => ResourceManager.GetString("IndexOutOfRange", resourceCulture);

		internal static string InsertCommandRequiresInsert => ResourceManager.GetString("InsertCommandRequiresInsert", resourceCulture);

		internal static string ItemWithSpecifiedSourceAlreadyExists => ResourceManager.GetString("ItemWithSpecifiedSourceAlreadyExists", resourceCulture);

		internal static string LocalGroupWithSpecifiedNameAlreadyExists => ResourceManager.GetString("LocalGroupWithSpecifiedNameAlreadyExists", resourceCulture);

		internal static string LocalizationKeyNotFound => ResourceManager.GetString("LocalizationKeyNotFound", resourceCulture);

		internal static string MemberExpressionRequired => ResourceManager.GetString("MemberExpressionRequired", resourceCulture);

		internal static string MinPropertyMustBeLessThenMaxProperty => ResourceManager.GetString("MinPropertyMustBeLessThenMaxProperty", resourceCulture);

		internal static string NameCannotBeBlank => ResourceManager.GetString("NameCannotBeBlank", resourceCulture);

		internal static string NameCannotContainSpaces => ResourceManager.GetString("NameCannotContainSpaces", resourceCulture);

		internal static string NoneIsOnlyUsedForInternalPurpose => ResourceManager.GetString("NoneIsOnlyUsedForInternalPurpose", resourceCulture);

		internal static string OnlyOneScriptRegistrarIsAllowedInASingleRequest => ResourceManager.GetString("OnlyOneScriptRegistrarIsAllowedInASingleRequest", resourceCulture);

		internal static string OnlyOneStyleSheetRegistrarIsAllowedInASingleRequest => ResourceManager.GetString("OnlyOneStyleSheetRegistrarIsAllowedInASingleRequest", resourceCulture);

		internal static string OnlyPropertyAndFieldExpressionsAreSupported => ResourceManager.GetString("OnlyPropertyAndFieldExpressionsAreSupported", resourceCulture);

		internal static string Pager_Of => ResourceManager.GetString("Pager_Of", resourceCulture);

		internal static string PagingMustBeEnabledToUsePageOnScroll => ResourceManager.GetString("PagingMustBeEnabledToUsePageOnScroll", resourceCulture);

		internal static string PropertyMustBeBiggerThenZero => ResourceManager.GetString("PropertyMustBeBiggerThenZero", resourceCulture);

		internal static string PropertyMustBePositiveNumber => ResourceManager.GetString("PropertyMustBePositiveNumber", resourceCulture);

		internal static string PropertyShouldBeInRange => ResourceManager.GetString("PropertyShouldBeInRange", resourceCulture);

		internal static string Rtl => ResourceManager.GetString("Rtl", resourceCulture);

		internal static string ScrollingMustBeEnabledToUsePageOnScroll => ResourceManager.GetString("ScrollingMustBeEnabledToUsePageOnScroll", resourceCulture);

		internal static string SiteMapShouldBeDefinedInViewData => ResourceManager.GetString("SiteMapShouldBeDefinedInViewData", resourceCulture);

		internal static string SourceMustBeAVirtualPathWhichShouldStartsWithTileAndSlash => ResourceManager.GetString("SourceMustBeAVirtualPathWhichShouldStartsWithTileAndSlash", resourceCulture);

		internal static string SpecifiedFileDoesNotExist => ResourceManager.GetString("SpecifiedFileDoesNotExist", resourceCulture);

		internal static string StringNotCorrectDate => ResourceManager.GetString("StringNotCorrectDate", resourceCulture);

		internal static string StringNotCorrectTimeSpan => ResourceManager.GetString("StringNotCorrectTimeSpan", resourceCulture);

		internal static string TheSpecifiedMethodIsNotAnActionMethod => ResourceManager.GetString("TheSpecifiedMethodIsNotAnActionMethod", resourceCulture);

		internal static string TimeOutOfRange => ResourceManager.GetString("TimeOutOfRange", resourceCulture);

		internal static string TooltipContainerShouldBeSet => ResourceManager.GetString("TooltipContainerShouldBeSet", resourceCulture);

		internal static string UrlAndContentUrlCannotBeSet => ResourceManager.GetString("UrlAndContentUrlCannotBeSet", resourceCulture);

		internal static string ValueNotValidForProperty => ResourceManager.GetString("ValueNotValidForProperty", resourceCulture);

		internal static string WebServiceUrlRequired => ResourceManager.GetString("WebServiceUrlRequired", resourceCulture);

		internal static string YouCannotAddMoreThanOnceColumnWhenSortModeIsSetToSingle => ResourceManager.GetString("YouCannotAddMoreThanOnceColumnWhenSortModeIsSetToSingle", resourceCulture);

		internal static string YouCannotCallBindToWithoutCustomBinding => ResourceManager.GetString("YouCannotCallBindToWithoutCustomBinding", resourceCulture);

		internal static string YouCannotCallRenderMoreThanOnce => ResourceManager.GetString("YouCannotCallRenderMoreThanOnce", resourceCulture);

		internal static string YouCannotCallStartMoreThanOnce => ResourceManager.GetString("YouCannotCallStartMoreThanOnce", resourceCulture);

		internal static string YouCannotConfigureASharedWebAssetGroup => ResourceManager.GetString("YouCannotConfigureASharedWebAssetGroup", resourceCulture);

		internal static string YouMustHaveToCallStartPriorCallingThisMethod => ResourceManager.GetString("YouMustHaveToCallStartPriorCallingThisMethod", resourceCulture);

		internal static string YouCannotOverrideModelExpressionName => ResourceManager.GetString("YouCannotOverrideModelExpressionName", resourceCulture);

		internal Exceptions()
		{
		}
	}


	public static class CollectionExtensions
	{
		public static void AddRange<T>(this ICollection<T> instance, IEnumerable<T> collection)
		{
			foreach (T item in collection)
			{
				instance.Add(item);
			}
		}
	}
	public class CompositeFilterDescriptor : FilterDescriptorBase
	{
		private FilterDescriptorCollection filterDescriptors;

		public FilterCompositionLogicalOperator LogicalOperator
		{
			get;
			set;
		}

		public FilterDescriptorCollection FilterDescriptors
		{
			get
			{
				if (filterDescriptors == null)
				{
					SetFilterDescriptors(new FilterDescriptorCollection());
				}
				return filterDescriptors;
			}
			set
			{
				if (filterDescriptors != value)
				{
					SetFilterDescriptors(value);
				}
			}
		}

		protected override Expression CreateFilterExpression(ParameterExpression parameterExpression)
		{
			FilterDescriptorCollectionExpressionBuilder filterDescriptorCollectionExpressionBuilder = new FilterDescriptorCollectionExpressionBuilder(parameterExpression, FilterDescriptors, LogicalOperator);
			filterDescriptorCollectionExpressionBuilder.Options.CopyFrom(base.ExpressionBuilderOptions);
			return filterDescriptorCollectionExpressionBuilder.CreateBodyExpression();
		}

		private void SetFilterDescriptors(FilterDescriptorCollection value)
		{
			FilterDescriptorCollection filterDescriptor = filterDescriptors;
			filterDescriptors = value;
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
			base.Serialize(json);
			json["logic"] = LogicalOperator.ToString().ToLowerInvariant();
			if (FilterDescriptors.Any())
			{
				json["filters"] = FilterDescriptors.OfType<JsonObject>().ToJson();
			}
		}
	}

	public class FilterDescriptorCollection : Collection<IFilterDescriptor>
	{
	}

	public class FilterDescriptor : FilterDescriptorBase
	{
		public object ConvertedValue => Value;

		public string Member
		{
			get;
			set;
		}

		public Type MemberType
		{
			get;
			set;
		}

		public FilterOperator Operator
		{
			get;
			set;
		}

		public object Value
		{
			get;
			set;
		}

		public FilterDescriptor()
			: this(string.Empty, FilterOperator.IsEqualTo, null)
		{
		}

		public FilterDescriptor(string member, FilterOperator filterOperator, object filterValue)
		{
			Member = member;
			Operator = filterOperator;
			Value = filterValue;
		}

		protected override Expression CreateFilterExpression(ParameterExpression parameterExpression)
		{
			FilterDescriptorExpressionBuilder filterDescriptorExpressionBuilder = new FilterDescriptorExpressionBuilder(parameterExpression, this);
			filterDescriptorExpressionBuilder.Options.CopyFrom(base.ExpressionBuilderOptions);
			return filterDescriptorExpressionBuilder.CreateBodyExpression();
		}

		public virtual bool Equals(FilterDescriptor other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (object.Equals(other.Operator, Operator) && object.Equals(other.Member, Member))
			{
				return object.Equals(other.Value, Value);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			FilterDescriptor filterDescriptor = obj as FilterDescriptor;
			if (filterDescriptor == null)
			{
				return false;
			}
			return Equals(filterDescriptor);
		}

		public override int GetHashCode()
		{
			return (((Operator.GetHashCode() * 397) ^ ((Member != null) ? Member.GetHashCode() : 0)) * 397) ^ ((Value != null) ? Value.GetHashCode() : 0);
		}

		protected override void Serialize(IDictionary<string, object> json)
		{
			base.Serialize(json);
			json["field"] = Member;
			json["operator"] = Operator.ToToken();
			if (Value != null && Value.GetType().IsEnumType())
			{
				Type underlyingType = Enum.GetUnderlyingType(Value.GetType().GetNonNullableType());
				json["value"] = Convert.ChangeType(Value, underlyingType);
			}
			else
			{
				json["value"] = Value;
			}
		}
	}

	public enum FilterOperator
	{
		IsLessThan,
		IsLessThanOrEqualTo,
		IsEqualTo,
		IsNotEqualTo,
		IsGreaterThanOrEqualTo,
		IsGreaterThan,
		StartsWith,
		EndsWith,
		Contains,
		IsContainedIn,
		DoesNotContain,
		IsNull,
		IsNotNull,
		IsEmpty,
		IsNotEmpty,
		IsNullOrEmpty,
		IsNotNullOrEmpty,
		MultiSelect
	}

	public class FilterDescriptorExpressionBuilder : FilterExpressionBuilder
	{
		private readonly FilterDescriptor descriptor;

		public FilterDescriptor FilterDescriptor => descriptor;

		public FilterDescriptorExpressionBuilder(ParameterExpression parameterExpression, FilterDescriptor descriptor)
			: base(parameterExpression)
		{
			this.descriptor = descriptor;
		}

		public override Expression CreateBodyExpression()
		{
			Expression memberExpression = CreateMemberExpression();
			Type type = memberExpression.Type;
			Expression valueExpression = CreateValueExpression(type, descriptor, CultureInfo.InvariantCulture);
			bool flag = true;
			if (TypesAreDifferent(descriptor, memberExpression, valueExpression))
			{
				if (!TryConvertExpressionTypes(ref memberExpression, ref valueExpression))
				{
					flag = false;
				}
			}
			else if ((memberExpression.Type.IsEnumType() || valueExpression.Type.IsEnumType()) && descriptor.Operator != FilterOperator.MultiSelect)
			{
				if (!TryPromoteNullableEnums(ref memberExpression, ref valueExpression))
				{
					flag = false;
				}
			}
			else if (type.IsNullableType() && memberExpression.Type != valueExpression.Type && !TryConvertNullableValue(memberExpression, ref valueExpression))
			{
				flag = false;
			}
			if (!flag)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Operator '{0}' is incompatible with operand types '{1}' and '{2}'", descriptor.Operator, memberExpression.Type.GetTypeName(), valueExpression.Type.GetTypeName()));
			}
			return descriptor.Operator.CreateExpression(memberExpression, valueExpression, base.Options.LiftMemberAccessToNull);
		}

		public FilterDescription CreateFilterDescription()
		{
			return new PredicateFilterDescription(CreateFilterExpression().Compile());
		}

		protected virtual Expression CreateMemberExpression()
		{
			Type memberType = FilterDescriptor.MemberType;
			MemberAccessExpressionBuilderBase memberAccessExpressionBuilderBase = ExpressionBuilderFactory.MemberAccess(base.ParameterExpression.Type, memberType, FilterDescriptor.Member);
			memberAccessExpressionBuilderBase.Options.CopyFrom(base.Options);
			memberAccessExpressionBuilderBase.ParameterExpression = base.ParameterExpression;
			Expression expression = memberAccessExpressionBuilderBase.CreateMemberAccessExpression();
			if (memberType != (Type)null && expression.Type.GetNonNullableType() != memberType.GetNonNullableType())
			{
				expression = Expression.Convert(expression, memberType);
			}
			return expression;
		}

		private static Expression CreateConstantExpression(FilterDescriptor filterDescriptor,Type type)
		{
			if (filterDescriptor.Value == null)
			{
				return ExpressionConstants.NullLiteral;
			}
			if (filterDescriptor.Operator == FilterOperator.MultiSelect)
			{
                if (type == typeof(string))
                {
					return Expression.Constant(filterDescriptor.Value?.ToString().Split(",").ToList());
                }
                else
                {
                    if (type == typeof(POStageType))
                    {
						List<POStageType> values = (filterDescriptor.Value as IEnumerable<object>).Cast<POStageType>().ToList();
						return Expression.Constant(values);
					}
					if (type == typeof(POFulfillmentStage))
					{
						List<POFulfillmentStage> values = (filterDescriptor.Value as IEnumerable<object>).Cast<POFulfillmentStage>().ToList();
						return Expression.Constant(values);
					}
					if (type == typeof(RoutingOrderStageType))
                    {
						List<RoutingOrderStageType> values = (filterDescriptor.Value as IEnumerable<object>).Cast<RoutingOrderStageType>().ToList();
						return Expression.Constant(values);
					}
				}
			}
			return Expression.Constant(filterDescriptor.Value);
		}

		private static Expression CreateValueExpression(Type targetType, FilterDescriptor filterDescriptor, CultureInfo culture)
		{
			object value = filterDescriptor.Value;
			Type type = targetType;
			if (targetType != typeof(string) && (!targetType.IsValueType() || targetType.IsNullableType()) && string.Compare(value as string, "null", StringComparison.OrdinalIgnoreCase) == 0)
			{
				value = null;
			}
			if (value != null)
			{
				Type nonNullableType = targetType.GetNonNullableType();
				if (value.GetType() != nonNullableType)
				{
					type = nonNullableType;
					if (nonNullableType.IsEnumType())
					{
                        if (filterDescriptor.Operator == FilterOperator.MultiSelect)
                        {
							var enums  = new List<object>();
                            var values = value.ToString().Split(",");

                            foreach (var item in values)
                            {
                                var parsedEnum = Enum.Parse(nonNullableType, item.ToString(), true);
								enums.Add(parsedEnum);
                            }
                            value = enums;
                        }
                        else
                        {
							value = Enum.Parse(nonNullableType, value.ToString(), true);
						}
					}
					else if (nonNullableType == typeof(Guid))
					{
						value = new Guid(value.ToString());
					}
					else if (value is IConvertible)
					{
						value = Convert.ChangeType(value, nonNullableType, culture);
					}
				}
			}
			filterDescriptor.Value = value;
			return CreateConstantExpression(filterDescriptor, type);
		}

		private static Expression PromoteExpression(Expression expr, Type type, bool exact)
		{
			if (expr.Type == type)
			{
				return expr;
			}
			ConstantExpression constantExpression = expr as ConstantExpression;
			if (constantExpression != null && constantExpression == ExpressionConstants.NullLiteral && (!type.IsValueType() || type.IsNullableType()))
			{
				return Expression.Constant(null, type);
			}
			if (expr.Type.IsCompatibleWith(type))
			{
				if (type.IsValueType() | exact)
				{
					return Expression.Convert(expr, type);
				}
				return expr;
			}
			return null;
		}

		private static bool TryConvertExpressionTypes(ref Expression memberExpression, ref Expression valueExpression)
		{
			if (memberExpression.Type != valueExpression.Type)
			{
				if (!memberExpression.Type.GetTypeInfo().IsAssignableFrom(valueExpression.Type.GetTypeInfo()))
				{
					if (!valueExpression.Type.GetTypeInfo().IsAssignableFrom(memberExpression.Type.GetTypeInfo()))
					{
						return false;
					}
					memberExpression = Expression.Convert(memberExpression, valueExpression.Type);
				}
				else
				{
					valueExpression = Expression.Convert(valueExpression, memberExpression.Type);
				}
			}
			return true;
		}

		private static bool TryConvertNullableValue(Expression memberExpression, ref Expression valueExpression)
		{
			ConstantExpression constantExpression = valueExpression as ConstantExpression;
			if (constantExpression != null)
			{
				try
				{
					valueExpression = Expression.Constant(constantExpression.Value, memberExpression.Type);
				}
				catch (ArgumentException)
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryPromoteNullableEnums(ref Expression memberExpression, ref Expression valueExpression)
		{
			if (memberExpression.Type != valueExpression.Type)
			{
				Expression expression = PromoteExpression(valueExpression, memberExpression.Type, true);
				if (expression == null)
				{
					expression = PromoteExpression(memberExpression, valueExpression.Type, true);
					if (expression == null)
					{
						return false;
					}
					memberExpression = expression;
				}
				else
				{
					valueExpression = expression;
				}
			}
			return true;
		}

		private static bool TypesAreDifferent(FilterDescriptor descriptor, Expression memberExpression, Expression valueExpression)
		{
			if ((descriptor.Operator == FilterOperator.IsEqualTo || descriptor.Operator == FilterOperator.IsNotEqualTo) && !memberExpression.Type.IsValueType())
			{
				return !valueExpression.Type.IsValueType();
			}
			return false;
		}
	}

	public abstract class FilterDescription : FilterDescriptorBase
	{
		public virtual bool IsActive => true;

		public abstract bool SatisfiesFilter(object dataItem);

		protected override Expression CreateFilterExpression(ParameterExpression parameterExpression)
		{
			return new FilterDescriptionExpressionBuilder(parameterExpression, this).CreateBodyExpression();
		}
	}

	internal static class FilterOperatorExtensions
	{
		internal static readonly MethodInfo StringToLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[0]);

		internal static readonly MethodInfo StringStartsWithMethodInfo = typeof(string).GetMethod("StartsWith", new Type[1]
		{
		typeof(string)
		});

		internal static readonly MethodInfo StringEndsWithMethodInfo = typeof(string).GetMethod("EndsWith", new Type[1]
		{
		typeof(string)
		});

		internal static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new Type[2]
		{
		typeof(string),
		typeof(string)
		});

		internal static readonly MethodInfo StringContainsMethodInfo = typeof(string).GetMethod("Contains", new Type[1]
		{
		typeof(string)
		});

		internal static readonly MethodInfo ListStringContainsMethodInfo = typeof(List<string>).GetMethod("Contains", new Type[1]
		{
		typeof(string)
		});

		internal static readonly MethodInfo ListPOStageEnumContainsMethodInfo = typeof(List<POStageType>).GetMethod("Contains", new Type[1]
		{
		typeof(POStageType)
		});


		internal static readonly MethodInfo ListBookingStageEnumContainsMethodInfo = typeof(List<POFulfillmentStage>).GetMethod("Contains", new Type[1]
		{
		typeof(POFulfillmentStage)
		});

		internal static readonly MethodInfo ListRoutingOrderStageEnumContainsMethodInfo = typeof(List<RoutingOrderStageType>).GetMethod("Contains", new Type[1]
		{
		typeof(RoutingOrderStageType)
		});

		internal static Expression CreateExpression(this FilterOperator filterOperator, Expression left, Expression right, bool liftMemberAccess)
		{
			if ((filterOperator != FilterOperator.IsNotNull && filterOperator != FilterOperator.IsNull) || !left.Type.IsValueType() || left.Type.IsNullableType())
			{
				switch (filterOperator)
				{
					case FilterOperator.IsLessThan:
						return GenerateLessThan(left, right, liftMemberAccess);
					case FilterOperator.IsLessThanOrEqualTo:
						return GenerateLessThanEqual(left, right, liftMemberAccess);
					case FilterOperator.IsEqualTo:
						return GenerateEqual(left, right, liftMemberAccess);
					case FilterOperator.IsNotEqualTo:
						return GenerateNotEqual(left, right, liftMemberAccess);
					case FilterOperator.IsGreaterThanOrEqualTo:
						return GenerateGreaterThanEqual(left, right, liftMemberAccess);
					case FilterOperator.IsGreaterThan:
						return GenerateGreaterThan(left, right, liftMemberAccess);
					case FilterOperator.StartsWith:
						return GenerateStartsWith(left, right, liftMemberAccess);
					case FilterOperator.EndsWith:
						return GenerateEndsWith(left, right, liftMemberAccess);
					case FilterOperator.Contains:
						return GenerateContains(left, right, liftMemberAccess);
					case FilterOperator.DoesNotContain:
						return GenerateNotContains(left, right, liftMemberAccess);
					case FilterOperator.IsContainedIn:
						return GenerateIsContainedIn(left, right, liftMemberAccess);
					case FilterOperator.IsEmpty:
						return GenerateIsEmpty(left);
					case FilterOperator.IsNotEmpty:
						return GenerateIsNotEmpty(left);
					case FilterOperator.IsNull:
						return GenerateIsNull(left);
					case FilterOperator.IsNotNull:
						return GenerateIsNotNull(left);
					case FilterOperator.IsNullOrEmpty:
						return GenerateIsNullOrEmpty(left);
					case FilterOperator.IsNotNullOrEmpty:
						return GenerateIsNotNullOrEmpty(left);
					case FilterOperator.MultiSelect:
                        if (left.Type.IsEnumType())
                        {
                            if (left.Type == typeof(POStageType))
                            {
								return GenerateIn(right, left, ListPOStageEnumContainsMethodInfo, liftMemberAccess);
							}

							if (left.Type == typeof(POFulfillmentStage))
							{
								return GenerateIn(right, left, ListBookingStageEnumContainsMethodInfo, liftMemberAccess);
							}

							if (left.Type == typeof(RoutingOrderStageType))
                            {
								return GenerateIn(right, left, ListRoutingOrderStageEnumContainsMethodInfo, liftMemberAccess);
							}
						}
						return GenerateIn(right, left, liftMemberAccess);

					default:
						throw new InvalidOperationException();
				}
			}
			if (filterOperator != FilterOperator.IsNotNull)
			{
				return ExpressionConstants.FalseLiteral;
			}
			return ExpressionConstants.TrueLiteral;
		}

		private static Expression GenerateEqual(Expression left, Expression right, bool liftMemberAccess)
		{
			//Because it is case-insensitive comparison on database, not need to leverage 'ToLower' method
			if (left.Type == typeof(string))
			{
				left = GenerateCall(left, liftMemberAccess);
				right = GenerateCall(right, liftMemberAccess);
			}
			return Expression.Equal(left, right);
		}

		private static Expression GenerateNotEqual(Expression left, Expression right, bool liftMemberAccess)
		{
			//Because it is case-insensitive comparison on database, not need to leverage 'ToLower' method
			if (left.Type == typeof(string))
			{
				left = GenerateCall(left, liftMemberAccess);
				right = GenerateCall(right, liftMemberAccess);
			}
			return Expression.NotEqual(left, right);
		}

		private static Expression GenerateGreaterThan(Expression left, Expression right, bool liftMemberAccess)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThan(GenerateCaseInsensitiveStringMethodCall(StringCompareMethodInfo, left, right, liftMemberAccess), ExpressionFactory.ZeroExpression);
			}
			return Expression.GreaterThan(left, right);
		}

		private static Expression GenerateGreaterThanEqual(Expression left, Expression right, bool liftMemberAccess)
		{
			if (left.Type == typeof(string))
			{
				return Expression.GreaterThanOrEqual(GenerateCaseInsensitiveStringMethodCall(StringCompareMethodInfo, left, right, liftMemberAccess), ExpressionFactory.ZeroExpression);
			}
			return Expression.GreaterThanOrEqual(left, right);
		}

		private static Expression GenerateLessThan(Expression left, Expression right, bool liftMemberAccess)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThan(GenerateCaseInsensitiveStringMethodCall(StringCompareMethodInfo, left, right, liftMemberAccess), ExpressionFactory.ZeroExpression);
			}
			return Expression.LessThan(left, right);
		}

		private static Expression GenerateLessThanEqual(Expression left, Expression right, bool liftMemberAccess)
		{
			if (left.Type == typeof(string))
			{
				return Expression.LessThanOrEqual(GenerateCaseInsensitiveStringMethodCall(StringCompareMethodInfo, left, right, liftMemberAccess), ExpressionFactory.ZeroExpression);
			}
			return Expression.LessThanOrEqual(left, right);
		}

		private static Expression GenerateNotContains(Expression left, Expression right, bool liftMemberAccess)
		{
			return Expression.Not(GenerateCaseInsensitiveStringMethodCall(StringContainsMethodInfo, left, right, liftMemberAccess));
		}

		private static Expression GenerateContains(Expression left, Expression right, bool liftMemberAccess)
		{
			return GenerateCaseInsensitiveStringMethodCall(StringContainsMethodInfo, left, right, liftMemberAccess);
		}

		private static Expression GenerateIn(Expression left, Expression right, bool liftMemberAccess)
		{
            return GenerateCaseInsensitiveStringMethodCall(ListStringContainsMethodInfo, left, right, liftMemberAccess);
        }

		private static Expression GenerateIn(Expression left, Expression right,MethodInfo method, bool liftMemberAccess)
		{
			return GenerateCaseInsensitiveStringMethodCall(method, left, right, liftMemberAccess);
		}

		private static Expression GenerateIsContainedIn(Expression left, Expression right, bool liftMemberAccess)
		{
			return GenerateCaseInsensitiveStringMethodCall(StringContainsMethodInfo, right, left, liftMemberAccess);
		}

		private static Expression GenerateStartsWith(Expression left, Expression right, bool liftMemberAccess)
		{
			return GenerateCaseInsensitiveStringMethodCall(StringStartsWithMethodInfo, left, right, liftMemberAccess);
		}

		private static Expression GenerateEndsWith(Expression left, Expression right, bool liftMemberAccess)
		{
			return GenerateCaseInsensitiveStringMethodCall(StringEndsWithMethodInfo, left, right, liftMemberAccess);
		}

		private static Expression GenerateIsEmpty(Expression left)
		{
			return Expression.Equal(left, Expression.Constant(string.Empty));
		}

		private static Expression GenerateIsNotEmpty(Expression left)
		{
			return Expression.NotEqual(left, Expression.Constant(string.Empty));
		}

		private static Expression GenerateIsNull(Expression left)
		{
			return Expression.Equal(left, Expression.Constant(null));
		}

		private static Expression GenerateIsNotNull(Expression left)
		{
			return Expression.NotEqual(left, Expression.Constant(null));
		}

		private static Expression GenerateIsNotNullOrEmpty(Expression left)
		{
			return Expression.And(Expression.NotEqual(left, Expression.Constant(null)), Expression.NotEqual(left, Expression.Constant(string.Empty)));
		}

		private static Expression GenerateIsNullOrEmpty(Expression left)
		{
			return Expression.Or(Expression.Equal(left, Expression.Constant(null)), Expression.Equal(left, Expression.Constant(string.Empty)));
		}

		private static Expression GenerateCaseInsensitiveStringMethodCall(MethodInfo methodInfo, Expression left, Expression right, bool liftMemberAccess)
		{
			//Because it is case-insensitive comparison on database, not need to leverage 'ToLower' method
			//Expression expression = GenerateToLowerCall(left, liftMemberAccess);
			//Expression expression2 = GenerateToLowerCall(right, liftMemberAccess);
			//if (methodInfo.IsStatic)
			//{
			//	return Expression.Call(methodInfo, new Expression[2]
			//	{
			//		expression,
			//		expression2
			//	});
			//}
			//return Expression.Call(expression, methodInfo, expression2);


			Expression expression = GenerateCall(left, liftMemberAccess);
			Expression expression2 = GenerateCall(right, liftMemberAccess);

			if (methodInfo.IsStatic)
            {
                return Expression.Call(methodInfo, new Expression[2]
                {
                    expression,
                    expression2
                });
            }
            return Expression.Call(expression, methodInfo, expression2);
        }

		private static Expression GenerateToLowerCall(Expression stringExpression, bool liftMemberAccess)
		{
			if (liftMemberAccess)
			{
				stringExpression = ExpressionFactory.LiftStringExpressionToEmpty(stringExpression);
			}
			return Expression.Call(stringExpression, StringToLowerMethodInfo);
		}

		private static Expression GenerateCall(Expression stringExpression, bool liftMemberAccess)
		{
			if (liftMemberAccess)
			{
				stringExpression = ExpressionFactory.LiftStringExpressionToEmpty(stringExpression);
			}
			return stringExpression;
		}
	}

	public static class FilterTokenExtensions
	{
		private static readonly IDictionary<string, FilterOperator> tokenToOperator = new Dictionary<string, FilterOperator>
	{
		{
			"eq",
			FilterOperator.IsEqualTo
		},
		{
			"neq",
			FilterOperator.IsNotEqualTo
		},
		{
			"lt",
			FilterOperator.IsLessThan
		},
		{
			"lte",
			FilterOperator.IsLessThanOrEqualTo
		},
		{
			"gt",
			FilterOperator.IsGreaterThan
		},
		{
			"gte",
			FilterOperator.IsGreaterThanOrEqualTo
		},
		{
			"startswith",
			FilterOperator.StartsWith
		},
		{
			"contains",
			FilterOperator.Contains
		},
		{
			"notsubstringof",
			FilterOperator.DoesNotContain
		},
		{
			"endswith",
			FilterOperator.EndsWith
		},
		{
			"doesnotcontain",
			FilterOperator.DoesNotContain
		},
		{
			"isnotnull",
			FilterOperator.IsNotNull
		},
		{
			"isnull",
			FilterOperator.IsNull
		},
		{
			"isempty",
			FilterOperator.IsEmpty
		},
		{
			"isnotempty",
			FilterOperator.IsNotEmpty
		},
		{
			"isnullorempty",
			FilterOperator.IsNullOrEmpty
		},
		{
			"isnotnullorempty",
			FilterOperator.IsNotNullOrEmpty
		},
		{ 
			"multiselect",
			FilterOperator.MultiSelect
		}
	};

		private static readonly IDictionary<FilterOperator, string> operatorToToken = new Dictionary<FilterOperator, string>
	{
		{
			FilterOperator.IsEqualTo,
			"eq"
		},
		{
			FilterOperator.IsNotEqualTo,
			"neq"
		},
		{
			FilterOperator.IsLessThan,
			"lt"
		},
		{
			FilterOperator.IsLessThanOrEqualTo,
			"lte"
		},
		{
			FilterOperator.IsGreaterThan,
			"gt"
		},
		{
			FilterOperator.IsGreaterThanOrEqualTo,
			"gte"
		},
		{
			FilterOperator.StartsWith,
			"startswith"
		},
		{
			FilterOperator.Contains,
			"contains"
		},
		{
			FilterOperator.DoesNotContain,
			"notsubstringof"
		},
		{
			FilterOperator.EndsWith,
			"endswith"
		},
		{
			FilterOperator.IsNotNull,
			"isnotnull"
		},
		{
			FilterOperator.IsNull,
			"isnull"
		},
		{
			FilterOperator.IsEmpty,
			"isempty"
		},
		{
			FilterOperator.IsNotEmpty,
			"isnotempty"
		},
		{
			FilterOperator.IsNullOrEmpty,
			"isnullorempty"
		},
		{
			FilterOperator.IsNotNullOrEmpty,
			"isnotnullorempty"
		},
		{
			FilterOperator.MultiSelect,
			"multiselect"
		},
	};

		public static FilterOperator ToFilterOperator(this FilterToken token)
		{
			return tokenToOperator[token.Value];
		}

		public static string ToToken(this FilterOperator filterOperator)
		{
			return operatorToToken[filterOperator];
		}
	}

	public class FilterToken
	{
		public FilterTokenType TokenType
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}
	}

	public enum FilterTokenType
	{
		Property,
		ComparisonOperator,
		Or,
		And,
		Not,
		Function,
		Number,
		String,
		Boolean,
		DateTime,
		LeftParenthesis,
		RightParenthesis,
		Comma,
		Null
	}

	internal class PredicateFilterDescription : FilterDescription
	{
		private readonly Delegate predicate;

		public PredicateFilterDescription(Delegate predicate)
		{
			this.predicate = predicate;
		}

		public override bool SatisfiesFilter(object dataItem)
		{
			return (bool)predicate.DynamicInvoke(dataItem);
		}
	}

	public class FilterDescriptionExpressionBuilder : FilterExpressionBuilder
	{
		private readonly FilterDescription filterDescription;

		public FilterDescription FilterDescription => filterDescription;

		private Expression FilterDescriptionExpression => Expression.Constant(filterDescription);

		private MethodInfo SatisfiesFilterMethodInfo => filterDescription.GetType().GetMethod("SatisfiesFilter", new Type[1]
		{
		typeof(object)
		});

		public FilterDescriptionExpressionBuilder(ParameterExpression parameterExpression, FilterDescription filterDescription)
			: base(parameterExpression)
		{
			this.filterDescription = filterDescription;
		}

		public override Expression CreateBodyExpression()
		{
			if (filterDescription.IsActive)
			{
				return CreateActiveFilterExpression();
			}
			return ExpressionConstants.TrueLiteral;
		}

		protected virtual Expression CreateActiveFilterExpression()
		{
			return CreateSatisfiesFilterExpression();
		}

		private MethodCallExpression CreateSatisfiesFilterExpression()
		{
			Expression expression = base.ParameterExpression;
			if (expression.Type.IsValueType())
			{
				expression = Expression.Convert(expression, typeof(object));
			}
			return Expression.Call(FilterDescriptionExpression, SatisfiesFilterMethodInfo, expression);
		}
	}



}
