using System.ComponentModel.DataAnnotations;

namespace StarBlog.Web.ViewModels.VisitRecord;

public class StatsDto {
    [Required] public int Year { get; set; }
    [Required] public int Month { get; set; }
    [Required] public int Day { get; set; }
}