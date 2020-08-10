# Understanding Async Programming in C# and Blazor

This article provides an insight into async programming in C# on Blazor.  I make no claim to be an expert: this is a summary of my recent experiences and knowledge acquisition.  There's some original content, but most of what I've written has been gleaned from other author's work.  There's a list of links at the bottom to articles, blogs and other material I've found useful, and have used in writing this article.

Modern programs interact with databases and services on other computers all the time.  Understanding async programming and developing applications that use it is fast becoming a key skill.

##### So, What do you know about Async(hronous) Programming?

Ask a programmer if they understand async progamming.  They will probably nod their heads and then after a pause say yes. I was one before I started to use async programming in earnest.  I soon became painfully aware of just how shallow that knowledge was. Yes, sure, I knew all about it and could explain it in broad terms. But actually write structured and well behaved code? A somewhat painful lesson in humility.

##### So, What is Async(hronous) Programming?

Put simply, asynchronous programming lets us multi-task - like driving a car whilst talking to the passenger.  There's a very good explanation on the Microsoft Docs site describing [how to make a parallel task hot or sequential luke warm breakfast](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).

##### When should we use it?

There are three principle situations were asynchronous processes have significant advantages over a single sequential process:

1. Processor Intensive Operations - such as complex mathematical calculations.
2. I/0 Operations - where tasks are offloaded to either subsystems on the same computer, or run on remote computers.
3. Improved User Interface experience.

In processor Intensive Operations, you want multiply processors or cores.  Hand off most of the processing to these cores and the program can interact with the UI on the main process updating progress and allowing user interaction.  Multi-tasking on the same processor buys nothing.  The program doen't need more balls to juggle, just more jugglers.

On the other hand I/O operations don't need multiple processes.  They dispatch requests to sub-systems or remote services and await responses.  It's multi-tasking that now buys time - set up and monitor several tasks at once and wait for them to complete.  Wait time becomes dependant on the longest running task, not the sum of the tasks.

Run everything serially and the User Interface gets locked whever a task is running.  Asynchronous tasks free up the UI process. The UI can be updated while tasks are running.

### Tasks, Threading, Scheduling, Contexts

it's very easy to be confused by the nomenclature.  Lets try and understand the fundimentals.

#### What's really going on under the Hood?

It doesn't matter where your code runs, locally or somewhere in the cloud, one processor and one core means one man on the job.  The operating system can multi-task just like a man (note - this task has nothing to do with DotNet Core tasks), but there's only one man do one thing at a time.  More parallel tasks, more time switching between tasks, less time doing real work - sound familiar! With four processors or four cores you get four men on the job. Without a supervisor, one man running code, three men telling hime how to do it!

Now switch back to DotNet Core.  Operating system multi-tasking is exposed through threads (NOT tasks). Tasks are blocks of code packaged up and run on threads. TaskSchedulers organise and schedule Tasks.  They're basically state machines that keep track of execution order and yielding to organise efficient code execution.

All DotNet Core applications start on a single thread and all applications have a thread pool - normally one per processor/core.  UI and web server applications have a dedicated application thread - running a Synchronisation Context - to run the UI or HTTP/Web context.  This is the only thread with direct access to the UI/web context.  Console applications start and run on a threadpool thread. This behavioural difference has significant implications which we will discuss later. In this discussion AppThread equals main application thread. DotNet Core applies various constraints to this thread's operations. One that affects coding Tasks is that Tasks are loaded through SynchronisationContext.Post rather than directly onto the thread.

UI and Web Context Applications run everything on the Synchronisation Context unless coded otherwise.  It's the programmers responsibility to decide where to run tasks and switch tasks to the threadpool.  Once on the threadpool, tasks are managed by the threadpool.

TaskSchedulers run Tasks on threads.  There's one per thread. When you execute a task, the task creation process first checks TaskScheduler.Current. If no tasks are currently running on the thread this will be null.  If a TaskScheduler exists then there's a task running and the new task is loaded into the current TaskScheduler.  If one doesn't exist, the task creation process checks for a SynchronisationContext on the thread.  If one exists it schedules the new task through the SynchronisationContext.  If there's no SynchronisationContext, the task creation process gets TaskScheduler.Default, the threadpool scheduler, and loads the task into the threadpool.

Lets now look at the classic pattern.

```c#
private async Task UIMethodAsync(...)
{
    await code_Async;
    dependant_code_Sync;
    await code2_Async;
    dependant_code2_Sync;
}
```

Expand out the syntactic sugar and what you really have is:

```c#
public Task MethodAsync(...)
{
    code_Async().ContinueWith(task => {
        dependant_code_Sync;
        code2_Async().ContinueWith(task => {
            dependant_code2_Sync;
        });
    });
}
```
The expanded version shows what's really happening, with code after an await wrapped inside the ContinueWith code block.  The second await creates a new task that is handed to the task scheduler running the outer Task.  The TaskScheduler can see the whole picture, so can schedule everything to happen in the right order and keep tabs on which code blocks are working and which have yielded while waiting for external things to happen.

If MethodAsync is called directly from the UI/Web Context, such as responding to a UI event, the tasks will be loaded onto the AppThread through the SynchronisationContext. All code will run on this thread.

Now let's take a look at a slightly different implementation:

