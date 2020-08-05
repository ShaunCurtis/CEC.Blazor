using System;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Services
{
    public class CosmicDirectoryService
    {
        public event EventHandler MessageChanged;

        public string Message { get; set; } = "Waiting for an intercosmic connection";

        // This is the unsafe function described in the Async Programming Article
        // Call it from the UI and see what happens when you click a button
        public void GetWorld(bool fast)
        {
            var task = this.GetWorldAsync(fast);
            task.Wait();
        }

        public async Task GetWorldAsync(bool fast)
        {
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
                    this.Message = "Fixed It";
                    this.MessageChanged?.Invoke(this, EventArgs.Empty);
                });
            }
            await Task.Delay(500);
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
                this.Message = "Hmm, a bit slow today...";
                this.MessageChanged?.Invoke(this, EventArgs.Empty);
                await Task.Delay(1000);
            }
            this.Message = "Ah there you are, hiding away down the Orion Arm";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            await Task.Delay(2000);
        }

        private Task FixTheCosmosAsync(int speed)
        {
            if (speed > 3000) this.Message = "What, it's broken again? Background task - fix the Cosmos.  It might be a bit slow for a while!";
            else this.Message = "Background task - greasing the cosmic wheels.  Make it superfast today.";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
            return Task.Delay(speed);
        }
    }
}
