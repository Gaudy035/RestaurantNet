using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_category")]
public class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string CategoryName { get; set; } = string.Empty;

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
