using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CTS_Models;
using CTS_Models.DBContext;
using System.DirectoryServices.AccountManagement;
using CTS_Core;
using RoleAdmin.Handlers;
using CTS_RoleAdmin.Models;
using System.Configuration;
using System;

namespace CTS_RoleAdmin.Controllers
{
    [CtsAuthorize(Roles = Roles.RoleAdminRoleName)]
    public class HomeController : Controller
    {
        private CtsDbContext _cdb = new CtsDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RoleAdminRoleList()
        {
            var model = new RolesIndexViewModel
            {
                CtsRoles = _cdb.CtsRole.Where(r => r.RoleName.ToLower().Contains("roleadmin")).ToList(),
                CtsUsers = _cdb.CtsUser.Include(m => m.CtsRoles).ToList()
            };

            return View("RoleList", model);
        }

        public ActionResult MiRoleList()
        {
            RolesIndexViewModel model;

            model = new RolesIndexViewModel
            {
                CtsRoles = _cdb.CtsRole.Where(r => r.RoleName.ToLower().StartsWith("ctsmi")).ToList(),
                CtsUsers = _cdb.CtsUser.Include(m => m.CtsRoles).ToList()
            };

            return View("RoleList", model);
        }

        public ActionResult LocationsRoleList()
        {
            RolesIndexViewModel model = new RolesIndexViewModel();
            try
            {
                model = new RolesIndexViewModel()
                {
                    CtsRoles = _cdb.CtsRole
                    .Where(r => r.RoleName.ToLower().StartsWith("ш")
                             || r.RoleName.ToLower().StartsWith("ст")
                             || r.RoleName.ToLower().StartsWith("цоф"))
                  .ToList(),
                    CtsUsers = _cdb.CtsUser.Include(m => m.CtsRoles).ToList()
                };
            }catch(Exception ex) { }
              return View("RoleList", model);
        }

        public ActionResult AnalyticRoleList()
        {
            var model = new RolesIndexViewModel
            {
                CtsRoles = _cdb.CtsRole
                .Where(r => r.RoleName.ToLower().StartsWith("anl")
                || r.RoleName.ToLower().Contains("anal"))
              .ToList(),
                CtsUsers = _cdb.CtsUser.Include(m => m.CtsRoles).ToList()
            };

            return View("RoleList", model);
        }

        public ActionResult RepoertsRoleList()
        {
            var model = new RolesIndexViewModel
            {
                CtsRoles = _cdb.CtsRole
                .Where(r => r.RoleName.ToLower().StartsWith("rep"))
              .ToList(),
                CtsUsers = _cdb.CtsUser.Include(m => m.CtsRoles).ToList()
            };

            return View("RoleList", model);
        }

        public ActionResult AddEditUser(string userLogin, string userDomain, string[] roles)
        {
            AddEditUserViewModel model;
            var ctsRoles = roles?.Select(r => _cdb.CtsRole.Find(r)) ?? _cdb.CtsRole;
            var user = _cdb.CtsUser.Find(userLogin, userDomain);
            if (user == null)
            {
                model = new AddEditUserViewModel(userLogin, userDomain, ctsRoles.ToList());
            }
            else
            {
                _cdb.Entry(user).Collection(x => x.CtsRoles).Load();
                model = new AddEditUserViewModel(user, ctsRoles.ToList());
            }

            model.AllRoles = roles == null;
            model.ReturnView = HttpContext.Request.UrlReferrer?.Segments.Last();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddEditUser(AddEditUserViewModel model)
        {
            var user = _cdb.CtsUser.Include(s => s.CtsRoles)
                .Where(x => x.Login == model.UserLogin)
                .FirstOrDefault(s => s.Domain == model.UserDomain);
            if (user == null)
            {
                user = new CtsUser()
                {
                    Login = model.UserLogin,
                    Domain = model.UserDomain
                };
                _cdb.CtsUser.Add(user);
            }
            foreach (var userRole in user.CtsRoles)
            {
                if (!model.CtsRoles.ContainsKey(userRole.RoleName))
                    model.CtsRoles.Add(userRole.RoleName, true);
            }

            user.CtsRoles.Clear();
            user.CtsRoles = model.CtsRoles?
                .Where(x => x.Value)
                .Select(x => _cdb.CtsRole.Find(x.Key))
                .ToList();
            _cdb.SaveChanges();

            return RedirectToAction(model.ReturnView ?? "index");
        }

        public ActionResult DeleteUser(string userLogin, string userDomain)
        {
            var user = _cdb.CtsUser.Find(userLogin, userDomain);
            if (user == null)
            {
                return HttpNotFound("Пользователь не найден");
            }

            foreach (var role in user.CtsRoles)
            {
                user.CtsRoles.Remove(role);
            }

            _cdb.CtsUser.Remove(user);
            _cdb.SaveChanges();

            return RedirectToAction("RoleList");
        }

        public ActionResult CheckIfUserExists(string userLogin)
        {
            UserPrincipal user;
            var domains = ConfigurationManager.AppSettings["DomainNames"].Split(',', ';').Select(s => s.Trim());

            foreach (var domain in domains)
            {
                user = DomainUsersHandler.FindUser(userLogin, domain.ToString());
                if (user != null)
                {
                    return Json(new { userFound = true, domain = user.Context.Name });
                }
            }

            return Json(new { userFound = false });
        }

    }
}