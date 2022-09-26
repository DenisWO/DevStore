using DS.Identity.API.Entities;
using DS.Identity.API.Services;
using Microsoft.AspNetCore.Mvc;


namespace DS.Identity.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<User> Get([FromRoute] Guid id)
        {
            return await _userService.GetById(id);
        }

        [HttpPost]
        public async Task Post([FromBody] User user)
        {
            await _userService.Add(user);
        }

        // PUT api/<UserController>/5
        [HttpPut()]
        public async Task Put([FromBody] User user)
        {
            await _userService.Update(user);
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            var user = await _userService.GetById(id);

            await _userService.Remove(user);
        }
    }
}
