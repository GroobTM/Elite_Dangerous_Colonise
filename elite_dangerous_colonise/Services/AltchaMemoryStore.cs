using Ixnas.AltchaNet;
using System.Collections.Concurrent;

namespace elite_dangerous_colonise.Services
{
    public class AltchaMemoryStore : IAltchaChallengeStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> store = new ConcurrentDictionary<string, DateTimeOffset>();

        public Task Store(string challenge, DateTimeOffset expiryUtc)
        {
            store.TryAdd(challenge, expiryUtc);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string challenge)
        {
            RemovedExpiredEntries();

            return Task.FromResult(store.ContainsKey(challenge));
        }

        private void RemovedExpiredEntries()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            List<string> expiredKey = store.Where(entry => entry.Value < now).Select(entry => entry.Key).ToList();

            foreach (string key in expiredKey)
            {
                store.TryRemove(key, out _);
            }
        }
    }
}
