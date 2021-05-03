# System.AppDomain

[![NuGet](https://img.shields.io/nuget/v/System.AppDomain.svg)](https://www.nuget.org/packages/System.AppDomain/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

Exposes the hidden System.AppDomain in .NET Core for version 1.x.

## Usage

While not every member is here, every member that is exposed via the API surface
will work as it would with the "real" `System.AppDomain`.

Most members that are not here are not in the .NET Core `AppDomain` so I cannot
expose them.

There are reasons to not use this. The biggest reason not to use this package is
that there either is a correct/new way to do something that you can do using
this package or that there should be. For example this library gives you access
to `System.AppDomain.UnhandledExcption`. While this is a useful event there
should be a way to get this information without resorting to reflection. As such
before using this library make sure there isn't a new way in .NET Core to do
what you want to do.

The other reason is this library uses reflection meaning it's not the fastest,
though I've done my best to deal with that. This also means it will not work in
UWP.

## Implementation

Internally **A LOT** of reflection is going on, making use of types in
`System.Reflection` and `System.Linq.Expressions`.

## Related Issues

Created By: [![Twitter
Follow](https://img.shields.io/twitter/follow/shmuelie.svg?style=social&label=Shmuelie)](https://www.twitter.com/shmuelie)