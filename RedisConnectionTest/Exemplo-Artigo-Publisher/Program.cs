using RedisBoost;
using System;
using System.Configuration;

namespace Exemplo_Artigo_Publisher {

    internal class Program {

        private static void Main(string[] args) {
            // RedisBoost usa connectionstring para configurar acesso ao Redis
            var connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;

            // Publica as mensagens
            RedisPublisher(connectionString);
        }

        private static void RedisPublisher(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    string[] channels = { "musica", "tecnologia", "gothamCity", "MundiPagg" };
                    string[] messages = { "stompin' at the savoy - Jim Hall", "Hub de eventos com Redis",
                                          "Gotham esta em paz", "Converta mais e aumente as vendas do seu e-commerce" };

                    // Publicando mensagens para os canais do array
                    for (int i = 0; i < channels.Length; i++) {
                        Console.WriteLine("Publicando no canal {0} a mensagem {1}", channels[i], messages[i]);
                        redisClient.PublishAsync(channels[i], messages[i]).Wait();
                    }

                    Console.WriteLine("Mensagens enviadas! :)");
                    Console.Read();
                }
            }
        }
    }
}