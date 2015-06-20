using AzureRedisCacheSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Threading.Tasks;

namespace AzureRedisCacheSample.Services
{
    public class ApplicationService
    {
        private readonly RedisCacheService _redisService;

        public ApplicationService()
        {
            _redisService = new RedisCacheService();
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync()
        {
            IEnumerable<Post> posts = await _redisService.GetRecentPostsAsync();

            if (posts == null || posts.Count() == 0)
            {
                var context = new RedisApplicationDataContext();
                posts = await context.Posts.Take(10).OrderByDescending(p => p.CreatedOn).ToListAsync();

                await _redisService.RefreshRecentPostsAsync(posts);
            }

            return posts;
        }

        public async Task<int> AddPostAsync(Post post)
        {
            var context = new RedisApplicationDataContext();
            context.Posts.Add(post);
            var id = await context.SaveChangesAsync();

            var value = await _redisService.AddNewPostToCacheAsync(post);

            return id;
        }
    }
}