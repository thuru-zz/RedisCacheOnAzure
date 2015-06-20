using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AzureRedisCacheSample.Models
{
    public class RedisApplicationDataContext : DbContext
    {
        public RedisApplicationDataContext()
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
    }
}