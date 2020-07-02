ProjectTemplate
===============

asp.net core 项目模板

该项目模板中使用了以下组件：
- Swagger
- AspNetCoreRateLimit(流量限制)
- Polly(错误处理框架)
- Dapper(实体映射框架)
- StackExchange.Redis
- Serilog(日志组件)
- MySql.Data

若以上组件有不需要的，直接从 csproj 文件中删除依赖项，再删除 IDE 提示报错的代码

若不需要前端页面模块，可以将 Views、wwwroot 目录所有文件删除

本地运行时，默认为 Development 环境，publish 后默认运行时为 Production 环境，可以通过 --environment Staging 来指定运行环境

appsettings.{env}.json 中的配置会根据环境的不同启用不同的配置项

Swagger
-------

若 api 接口需要加入到 Swagger 中，需要将 Controller 标注 ApiController 特性，Action 需要明确指定路由

AspNetCoreRateLimit
-------------------

流量限制只需修改 appsettings.json 中的配置即可，若需要进一步了解可访问[wiki](https://github.com/stefanprodan/AspNetCoreRateLimit/wiki)

StackExchange.Redis
-------------------

程序中需要使用 Redis 客户端时，直接通过依赖注入，将 IDatabase 注入到 Controller 中，服务器链接、数据库修改参照配置文件

Serilog
-------

Serilog 日志组件目前仅输出到 Console 和本地文件，若需要接入其他方式可参考[wiki](https://github.com/serilog/serilog/wiki/Provided-Sinks)

注：Serilog 的配置应放在同一个配置文件中，不要出现 appsettings.json 和 appsettings.{env}.json 同时存在的情况，否则会出现配置覆盖的情况。因为 WriteTo 是一个数组，index 会被作为 key，因此当 appsettings.json WriteTo[0] 配置的是 Console，appsettings.{env}.json WriteTo[0] 配置的是 Elasticsearch，则最终 Console 将不会输出，因为被 Elasticsearch 覆盖了

数据库
---------

本项目使用了一个工厂类 DbConnectionFactory 来同时支持多种数据库，并且每种数据库还支持多个连接。工厂类中根据 $"{clientType}:ConnectionStrings:{connectionName}" 此规则从配置中获取连接字符串，详细可参照代码与配置。若需要增加对其他数据库的支持，可参照工厂类代码，编写对应逻辑即可

Polly
-----

使用 Polly 的时候，无论是 http、数据库、redis，都需要保证 Policy 的粒度足够小，确保一个 Policy 只能干预到一个操作

http 参照 GithubService，数据库参照 DbConnectionExtension，redis 参照 RedisDbExtension

以此为模板创建项目
----------------

1. Clone 此项目到本地
2. dotnet new --install {ProjectRoot}/src
3. 在需要创建项目的目录下打开新的命令行窗口
4. dotnet new base --ProjectName Base.Test -o Base.Test
