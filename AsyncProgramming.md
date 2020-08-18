# Understanding and Using Async Programming in DotNetCore and Blazor

This article provides an insight into async programming in DoteNetCore and implementing it in Blazor.  I make no claim to be an expert: this is a summary of my recent experiences and knowledge acquisition.  There's some original content, but most of what I've written has been gleaned from other author's work.  There's a list of links at the bottom to articles, blogs and other material I've found useful, and have mined in writing this article.  Constructive criticism and bug fixing welcome.

Modern programs rely on databases and services running remotely.  They need to be prepared for latency and delay.  Understanding async programming and developing applications that use it is fast becoming a key skill.

##### What do you know about Async(hronous) Programming?

Ask a programmer if they understand async progamming.  They will probably nod their heads and then after a pause say yes. I was one before I started using async programming in earnest.  I soon became painfully aware of just how shallow that knowledge was. Yes, sure, I knew all about it and could explain it in broad terms. But actually write structured and well behaved code? There followed a somewhat painful lesson in humility.

##### So, What is Async(hronous) Programming?

Put simply, asynchronous programming lets us multi-task - like driving a car whilst talking to the passenger.  There's a very good explanation on the Microsoft Docs site describing [how to make a parallel task hot or sequential luke warm breakfast](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/).

##### When should we use it?

There are three principle situations were asynchronous processes have significant advantages over a single sequential process:

1. Processor Intensive Operations - such as complex mathematical calculations.
2. I/0 Operations - where tasks are offloaded to either subsystems on the same computer, or run on remote computers.
3. Improved User Interface experience.

In processor intensive operations, you want multiple processors or cores.  Hand off most of the processing to these cores and the program can interact with the UI on the main process updating progress and handling user interaction.  Multi-tasking on the same processor buys nothing.  The program doen't need more balls to juggle, just more jugglers.

On the other hand I/O operations don't need multiple processors.  They dispatch requests to sub-systems or remote services and await responses.  It's multi-tasking that now buys time - set up and monitor several tasks at once and wait for them to complete.  Wait time becomes dependant on the longest running task, not the sum of the tasks.

Run everything serially and the User Interface gets locked whever a task is running.  Asynchronous tasks free up the UI process. The UI can interact with the user while tasks are running.

### Tasks, Threading, Scheduling, Contexts

It's very easy to be confused by the nomenclature.  Lets try and understand the fundimentals.

#### What's really going on under the Hood?

It doesn't matter where your code runs, locally or somewhere in the cloud, one processor and one core means one man on the job.  The operating system can multi-task just like a man (note - this task has nothing to do with DotNet Core tasks), but there's only one man do one thing at a time.  More parallel tasks, more time switching between tasks, less time doing real work - sound familiar! With four processors or four cores you get four men on the job. Without a supervisor, one man running code, three men telling him how to do it!

Now switch back to DotNet Core.  Operating system multi-tasking is exposed through threads (NOT tasks). Tasks are blocks of code packaged up and run on threads. TaskSchedulers organise and schedule Tasks.  They're basically state machines that keep track of execution order and yielding to organise efficient code execution.

All DotNet Core applications start on a single thread and all applications have a thread pool - nominally one per processor/core (out on the cloud though - only the cloud knows).  UI and web server applications have a dedicated application thread - with a Synchronisation Context to run the UI or HTTP/Web context.  This is the only thread with direct access to the UI/web context.  Console applications start and run on a threadpool thread. This behavioural difference has significant implications which we will discuss later. In this discussion AppThread equals main application thread. Tasks are loaded through SynchronisationContext.Post on the Synchronisation Context rather than directly onto the thread.

UI and Web Context Applications run everything on the Synchronisation Context unless coded otherwise.  It's the programmers responsibility to decide where to run tasks and switch them to the threadpool.  Once on the threadpool, tasks are managed by the threadpool.

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
    });
    code2_Async().ContinueWith(task => {
            dependant_code2_Sync;
    });
}
```
The expanded version shows what's really happening, with code after an await wrapped inside the ContinueWith code block.  The second await only gets called after the first Task completes, so the two are unrelated and independant.

If MethodAsync is called directly from the UI/Web Context, such as responding to a UI event, the tasks are loaded onto the Application Thread through the SynchronisationContext. All code runs on this thread.

Now let's take a look at a slightly different implementation:

```c#
private async Task MethodAsync(...)
{
    await Task.Run(() => code_Async);
    await code2_Async;
}
```

The first await creates a new Task via Task.Run(..).  We're now managing where the task gets run - Task.Run() specifically runs tasks on the threadpool. The second wait block is run on the calling method thread - probably the Application thread.

To ensure everything is run on the threadpool do something like this:

```c#
private async EventHandler(...) => await Task.Run(() => MethodAsync(...));

