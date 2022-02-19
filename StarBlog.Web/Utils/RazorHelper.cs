using Microsoft.AspNetCore.Mvc.Rendering;

namespace StarBlog.Web.Utils; 

public class RazorHelper {
    public static string GetCurrentControllerName(ViewContext viewContext) {
        return viewContext.RouteData.Values["controller"]?.ToString() ?? "Home";
    }

    public static string GetCurrentActionName(ViewContext viewContext) {
        return viewContext.RouteData.Values["action"]?.ToString() ?? "Index";
    }
}