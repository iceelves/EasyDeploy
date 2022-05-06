@ECHO OFF

:: 应用变量
set ProjectPath=..\EasyDeploy.sln
set AppVersion=netcoreapp3.1
set ReleasePath=..\EasyDeploy\bin\Publish

:: 删除生成文件夹
rd/s/q %ReleasePath%

:: nuget 引用
nuget restore %ProjectPath%

:: 编译代码
MSBuild %ProjectPath% /t:Build /t:Publish /p:PublishProfile=FolderProfile.pubxml

:: 打包
makensis.exe EasyDeploySetup.nsi

pause
exit
