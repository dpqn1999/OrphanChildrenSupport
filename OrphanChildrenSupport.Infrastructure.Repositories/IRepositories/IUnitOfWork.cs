using System;
using System.Threading.Tasks;

namespace OrphanChildrenSupport.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task SaveChanges();
    }
}
