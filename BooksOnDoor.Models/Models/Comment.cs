namespace BooksOnDoor.Models.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public required string UserName { get; set; }
        public required string CommentText { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
