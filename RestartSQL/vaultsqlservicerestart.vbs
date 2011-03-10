option explicit

Dim strComputer, strServiceName
strComputer = "." ' Local Computer
strServiceName = "MSSQL$AUTODESKVAULT"

if Not isServiceStopped(strComputer,strServiceName) then
	wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)
	wscript.echo "Stopping '" & strServiceName & "' service"
	iServiceStop strComputer,strServiceName
	Do
		wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)
		wscript.sleep(5000)
	Loop Until isServiceStopped(strComputer,strServiceName)
	wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)
else
	wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)
end if

wscript.echo "Restarting '" & strServiceName & "' service"
iServiceStart strComputer,strServiceName
Do
	wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)
	wscript.sleep(5000)
Loop Until isServiceRunning(strComputer,strServiceName)
wscript.echo "The '" & strServiceName & "' service is " & sServiceState(strComputer,strServiceName)


function isServiceRunning(strComputer,strServiceName)
	Dim objWMIService, strWMIQuery
	strWMIQuery = "Select * from Win32_Service Where Name = '" & strServiceName & "' and state='Running'"
	Set objWMIService = GetObject("winmgmts:" _
		& "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
	if objWMIService.ExecQuery(strWMIQuery).Count > 0 then
		isServiceRunning = true
	else
		isServiceRunning = false
	end if
end function

function isServiceStopped(strComputer,strServiceName)
	Dim objWMIService, strWMIQuery
	strWMIQuery = "Select * from Win32_Service Where Name = '" & strServiceName & "' and state='Stopped'"
	Set objWMIService = GetObject("winmgmts:" _
		& "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
	if objWMIService.ExecQuery(strWMIQuery).Count > 0 then
		isServiceStopped = true
	else
		isServiceStopped = false
	end if
end function

function sServiceState(strComputer,strServiceName)
	Dim objWMIService, strWMIQuery, colServices, Service
	strWMIQuery = "Select * from Win32_Service Where Name = '" & strServiceName & "'"
	Set objWMIService = GetObject("winmgmts:" _
		& "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
	Set colServices = objWMIService.ExecQuery(strWMIQuery)
	if colServices.Count > 0 then
		For Each Service in colServices
			sServiceState = Service.State
		Next
	else
		sServiceState = "N/A"
	end if
end function

function iServiceStart(strComputer,strServiceName)
	Dim objWMIService, colListOfServices, objService
	Set objWMIService = GetObject("winmgmts:" _
		& "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
	Set colListOfServices = objWMIService.ExecQuery ("Select * from Win32_Service Where Name ='" & strServiceName & "'")
	For Each objService in colListOfServices
		objService.StartService()
	Next
	iServiceStart = 1
end function

function iServiceStop(strComputer,strServiceName)
	Dim objWMIService, colListOfServices, objService
	Set objWMIService = GetObject("winmgmts:" _
		& "{impersonationLevel=impersonate}!\\" & strComputer & "\root\cimv2")
	Set colListOfServices = objWMIService.ExecQuery ("Select * from Win32_Service Where Name ='" & strServiceName & "'")
	For Each objService in colListOfServices
		objService.StopService()
	Next
	iServiceStop = 1
end function

