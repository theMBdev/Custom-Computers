using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CustomComputersGU.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;
using CustomComputersGU.Models.Poco;
using System.Web.Security;

namespace CustomComputersGU.Controllers
{
    [Authorize]
    [NoDirectAccess]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private void MigrateShoppingCart(string UserName)
        {
            // Associate shopping cart items with logged-in user
            var cart = ShoppingCart.GetCart(this.HttpContext);

            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Create the Admin account using setting in Web.Config (if needed)
            CreateAdminIfNeeded();
            CreateStoresManager();
            CreateStoreManager();
            CreateSalesAssistant();
            CreateAssistantManager();
            //CreateInvoiceClerk();
            CreateCustomer();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.Email, model.Password))
                {
                    MigrateShoppingCart(model.Email);

                    FormsAuthentication.SetAuthCookie(model.Email, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
    //        return View(model);
    //    }
    //}

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            using (var context = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var result = await UserManager.CreateAsync(user, model.Password);
                                        
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);
                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                   
                    if (result.Succeeded)
                    {
                        //Assigns role to customer
                        userManager.AddToRole(user.Id, "Customer");

                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        //ADDED
                        MigrateShoppingCart(model.Email);

                        FormsAuthentication.SetAuthCookie(model.Email, false /*createPersistentCookie*/);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        AddErrors(result);
                    }                             
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }               
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        // Utility

        // Add RoleManager
        #region public ApplicationRoleManager RoleManager
        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        // Add CreateAdminIfNeeded
        #region private void CreateAdminIfNeeded()
        private void CreateAdminIfNeeded()
        {
            // Get Admin Account
            string AdminUserName = ConfigurationManager.AppSettings["AdminUserName"];
            string AdminPassword = ConfigurationManager.AppSettings["AdminPassword"];

            // See if Admin exists
            var objAdminUser = UserManager.FindByEmail(AdminUserName);

            if (objAdminUser == null)
            {
                //See if the Admin role exists
                if (!RoleManager.RoleExists("Administrator"))
                {
                    // Create the Admin Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("Administrator");
                    RoleManager.Create(objAdminRole);
                }

                // Create Admin user
                var objNewAdminUser = new ApplicationUser { UserName = AdminUserName, Email = AdminUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, AdminPassword);
                // Put user in Admin role
                UserManager.AddToRole(objNewAdminUser.Id, "Administrator");
            }
        }
        #endregion

        // Add AssistantManager
        #region private void CreateAssistantManager()
        private void CreateAssistantManager()
        {
            // Get AssistantManager Account
            string AssistantManagerUserName = "AssistantManager@customcomputers.com";
            string AssistantManagerPassword = "Password#1";

            // See if AssistantManager exists
            var objAdminUser = UserManager.FindByEmail(AssistantManagerUserName);

            if (objAdminUser == null)
            {
                //See if the AssistantManager role exists
                if (!RoleManager.RoleExists("AssistantManager"))
                {
                    // Create the AssistantManager Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("AssistantManager");
                    RoleManager.Create(objAdminRole);
                }

                // Create AssistantManager user
                var objNewAdminUser = new ApplicationUser { UserName = AssistantManagerUserName, Email = AssistantManagerUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, AssistantManagerPassword);
                // Put user in AssistantManager role
                UserManager.AddToRole(objNewAdminUser.Id, "AssistantManager");
            }
        }
        #endregion

        // Add StoreManager
        #region private void CreateStoreManager()
        private void CreateStoreManager()
        {
            // Get StoreManager Account
            string StoreManagerUserName = "StoreManager@customcomputers.com";
            string StoreManagerPassword = "Password#1";

            // See if StoreManager exists
            var objAdminUser = UserManager.FindByEmail(StoreManagerUserName);

            if (objAdminUser == null)
            {
                //See if the StoreManager role exists
                if (!RoleManager.RoleExists("StoreManager"))
                {
                    // Create the StoreManager Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("StoreManager");
                    RoleManager.Create(objAdminRole);
                }
                // Create StoreManager user
                var objNewAdminUser = new ApplicationUser { UserName = StoreManagerUserName, Email = StoreManagerUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, StoreManagerPassword);
                // Put user in StoreManager role
                UserManager.AddToRole(objNewAdminUser.Id, "StoreManager");
            }
        }
        #endregion

        // Add StoresManager
        #region private void CreateStoresManager()
        private void CreateStoresManager()
        {
            // Get StoresManager Account
            string StoresManagerUserName = "StoresManager@customcomputers.com";
            string StoresManagerPassword = "Password#1";

            // See if StoreManager exists
            var objAdminUser = UserManager.FindByEmail(StoresManagerUserName);

            if (objAdminUser == null)
            {
                //See if the StoresManager role exists
                if (!RoleManager.RoleExists("StoresManager"))
                {
                    // Create the StoresManager Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("StoresManager");
                    RoleManager.Create(objAdminRole);
                }

                // Create StoresManager user
                var objNewAdminUser = new ApplicationUser { UserName = StoresManagerUserName, Email = StoresManagerUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, StoresManagerPassword);
                // Put user in StoresManager role
                UserManager.AddToRole(objNewAdminUser.Id, "StoresManager");
            }
        }
        #endregion

        // Add SalesAssistant
        #region private void CreateSalesAssistant()
        private void CreateSalesAssistant()
        {
            // Get SalesAssistant Account
            string SalesAssistantUserName = "SalesAssistant@customcomputers.com";
            string SalesAssistantPassword = "Password#1";

            // See if SalesAssistant exists
            var objAdminUser = UserManager.FindByEmail(SalesAssistantUserName);

            if (objAdminUser == null)
            {
                //See if the SalesAssistant role exists
                if (!RoleManager.RoleExists("SalesAssistant"))
                {
                    // Create the SalesAssistant Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("SalesAssistant");
                    RoleManager.Create(objAdminRole);
                }

                // Create SalesAssistant user
                var objNewAdminUser = new ApplicationUser { UserName = SalesAssistantUserName, Email = SalesAssistantUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, SalesAssistantPassword);
                // Put user in SalesAssistant role
                UserManager.AddToRole(objNewAdminUser.Id, "SalesAssistant");
            }
        }
        #endregion

        // Add InvoiceClerk
        //#region private void CreateInvoiceClerk()
        //private void CreateInvoiceClerk()
        //{
        //    // Get InvoiceClerk Account
        //    string InvoiceClerkUserName = "InvoiceClerc@customcomputers.com";
        //    string InvoiceClerkUserPassword = "Password#1";

        //    // See if InvoiceClerk exists
        //    var objAdminUser = UserManager.FindByEmail(InvoiceClerkUserName);

        //    if (objAdminUser == null)
        //    {
        //        //See if the InvoiceClerk role exists
        //        if (!RoleManager.RoleExists("InvoiceClerk"))
        //        {
        //            // Create the InvoiceClerk Role (if needed)
        //            IdentityRole objAdminRole = new IdentityRole("InvoiceClerk");
        //            RoleManager.Create(objAdminRole);
        //        }

        //        // Create InvoiceClerk user
        //        var objNewAdminUser = new ApplicationUser { UserName = InvoiceClerkUserName, Email = InvoiceClerkUserName };
        //        var AdminUserCreateResult = UserManager.Create(objNewAdminUser, InvoiceClerkUserPassword);
        //        // Put user in InvoiceClerk role
        //        UserManager.AddToRole(objNewAdminUser.Id, "InvoiceClerk");
        //    }
        //}
        //#endregion

        // Add Customer
        #region private void CreateCustomer()
        private void CreateCustomer()
        {
            // Get SalesAssistant Account
            string CreateCustomerUserName = "Customer@test.com";
            string CreateCustomerPassword = "Password#1";

            // See if Customer exists
            var objAdminUser = UserManager.FindByEmail(CreateCustomerUserName);

            if (objAdminUser == null)
            {
                // Create Customer user
                var objNewAdminUser = new ApplicationUser { UserName = CreateCustomerUserName, Email = CreateCustomerUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, CreateCustomerPassword);
                // Put user in Customer role
                UserManager.AddToRole(objNewAdminUser.Id, "Customer");
            }
        }
        #endregion


        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                //case MembershipCreateStatus.DuplicateUserName:
                //    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}