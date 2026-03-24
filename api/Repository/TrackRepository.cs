using api.Data;
using api.Dtos.Track;
using api.Models;
using api.Interfaces;
using Microsoft.EntityFrameworkCore;
using api.Helpers;

namespace api.Repository;

public class TrackRepository : ITrackRepository
{
    private readonly ApplicationDBContext _context;

    public TrackRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<Track> CreateTrackAsync(Track track)
    {
        await _context.Tracks.AddAsync(track);
        await _context.SaveChangesAsync();
        return track;
    }

    public async Task<Track?> DeleteTrackAsync(Guid userId, int trackId)
    {
        var track = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.UserId == userId);
        if (track is null) return null;

        _context.Tracks.Remove(track);
        await _context.SaveChangesAsync();
        return track;
    }

    public async Task<Track?> GetTrackAsync(int id)
    {
        return await _context.Tracks.FindAsync(id);
    }

    public async Task<IEnumerable<Track>> GetTracksByUserAsync(Guid userId, TrackQueryObject queryObj)
    {
        var query = _context.Tracks.AsQueryable();

        query = ApplyFilters(query, queryObj, userId);
        query = ApplySorting(query, queryObj);
        query = ApplyPaging(query, queryObj);

        return await query.ToListAsync();
    }

    public async Task<Track?> UpdateTrackAsync(Guid userId, int trackId, UpdateTrackRequest newTrack)
    {
        var existingTrack = await _context.Tracks.FirstOrDefaultAsync(t => t.Id == trackId && t.UserId == userId);
        if (existingTrack == null)
            return null;

        existingTrack.Title = newTrack.Title ?? existingTrack.Title;
        existingTrack.FilePath = newTrack.FilePath ?? existingTrack.FilePath;
        existingTrack.FileSize = newTrack.FileSize ?? existingTrack.FileSize;
        existingTrack.Format = newTrack.Format ?? existingTrack.Format;
        existingTrack.Duration = newTrack.Duration ?? existingTrack.Duration;
        existingTrack.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingTrack;
    }

    private static IQueryable<Track> ApplyFilters(IQueryable<Track> query, TrackQueryObject q, Guid userId)
    {
        if (q == null) return query;

        query = query.Where(t => t.UserId == userId);
        if (!string.IsNullOrWhiteSpace(q.TitleContains))
            query = query.Where(t => t.Title.Contains(q.TitleContains));

        return query;
    }

    private static IQueryable<Track> ApplySorting(IQueryable<Track> query, TrackQueryObject q)
    {
        if (q == null) return query.OrderBy(t => t.Id);

        bool desc = q.IsDescending;
        string sortBy = q.SortBy ?? string.Empty;

        if (string.Equals(sortBy, "Title", StringComparison.OrdinalIgnoreCase))
            return desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);

        if (string.Equals(sortBy, "LastModified", StringComparison.OrdinalIgnoreCase))
            return desc ? query.OrderByDescending(t => t.LastModified) : query.OrderBy(t => t.LastModified);

        return desc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id);
    }

    private static IQueryable<Track> ApplyPaging(IQueryable<Track> query, TrackQueryObject q)
    {
        if (q == null) return query;

        if (q.PageNumber > 0 && q.PageSize > 0)
            query = query.Skip((q.PageNumber - 1) * q.PageSize).Take(q.PageSize);

        return query;
    }
}