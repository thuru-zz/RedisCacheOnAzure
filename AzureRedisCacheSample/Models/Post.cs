using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzureRedisCacheSample.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; }
        public Guid UserId { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}