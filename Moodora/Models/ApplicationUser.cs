using Microsoft.AspNetCore.Identity;

namespace Moodora.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeleteDate { get; set; }
    public bool IsBlocked { get; set; }
}
