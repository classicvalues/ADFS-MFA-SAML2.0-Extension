﻿###########################################################################
# Copyright 2017 SURFnet bv, The Netherlands
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
###########################################################################

$ErrorActionPreference = "Stop"

function Remove-EventLogForMfaPlugin {
    Param(
        [Parameter(Mandatory = $false)]
        [string]$LogName = "AD FS Plugin"
    )

    $logFileExists = Get-EventLog -list | Where-Object { $_.logdisplayname -eq $LogName } 
    if ($logFileExists) {
        Write-Host -ForegroundColor Green "Removing eventlog '$LogName'"		
        Remove-EventLog -LogName $LogName
    }
    else {
        Write-Host -ForegroundColor DarkYellow "Eventlog '$LogName' already removed in another uninstallation."
    }
}