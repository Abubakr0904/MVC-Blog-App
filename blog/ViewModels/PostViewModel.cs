using System.ComponentModel.DataAnnotations;
using blog2.Entity;

namespace blog2.ViewModels;

public class PostViewModel
{ 
    public Guid? Id { get; set; } = default(Guid);
    
    [Required]
    [MaxLength(15)]
    public string Title { get; set; }
    
    [Required]
    public string Content { get; set; }
    public bool Edited { get; set; }

    [RegularExpression("^[A-Za-z0-9,]+$")]
    public string Tags { get; set; }
    
    public ulong Claps { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string BannerImageUrl { get; set; }
    public Guid AuthorId { get; set; }
    public bool CanEdit { get; set; } = false;
}