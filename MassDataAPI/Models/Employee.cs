using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MassDataAPI.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(60)]
    public string Department { get; set; } = string.Empty;

    [Column(TypeName = "TEXT")]
    public string Gender { get; set; } = string.Empty;

    public int Age { get; set; }

    [Column(TypeName = "REAL")]
    public decimal Salary { get; set; }

    public DateOnly JoinDate { get; set; }
}
