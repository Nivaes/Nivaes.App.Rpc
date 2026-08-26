var builder = DistributedApplication.CreateBuilder(args);

#region Database
var postgres = builder.AddPostgres("RpcPostgres", port: 5432)
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithDataVolume()
                      .WithPgAdmin();
var serverDb = postgres.AddDatabase("DbAppSample");

var mongo = builder.AddMongoDB("RpcMongoDB")
                .WithLifetime(ContainerLifetime.Session)
                //.WithDataVolume()
                .WithMongoExpress()
                .WithDbGate();

var mongoDb = mongo.AddDatabase("DbAppMongo");
#endregion

#region Server
var appWebApi = builder.AddProject<Projects.Nivaes_App_Rpc_Sample_Server>("RpcSampleSerice")
                .WithHttpsEndpoint(name: "grpc")
                .WithHttpHealthCheck("/health")
                .WithReference(serverDb)
                .WaitFor(serverDb)
                .WithReference(mongoDb)
                .WaitFor(mongoDb);
#endregion

#region Cliente
var appConsole = builder.AddProject<Projects.Nivaes_App_RPC_Sample_Console>("RpcSampleConsole")
                .WithReference(appWebApi)
                .WaitFor(appWebApi);

#endregion

builder.Build().Run();
