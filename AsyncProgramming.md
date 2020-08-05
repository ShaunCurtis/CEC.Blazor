# Understanding Async Programming in C# and Blazor

This article provides an insight into async programming in C# in Blazor.  I make no claim to be an expert, so this is a summary of my recent
experiences and knowledge acquisition.  Ask a programmer if they understand async progamming.  They will probably nod their heads and then say yes. 
I was one of those until I started to use async programming in earnest.  I soon became painfully aware of the shallow depth of that knowledge. 
Yes I knew what it was and could explain it in broad terms, but actually writing well behaved code, ....

##### So, What is Async(hronous) Programming?

Put simply, asynchronous programming lets us run several tasks in parallel - like 
driving your car whilst talking to your passenger.  There's a very good explanation on the Microsoft Docs site describing 
[how to make a parallel task hot or sequential luke warm breakfast](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).

##### Do we need it?

A program running in isolation takes longer to running tasks in parallel - there's task switching involved which eats up pocessor cycles.

Switch to the modern world: programs interacts with databases and services on other computers.  Execute a 
set of tasks that depend on such services sequentially and our program spends considerable time twiddling its 
thumbs. Execute the same set of tasks in parallel and it only waits on the longest running task.

In the SPA world (Single Page Applications running in a web browser), we can also use async programming to interact with the 
user while we are loading and executing services - not just present a blank web page.

### Tasks and Threading

Don't confuse tasks with threading.  Tasks run on a thread, but then so does all code.  Tasks are executed by a Task Scheduler which is 
responsible for any threading operations.  A Task is equivalent to a promise in other languages.

### Example and Code

The example site and code repository are part of a larger project associated with several Blazor articles.

