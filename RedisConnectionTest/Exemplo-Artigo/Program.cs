using System;
using System.Configuration;
using RedisBoost;

namespace Exemplo_Artigo_Subscriber {

    internal class Program {

        private static void Main(string[] args) {
            // RedisBoost usa connectionstring para configurar acesso ao Redis
            var connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;

            // Canal que o subscriber irá ouvir
            var channels = ConfigurationManager.AppSettings["channels"];

            //RedisCache(connectionString);

            Console.Write("Escreva o canal para assinar: ");
            var channel = Console.ReadLine();
            RedisPubSub(connectionString, channel);
        }

        // Redis com cache
        private static void RedisCache(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;

                // Chave
                var cadernoHoraDeAventura = "caderno:hora-de-aventura";
                var cadernoTheJoker = "caderno:the-joker";

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis
                    redisClient.SetAsync(cadernoHoraDeAventura, "2000").Wait();
                    Console.WriteLine("Item {0} adicionado com sucesso ao carrinho!", cadernoHoraDeAventura);

                    // Get Item
                    var redisHoraDeAventura = redisClient.GetAsync(cadernoHoraDeAventura).Result.As<string>();
                    Console.WriteLine("Valor {0} do item {1}.", redisHoraDeAventura, cadernoHoraDeAventura);

                    // Deletando a chave
                    var resultHoraDeAventuraDelete = redisClient.DelAsync(cadernoHoraDeAventura).Result;
                    Console.WriteLine("Chave {0} foi deletada? {1}", cadernoHoraDeAventura, Convert.ToBoolean(resultHoraDeAventuraDelete));

                    // Inserindo
                    redisClient.SetAsync(cadernoTheJoker, "2200").Wait();
                    Console.WriteLine("Item {0} adicionado com sucesso ao carrinho!", cadernoTheJoker);

                    // Get Item
                    var redisTheJoker = redisClient.GetAsync(cadernoTheJoker).Result.As<string>();
                    Console.WriteLine("Valor {0} do item {1}.", redisTheJoker, cadernoTheJoker);

                    // Adicionando o tempo para expirar no caderno:the-joker
                    redisClient.ExpireAsync(cadernoTheJoker, 180);
                    Console.WriteLine("Chave {0} com TTL de 180 segundos.", cadernoTheJoker);
                    Console.Read();
                }
            }
        }

        // Redis com PubSub
        private static void RedisPubSub(string connectionString, string channel) {
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Usando o PSubscribe para escutar canais específicos e genéricos
                    using (var subscriber = redisClient.PSubscribeAsync(channel).Result) {

                        Console.WriteLine("Ouvindo o canal {0}", channel);

                        // Obtendo a primeira mensagem do canal, utilizando o subscriber
                        var channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;

                        // Se não tiver nenhuma mensagem, fica esperando
                        while (channelMessage.Value.As<string>() != null) {
                            var messageFomChannel = channelMessage.Value.As<string>();
                            Console.WriteLine("Mensagem: {0}", messageFomChannel);
                            channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;
                        }
                    }
                }
            }
        }
    }
}