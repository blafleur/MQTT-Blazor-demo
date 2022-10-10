using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;

namespace MQTT_Connector;

public class Worker : BackgroundService
{

    private readonly MQTTService MqttService;
    private readonly MqttFactory MqttFactory;

    public Worker(MQTTService mQTTService)
    {
        this.MqttService = mQTTService;
        this.MqttFactory = new MqttFactory();
    }

    public async Task Update(string message)
    {
        await MqttService.RefreshAsync(message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        using IMqttClient mqttClient = MqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost")
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        
        var mqttSubscribeOptions = MqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => { f.WithTopic("vstopic"); })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        mqttClient.ApplicationMessageReceivedAsync += async delegate (MqttApplicationMessageReceivedEventArgs args)
        {
            var mqttMessage = "";
            if (args.ApplicationMessage.Payload is null)
            {
                mqttMessage = "Empty message";
            }
            else
            {
                mqttMessage = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            }

            Console.WriteLine("Console Response = {0}", mqttMessage);
            await Update(mqttMessage).ConfigureAwait(false);
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await mqttClient.TryPingAsync(cancellationToken: stoppingToken))
                {
                    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                    // Subscribe to topics when session is clean etc.
                    Console.WriteLine("The MQTT client is connected.");
                    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
                    
                }
            }
            catch (Exception ex)
            {
                // Some message here
            }
            finally
            {
                // Delay 5 seconds
                await Task.Delay(5 * 1000, stoppingToken);
            }
        }
    }
}

