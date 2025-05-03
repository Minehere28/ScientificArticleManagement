using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using ScientificArticleManagement.Models;

namespace ScientificArticleManagement.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name = "Article Title")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required, Display(Name = "Summary")]
        public string Summary { get; set; }

        [Required, Display(Name = "Content")]
        public string Content { get; set; }

        [Display(Name = "Submission Date")]
        [DataType(DataType.Date)]
        public DateTime SubmissionDate { get; set; }

        [Display(Name = "Accepted Date")]
        [DataType(DataType.Date)]
        public DateTime? AcceptedDate { get; set; }

        [Display(Name = "Denied Date")]
        [DataType(DataType.Date)]
        public DateTime? DeniedDate { get; set; }

        [Required, Display(Name = "Status")]
        [StringLength(20)]
        public string Status { get; set; } // "Pending", "Approved", "Rejected"

        public int CurrentView { get; set; } = 0; //cái này đổi thành CurrentView

        // Foreign Key
        [ForeignKey("Author")]
        public int UserId { get; set; }

        [ForeignKey("Topic")]
        public int TopicId { get; set; }

        // Navigation properties
        public virtual User Author { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
