#if OAUTH
using Quest.Mobile.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Mobile
{
 public static class DefaultUsersConfig
    {
        /// <summary>
        /// Administrator: can access all areas
        /// SysAdmin: administer the system
        /// Guests: can see their itinary
        /// PartnerManager: manager of a partner
        /// PartnerUser: salesman of a partner
        /// SupplierManager: manager of a partner
        /// SupplierUser: salesman of a partner
        /// Purchaser: back office
        /// </summary>

        public static void RegisterDefaultUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "administrator", "user" };
            foreach (var role in roleManager.Roles.ToList())
            {
                if (!roles.Contains(role.Name))
                    roleManager.Delete(role);
            }


            // create roles
            foreach (var role in roles)
                if (!roleManager.RoleExists(role))
                    roleManager.Create(new IdentityRole(role));

            // List<string> adminusers = new List<string>() { "administrator", "marcuspoulton", "susannahmoney" };

            AddUsers(
                userManager,
                new List<string>() { "administrator:Administrator:siobhan89", "marcuspoulton:Marcus Poulton:siobhan89" },
                new List<string>() { "administrator", "user" }
                );

            AddUsers(
                userManager,
                new List<string>() {
                    "mbrady:Micheal Brady:quest3000",
                    "iyoung:Ian Young:quest3000",
                    "guest:Guest:quest3000",
                    "dispatcher:Dispatcher:quest3000",
                    "calltaker:Calltaker:quest3000",
                    "supervisor:Supervisor:quest3000",
                    "analyst:Analyst:quest3000",
                    "guest:Guest:quest3000",
                }, 
                new List<string>() { "user" }
                );

        }

        /// <summary>
        /// add these users to the system, each user is given the roles in the list
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="users"></param>
        /// <param name="roles"></param>
        static void AddUsers(UserManager<ApplicationUser> userManager, List<string> users, List<string> roles)
        {
            foreach (var userparts in users)
            {
                var username = userparts.Split(':');

                var user = userManager.FindByName(username[0]);
                if (user == null)
                {
                    user = new ApplicationUser()
                    {
                        UserName = username[0],
                        Fullname = username[1]
                    };

                    userManager.Create(user, username[2]);
                }

                foreach (var r in roles)
                {
                    try
                    {
                        userManager.AddToRole(user.Id, r);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

        }
    }
}
#endif
