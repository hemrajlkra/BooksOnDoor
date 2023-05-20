namespace BooksOnDoorWeb.Models
{
	public class Category
	{
		// [key] : this annotation defines particular property is primary key
        public int Id { get; set; }
		public string Name { get; set; }
		public int DisplayOrder { get; set; }
    }
}
