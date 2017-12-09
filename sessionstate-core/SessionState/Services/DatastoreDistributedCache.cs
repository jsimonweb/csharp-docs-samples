using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Google.Cloud.Datastore.V1;

class DatastoreDistributedCache : IDistributedCache
{
    /// <summary>
    /// My connection to Google Cloud Datastore.
    /// </summary>
    private DatastoreDb _datastore;
    private KeyFactory _sessionKeyFactory;

    /// <summary>
    /// Property names and kind names for the datastore entities.
    /// </summary>
    private const string
        EXPIRES = "expires",
        TIMEOUT = "timeout",
        ITEMS = "items",
        SESSION_KIND = "Session";

    public DatastoreDistributedCache()
    {
        _datastore = DatastoreDb.Create("arc-nl", "sessionStateApplication");
        _sessionKeyFactory = _datastore.CreateKeyFactory(SESSION_KIND);
    }

    public byte[] Get(string key)
    {
        var entity = _datastore.Lookup(_sessionKeyFactory.CreateKey(key));
        if (entity == null)
        {
            return null;
        }
        else
        {
            return entity[ITEMS]?.BlobValue?.ToByteArray();
        }
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
    {
        var entity = await _datastore.LookupAsync(_sessionKeyFactory.CreateKey(key), 
        callSettings:Google.Api.Gax.Grpc.CallSettings.FromCancellationToken(token));
        if (entity == null)
        {
            return null;
        }
        else
        {
            return entity[ITEMS]?.BlobValue?.ToByteArray();
        }
    }

    public void Refresh(string key)
    {
        throw new System.NotImplementedException();
    }

    public Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
    {
        throw new System.NotImplementedException();
    }

    public void Remove(string key)
    {
        throw new System.NotImplementedException();
    }

    public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
    {
        throw new System.NotImplementedException();
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        throw new System.NotImplementedException();
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
    {
        throw new System.NotImplementedException();
    }
}