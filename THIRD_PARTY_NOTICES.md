# Third-party notices

SmartBotAPI is distributed under the GNU General Public License v3.0, except
for third-party material and dependencies governed by the terms listed below.
Copyrights and trademarks remain the property of their respective owners.

## Material included in this repository

### SparkFun VL53L5CX example

`other/arduino_alpha/arduino_alpha.ino` is based on an example by Nathan
Seidle of SparkFun Electronics dated October 26, 2021. It remains available
under the MIT License included in `other/arduino_alpha/LICENSE`.

### Adafruit TB6612 schematic

`docs/adafruit_tb6612_schem.png` is a local copy of
**“adafruit_products_schem.png”** by **lady ada / Adafruit Industries**. It is
licensed under the [Creative Commons Attribution-ShareAlike 3.0 Unported License](https://creativecommons.org/licenses/by-sa/3.0/).
The repository copy uses a descriptive filename.

Source: [Adafruit Learning System asset 24267](https://learn.adafruit.com/assets/24267).

### Bootstrap 5.1.0

`src/server/SmartBotBlazorApp/wwwroot/bootstrap/bootstrap.min.css` and its
`bootstrap.min.css.map` source map contain Bootstrap 5.1.0, copyright
2011–2021 the Bootstrap Authors and Twitter, Inc., licensed under the
[MIT License](https://github.com/twbs/bootstrap/blob/v5.1.0/LICENSE).
The upstream copyright and license header is retained in the CSS file.

### “New Direction” music

`other/media/smartbot-driving-demo-music.mp4` contains an edited excerpt of
**“New Direction” by Kevin MacLeod (incompetech.com)**. The track is licensed
under the [Creative Commons Attribution 4.0 International License](https://creativecommons.org/licenses/by/4.0/).

Source: [Incompetech track page](https://incompetech.com/music/royalty-free/index.html?Search=Search&isrc=USUAN1100677).

Changes: the track was trimmed and mixed with the SmartBot driving-test montage.

## .NET and NuGet dependencies

NuGet packages retain their upstream license files and metadata. The direct
dependencies declared by the server, client, and test projects, together with
the transitive dependency carrying nonstandard Microsoft terms, are:

| Package or package group | Version | License |
|---|---:|---|
| Azure.Identity | 1.13.1 | [MIT](https://licenses.nuget.org/MIT) |
| Microsoft.AspNetCore.Components.WebAssembly, .Authentication, .Server; Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore; Microsoft.AspNetCore.Identity.EntityFrameworkCore; Microsoft.AspNetCore.SignalR.Client | 8.0.10 | [MIT](https://licenses.nuget.org/MIT) |
| Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Tools | 9.0.0 | [MIT](https://licenses.nuget.org/MIT) |
| MudBlazor | 7.15.0 | [MIT](https://licenses.nuget.org/MIT) |
| Newtonsoft.Json | 13.0.3 | [MIT](https://licenses.nuget.org/MIT) |
| System.Formats.Asn1 | 9.0.0 | [MIT](https://licenses.nuget.org/MIT) |
| SixLabors.ImageSharp | 3.1.12 | Apache-2.0 grant for qualifying open-source use under the [Six Labors Split License 1.0](https://sixlabors.com/posts/license-changes/) |
| Microsoft.Data.SqlClient.SNI.runtime (transitive runtime component) | 5.1.1 | [Microsoft Software License Terms](https://www.nuget.org/packages/Microsoft.Data.SqlClient.SNI.runtime/5.1.1) |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets (build tooling) | 1.21.0 | [Microsoft Software License Terms](https://www.nuget.org/packages/Microsoft.VisualStudio.Azure.Containers.Tools.Targets/1.21.0) |
| coverlet.collector, Microsoft.NET.Test.Sdk | 10.0.1, 18.7.0 | [MIT](https://licenses.nuget.org/MIT) |
| xunit, xunit.runner.visualstudio | 2.9.3, 3.1.5 | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) |

The restore verified on July 15, 2026 resolves 144 unique package/version pairs
across the server, client, and test projects. Their NuGet metadata consists of
125 MIT license expressions, 7 Apache-2.0 expressions, 9 legacy upstream
license URLs, and 3 embedded license files. The three embedded licenses are the
SNI runtime, ImageSharp, and the Visual Studio container build tooling listed
above; the legacy URLs belong to Microsoft/.NET Core packages and
`xunit.abstractions`. This accounts for the complete restored graph at that
date. Exact transitive versions can change after a future restore, so each
package's `.nuspec` metadata and included license file remain authoritative.

## Firmware libraries installed separately

The Arduino libraries are dependencies installed through Arduino Library
Manager; their source code is not vendored in this repository.

| Library | License |
|---|---|
| ArduinoJson | [MIT](https://github.com/bblanchon/ArduinoJson/blob/master/LICENSE.txt) |
| arduinoWebSockets by Markus Sattler | [LGPL-2.1](https://github.com/Links2004/arduinoWebSockets/blob/master/LICENSE) |
| SparkFun VL53L5CX Arduino Library | [Upstream license](https://github.com/sparkfun/SparkFun_VL53L5CX_Arduino_Library/blob/main/LICENSE.md) |
| Adafruit MPU6050 | [BSD](https://github.com/adafruit/Adafruit_MPU6050/blob/master/license.txt) |
| Adafruit Unified Sensor | [Apache-2.0](https://github.com/adafruit/Adafruit_Sensor/blob/master/LICENSE.txt) |

## Externally loaded assets and trademarks

The web application loads Font Awesome Free 6.0.0-beta3 from cdnjs. Font
Awesome Free uses CC BY 4.0 for icons, SIL OFL 1.1 for fonts, and MIT for code;
see the [Font Awesome Free License](https://fontawesome.com/license/free).

`src/server/SmartBotBlazorApp/wwwroot/GitHub_Logo.png` is used only to link
to the SmartBotAPI repository. GitHub and the GitHub logo are trademarks of
GitHub, Inc.; use is subject to the [GitHub Logo Policy](https://docs.github.com/en/site-policy/other-site-policies/github-logo-policy).

The Akademia Tarnowska logo is loaded from the institution's website and links
back to it. The name and logo belong to Akademia Tarnowska. Their appearance
does not imply endorsement of this repository.
