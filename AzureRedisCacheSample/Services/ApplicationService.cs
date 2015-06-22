using AzureRedisCacheSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Configuration;
using Newtonsoft.Json;

namespace AzureRedisCacheSample.Services
{
    public class ApplicationService
    {
        private readonly RedisApplicationDataContext _dbContext;
        private readonly RedisCacheService _cacheService;

        public ApplicationService()
        {
            _dbContext = new RedisApplicationDataContext();
            _cacheService = new RedisCacheService();
        }

        /// <summary>
        /// Simple key value example
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public async Task<bool> LoadUserInfoAsync(string userId)
        {
            var profile = await _dbContext.UserProfiles.SingleOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
            if (profile != null)
            {
                if (!await _cacheService.SetUserProfileAsync(userId, profile))
                {
                    throw new ApplicationException("Failed to set key value in cache");
                }
            }
            throw new ApplicationException("User profile Not Found");
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync()
        {
            var posts = await _cacheService.GetRecentPostsAsync();

            if (posts.Count == 0)
            {
                posts = await _dbContext.Posts.Take(10).OrderByDescending(p => p.CreatedOn).ToListAsync();
                await _cacheService.RefreshRecentPostsAsync(posts);
                return posts;
            }

            return posts;
        }

        public async Task<int> AddPostAsync(Post post)
        {
            _dbContext.Posts.Add(post);
            var id = await _dbContext.SaveChangesAsync();

            await _cacheService.AddNewPostAsync(post);
             
            return id;
        }

        
        //public async Task<int> LikePostAsync(int id)
        //{
            
        //}

    }
}