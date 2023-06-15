using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BooksOnDoorWeb.Models
{
	public class Category
	{
		// [key] : this annotation defines particular property is primary key
        public int Id { get; set; }
		[Required]
		[MaxLength(50)]
		[DisplayName("Category Name")]
		public string Name { get; set; }
		[DisplayName("Display Order")]
		[Range(0,50)]
		public int DisplayOrder { get; set; }
    }
}
