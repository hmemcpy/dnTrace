24/02/2016 09:08:24 g# dnTrace
Trace any method in any assembly in any (live) .NET process!

Here's what it does: attaching to a live .NET process (e.g. a [GUI written in Visual Basic, to track the killer's IP!](https://www.youtube.com/watch?v=hkDD03yeLnU)), and tracing an `OnClick` method of `System.Windows.Forms.Button`.

The output is a method call with captured parameter and return values (when available).

![](https://i.imgur.com/wVFSmK9.gif)

<hr/>

<img src="http://getcodecop.com/content/img/photo.png" style="float: right; padding: 0 5"/>dnTrace is powered by [CodeCop](http://getcodecop.com/) - a library that allows intercepting any method in .NET, using external JSON configuration or a Fluent API.

Note: CodeCop is a commercial library with a free version that allows tracing up to 25 methods per session. If you require more, you could [purchase a license](http://getcodecop.com/#pricing) to CodeCop, and apply it to dnTrace.

Please visit http://getcodecop.com for more information.