using Nivaes.App.Rpc;

namespace Nivaes.App.Cross
{
    public static class RpcDataModelTypeContainerHelper
    {
        public sealed class RpcDataModelTypeItem
        {
            required public string TypeNameRpcDataModel;
            required public Type TypeRpcDataModel;
        }

        public static RpcDataModelTypeItem New<TRpcDataModel>()
            where TRpcDataModel : IRpcDataModel
        {
                return new RpcDataModelTypeItem
                {
                    TypeNameRpcDataModel = typeof(TRpcDataModel).FullName!,
                    TypeRpcDataModel = typeof(TRpcDataModel),
                };
        }
       
        public static void RegisterRpcDataModels(RpcDataModelTypeItem[] items)
        {
            var combinersContainer = Singleton<RpcDataModelsTypeContainer>.Instance;

            foreach(var item in items)
            {
                combinersContainer.RpcDataModelsType.Add(item.TypeNameRpcDataModel, item.TypeRpcDataModel);
            }
        }
    }
}
