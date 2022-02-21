using blog2.Data;
using blog2.Entity;
using blog2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace intro.Controllers;

[Route("")]
public class BlogsController : Controller
{
    private readonly ILogger<BlogsController> _logger;
    private readonly BlogAppDbContext _context;
    private readonly UserManager<User> _userM;
    private readonly SignInManager<User> _signInManager;

    public BlogsController(
        ILogger<BlogsController> logger,
        BlogAppDbContext context,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _logger = logger;
        _context = context;
        _userM = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("blogs")]
    public async Task<IActionResult> GetBlogs()
    {
        return View(new BlogsViewModel()
        {
            Blogs = await _context.Posts.Select(p => new BlogViewModel()
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Content,
                Author = _userM.FindByIdAsync(p.CreatedBy.ToString()).GetAwaiter().GetResult(),
                Tags = p.Tags.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
            })
            .ToListAsync()
        });
    }

    [Authorize]
    [HttpGet("write")]
    public IActionResult Write()
    {
        return View();
    }

    [Authorize]
    [HttpPost("write")]
    public async Task<IActionResult> Write([FromForm] PostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest($"{ModelState.ErrorCount} errors detected!");
        }

        if (model.Edited)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == model.Id);
            if (post.Title == model.Title && post.Content == model.Content && model.Tags == post.Tags)
            {
                return LocalRedirect($"~/post/{post.Id}");
            }

            post.ModifiedAt = DateTimeOffset.UtcNow.ToLocalTime();
            post.Title = model.Title;
            post.Content = model.Content;
            post.Tags = model.Tags;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return LocalRedirect($"~/post/{post.Id}");
        }

        var userId = _userM.GetUserId(User);
        var newPost = new Post(model.Title, model.Content, Guid.Parse(userId))
        {
            Tags = model.Tags
        };

        _context.Posts.Add(newPost);
        await _context.SaveChangesAsync();

        return LocalRedirect($"~/post/{newPost.Id}");
    }

    [HttpGet("post/{id}")]
    public async Task<IActionResult> Post(Guid id)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        var model = new PostViewModel()
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Edited = post.Edited,
            Tags = post.Tags,
            Claps = post.Claps,
            CreatedAt = post.CreatedAt,
            AuthorId = post.CreatedBy
        };
        if(_signInManager.IsSignedIn(User) && model.AuthorId.ToString() == _userM.GetUserId(User))
        {
            model.CanEdit = true;
            _logger.LogError("Keldi");
        }
        else
        {
            model.CanEdit = false;
            _logger.LogError("Keldi");
        }

        return View(model);
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
        var model = new PostViewModel()
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Tags = post.Tags,
            Edited = true,
            Claps = post.Claps,
            CreatedAt = post.CreatedAt
        };

        return View("Write", model);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlogsByTag([FromRoute]string id)
    {
        var model = new BlogsViewModel()
            {
                Blogs = await _context.Posts.Where(p => p.Tags.Contains(id)).Select( p =>
                    new BlogViewModel()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Content,
                        Tags = p.Tags.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                    })
                    .ToListAsync()
            };
        return View(model);
    }
}