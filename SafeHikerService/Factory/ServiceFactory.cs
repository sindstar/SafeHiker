﻿using SafeHikerService.Common;
using Storage;

namespace SafeHikerService.Factory
{
    public static class ServiceFactory
    {
        private static AzureStorageServiceClient _storageClient;

        static ServiceFactory()
        {
            _storageClient = new AzureStorageServiceClient(Constants.StorageAccountName,
                                                            Constants.NotifyUserTableName,
                                                            Constants.NotifyEmergencyTableName,
                                                            Constants.UserHikesTableName,
                                                            Constants.UserInfoTableName);
        }

        public static AzureStorageServiceClient GetStorageClient()
        {
            return _storageClient;
        }
    }
}