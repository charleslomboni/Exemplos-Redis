using RedisBoost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisBoost_ListenChannel {
    class Program {
        static void Main(string[] args) {


            // RedisBoost usa connectionstring para configurar acesso ao Redis
            var connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;
            var client = RedisClient.ConnectAsync(connectionString).Result;

            //SimpleExample(connectionString);
            //ClassExample(connectionString);
            //PubSubMessage(connectionString);
            ListenToChannel(connectionString);
        }


        private static void ListenToChannel(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {

                IRedisClient redisClient;

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis

                    

                    using (var subscriber = redisClient.PSubscribeAsync("__keyspace@0__:*", "__keyevent@0__:*").Result) {
                        // Obtendo a primeira mensagem do canal, utilizando o subscriber2
                        var channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;
                        //Loop para pegar as 10 mensagens no canal
                        //while (channelMessage.Value.As<string>() != null) {
                        while (true) {
                            var r3 = channelMessage.Value.As<string>();
                            Console.WriteLine("Valor: {0}", r3);
                            channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;
                        }
                        var c = channelMessage.Channels[0];
                        var r = channelMessage.Value.As<string>();
                    }
                }
            }
        }
    }
}
