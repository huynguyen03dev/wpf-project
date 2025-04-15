using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wpf_project.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        public bool IsPaid { get; set; } = false;
        
        public DateTime? PaymentDate { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
