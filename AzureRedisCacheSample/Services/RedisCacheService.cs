using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureRedisCacheSample.Models;
using StackExchange.Redis;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureRedisCacheSample.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService()
        {
            var connection = ConnectionMultiplexer.
                ConnectAsync(ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString).GetAwaiter().GetResult();

            _cache = connection.GetDatabase();
        }

        public async Task<IEnumerable<Post>> GetRecentPostsAsync()
        {
            var collection = await _cache.ListRangeAsync("recent-posts", 0, -1);
            var result = new List<Post>();

            foreach(var item in collection)
            {
                result.Add(JsonConvert.DeserializeObject<Post>(item.ToString()));
            }

            return result;
        }

        public async Task<object> RefreshRecentPostsAsync(IEnumerable<Post> posts)
        {
            var values = new RedisValue[10];

            for(int i = 0; i < 10; i++)
            {
                values[i] = JsonConvert.SerializeObject(posts.ElementAtOrDefault(i));
            }

            return await _cache.ListRightPushAsync("recent-posts", values, CommandFlags.FireAndForget);
        }

        public async Task<long> AddNewPostToCacheAsync(Post post)
        {
            var jsonPost = JsonConvert.SerializeObject(post);
            var position = await _cache.ListLeftPushAsync("recent-posts", new RedisValue[] { jsonPost });
            
            await _cache.ListTrimAsync("recent-posts", 0, 9, CommandFlags.FireAndForget);
            return position;
        }
    }
}