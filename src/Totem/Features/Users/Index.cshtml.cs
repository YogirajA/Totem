using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Totem.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Totem.Features.Users
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public IndexModel(UserManager<IdentityUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public PaginatedList<UserDetail> UserDetails { get; set; }

        public class UserDetail
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public int CurrentPage { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int currentPage = 1)
        {
            CurrentPage = currentPage;
            var query = _userManager.Users.ProjectTo<UserDetail>(_mapper.ConfigurationProvider);
            var users = await PaginatedList<UserDetail>.CreateAsync(query, CurrentPage);
            UserDetails = users;

            return Page();
        }
    }
}
