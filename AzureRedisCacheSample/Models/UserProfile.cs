using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureRedisCacheSample.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public string Recognition { get; set; }
    }
}