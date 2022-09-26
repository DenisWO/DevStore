using DS.Identity.API.Data.Repositories.Interfaces;
using DS.Identity.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DS.Identity.API.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersContext _context;
        public UserRepository(UsersContext context)
        {
            _context = context;
        }

        public async Task Add(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task Remove(User user)
        {
            _context.Users.Remove(user);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<User> GetById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
