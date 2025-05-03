using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScientificArticleManagement.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public string? Image { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } // "Author" hoặc "Admin"

        // Navigation property
        public virtual ICollection<Article>? Articles { get; set; }
    }
}
