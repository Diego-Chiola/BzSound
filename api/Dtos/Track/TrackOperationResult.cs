namespace api.Dtos.Track;

public enum TrackOperationStatus
{
    Success,
    InvalidRequest,
    InvalidFile,
    UserNotFound,
    TrackNotFound
}

public record TrackOperationResult(
    TrackOperationStatus Status,
    string? ErrorMessage = null)
{
    public bool IsSuccess => Status == TrackOperationStatus.Success;

    public static TrackOperationResult Success()
        => new(TrackOperationStatus.Success);

    public static TrackOperationResult Failure(TrackOperationStatus status, string? errorMessage)
        => new(status, errorMessage);
}

public record TrackOperationResult<T>(
    TrackOperationStatus Status,
    T? Data = default,
    string? ErrorMessage = null)
{
    public bool IsSuccess => Status == TrackOperationStatus.Success;

    public static TrackOperationResult<T> Success(T data)
        => new(TrackOperationStatus.Success, data);

    public static TrackOperationResult<T> Failure(TrackOperationStatus status, string? errorMessage)
        => new(status, default, errorMessage);
}