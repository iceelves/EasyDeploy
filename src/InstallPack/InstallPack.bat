@ECHO OFF

:: 应用变量
set ProjectPath=..\EasyDeploy.sln
set ReleasePath=..\EasyDeploy\bin\Release

:: 删除生成文件夹
rd/s/q %ReleasePath%

:: nuget 引用
nuget restore %ProjectPath%

:: 编译代码
MSBuild %ProjectPath% /t:Publish /p:Configuration=Release /p:PublishProfile=FolderProfileX86.pubxml
MSBuild %ProjectPath% /t:Publish /p:Configuration=Release /p:PublishProfile=FolderProfileX64.pubxml

:: 打包
makensis.exe /V4 /DProcessorArchitecture=x86 EasyDeploySetup.nsi
makensis.exe /V4 /DProcessorArchitecture=x64 EasyDeploySetup.nsi

pause
exit
