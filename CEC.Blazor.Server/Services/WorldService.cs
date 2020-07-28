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

        public WorldService(IMemoryCache memoryCache)
        {
            this.MemoryCache = memoryCache;
        }

        public async Task GetWorld(bool yes)
        {
            await Task.Delay(1000);
            this.Message = "Looking you up.....";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = "Hmm, taking a while today...";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = "Ah there you are, hiding away down the Orion Arm";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = @"Greetings Earthing \\//";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task GetWorld()
        {
            await Task.Delay(1000);
            this.Message = "Looking you up.....";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = "Hmm, taking a while today...";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = "Ah there you are, hiding away down the Orion Arm";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = @"Greetings Earthing \\//";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
