using blog2.Data;
using blog2.Entity;
using Microsoft.AspNetCore.Mvc;

namespace blog2.Controllers;

public class SearchController : Controller
{
    private readonly BlogAppDbContext _ctx;

    public SearchController(BlogAppDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpPost("{query}")]
    public IActionResult ByTagAndWord(string query)
    {
        var posts = _ctx.Posts.ToList();
        var result = new List<Post>();
        foreach (var post in posts)
        {
            if(post.Tags.ToLower().Contains(query.ToLower()) 
                || post.Title.ToLower().Contains(query.ToLower()))
            {
                result.Add(post);
            }
        }
        return View("ByTagAndWord", result);
    }
}