private async Task MethodAsync(...)
{
    await code_Async;
    await code2_Async;
}
```

To run multiple Tasks in parallel in the same method: create a Task List, and run it from a Task with *WhenAll()*:

```c#
private async Task MethodAsync(...)
{
    var tasks = new List<Task>();
    tasks.Add(code_Async);
    tasks.Add(code2_Async);
    await Task.WhenAll(tasks);
}
```

### Asnyc Coding Patterns

At the core of Async programming is the Task object.  All methods that run asynchronously return a Task: there is one except to this that we will cover shortly. Think of the Task as a wrapper object for a block of code. Internally it's like a state machine, kkep track of of execution order and yielding.  Externally it provides information about the state of the codeblock, a certain level of control, and exposes the return value on Task completion.

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

These patterns don't contain the async keyword, but can run asynchronously. They contain normal code, often a relatively long running calculation. The're also useful for declaring async methods in Interfaces and base classes.  The compiler complains if you label a method async that contains no awaits.

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
until complete. It should only be used as an event handler.


#### Blocking and Deadlocking

One of the biggest challenges you'll face is the Deadlock. Async code that either always locks, or locks under load. In Blazor, this manifests itself as a locked page. The lights are on but there's no one at home. You've killed the application process running your SPA instance. The only way out is to reload the page (F5).

The normal reason is blocking code - program execution on the application thread is halted waiting for a task to complete. The task is further down the code pipeline on the thread. The halt blocks execution of the code it's waiting on. Deadlock. If you move the task to the threadpool the task completes and the block unblocks. However, no UI updates happen and UI events go answered. Shifting code to the taskpool so you can block the application thread isn't the answer. Nor is blocking threadpool threads.  Under load you application may block all the threads available.

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

*Task.Wait()* and *task.Result* are blocking actions.  They stop execution on the thread and wait for *task* to complete. *Task* can't complete because the thread is blocked.

### Recommendations

1. **Async and Await All The Way**. Don't mix synchronous and asynchronous methods.  Start at the bottom - the data or process interface - and code async all the way up though the data and business/logic layers to the UI.  Blazor components implement both async and sync events, so there's no reason for sync if your base library provides async interfaces.  
2. Only assign processor intensive tasks to the threadpool.  Don't do it because you can.
3. Don't use *Task.Run()* in your libraries. in your libraries. Keep that decision as far up in the application code as poddible. Make your libraries context agnostic.  
4. Never block in your libraries.  Seems obvious but... if you absolutely must block do it in the front end.
5. Always use *Async* and *Await*, don't get fancy.
6. If your library provides both async and sync calls, code them separately.  "Code it once" best practice doesn't apply here.  NEVER call one from the other if you don't want to shoot yourself in the foot at some point!
7. Only use *Async Void* for Event Handlers.  Never use it anywhere else.


### Some Real World Examples

Lets look at some real world examples with a little more complexity and alternative ways of achieving the same result.

The example site and code repository are part of a larger project associated with several Blazor articles.

The site is [here on Azure](https://cec-blazor-server.azurewebsites.net/).

The code is available on Github at [CEC.Blazor Repository](https://github.com/ShaunCurtis/CEC.Blazor).

The code shown runs at [CEC.Blazor - Async](https://cec-blazor-server.azurewebsites.net/Async).

The page gets/calculates salaries.  It uses the Data/Controller/UI pattern for code segregation.  The Data layer runs as a Singleton Service, pullling data from a Entitiy Framework source.  The Business/Controller layer runs as a Transient Service.

###### Data Service

The code uses *Task.Delay* to simulate query latency and a sync *FirstOrDefault* to get the record from a local list object and looks like this:
```c#
public async Task<EmployeeSalaryRecord> GetEmployeeSalaryRecord(int EmployeeID)
{
    await Task.Delay(2000);
    return this.EmployeeSalaryRecords.FirstOrDefault(item => item.EmployeeID == EmployeeID);
}
```
where the live code would look something like this:
```c#
public async Task<EmployeeSalaryRecord> GetEmployeeSalaryRecord(int EmployeeID)
{
    return await mydatacontext.GetContext().EmployeeSalaryTable.FirstOrDefaultAsync(item => item.EmployeeID == EmployeeID);
}
```
Note the use of *FirstorDefaultAsync* to make the EF call, rather than the *FirstorDefault* sync version.

###### Controller (Business Layer) Service

The full code block looks like this: 

```c#
public async Task<decimal> GetEmployeeSalary(int employeeID, int egorating)
{
    this.Message = "Getting Employee record";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
    var rec = await this.SalaryDataService.GetEmployeeSalaryRecord(employeeID);
    if (rec.HasBonus)
    {
        this.Message = "Wow big bonus to calculate - this could take a while!";
        this.MessageChanged?.Invoke(this, EventArgs.Empty);
        var bonus = await Task.Run(() => this.CalculateBossesBonus(egorating));
        this.Message = "Overpaid git!";
        return rec.Salary + bonus;
    }
    else
    {
        this.Message = "You need a rise!";
        return rec?.Salary ?? 0m;
    }
}
```

The call into the data layer is:

```c#
var rec = await this.SalaryDataService.GetEmployeeSalaryRecord(employeeID);
```

Note the *await*.  While we're assigning the result to a local variable, we're awaiting (yielding control back to the calling method), not blocking. The code block below looks almost identical, but it's blocks - a NONO.

```c#
var rec = this.SalaryDataService.GetEmployeeSalaryRecord(employeeID).Result;
```

The second section in the method checks if a bonus calculation is required. The bonus calculation is processor intensive, so we offload it to another thread (otherwise the UI may get a little sluggish). We use *Task.Run* to switch threads/context.

```c#
var bonus = await Task.Run(() => this.CalculateBossesBonus(egorating));
```

The blocker task looks like this (note it doesn't block, it's the caller in the UI code that blocks):

```c#
public async Task<bool> BlockerTask()
{
    this.Message = "That's blown it.  F5 to get out of this one.";
    this.MessageChanged?.Invoke(this, EventArgs.Empty);
    await Task.Delay(1000);
    return true;
}
```

The messaging stuff updates the display message and tells the UI it needs to update through the MessageChanged event.

```c#
this.Message = "Getting Employee record";
this.MessageChanged?.Invoke(this, EventArgs.Empty);
```

###### UI Code
  
Button clicks are wired up two event handlers in the UI Code - *ButtonClicked* and *UnsafeButtonClicked*.  Both event handlers use the correct async event hanlder pattern - *async void*.  The non blocking code looks like this:
```c#
public async void ButtonClicked(int employeeID)
{
    .....
    this.Salary = await this.SalaryControllerService.GetEmployeeSalary(employeeID, 3);
    ...
}
```

While the blocking code looks like this:

```c#
public async void ButtonClicked(int employeeID)
{
    .....
    var x = this.SalaryControllerService.BlockerTask().Result;
    ...
}
```
Other points to not in the Index.razor code:
1. There's some quick and dirty RenderTreeBuilder code to make fancy activity aware buttons.
2. *StateHasChanged()* is called via InvokeAsync.  This makes sure it's executed by the Dispatcher on the correct application thread.
3. Dependency Injection for the Controller Service.
4. The use of the Controller Service EventHandler to control component UI updating.

Note that all the methods, from library (EF) to the UI events, are async functions with awaits.  Async All The Way.

That's it.  Please drop comments to tell me what I've got wrong, describe inaccurately, etc.  And a big thanks to those responsible for the articles listed below, and the many other nuggets of information that go unrecognised.


### A Note on the Demo Project Code and Site

It's the demo site for a series of articles on building Blazor Applications.  This is the first.  It expands on the basic Weather Forecast Blazor 
site demonstrating a set of methodologies, patterns and practices for building real world Blazor Server projects.   Many sections of code are still "Under Development" and need serious improvement and refactoring.

#### Useful Resources and Sources of My Knowledge
[Async Programming - Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/async#:~:text=C%23%20has%20a%20language-level%20asynchronous%20programming%20model%20which,is%20known%20as%20the%20Task-based%20Asynchronous%20Pattern%20%28TAP%29.)

[Stephen Cleary - A Tour of Task and other articles](https://blog.stephencleary.com/2014/04/a-tour-of-task-part-0-overview.html)

[Eke Peter - Understanding Async, Avoiding Deadlocks in C#](https://medium.com/rubrikkgroup/understanding-async-avoiding-deadlocks-e41f8f2c6f5d)

[Stephen Cleary - MSDN - Best Practices in Asynchronous Programming](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

Many StackOverflow Answers to Questions

