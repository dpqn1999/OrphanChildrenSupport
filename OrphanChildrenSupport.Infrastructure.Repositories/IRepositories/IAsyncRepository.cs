using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OrphanChildrenSupport.DataContracts;
using OrphanChildrenSupport.Services.Models;
using OrphanChildrenSupport.Infrastructure.Repositories.Specifications;

namespace OrphanChildrenSupport.Infrastructure.Repositories
{
    public interface IAsyncRepository<T> where T : EngineEntity
    {
        Task<T> FindSingle(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool disableTracking = true);

        Task<T> FindFirst(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool disableTracking = true);

        Task<T> FindLast(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool disableTracking = true);

        IQueryable<T> FindAll(Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, bool disableTracking = true, PagingSpecification pagingSpecification = null);

        Task<QueryResult<T>> FindAll(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool disableTracking = true, PagingSpecification pagingSpecification = null);

        Task Add(T entity);

        Task AddRange(List<T> entities);

        void Update(T entity);

        void UpdateRange(List<T> entities);

        void Remove(T entity);

        void RemoveRange(List<T> entities);
    }
}
