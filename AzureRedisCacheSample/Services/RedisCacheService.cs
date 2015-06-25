using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AzureRedisCacheSample.Models;
using Newtonsoft.Json;

namespace AzureRedisCacheSample.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _cache;

        /// <summary>
        /// constructor creates redis cache connection
        /// </summary>
        public RedisCacheService()
        {
            var connection = ConnectionMultiplexer.ConnectAsync(ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString)
               .GetAwaiter().GetResult();

            _cache = connection.GetDatabase();
        }

        #region KeyValue operations

        /// <summary>
        /// Sets an object as value for a string key
        /// TTL is 60 minutes, cache is set only if the item does not exist
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        internal async Task<bool> SetUserProfileAsync(string userId, UserProfile profile)
        {
            var jsonProfile = JsonConvert.SerializeObject(profile);
            return await _cache.StringSetAsync(userId, jsonProfile, TimeSpan.FromMinutes(60), When.NotExists, CommandFlags.FireAndForget);
        }

        /// <summary>
        /// retrieves an object using key
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal async Task<UserProfile> GetUserProfileAsync(string userId)
        {
            var profile = await _cache.StringGetAsync(userId);
            if (!profile.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<UserProfile>(profile);
            }
            return null;
        }

        #endregion

        #region List operations

        /// <summary>
        /// gets the range of values from list
        /// gets all values 0 to tail -1, since lists in redis are linked lists
        /// </summary>
        /// <returns></returns>
        internal async Task<List<Post>> GetRecentPostsAsync()
        {
            var collection = await _cache.ListRangeAsync("recent-posts", 0, -1);

            var result = new List<Post>();

            foreach (var item in collection)
            {
                result.Add(JsonConvert.DeserializeObject<Post>(item.ToString()));
            }

            return result;
        }

        /// <summary>
        /// loads the recent posts 
        /// creates the lists if it does not exist
        /// </summary>
        /// <param name="posts"></param>
        /// <returns></returns>
        internal async Task<long> RefreshRecentPostsAsync(IEnumerable<Post> posts)
        {
            var values = new RedisValue[10];

            for (int i = 0; i < 10; i++)
            {
                values[i] = JsonConvert.SerializeObject(posts.ElementAtOrDefault(i));
            }

            var value = await _cache.ListLeftPushAsync("recent-posts", values, CommandFlags.FireAndForget);
            await _cache.ListTrimAsync("recent-posts", 0, 9, CommandFlags.FireAndForget);
            return value;
        }

        /// <summary>
        /// setst the post in the head of the list
        /// usage of capped lists by trimming it
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        internal async Task AddNewPostAsync(Post post)
        {
            var jsonPost = JsonConvert.SerializeObject(post);

            // adding as a head element of the list.
            await _cache.ListLeftPushAsync("recent-posts", new RedisValue[] { jsonPost }, CommandFlags.FireAndForget);

            // trimming the list : capped lists
            await _cache.ListTrimAsync("recent-posts", 0, 9, CommandFlags.FireAndForget);
        }

        #endregion

        #region Basic Function operations
        
        /// <summary>
        /// increments the online user count
        /// if key does not exist it will set the key to 0 before performing any operations
        /// </summary>
        /// <returns></returns>
        internal async Task<long> IncrementOnlineUserCountAsync()
        {
           return await _cache.StringIncrementAsync("online-count", 1);
        }

        /// <summary>
        /// decerements the online user count
        /// if key does not exist it will set the key to 0 before performing any operations
        /// </summary>
        /// <returns></returns>
        internal async Task<long> DecrementOnlineUserCountAsync()
        {
            return await _cache.StringDecrementAsync("online-count", 1);
        }

        #endregion

        
        #region SetOperations

        /// <summary>
        /// add tags for a post in Redis set
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        internal async Task<long> TagPostAsync(int postId, IEnumerable<Tag> tags)
        {
            var redisValues = new RedisValue[tags.Count()];

            var index = 0;

            foreach(var item in tags)
            {
                redisValues[index] = item.Name;
                index++;
            }

            return await _cache.SetAddAsync(postId.ToString(), redisValues, CommandFlags.FireAndForget);
        }

        #endregion
    }
}