namespace api.Helpers;

public class TrackQueryObject
{
    public string? TitleContains { get; set; } = null;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = null;
    public bool IsDescending { get; set; } = false;
}