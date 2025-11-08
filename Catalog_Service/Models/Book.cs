using System.ComponentModel.DataAnnotations;

namespace CATALOGSERVICE.Modele
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Topic { get; set; }
        public required int Quantity { get; set; }
        public required decimal Price { get; set; }

    }
}
