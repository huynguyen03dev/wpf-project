using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wpf_project.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }
        
        public bool IsAdmin { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }


        // Navigation property
        public virtual ICollection<Order> Orders { get; set; }
    }
}
