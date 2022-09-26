using DS.Core.DomainObjects;

namespace DS.Core.Services
{
    public interface IService<T> where T : Entity
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(Guid id);
        Task Add(T t);
        Task Update(T t);
        Task Remove(T t);
    }
}
