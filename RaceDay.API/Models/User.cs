
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("User_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Password_Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = string.Empty;

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; }

    public Organiser? Organiser { get; set; }

    public Participant? Participant { get; set; }
}
