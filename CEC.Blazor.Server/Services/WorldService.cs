using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class WorldService
    {
        public event EventHandler MessageChanged;

        public string Message { get; set; } = "Finding You...";

        public IMemoryCache MemoryCache { get; set; }

        public WorldService (IMemoryCache memoryCache)
        {
            this.MemoryCache = memoryCache;
        }

        public Task GetWorld(bool yes)
        {
            return MemoryCache.GetOrCreateAsync(yes, async (e) =>
            {
                e.SetOptions(new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });
                this.Message = "Wait a para sec, getting a cosmic connection.....";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000);
                this.Message = "Hmm, taking a while today...";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(5000);
                this.Message = "Ah there you are, hiding away down the Orion Arm";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000);
                this.Message = @"Greetings Earthing \\//";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            });
        }

        public async Task GetWorld()
        {
                this.Message = "Wait a para sec, getting a cosmic connection.....";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000);
                this.Message = "Hmm, taking a while today...";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(5000);
                this.Message = "Ah there you are, hiding away down the Orion Arm";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(3000);
                this.Message = @"Greetings Earthing \\//";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
