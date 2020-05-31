using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace VMenu.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public abstract class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<VmUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IHtmlLocalizer<SharedResource> _htmlPageLoc;

        public ResendEmailConfirmationModel(UserManager<VmUser> userManager, IEmailSender emailSender, IHtmlLocalizer<SharedResource> htmlPageLoc)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _htmlPageLoc = htmlPageLoc;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, _htmlPageLoc["Verification email sent. Please check your email."].Value);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                 _htmlPageLoc["Confirm your email"].Value,
                 _htmlPageLoc["Please confirm your account by", HtmlEncoder.Default.Encode(callbackUrl) ].Value
                );

            ModelState.AddModelError(string.Empty, _htmlPageLoc["Verification email sent.Please check your email."].Value);
            return Page();
        }
    }
}
