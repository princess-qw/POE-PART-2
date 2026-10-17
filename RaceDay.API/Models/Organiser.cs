
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Organisers")]
public class Organiser
{
    [Key]
    [Column("Organiser_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrganiserId { get; set; }

    [Column("User_ID")]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("First_Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("Last_Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("Organisation_Name")]
    public string OrganisationName { get; set; } = string.Empty;

    [Column("Created_At")]
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
