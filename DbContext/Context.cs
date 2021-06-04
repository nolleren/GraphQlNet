
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GraphQLCore.Class;
using MongoDB.Driver;

namespace GraphQLCore.DbContext
{
    public class Context<T> where T : BaseEntity
    {
        public IMongoCollection<T> Collection { get; private set; }

        public Context(string collection)
        {
            var client = new MongoClient("mongodb+srv://admin:ab87935000@cluster0.im4bm.mongodb.net/alternative?retryWrites=true&w=majority");
            var database = client.GetDatabase("alternative");
            Collection = database.GetCollection<T>(collection);
        }

        public async Task<T> GetById(string id)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, id);
            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IList<T>> GetAll(Expression<Func<T, bool>> where = null)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Active, true);
            if (!(where is null))
            {
                filter = Builders<T>.Filter.And(filter, Builders<T>.Filter.Where(where));
            }
            
            return await Collection.Find(filter).ToListAsync();
        }

        public async Task<T> Create(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await Collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T> Update(string id, Dictionary<Expression<Func<T, object>>, object> changes)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));
            }

            if (changes is null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            
            var builder = Builders<T>.Update;
            var updates = changes.Select(c => builder.Set(c.Key, c.Value)).ToList();
            var update = builder.Combine(updates);

            var options = new FindOneAndUpdateOptions<T> { ReturnDocument = ReturnDocument.After };
            return await Collection.FindOneAndUpdateAsync(filter, update, options);
        }
    }
}