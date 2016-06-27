using RedisBoost;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace RedisConnection_RedisBoost {

    internal class Program {

        private static void Main(string[] args) {
            // RedisBoost usa connectionstring para configurar acesso ao Redis
            var connectionString = ConfigurationManager.ConnectionStrings["Redis"].ConnectionString;
            //var client = RedisClient.ConnectAsync(connectionString).Result;

            SimpleExample(connectionString);
            //ClassExample(connectionString);
            //PubSubMessage(connectionString);
            //ListenToChannel(connectionString);
        }

        private static void SimpleExample(string connectionString) {
            // Conexão
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;
                var redisKey = "RedisBoostKey";
                var redisKeyDelete = "RedisBoostKeyDelete";

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis
                    redisClient.SetAsync(redisKey, "RedisBoost mandando bala!").Wait();


                    // Recupera de forma assincrona convertendo para string
                    // Outras formas de serializar com o RedisBoost
                    // https://github.com/andrew-bn/RedisBoost/wiki/Serialization
                    var redisReturn = redisClient.GetAsync(redisKey).Result.As<string>();
                    //redisClient.ExpireAsync(redisKey, 60);
                }

                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    redisClient.SetAsync(redisKeyDelete, "RedisBoost Delete key.").Wait();
                    var deleteKey = redisClient.GetAsync(redisKeyDelete).Result.As<string>();

                    // Deleta
                    var resultDelete = redisClient.DelAsync(redisKeyDelete).Result;
                }
            }
        }

        private static void ClassExample(string connectionString) {
            // Conexão
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;
                var redisKey = "RedisBoostKeyClass";
                var redisKeyDelete = "RedisBoostKeyDeleteClass";

                var func = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles1",
                    LastName = "Lomboni1",
                    Age = 28
                };

                var func2 = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles2",
                    LastName = "Lomboni2",
                    Age = 28
                };

                var func3 = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles3",
                    LastName = "Lomboni3",
                    Age = 28
                };

                var func4 = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles4",
                    LastName = "Lomboni4",
                    Age = 28
                };

                List<Funcionario> funcs = new List<Funcionario>();

                funcs.Add(func);
                funcs.Add(func2);
                funcs.Add(func3);
                funcs.Add(func4);

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis

                    //redisClient.SetAsync(redisKey, funcs).Wait();
                    redisClient.SAddAsync(redisKey, funcs).Wait();

                    // Recupera de forma assincrona convertendo para string
                    // Outras formas de serializar com o RedisBoost
                    // https://github.com/andrew-bn/RedisBoost/wiki/Serialization
                    var redisReturn = redisClient.GetAsync(redisKey).Result.As<List<Funcionario>>();
                    //redisClient.ExpireAsync(redisKey, 60);
                }

                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    redisClient.SetAsync(redisKeyDelete, func).Wait();
                    var deleteKey = redisClient.GetAsync(redisKeyDelete).Result.As<Funcionario>();

                    // Deleta
                    var resultDelete = redisClient.DelAsync(redisKeyDelete).Result;
                }
            }
        }

        private static void PubSubMessage(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;
                var redisKey = "RedisBoostKeyClass";
                var redisKeyDelete = "RedisBoostKeyDeleteClass";

                var func = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles",
                    LastName = "Lomboni",
                    Age = 28
                };

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis
                    using (var subscriber = redisClient.SubscribeAsync("channel").Result) {
                        // Cria o client
                        using (var redisClient2 = pool.CreateClientAsync(connectionString).Result) {
                            using (var publisher = redisClient2) {
                                // Publicando 10 mensagens para o canal
                                for (int i = 0; i < 10; i++) {

                                    func.Name += i;
                                    publisher.PublishAsync("channel", func).Wait();
                                }
                            }
                        }

                        var channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message |
                                                                         ChannelMessageType.PMessage).Result;
                        // Criando um segundo subscriber
                        var subscriber2 = redisClient.SubscribeAsync("channel").Result;

                        // Obtendo a primeira mensagem do canal, utilizando o subscriber2
                        var channelMessage2 = subscriber2.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;

                        //Loop para pegar as 10 mensagens no canal
                        while (channelMessage2.Value.As<Funcionario>() != null) {

                            var r3 = channelMessage2.Value.As<Funcionario>();
                            Console.WriteLine("Valor: {0}", r3.Name);
                            channelMessage2 = subscriber2.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;

                        }

                        var c = channelMessage.Channels[0];
                        var r = channelMessage.Value.As<Funcionario>();
                    }
                }
            }
        }

        private static void ListenToChannel(string connectionString) {
            using (var pool = RedisClient.CreateClientsPool()) {
                IRedisClient redisClient;
                var redisKey = "RedisBoostKeyClass";
                var redisKeyDelete = "RedisBoostKeyDeleteClass";

                var func = new Funcionario {
                    IdFuncionario = Guid.NewGuid(),
                    Name = "Charles",
                    LastName = "Lomboni",
                    Age = 28
                };

                // Cria o client
                using (redisClient = pool.CreateClientAsync(connectionString).Result) {
                    // Faz de forma assincrona o inserir no Redis
                    using (var subscriber = redisClient.SubscribeAsync("channel").Result) {
                        // Obtendo a primeira mensagem do canal, utilizando o subscriber2
                        var channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;
                        //Loop para pegar as 10 mensagens no canal
                        while (channelMessage.Value.As<Funcionario>() != null) {
                            var r3 = channelMessage.Value.As<Funcionario>();
                            Console.WriteLine("Valor: {0}", r3.Name);
                            channelMessage = subscriber.ReadMessageAsync(ChannelMessageType.Message | ChannelMessageType.PMessage).Result;
                        }
                        var c = channelMessage.Channels[0];
                        var r = channelMessage.Value.As<Funcionario>();
                    }
                }
            }
        }




    }

    public class Funcionario {
        public Guid IdFuncionario { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}