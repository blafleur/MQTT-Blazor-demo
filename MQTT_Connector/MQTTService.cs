using System;
namespace MQTT_Connector
{
    public class MQTTService
    {
        public string Message { get; set; }
        public event Func<Task> Notify;

        public async Task RefreshAsync(string message)
        {
            if (Notify is { })
            {
                Message = message;
                await Notify.Invoke();
            }
        }
    }
}

