# dnTrace
Trace any method in any assembly in any (live) .NET process!

Here's what it does: attaching to a live .NET process (e.g. a [GUI written in Visual Basic, to track the killer's IP!](https://www.youtube.com/watch?v=hkDD03yeLnU)), and tracing an `OnClick` method of `System.Windows.Forms.Button`.

The output is a method call with captured parameter and return values (when available).

![](https://i.imgur.com/wVFSmK9.gif)

## On the shoulders of giants

<img src="http://getcodecop.com/content/img/photo.png" align="right" />dnTrace is powered by [CodeCop](http://getcodecop.com/) - a library that allows intercepting any method in .NET, using external JSON configuration or a Fluent API.

Note: CodeCop is a commercial library with a free version that allows tracing up to 25 methods per session. If you require more, you could [purchase a license](http://getcodecop.com/#pricing) to CodeCop, and apply it to dnTrace.

Please visit http://getcodecop.com for more information.

## Acknowledgments

This tool wouldn't have been possible without the use of such wonderful libraries as:

* [CodeCop](http://getcodecop.com/) - method interception and runtime instrumentation
* [dnlib](https://github.com/0xd4d/dnlib) - a .NET assembly reader/writer library
* [pinvoke](https://github.com/AArnott/pinvoke) - a collection of libraries that contain P/Invoke signatures
* [Colorful.Console](http://colorfulconsole.com/) - a `System.Console` wrapper with <font color="red">c</font><font color="orange">o</font><font color="cantaloupe">l</font><font color="green">o</font><font color="cyan">r</font><font color="blue">f</font><font color="darkblue">u</font><font color="purple">l</font> output
* [CommandLineParser](https://github.com/gsscoder/commandline) - a command-line parsing library
