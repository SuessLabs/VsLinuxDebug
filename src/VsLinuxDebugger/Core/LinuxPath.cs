//-----------------------------------------------------------------------------
// FILE:	    LinuxPath.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// References:
//  - https://github.com/nforgeio/neonKUBE/blob/master/Lib/Neon.Common/IO/LinuxPath.cs
//  - https://github.com/nforgeio/neonKUBE/blob/master/Lib/Neon.Common/Diagnostics/Covenant.cs

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VsLinuxDebugger.Core
{
  /// <summary>
  /// Implements functionality much like <see cref="Path"/>, except for
  /// this class is oriented towards handling Linux-style paths on
  /// a remote (possibly a Windows) machine.
  /// </summary>
  public static class LinuxPath
  {
    /// <summary>
    /// Changes the file extension.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="extension">The new extension.</param>
    /// <returns>The modified path.</returns>
    public static string ChangeExtension(string path, string extension)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(extension), nameof(extension));

      return Path.ChangeExtension(Normalize(path), extension).ToLinux();
    }

    /// <summary>
    /// Combines an array of strings into a path.
    /// </summary>
    /// <param name="paths">The paths.</param>
    /// <returns>The combined paths.</returns>
    public static string Combine(params string[] paths)
    {
      Covenant.Requires<ArgumentNullException>(paths != null, nameof(paths));

      return Path.Combine(paths).ToLinux();
    }

    /// <summary>
    /// Extracts the directory portion of a path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The directory portion.</returns>
    public static string GetDirectoryName(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Path.GetDirectoryName(Normalize(path)).ToLinux();
    }

    /// <summary>
    /// Returns the file extension from a path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file extension.</returns>
    public static string GetExtension(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Path.GetExtension(Normalize(path));
    }

    /// <summary>
    /// Returns the file name and extension from a path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file name and extension.</returns>
    public static string GetFileName(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Path.GetFileName(Normalize(path));
    }

    /// <summary>
    /// Returns the file name from a path without the extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The file name without the extension.</returns>
    public static string GetFileNameWithoutExtension(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Path.GetFileNameWithoutExtension(Normalize(path));
    }

    /// <summary>
    /// Determines whether a path has a file extension.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> if the path has an extension.</returns>
    public static bool HasExtension(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Path.HasExtension(Normalize(path));
    }

    /// <summary>
    /// Determines whether the path is rooted.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns><c>true</c> ifc the path is rooted.</returns>
    public static bool IsPathRooted(string path)
    {
      Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

      return Normalize(path).ToLinux().StartsWith("/");
    }

    /// <summary>
    /// Ensures that the path passed is suitable for non-Windows platforms
    /// by conmverting any backslashes to forward slashes.
    /// </summary>
    /// <param name="path">The input path (or <c>null</c>).</param>
    /// <returns>The normalized path.</returns>
    private static string Normalize(string path)
    {
      if (path == null || RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        return path;
      }

      return path.Replace('\\', '/');
    }

    /// <summary>
    /// Converts a Windows style path to Linux.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The converted path.</returns>
    private static string ToLinux(this string path)
    {
      return path.Replace('\\', '/');
    }

    /// <summary>A simple, lightweight, and partial implementation of the Microsoft Dev Labs <c>Contract</c> class.</summary>
    /// <remarks>
    ///   <para>
    ///     This class is intended to be a drop-in replacement for code contract assertions by simply
    ///     searching and replacing <b>"Contract."</b> with "<see cref="Covenant"/>." in all source code.
    ///     In my experience, code contracts slow down build times too much and often obsfucate
    ///     <c>async</c> methods such that they cannot be debugged effectively using the debugger.
    ///     Code Contracts are also somewhat of a pain to configure as project propoerties.
    ///   </para>
    ///   <para>
    ///     This class includes the <see cref="Requires(bool, string)"/>, <see cref="Requires{TException}(bool, string, string)"/>
    ///     and <see cref="Assert(bool, string)"/> methods that can be used to capture validation
    ///     requirements in code, but these methods don't currently generate any code.
    ///   </para>
    /// </remarks>
    private class Covenant
    {
      private static Type[] _oneStringArg = new Type[] { typeof(string) };
      private static Type[] _twoStringArgs = new Type[] { typeof(string), typeof(string) };

      /// <summary>
      /// Verifies a method pre-condition throwing a custom exception.
      /// </summary>
      /// <typeparam name="TException">The exception to be thrown if the condition is <c>false</c>.</typeparam>
      /// <param name="condition">The condition to be tested.</param>
      /// <param name="arg1">The first optional string argument to the exception constructor.</param>
      /// <param name="arg2">The second optional string argument to the exception constructor.</param>
      /// <remarks>
      /// <para>
      /// This method throws a <typeparamref name="TException"/> instance when <paramref name="condition"/>
      /// is <c>false</c>.  Up to two string arguments may be passed to the exception constructor when an
      /// appropriate constructor exists, otherwise these arguments will be ignored.
      /// </para>
      /// </remarks>
      public static void Requires<TException>(bool condition, string arg1 = null, string arg2 = null)
          where TException : Exception, new()
      {
        if (condition)
        {
          return;
        }

        var exceptionType = typeof(TException);

        // Look for a constructor with two string parameters.

        var constructor = exceptionType.GetConstructor(_twoStringArgs);

        if (constructor != null)
        {
          throw (Exception)constructor.Invoke(new object[] { arg1, arg2 });
        }

        // Look for a constructor with one string parameter.

        constructor = exceptionType.GetConstructor(_oneStringArg);

        if (constructor != null)
        {
          throw (Exception)constructor.Invoke(new object[] { arg1 });
        }

        // Fall back to the default constructor.

        throw new TException();
      }
    }
  }
}
