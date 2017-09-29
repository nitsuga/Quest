using System;

namespace Quest.Common.Messages
{
    /// <summary>
    ///     Sent by the device when wishing to take part in dispatch.
    /// </summary>

    [Serializable]
    public class LoginRequest : Request
    {
        /// <summary>
        ///     Indicates that this device is compatible with a specific Quest API version. As the Quest Api grows this
        ///     will change
        /// </summary>
        
        public int QuestApi { get; set; }

        /// <summary>
        ///     This is a unique identity for this device
        /// </summary>        
        public string DeviceIdentity { get; set; }

        /// <summary>
        ///     a language locale of the device. defaults to en-GB if left empty
        /// </summary>        
        public string Locale { get; set; } = "en-GB";

        /// <summary>
        ///     a unique id to be passed to the delivery provider, such as GCM or Apple
        ///     <seealso cref="NotificationTypeId" />
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        ///     defines the method notification
        ///     e.g. GCM, APN
        ///     <seealso cref="https://github.com/Redth/PushSharp" />
        /// </summary>
        
        public string NotificationTypeId { get; set; }

        /// <summary>
        ///     username of the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     operating system of the device
        /// </summary>        
        public string OSVersion { get; set; }

        /// <summary>
        ///     Make of device
        /// </summary>        
        public string DeviceMake { get; set; }

        /// <summary>
        ///     Model of device
        /// </summary>        
        public string DeviceModel { get; set; }

        public override string ToString()
        {
            return
                $"Logon DeviceIdentity={DeviceIdentity} NotificationTypeId={NotificationTypeId} NotificationId={NotificationId}";
        }
    }
}