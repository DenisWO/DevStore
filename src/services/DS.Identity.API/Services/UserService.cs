using DS.Identity.API.Data.Repositories.Interfaces;
using DS.Identity.API.Entities;

namespace DS.Identity.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Add(User user)
        {
            await _userRepository.Add(user);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userRepository.GetAll();
        }

        public async Task<User> GetById(Guid id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task Remove(User user)
        {
            await _userRepository.Remove(user);
        }

        public async Task Update(User user)
        {
            await _userRepository.Update(user);
        }
    }
}
