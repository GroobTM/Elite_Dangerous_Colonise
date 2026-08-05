using Ixnas.AltchaNet;
using System.Collections.Concurrent;

namespace elite_dangerous_colonise.Services
{
    /// <summary> Stores and verifies Altcha captcha challenge results. </summary>
    public class AltchaMemoryStore : IAltchaChallengeStore
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> store = new ConcurrentDictionary<string, DateTimeOffset>();

        /// <summary> Stores an Altcha challenge result. </summary>
        /// <param name="challenge"> The challenge result. </param>
        /// <param name="expiryUtc"> The expiry date of the challenge result. </param>
        public Task Store(string challenge, DateTimeOffset expiryUtc)
        {
            store.TryAdd(challenge, expiryUtc);
            return Task.CompletedTask;
        }

        /// <summary> Checks if a challenge result is present in storage. </summary>
        /// <param name="challenge"> The challenge result to look for. </param>
        /// <returns> If the challege result was found. </returns>
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
