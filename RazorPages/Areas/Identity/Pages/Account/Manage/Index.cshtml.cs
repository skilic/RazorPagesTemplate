using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace VMenu.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<VmUser> _userManager;
        private readonly SignInManager<VmUser> _signInManager;
        private readonly ILogger<IndexModel> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedLoc;
        public IndexModel(
            UserManager<VmUser> userManager,
            SignInManager<VmUser> signInManager,
            ILogger<IndexModel> logger,
            IStringLocalizer<SharedResource> sharedLoc)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _sharedLoc = sharedLoc;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
        }

        private async Task LoadAsync(VmUser user)
        {
          //  var userName = await _userManager.GetUserNameAsync(user);
            var usr = await _userManager.GetUserAsync(User);

            Username = usr.UserName;

            Input = new InputModel
            {
                PhoneNumber = usr.PhoneNumber,
                FirstName = usr.FirstName,
                LastName = usr.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            user.PhoneNumber = Input.PhoneNumber;
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            var setProfileResult = await _userManager.UpdateAsync(user);
            if (!setProfileResult.Succeeded)
            {
                _logger.LogError("Unexpected error when trying to set profile update");
                StatusMessage = _sharedLoc["Unexpected error when trying to set profile update."];
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _sharedLoc["Your profile has been updated"];
            return RedirectToPage();
        }
    }
}
