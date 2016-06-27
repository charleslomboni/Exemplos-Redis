using RedisBoost;
using System;
using System.Configuration;
using System.Linq;

namespace RedisBoost_ListenChannel {

    internal class ListenChannel {

        private static void Main(string[] args) {
            // RedisBoost usa connectionstring para configurar acesso ao Redis
            var connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;
            ListenToChannel(connectionString);
        }

        private static void ListenToChannel(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {
                //IRedisClient redisClient;

                // Cria o client
                using (var redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis

                    // http://redis.io/topics/notifications
                    // Usando Redis Keyspace Notifications
                    // Rodar no client redis o comando redis-cli config set notify-keyspace-events KEA
                    // para mais informações, olhar a seção Configuration no link acima

                    //criando um subscriber que vai assinar os canais com o padrão definido
                    //using (var subscriber = redisClient.PSubscribeAsync("__key*__:*").Result) {
                    using (var subscriber = redisClient.PSubscribeAsync("*").Result) {
                        // Obtendo a primeira mensagem do canal, utilizando o subscriber
                        var channelMessage = subscriber.ReadMessageAsync().Result;

                        while (true) {
                            var r3 = channelMessage.Value.As<string>();

                            if (channelMessage.Channels.Count() > 1) {
                                var c = channelMessage.Channels[0];
                                var c2 = channelMessage.Channels[1];
                                Console.WriteLine("Valor: {0}, Channel: {1}", r3, c2);
                            }
                            channelMessage = subscriber.ReadMessageAsync().Result;
                        }
                    }
                }
            }
        }
    }
}