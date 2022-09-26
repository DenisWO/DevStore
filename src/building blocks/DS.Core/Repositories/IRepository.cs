using DS.Core.DomainObjects;

namespace DS.Core.Repositories
{
    public interface IRepository<T> : IDisposable where T : Entity
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(Guid id);
        Task Add(T entity);
        Task Update(T entity);
        Task Remove(T entity);
    }
}
