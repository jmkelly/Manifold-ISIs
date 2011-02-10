// This software is provided 'as-is', without any express or implied warranty.
// In no event will the author be held liable for any damages arising from the
// use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented. If you use the
// software in a product, an acknowledgement in the product documentation would
// be appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not be
// misrepresented as being the original software.
//
// 3. This notice may not be removed or altered in any source distribution.

using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyProduct("Manifold Geocoding Server for Geocoder.US")]
[assembly: AssemblyTitle("Manifold Geocoding Server for Geocoder.US")]
[assembly: AssemblyVersion("4.0.0.0")]
[assembly: ComVisible(false)]

// permissions
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true, UnmanagedCode = true)]
[assembly: WebPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