The site is [here on Azure](https://cec-blazor-server.azurewebsites.net/).

The code is available on Github at [CEC.Blazor Repository](https://github.com/ShaunCurtis/CEC.Blazor).

### Asnyc Coding Patterns

At the core of Async programming is the Task object.  All methods that run asynchronously return a Task: there is one except to this that we will cover 
shortly. Think of the Task as a wrapper object that provides information about the state of the method called, a certain level of control, and 
exposes the return value when the Task completes.

First a word about the *async* keyword.  Along with *await* it's syntactic sugar to simplify coding async methods. 
It labels a method as asynchronous and :
1. Lets you use the *await* keyword to wait on the completion of an asynchronous task.
2. Wraps any return value in a task object set to complete.

> A word on naming conventions.  It's become common practice to post fix Async on the end of an method name that can be run asynchronously.  

##### The Async Task Patterns

The classic pattern:
```c#
private async Task MethodAsync(...)
{
    do some work;
}
```
or 
```c#
private async Task<myobject> MethodAsync(...)
{
    do some work;
    return new myobject;
}
```

##### The Task Patterns

While these patterns don't contain the *async* keyword, they can be run asynchronously.

```c#
private Task MethodAsync(...)
{
    do some work;
    return Task.CompletedTask;
}
```
or 
```c#
private Task<myobject> MethodAsync(...)
{
    do some work;
    return Task.FromResult(new myobject);
}
```
or 
```c#
private Task MethodAsync(...)
{
    return  another_task_returning_method_of_the_same_pattern();
}
```

##### The Event Pattern

```c#
public async void MethodAsync(...);
```

This is the exception.  It returns a void giving the calling function no control mechanism. It's a fire and forget pattern that runs in the background 
until complete.  It's normally triggered by events such as a mouse click.

### Some Real World Examples

Lets look at some real world examples with a little more complexity and alternative ways of achieving the same result.

##### Handling Events

In the Cosmic Locator example project - an alternative take on Hello World - the locator runs on page load and when a button is clicked.  Lets 
look at the button event.

The button onclick handler calls the *ButtonClicked* event through an anonymous function call.

```html
<button class="btn btn-dark" @onclick="e => this.ButtonClicked(true)">Superfast Dark Cosmos</button>
```

The *ButtonClicked* event handler is labelled async but returns a void so is fire and forget.  It calls and waits on the *GetWorldAsync* async method on 
the CosmicDirectoryService, and then updates the UI. *It doesn't really need to as the UI updating is handled differently, it's just here for demo purposes in the alternative coding methods below.*

```c#
public async void ButtonClicked(bool fast)
{
    await this.CosmicDirectoryService.GetWorldAsync(fast);
    InvokeAsync(this.StateHasChanged);
}
```

The same method could be written as follows and still achieve the same result (it's very similar to how the CIL implements async and await).

```c#
public void ButtonClicked(bool fast)
{
    this.CosmicDirectoryService.GetWorldAsync(fast).ContinueWith(task => {
        InvokeAsync(this.StateHasChanged);
    });
}
```

Note the *async* label has gone, with the async behaviour moved directly to the *GetWorldAsync* method.  It returns a Task on which we call 
*ContinueWith*.  The code block within the anonymous function is run once *GetWorldAsync* returns completed.

A third possible way to write the method is:

```c#
public void ButtonClicked(bool fast)
{
    var task = this.CosmicDirectoryService.GetWorldAsync(fast);
    task.Wait();
    InvokeAsync(this.StateHasChanged);
}
```

This looks the same as the others, but be warned it isn't.  It's unsafe and potentially blocking if the method you call also creates further Tasks. 
Leave *Task.Wait* in the toolbox and use *ContinueWith*.

Clicking on the Black Hole button "executes" this code. You'll need to reload the page afterwards!

##### Service Task Methods

The background task in the example is shown below.  It delays for x seconds before completing.  It runs a block of normal synchronous code and then returns the delay Task.
```c#
private Task FixTheCosmosAsync(int speed)
{
    if (speed > 3000) this.Message = "What, it's broken again? Background task - fix the Cosmos.  It might be a bit slow today!";
    else this.Message = "Background task - grease the cosmic wheels.  Make it superfast today";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
    return Task.Delay(speed);
}
```

*GetWorldAsync* is the method called from the UI.  It kicks off the backtask - *FixTheCosmosAsync* - but doesn't wait on it. It just waits 2 seconds for the 
user to read the message, and then kicks off and awaits the *LookupWorldAsync* method.  When that completes it checks if the backtask is complete.  If 
it isn't it displays a new message and waits for it to complete.

```c#
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
            this.Message = "At last!!!";
            this.MessageChanged?.Invoke(this, EventArgs.Empty);
        });
    }
    await Task.Delay(500);
    this.Message = @"Greetings Earthing \\//";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
}
```
The final method is the *LookupWorldAsync*.  This does the actual lookup.  It checks the background task and if it's not completed it complains about it and waits.  

```c#
private async Task LookupWorldAsync(Task fixthecosmos)
{
    this.Message = "Looking you up.....";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
    await Task.Delay(1000);
    if (!fixthecosmos.IsCompleted)
    {
        this.Message = "Hmm, taking a while today...";
        this.MessageChanged?.Invoke(this, EventArgs.Empty);
        await Task.Delay(1000);
    }
    this.Message = "Ah there you are, hiding away down the Orion Arm";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
    await Task.Delay(2000);
}
```

#### Getting the Message Through

In the example the message changes at various points in code execution so we need to make sure the UI updates whenever the message is changed.

UI Updating is handled by Events and Event Handlers.  Page loading is async so there is no guarantee of code execution order. The UI must be prepared
to handle null data objects.

We wire the UI Message Property direct to the Service with added null service error handling.

```c#
protected string Message => CosmicDirectoryService?.Message ?? "Waiting...";
```
The Page Load events look like this:

```c#
protected override Task OnInitializedAsync()
{
    this.CosmicDirectoryService.MessageChanged += this.MessageUpdated;
    return Task.CompletedTask;
}

protected override Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender) return this.CosmicDirectoryService.GetWorldAsync(true);
    else return Task.CompletedTask;
}
```
We do the first lookup once the page rendering is complete.

UI Updates are handled by the *MessageUpdated* event handler.
```c#
protected void MessageUpdated(object sender, EventArgs e) => InvokeAsync(this.StateHasChanged);
```

This is wired to the *CosmicDirectoryService.MessageChanged* event in *OnInitializedAsync()*.

```c#
this.Message = this.CosmicDirectoryService.Message;
```

This event is triggered whenever the service changes the message.

```c#
this.Message = "Ah there you are, hiding away down the Orion Arm";
this.MessageChanged?.Invoke(this, EventArgs.Empty);
```

firing the *MessageUpdated* event handler which updates the message and invokes a *StateHasChanged* event forcing a UI update.

That's it.  Constructive critisism welcome.

### A Note on the Demo Project Code and Site

It's the demo site for a series of articles on building Blazor Applications.  This is the first.  It expands on the basic Weather Forecast Blazor 
site demonstrating a set of methodologies, patterns and practices for building real world Blazor Server projects.

