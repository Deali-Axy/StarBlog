using Microsoft.AspNetCore.Mvc.Rendering;

namespace StarBlog.Web.Utils;

public static class RazorHelper {
    public static string GetCurrentControllerName(ViewContext viewContext) {
        return viewContext.RouteData.Values["controller"]?.ToString() ?? "Home";
    }

    public static string GetCurrentActionName(ViewContext viewContext) {
        return viewContext.RouteData.Values["action"]?.ToString() ?? "Index";
    }

    public static string ControllerHighlight(ViewContext viewContext, string controllerName,
        string className = "active") {
        return controllerName == GetCurrentControllerName(viewContext) ? className : "";
    }

    public static string ActionHighlight(ViewContext viewContext, string actionName, string className = "active") {
        return actionName == GetCurrentActionName(viewContext) ? className : "";
    }

    public static string NavItemActive(ViewContext viewContext, string controllerName, string actionName,
        string className = "active") {
        if (GetCurrentControllerName(viewContext) == controllerName
            && GetCurrentActionName(viewContext) == actionName) {
            return className;
        }

        return "";
    }
}

public static class ViewContextExtension {
    public static string NavItemActive(this ViewContext viewContext, string controllerName, string actionName,
        string className = "active") {
        return RazorHelper.NavItemActive(viewContext, controllerName, actionName, className);
    }
}