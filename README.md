# Easy Deploy （轻松部署）

## Project description （项目描述）
在实际项目应用中，有很多的应用由于不需要 UI 界面，所以简便的使用控制台程序开发，而控制台程序在实际部署的时候，又存在很多问题，例如图标使用开发语言默认样式，或使用控制台默认图标，导致程序异常崩溃后不易排查缺少了哪些，或是需要配置开机自启，统一管理更容易一些。或是像 [XAMPP](https://www.apachefriends.org/index.html) 一样部署一些服务程序。当然包含 UI 的程序也可以部署在上边，但是似乎只能当成快捷启动器一样使用，或是当成看门狗程序一样保证程序始终运行，崩溃时自动重启。

In actual project applications, many applications do not need UI interface, so it is easy to use the console program for development. However, when the console program is actually deployed, there are many problems. For example, the icon uses the default style of the development language, or uses the default icon of the console, which makes it difficult to check what is missing after the program crashes abnormally, or it is easier to configure startup and self startup for unified management. Or like [XAMPP](https://www.apachefriends.org/index.html)Deploy some service programs like. Of course, the program containing UI can also be deployed on it, but it seems that it can only be used as a quick launcher, or as a watchdog program to ensure that the program always runs and automatically restarts when it crashes.

## MainWindow 主窗体
<img src="/images/MainWindow.jpg"/>

## Supported file types 支持文件类型
默认支持可执行文件(*.exe) 与批处理文件(*.bat) 执行。
同时也支持脚本文件，例如：Python(*.py)、Powershell(*.ps1)。
### Collocation method 配置方法
#### Executable file 可执行文件(*.exe)
| 类型 | 内容 |
| --- | --- |
| Service Name | 自定义服务名称 |
| Service Path | 选择 *.exe 文件 |
| Service Path Type | 以绝对路径或以相对路径保存 |
| Parameter | 应用程序参数 |

#### Batch file 批处理文件(*.bat)
| 类型 | 内容 |
| --- | --- |
| Service Name | 自定义服务名称 |
| Service Path | 选择 *.bat 文件 |
| Service Path Type | 以绝对路径或以相对路径保存 |
| Parameter | 无应用程序参数 |

#### Python script(*.py)
| 类型 | 内容 |
| --- | --- |
| Service Name | 自定义服务名称 |
| Service Path | 固定内容：python |
| Service Path Type | 任意 |
| Parameter | Python 脚本文件路径 |

#### Powershell script(*.ps1)
| 类型 | 内容 |
| --- | --- |
| Service Name | 自定义服务名称 |
| Service Path | 固定内容：powershell |
| Service Path Type | 任意 |
| Parameter | Powershell 脚本文件路径 |