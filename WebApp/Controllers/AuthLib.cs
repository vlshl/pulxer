using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public static class AuthLib
    {
        public static int GetUserId(this ControllerBase controller)
        {
            int userId;
            if (!int.TryParse(controller.HttpContext.User.Identity.Name, out userId))
            {
                return 0;
            }

            return userId;
        }

        public static bool IsAdminRole(this ControllerBase controller)
        {
            return controller.HttpContext.User.IsInRole("admin");
        }

        public static bool IsLeechRole(this ControllerBase controller)
        {
            return controller.HttpContext.User.IsInRole("leech");
        }

        public static bool IsRole(this ControllerBase controller, string role)
        {
            return controller.HttpContext.User.IsInRole(role);
        }
    }
}
