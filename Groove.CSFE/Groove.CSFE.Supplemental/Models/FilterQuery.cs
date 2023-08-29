namespace Groove.CSFE.Supplemental.Models;

public class QueryFilter
{
    public virtual IEnumerable<FilterModel>? filters { get; set; }
    public string? logic { get; set; }
}

public class FilterModel
{
    public string? field { get; set; }
    public string? @operator { get; set; }
    public object? value { get; set; }
}

public class FilterRoot
{
    public QueryFilter? filter { get; set; }
    public virtual IEnumerable<object>? group { get; set; }
    public int skip { get; set; }
    public virtual IEnumerable<SortModel>? sort { get; set; }
    public int take { get; set; }
    public bool isInternal { get; set; } = false;
    public virtual IEnumerable<int>? principles { get; set; }
}

public class SortModel
{
    public string? dir { get; set; }
    public string? field { get; set; }
}
