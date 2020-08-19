# Building a Blazor Project
# Part 3 - The UI

In the first two articles I looked at project structure and implementing the data layers for a project.  In this article, I'll look at how I implement the Presentation Layer.  I use Bootstrap for my UI design, so I'm splitting this section into two articles.  This article covers building the data components.  The second article covers a structured approach to building the actual HTML components with Bootstrap.

### The Component

The Blazor UI is based on the component implemented as the base class *ComponentBase*.  Everything inherits from this class.  Add a *.razor* file and by default it inherits from *ComponentBase*.

It's hard to get away from, but you need to forget the classic concept of a page in Razor.  The only real page is  *_host.chtml* which is loaded when a Blazor SPA [Single Page Application] starts.  After that, it's all routing, no browser navigation.  Components get loaded into and out of *_host.chtml* as the user gets routed around the application.  Everything beyond the initial page load is DOM manipulation.

Think of two types of component:
1. Components
2. Routed Components

The only difference is Routed Components contain *@page* routing directives and can contain a *@Layout* directive.

```html
@page "/WeatherForecast"
@page "/WeatherForecasts"

@layout MainLayout
```

Routed Components are loaded into the *app* slot in the *_host.chtml* using either the specified Layout or the default Layout defined in *App.razor*.

```html
<body>
    <app>
        <component type="typeof(App)" render-mode="ServerPrerendered" />
    </app>
    .....
</body>
```
Note here the *render-mode*, which in a Blazor Server App is normally set to *ServerPrerendered*.  We'll look at the implications of this later in the Page lifecycle section.

Normal components load where they are declared.

```html
<WeatherList UIOptions="this.UIOptions" ></WeatherList>
```
