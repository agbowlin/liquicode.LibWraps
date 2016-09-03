

# liquicode.LibWraps.Net

A collection of .NET wrappers for other (3rd party) libraries.

The LibWraps project is designed to support, and make accessible,
the most commonly used functionality found in other 3rd party libraries.
The wrappers are very lightweight and provide a simpler facade to the
complexities of an underlying library.

Note that, in order to develop with some of the projects, you will need to have
a reference to the underlying library in addition to the `LibWraps` library.
This is because a lightweight wrapper may expose underlying types as function
parameters or function return values. 

All projects exist in the `liquicode.LibWraps` namespace and each exports a
single static class that is used to invoke underlying functionality (e.g.
`Wraps_Ghostscript`).
 
The LibWraps source code can be used for ideas on how to interact with various
libraries. The NUnit test project(s) (when I make them) can be used to for
ideas on how to interact with the LibWraps objects.

For convenience, a copy of the 3rd party distribution files are located under
the `_references` folder. This makes it easy for you to download, compile, and
work with `LibWraps` without having to track down all of the dependent
distributions yourself. You are, however, encouraged to do so for these reasons:

- Update the underlying code base.
- Resolve issues when working with `LibWraps` and the underlying libraries.
- Gain insight into other library features that are not in `LibWraps`.
- Implement other library features that are not in `LibWraps`.
- To generally honor those who have worked so hard to bring you the coolness.

Every effort is made to give credit to individual authors and to clearly
identify who wrote which code. Please let me know if I have made any errors or
omissions. My goal is to illuminate rather than obfuscate.


# liquicode.LibWraps Projects

------------------------------------------

### liquicode.LibWraps.Wraps_NTwain.Net35

#### About

This project wraps the NTwain API found in the soukoku-ntwain project available
here:
 
- [https://bitbucket.org/soukoku/ntwain](https://bitbucket.org/soukoku/ntwain)
- [https://bitbucket.org/soukoku/ntwain/downloads](https://bitbucket.org/soukoku/ntwain/downloads)

See also: [_references/NTwain/NTwain.md](_references/NTwain/NTwain.md)

Functionality in this library is accessed via the static class
`Wraps_NTwain`.

#### Purpose

NTwain is a wrapper around the Windows Twain API. (So, `Wraps_NTwain` is a
wrapper of a wrapper). I decided to wrap this library in order to expose the
primary functionality of capturing images from scanners and cameras. There is a
bit of Twain engine logic going on behind the scenes which you don't need to
worry about if all you want to do is scan a document.  

#### Deploying

This project can be deployed with the traditional XCopy deployment approach.

This project requires a minimum .NET version of 2.0.

You will need to deploy these files:

- liquicode.LibWraps.Wraps_NTwain.Net35.dll
- NTwain.dll

------------------------------------------

### liquicode.LibWraps.Wraps_Ghostscript.Net20.xxx

#### About

This section related to both of these `LibWraps` projects:

- liquicode.LibWraps.Wraps_Ghostscript.Net20.x86
- liquicode.LibWraps.Wraps_Ghostscript.Net20.x64

These projects provide a 32-bit and 64-bit wrapper for the Ghostscript DLL:

- [http://ghostscript.com/download/gsdnld.html](http://ghostscript.com/download/gsdnld.html)

See also: [_references/Ghostscript/Ghostscript.md](_references/Ghostscript/Ghostscript.md)

Functionality in this library is accessed via the static class
`Wraps_Ghostscript`.

#### Purpose

The Ghostscript DLL uses a command-line like invocation which can be a little
awkward to use. The `Wraps_Ghostcript` class makes it easier to do a few simple
things with the Ghostscript DLL.

#### Deploying

This project can be deployed with the traditional XCopy deployment approach.

This project requires a minimum .NET version of 3.5.

You will need to deploy these files:

- For 32-bit deployments:
	- liquicode.LibWraps.Wraps_Ghostscript.Net20.x86.dll
	- gsdll32.dll
- For 64-bit deployments:
	- liquicode.LibWraps.Wraps_Ghostscript.Net20.x64.dll
	- gsdll64.dll

