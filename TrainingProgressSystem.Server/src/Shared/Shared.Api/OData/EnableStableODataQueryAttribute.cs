using Microsoft.AspNetCore.OData.Query;

namespace Shared.Api.OData;

public sealed class EnableStableODataQueryAttribute : EnableQueryAttribute
{
    public EnableStableODataQueryAttribute()
    {
        PageSize = ODataDefaults.PageSize;
        MaxTop = ODataDefaults.MaxTop;
        MaxNodeCount = ODataDefaults.MaxNodeCount;
        EnsureStableOrdering = true;
        AllowedQueryOptions = AllowedQueryOptions.Select
                              | AllowedQueryOptions.Filter
                              | AllowedQueryOptions.OrderBy
                              | AllowedQueryOptions.Skip
                              | AllowedQueryOptions.Top
                              | AllowedQueryOptions.Count;
    }
}