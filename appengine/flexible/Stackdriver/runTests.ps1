# Copyright(c) 2017 Google Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License"); you may not
# use this file except in compliance with the License. You may obtain a copy of
# the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
# WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
# License for the specific language governing permissions and limitations under
# the License.

Import-Module -DisableNameChecking ..\..\..\BuildTools.psm1

dotnet restore
BackupAndEdit-TextFile "appsettings.json" `
    @{"your-google-project-id" = $env:GOOGLE_PROJECT_ID} `
{
	dotnet build
	$before = Get-Date
	Run-KestrelTest 5582
	$after = Get-Date
	# Wait for 1.5 minutes for the log entry to arrive.
	$count = 30
	while ($true) {
		$log = Get-GcLogEntry -Project $env:GOOGLE_PROJECT_ID `
			-LogName testStackdriverLogging -Before $after -After $before
		if ($log) { break }
		$count -= 1
		if ($count -le 0) {
			throw "Failed to find log entry."
		}
		Start-Sleep 3
	}
}