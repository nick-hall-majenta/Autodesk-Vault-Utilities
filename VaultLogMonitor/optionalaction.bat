:
: Vault Log Monitor
: Version 1.0
: (c) Majenta Solutions 2011
:
: This software is provided "as is" and any expressed or implied warranties,
: including, but not limited to, the implied warranties of merchantability and
: fitness for a particular purpose are disclaimed. in no event shall the 
: regents or contributors be liable for any direct, indirect, incidental, 
: special, exemplary, or consequential damages (including, but not limited to,
: procurement of substitute goods or services; loss of use, data, or profits;
: or business interruption) however caused and on any theory of liability,
: whether in contract, strict liability, or tort (including negligence or
: otherwise) arising in any way out of the use of this software, even if
: advised of the possibility of such damage.

@echo off
set NET=c:\windows\SysWOW64\net.exe
%NET% STOP "Autodesk Data Management Job Dispatch"
%NET% STOP "MSSQL$AUTODESKVAULT"
%NET% STOP "W3SVC"
%NET% START "W3SVC"
%NET% START "MSSQL$AUTODESKVAULT"
%NET% START "Autodesk Data Management Job Dispatch"
