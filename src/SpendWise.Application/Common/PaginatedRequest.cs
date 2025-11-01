namespace SpendWise.Application.Common;

public class PaginatedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Validações
    public int GetValidPage() => Page < 1 ? 1 : Page;
    public int GetValidPageSize() => PageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => PageSize
    };
    
    public int GetSkip() => (GetValidPage() - 1) * GetValidPageSize();
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
