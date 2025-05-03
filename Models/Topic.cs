using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ScientificArticleManagement.Models
{
    public class Topic
    {
        [Key]
        public int TopicId { get; set; }

        [Required, Display(Name = "Topic Name")]
        [StringLength(100)]
        public string TopicName { get; set; }

        // Navigation property
        public virtual ICollection<Article> Articles { get; set; }
    }
}
