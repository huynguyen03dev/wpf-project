using System.ComponentModel.DataAnnotations;

namespace wpf_project.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Author { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; }
        
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }
        
        public string Description { get; set; }
        
        public string ImagePath { get; set; }
        
        [MaxLength(50)]
        public string Genre { get; set; }
    }
}