```c#
private async Task MethodAsync(...)
{
    await Task.Run(() => code_Async);
    await code2_Async;
}
```

The first await creates a new Task via Task.Run(..).  We're now managing where the task get run - Task.Run() specifically runs tasks on the threadpool.  The interesting part is where the second task gets run. Answer: on the main SynchronisationContext.  Why?  The code block is being executed on the SynchronisationContext, so the reference to the Task exists on the SynchronisationContext and any new tasks get created on the SynchronisationContext unless otherwise directed.

To run everything on the threadpool you would do something like this:

```c#
private async EventHandler(...) => await Task.Run(() => MethodAsync(...));

private async Task MethodAsync(...)
{
    await code_Async;
    await code2_Async;
}
```

### Example and Code

The example site and code repository are part of a larger project associated with several Blazor articles.

The site is [here on Azure](https://cec-blazor-server.azurewebsites.net/).

The code is available on Github at [CEC.Blazor Repository](https://github.com/ShaunCurtis/CEC.Blazor).

### Asnyc Coding Patterns

At the core of Async programming is the Task object.  All methods that run asynchronously return a Task: there is one except to this that we will cover shortly. Think of the Task as a wrapper object for a block of code.  It provides information about the state of the codeblock, a certain level of control, and exposes the return value on Task completion.

*Async* along with *await* is syntactic sugar designed to simplify coding async methods. 

*Async* labels a method as asynchronous and :
1. Lets you use the *await* keyword to wait on the completion of an asynchronous task.
2. Attaches any return value to the task object on completion.

*Await* suspends the current method/codeblock and yields control back to it's caller until the task completes.

> It's become common practice to postfix Async on the end of a method name that runs as a task.  

##### The Async Task Patterns

The Async Task patterns are used when you want to run one or more Tasks within your method.  You can run normal synchronous code within the same method.

The classic pattern:
```c#
private async Task MethodAsync(...)
{
    do_normal_stuff;_
    await do_some_work_Async;
    do_dependant_stuff;_
}
```
or 
```c#
private async Task<myobject> MethodAsync(...)
{
    do_normal_stuff;_
    await do_some_work_Async;
    do_dependant_stuff;_
    return new myobject;
}
```

##### The Task Patterns

While these patterns don't contain the *async* keyword, they can be run asynchronously.  They contain normal code, 
often a relatively long running calculation.  They are also used in method declarations in Interfaces and base classes.

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


#### Blocking and Deadlocking

One of the biggest challenges you will face is the Deadlock.  You'll write some async code that either always locks, or under load locks your program.  In Blazor, this manifests itself as a locked page.  The lights are on but there's no one at home. You've killed the application process running your SPA instance.  The only way out is to reload the page.

The normal reason is blocking code - program execution on the application thread is halted waiting for a task to complete. The task is further down the code pipeline on the thread.  The halt blocks execution of the code it's waiting on.  Deadlock.  If you move the task to the threadpool the task completes and the block continues.  However, no UI updates happened or UI events were answered.  So shifting code to the taskpool so you can block the application thread isn't the answer.  Nor is blocking threadpool threads because under load you application may block all the threads available.

Here's so classic blocking code - in this case a button click event in the UI.

```c#
public void ButtonClicked()
{
    var task = this.SomeService.GetAListAsync();
    task.Wait();
}
```

and more:

```c#
public void GetAListAsync()
{
    var task = myDataContext.somedataset.GetListAsync();
    var ret = task.Result;
}
```

In both instances - *task.Wait()* and *task.Result* are blocking actions.  They stop execution on the thread they are running on and wait for *task* to complete. However, *task* can't complete because the thread is blocked.

### Recommendations

1. **Async and Await All The Way**. Don't mix synchronous and asynchronous methods.  Start at the bottom - the data or process interface - and code async all the way up though the data and business/logic layers to the UI.  Blazor components implement both async and sync events, so there's no reason of any sync methods if your base library provides async interfaces.  
2. Only assign processor intensive tasks to the threadpool.  Don't do it because you can.
3. Don't use *Task.Run()* in your libraries.  Make the where to run a task decision in your application front end, and keep your libraries context agnostic.  
4. Never block in your libraries.  Seems obvious but... if you absolutely must block do it in the front end.
5. Always use *Async* and *Await*, don't get fancy.
6. If your library provides both async and sync calls, code them separately.  "Code it once" best practice doesn't apply here.  NEVER call one from the other if you don't want to shoot yourself in the foot at some point!
7. Only use *Async Void* for Event Handlers.  Never use it anywhere else.


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
    this.CosmicDirectoryService.MessageChanged += this.MessageUpdated;
```

This event is triggered whenever the service changes the message.  For example:

```c#
this.Message = "Ah there you are, hiding away down the Orion Arm";
this.MessageChanged?.Invoke(this, EventArgs.Empty);
```

firing the *MessageUpdated* event handler which updates the message and invokes a *StateHasChanged* event forcing a UI update.

That's it.  Constructive critisism welcome.

### A Note on the Demo Project Code and Site

It's the demo site for a series of articles on building Blazor Applications.  This is the first.  It expands on the basic Weather Forecast Blazor 
site demonstrating a set of methodologies, patterns and practices for building real world Blazor Server projects.

