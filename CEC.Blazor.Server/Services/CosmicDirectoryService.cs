using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class CosmicDirectoryService
    {
        public event EventHandler MessageChanged;

        public string Message { get; set; } = "Ready for a intercosmic lookup.  Choose your speed?";

        public async Task<bool> BlackHoleWarning(bool escaped)
        {
            this.Message = "Oh no, here comes a Black Hole. Be warned it may be the last you hear froommm mmmmmeeeeeeee....!";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            this.Message = escaped ?  "TTThhat was close" : ".............";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            return escaped;
        }

        // This is the unsafe function described in the Async Programming Article
        // Call it from the UI and see what happens when you click a button
        public async Task GetWorld(bool fast)
        {
            var thread = Thread.CurrentThread;
            var ts = TaskScheduler.Current;
            var tsd = TaskScheduler.Default;
            var sc = SynchronizationContext.Current;

            var task = this.GetWorldAsync(fast);
            this.Message = "Close encounter with a Black Hole. Be warned it may be the last you hear froommm mmmmmeeeeeeee....!";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            await task;
        }

        public async Task GetWorldAsync(bool fast)
        {
            var sc = SynchronizationContext.Current;

            var cosmosspeed = fast ? 2000 : 8000;

            var backtask = FixTheCosmosAsync(cosmosspeed);
            await Task.Delay(2000);
            await LookupWorldAsync(backtask);
            if (!backtask.IsCompleted)
            {
                this.Message = "Where is that Cosmos when you need it!";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await backtask.ContinueWith(task =>
                {
                    this.Message = "Fixed";
                    this.MessageChanged?.Invoke(this, EventArgs.Empty);
                });
            }
            else
            {
                this.Message = "Surfing at light speed today";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
            }
            await Task.Delay(1000);
            this.Message = @"Greetings Earthing \\//";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task LookupWorldAsync(Task fixthecosmos)
        {
            this.Message = "Looking you up.....";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(1000);
            if (!fixthecosmos.IsCompleted)
            {
                this.Message = "Hmm, c's not squared today...";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(1000);
            }
            this.Message = "Ah there you are, hiding away down the Orion Arm";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(2000);
        }

        private Task FixTheCosmosAsync(int speed)
        {
            if (speed > 3000) this.Message = "What, it's broken again? Background task - fix c.  Space/time might be a wee slow for a while!";
            else this.Message = "Background task - greasing the cosmic wheels, we need light speed.";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            return Task.Delay(speed);
        }
    }
}
