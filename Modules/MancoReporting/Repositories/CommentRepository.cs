using IntegrationGateway.Api.Modules.MancoReporting.Data;
using IntegrationGateway.Api.Modules.MancoReporting.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Modules.MancoReporting.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly MancoDbContext _context;

    public CommentRepository(MancoDbContext context)
    {
        _context = context;
    }

    public Task<List<MancoComment>> GetByReportAsync(Guid reportId) =>
        _context.MancoComments.Include(c => c.Author)
            .Where(c => c.ReportId == reportId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public Task<List<MancoComment>> GetByProjectAsync(Guid projectId) =>
        _context.MancoComments.Include(c => c.Author)
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<MancoComment> CreateAsync(MancoComment comment)
    {
        _context.MancoComments.Add(comment);
        await _context.SaveChangesAsync();
        return await _context.MancoComments.Include(c => c.Author).FirstAsync(c => c.CommentId == comment.CommentId);
    }

    public Task<MancoComment?> GetByIdAsync(Guid commentId) =>
        _context.MancoComments.Include(c => c.Author).FirstOrDefaultAsync(c => c.CommentId == commentId);

    public Task<MancoUser?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId) =>
        _context.Users.FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId && u.IsActive);

    public async Task UpdateAsync(MancoComment comment)
    {
        _context.MancoComments.Update(comment);
        await _context.SaveChangesAsync();
    }
}
