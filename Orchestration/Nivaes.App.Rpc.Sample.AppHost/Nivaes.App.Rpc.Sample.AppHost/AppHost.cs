var builder = DistributedApplication.CreateBuilder(args);

#region Database
//var postgres = builder.AddPostgres("RpcPostgres", port: 5432)
//                      .WithLifetime(ContainerLifetime.Persistent)
//                      .WithDataVolume()
//                      .WithPgAdmin();
//var serverDb = postgres.AddDatabase("DbAppSample");

var mongo = builder.AddMongoDB("RpcMongoDB")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithDataVolume()
                .WithMongoExpress()
                .WithDbGate();

var mongoDb = mongo.AddDatabase("DbAppMongo");
#endregion

#region Server
var appWebApi = builder.AddProject<Projects.Nivaes_App_Rpc_Sample_Server>("RpcSampleSerice")
                .WithHttpsEndpoint(name: "grpc")
                .WithHttpHealthCheck("/health")
                //.WithReference(serverDb)
                //.WaitFor(serverDb)
                .WithReference(mongoDb)
                .WaitFor(mongoDb);
#endregion

#region Clients
var appConsole = builder.AddProject<Projects.Nivaes_App_Rpc_Sample_Console>("RpcSampleConsole")
                .WithReference(appWebApi)
                .WaitFor(appWebApi);

var appConsoleRead = builder.AddProject<Projects.Nivaes_App_Rpc_Sample_Console_Reader>("RpcSampleConsoleRead")
                .WithReference(appWebApi)
                .WaitFor(appWebApi);

var appConsoleWriter = builder.AddProject<Projects.Nivaes_App_Rpc_Sample_Console_Writer>("RpcSampleConsoleWriter")
                .WithReference(appWebApi)
                .WaitFor(appWebApi);

#endregion

builder.Build().Run();
