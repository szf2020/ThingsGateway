cd ..
sc create ThingsGatewayManagementServer binPath= %~dp0ThingsGateway.ManagementServer.exe start= auto 
sc description ThingsGatewayManagementServer "ThingsGatewayManagementServer"
Net Start ThingsGatewayManagementServer
pause
