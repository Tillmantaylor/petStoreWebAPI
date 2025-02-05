﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petStoreMonitoringApp.Models.ViewModels;
using System.Diagnostics;

namespace petStoreMonitoringApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MaintainMetrics()
        {
            return View();
        }

        public IActionResult MaintainProjects()
        {
            return View();
        }

        public async Task<IActionResult> MaintainUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var maintainUserVM = new List<MaintainUsersVM>();
            foreach (var user in users)
            {
                var tempViewModel = new MaintainUsersVM();
                tempViewModel.UserId = user.Id;
                tempViewModel.UserName = user.UserName;
                tempViewModel.Email = user.Email;
                tempViewModel.Roles = await GetUserRoles(user);
                maintainUserVM.Add(tempViewModel);
            }
            return View(maintainUserVM);
        }

        #region GetUserRoles
        private async Task<List<string>> GetUserRoles(IdentityUser user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
        #endregion

        #region ManageUserRolesGet
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesVM>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesVM
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }
        #endregion

        #region ManageUserRolesPost
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<ManageUserRolesVM> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("MaintainUsers");
        }
        #endregion

        #region DeleteUserPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("MaintainUsers");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("MaintainUsers");
            }
        }
        #endregion

        #region ChangeEmailGet
        public async Task<ActionResult> ChangeEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }
            var model = new MaintainEmailVM
            {
                UserId = user.Id,
                Email = user.Email
            };
            return View(model);
        }
        #endregion

        #region ChangeEmailPost
        [HttpPost]
        public async Task<ActionResult> ChangeEmail(MaintainEmailVM model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("MaintainUsers");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.UserName = model.Email;
                user.NormalizedUserName = model.Email.ToUpper();

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("MaintainUsers");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            return RedirectToAction("ManageUsers");
        }
        #endregion
    }
}
