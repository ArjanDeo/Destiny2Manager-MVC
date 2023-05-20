namespace Destiny2Manager.Models.Bungie
{
    public class BungieCharacterEquipmentModelData
    {
        public List<BungieCharacterEquipmentModelItem> items { get; set; }
    }

    public class BungieCharacterEquipmentModelEquipment
    {
        public BungieCharacterEquipmentModelData data { get; set; }
        public int privacy { get; set; }
    }

    public class BungieCharacterEquipmentModelItem
    {
        public object itemHash { get; set; }
        public string itemInstanceId { get; set; }
        public int quantity { get; set; }
        public int bindStatus { get; set; }
        public int location { get; set; }
        public object bucketHash { get; set; }
        public int transferStatus { get; set; }
        public bool lockable { get; set; }
        public int state { get; set; }
        public int dismantlePermission { get; set; }
        public bool isWrapper { get; set; }
        public List<int> tooltipNotificationIndexes { get; set; }
        public int versionNumber { get; set; }
        public long? overrideStyleItemHash { get; set; }
    }

    public class BungieCharacterEquipmentModelMessageData
    {
    }

    public class BungieCharacterEquipmentModelResponse
    {
        public BungieCharacterEquipmentModelEquipment equipment { get; set; }
        public BungieCharacterEquipmentModelUninstancedItemComponents uninstancedItemComponents { get; set; }
    }

    public class BungieCharacterEquipmentModel
    {
        public BungieCharacterEquipmentModelResponse Response { get; set; }
        public int ErrorCode { get; set; }
        public int ThrottleSeconds { get; set; }
        public string ErrorStatus { get; set; }
        public string Message { get; set; }
        public BungieCharacterEquipmentModelMessageData MessageData { get; set; }
    }

    public class BungieCharacterEquipmentModelUninstancedItemComponents
    {
    }
}