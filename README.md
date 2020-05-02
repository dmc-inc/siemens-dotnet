# siemens-dotnet
A C# library for interfacing with various Siemens software components


## Version 2.0 is coming!
The code from the [first release](https://github.com/dmc-inc/siemens-dotnet/releases/tag/1.0.0) was initially migrated from an internal library that was made open source. The initial goals of this project were very broad and far-reaching. Since then, reality has set in. This project does not need to automate every single task that could ever be imagined with a Siemens PLC project. It should stick to what it's good at and be architected to do that and only that.

So what does this mean? A rewrite is coming! Yes, a ground-up restructuring with every breaking change you could possibly imagine. We are excited about the future.

Short-term goals:
* Create a concise and efficient code architecture
* Use templating to easily handle different Portal versions
* Generate a NuGet package that can be used for releases
* Increase external contributions

Long-term goals:
* Add testing coverage
* Allow for localization of entire code base