using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using CheckInFrontend.Hubs;
using CheckInFrontend.Models;

namespace CheckInFrontend.Services
{
    //Vi opretter en StudentService klasse, som nedarver fra BackgroundService, 
    public class StudentService : BackgroundService
    {
        private readonly IHubContext<StudentHub> _hubContext;
        private IConnection _connection;
        private IModel _channel;


        //Vi opretter en constructor, og her injecter vi IHubContext<StudentHub> for at bruge signalR til real-time data
        //og opretter en forbindelse til RabbitMQ via ConnectionFactory
        public StudentService(IHubContext<StudentHub> hubContext)
        {
            _hubContext = hubContext;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumer(stoppingToken), stoppingToken);
        }

        private void StartConsumer(CancellationToken stoppingToken)
        {
            //Vi tager fat i den kø, fra vores Reciever projekt, så vi kan få studentData objekterne.
            string queueName = "student_checkin_queue";

            _channel.QueueDeclare(queue: queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            Console.WriteLine(" [*] Waiting for student data messages.");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($" [x] Received student data: {message}");

                // Vi deserialiserer studentData objektet, og sender det til alle clients via signalR
                var studentData = JsonSerializer.Deserialize<StudentData>(message);

                await _hubContext.Clients.All.SendAsync("ReceiveStudentData", studentData);
            };

            _channel.BasicConsume(queue: queueName,
                                  autoAck: true,
                                  consumer: consumer);

            //Vi laver en while loop, som kører indtil stoppingToken er cancelled.
            while (!stoppingToken.IsCancellationRequested)
            {
                Task.Delay(1000).Wait();
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

}
