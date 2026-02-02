using Desaka.DataAccess.Entities;

namespace Desaka.DataAccess.Repositories;

public sealed class AutopollBatchCommitRepository : EfRepository<AutopollBatchCommit>, IAutopollBatchCommitRepository
{
    public AutopollBatchCommitRepository(DesakaDbContext db) : base(db)
    {
    }
}

