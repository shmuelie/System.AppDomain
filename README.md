# DO NOT USE UNLESS YOU HAVE NO OTHER CHOICE! THINK ABOUT IT!
Seriously, using this library is usually a sign that something (\*cough\*dotnetcore\*cough\*) is wrong.

# System.AppDomain
[![NuGet](https://img.shields.io/nuget/v/System.AppDomain.svg)](https://www.nuget.org/packages/System.AppDomain/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

Exposes the hidden System.AppDomain in .NET Core

## Usage
While not every member is here, every member that is exposed via the API surface will work as it would with the "real" `System.AppDomain`.

Most members that are not here are not in the .NET Core `AppDomain` so I cannot expose them.

## Implementation
Internally **A LOT** of reflection is going on, making use of types in `System.Reflection` and `System.Linq.Expressions`.

## Related Issues
[![GitHub Issue](https://img.shields.io/badge/corefx-6398-yellow.svg)](https://github.com/dotnet/corefx/issues/6398)

Created By: [![Twitter Follow](https://img.shields.io/twitter/follow/shmuelie.svg?style=social&label=Shmuelie)](https://www.twitter.com/shmuelie)