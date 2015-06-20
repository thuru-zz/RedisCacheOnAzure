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
        private readonly IDatabase _cache;

        public ApplicationService()
        {             
            _dbContext = new RedisApplicationDataContext();

            var connection = ConnectionMultiplexer.ConnectAsync(ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString)
                .GetAwaiter().GetResult();

            _cache = connection.GetDatabase();
        }

        /// <summary>
        /// Simple key value example
        /// stores user profile against the user Id with 60 minutes time to live property
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> LoadUserInfoAsync(string userId)
        {          
            var profile = await _dbContext.UserProfiles.SingleOrDefaultAsync(u => u.UserId == Guid.Parse(userId));
            if (profile != null)
            {
                var jsonProfile = JsonConvert.SerializeObject(profile);
                return await _cache.StringSetAsync(userId, jsonProfile, TimeSpan.FromMinutes(60), When.Always, CommandFlags.FireAndForget);
            }
            else
            {
                return false;
            }
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync()
        {
            var collection = await _cache.ListRangeAsync("recent-posts", 0, -1);
            var result = new List<Post>();

            if (collection.Length > 0)
            {
                foreach (var item in collection)
                {
                    result.Add(JsonConvert.DeserializeObject<Post>(item.ToString()));
                }

                return result;
            }

            var posts = await _dbContext.Posts.Take(10).OrderByDescending(p => p.CreatedOn).ToListAsync();

            var values = new RedisValue[10];

            for (int i = 0; i < 10; i++)
            {
                values[i] = JsonConvert.SerializeObject(posts.ElementAtOrDefault(i));
            }

            await _cache.ListRightPushAsync("recent-posts", values, CommandFlags.FireAndForget);

            return posts;
        }

        public async Task<int> AddPostAsync(Post post)
        {
            _dbContext.Posts.Add(post);
            var id = await _dbContext.SaveChangesAsync();

            var jsonPost = JsonConvert.SerializeObject(post);

            // adding as a head element of the list.
            await _cache.ListLeftPushAsync("recent-posts", new RedisValue[] { jsonPost });

            // trimming the list : capped lists
            // FireAndForget is very suitable here as we do not get the posts right now.
            await _cache.ListTrimAsync("recent-posts", 0, 9, CommandFlags.FireAndForget);
           
            return id;
        }

        public async Task<int> LikePostAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}