@ECHO OFF

:: 应用变量
set ProjectPath=..\EasyDeploy.sln
set AppVersion=netcoreapp3.1
set ReleasePath=..\EasyDeploy\bin\Release

:: 删除生成文件夹
rd/s/q %ReleasePath%

:: nuget 引用
nuget restore %ProjectPath%

:: 编译代码
MSBuild %ProjectPath% /property:Configuration=Release

:: 删除多余文件
del /s %ReleasePath%\%AppVersion%\*.pdb
del /s %ReleasePath%\%AppVersion%\*.xml
del /s %ReleasePath%\%AppVersion%\*.txt

:: 打包
makensis.exe EasyDeploySetup.nsi

pause
exit
