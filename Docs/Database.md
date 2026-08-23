# Database

## Instalar dotnet-ef

Intalar dotnet-ef si no está instalado

```shell
dotnet tool install --global dotnet-ef
```

Actulizar dotnet-ef si ya está instalado

```shell
dotnet tool update --global dotnet-ef
```

## Database de servidor

Desde Nivaes.App.RPC.Sample.Server

```shell
dotnet ef migrations add InitialCreate `
	--project Nivaes.App.RPC.Sample.Server.csproj `
	--context Nivaes.App.RPC.Sample.Server.ServerDatabaseContext `
	--output-dir Sources/Database/Migrations 
```

## Database de cliente RpcSync

Desde Nivaes.App.Rpc.Client

Crear migración

```shell
 dotnet ef migrations add InitialCreate `
   --project Nivaes.App.RPC.Client.csproj `
   --startup-project ../Nivaes.App.Rpc.Client.Database.Tools/Nivaes.App.Rpc.Client.Database.Tools.csproj `
   --context Nivaes.App.RPC.Client.RpcSyncDatabaseContext `
   --output-dir Sources/Database/Migrations `
   --framework net10.0 
```

Crear modelo optimizado para ser compatible on AoT.
```shell
dotnet ef dbcontext optimize `
    --project Nivaes.App.RPC.Client.csproj `
    --context Nivaes.App.RPC.Client.RpcSyncDatabaseContext `
    --startup-project ../Nivaes.App.Rpc.Client.Database.Tools/Nivaes.App.Rpc.Client.Database.Tools.csproj `
    --output-dir Sources/Database/CompiledModel `
    --namespace Nivaes.App.RPC.Client.Database `
    --nativeaot `
    --framework net10.0 
```

Probar en un futuro     --precompile-queries `

## Database de cliente

Desde Nivaes.App.RPC.Sample.Client

Crear migración

```shell
 dotnet ef migrations add InitialCreate `
   --project Nivaes.App.RPC.Sample.Client.csproj `
   --context Nivaes.App.RPC.Sample.Client.DatabaseContext `
   --output-dir Sources/Database/Migrations `
   --framework net10.0 
```

Crear modelo optimizado para ser compatible on AoT.
```shell
dotnet ef dbcontext optimize `
    --project Nivaes.App.RPC.Sample.Client.csproj `
    --context Nivaes.App.RPC.Sample.Client.DatabaseContext `
    --output-dir Sources/Database/CompiledModel `
    --namespace Nivaes.App.RPC.Sample.Client.Database `
    --nativeaot `
    --precompile-queries 
```

Genera script

```shell
dotnet ef migrations script `
     --project Nivaes.App.RPC.Sample.Client.csproj `
     --context Nivaes.App.RPC.Sample.Client.DatabaseContext `
     --output Sources/Database/Migrations/Migration.sql
```

```shell
dotnet ef migrations script 0 20260818162121_InitialCreate `
     --project Nivaes.App.RPC.Sample.Client.csproj `
     --context Nivaes.App.RPC.Sample.Client.DatabaseContext `
     --output Sources/Database/Migrations/20260818162121_InitialCreate.sql
```