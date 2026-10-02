using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_category")]
public class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; init; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string CategoryName { get; init; } = string.Empty;

    public ICollection<MenuItem> MenuItems { get; init; } = new List<MenuItem>();
}
