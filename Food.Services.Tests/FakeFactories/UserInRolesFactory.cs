using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    static class UserInRolesFactory
    {
        public static UserInRole CreateRoleForUser(User user)
        {
            var role = RoleFactory.CreateRole();
            ContextManager.Get().Roles.Add(role);
            var uic = ContextManager.Get().UsersInRoles.Add(new UserInRole()
            {
                Role = role, RoleId = role.Id, User = user, UserId = user.Id
            });
            return uic.Entity;
        }
        public static UserInRole CreateRoleForUser(User user, Role role)
        {

            var uic = ContextManager.Get().UsersInRoles.Add(new UserInRole()
            {
                Role = role,
                RoleId = role.Id,
                User = user,
                UserId = user.Id,
                IsDeleted = false
            });
            return uic.Entity;
        }

        public static UserInRole CreateUserInRole()
        {
            var role = RoleFactory.CreateRole();
            var user = UserFactory.CreateUser();
            var uic = ContextManager.Get().UsersInRoles.Add(new UserInRole()
            {
                Role = role,
                RoleId = role.Id,
                User = user,
                UserId = user.Id
            });
            return uic.Entity;
        }
    }
}
