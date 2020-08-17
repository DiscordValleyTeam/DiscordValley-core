using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord_Valley
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient Client;
        private CommandService Commands;
        private CommandHandler Handler;
        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();
            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Client.Ready += Client_Ready;

            Commands = new CommandService();
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            Handler = new CommandHandler(Client, Commands);
            await Handler.InstallCommandsAsync(Client);

            Console.Title = "Discord Valley";

            await Client.LoginAsync(TokenType.Bot, File.ReadAllText("token.txt"));
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Client_Ready()
        {
            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task MessageReceived(SocketMessage Message)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        public class CommandHandler
        {
            private readonly DiscordSocketClient _client;
            private readonly CommandService _commands;

            public CommandHandler(DiscordSocketClient client, CommandService commands)
            {
                _commands = commands;
                _client = client;
            }

            public async Task InstallCommandsAsync(DiscordSocketClient client)
            {
                // Hook the MessageReceived event into our command handler
                _client.MessageReceived += HandleCommandsAsync;
                await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
            }

            private async Task HandleCommandsAsync(SocketMessage messageParam)
            {
                // Don't process the command if it was a system message
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                // Create a number to track where the prefix ends and the command begins
                int argPos = 0;

                // Determine if the message is a command based on the prefix and make sure no bots trigger commands
                if (!message.HasCharPrefix('!', ref argPos) || message.Author.IsBot)
                {
                    return;
                }

                // Create a WebSocket-based command context based on the message
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context: context, argPos: argPos, services: null);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"[{DateTime.Now} at Commands] Something went wrong with a command. Text: {context.Message.Content} | Error: {result.ErrorReason}");
                    await context.Channel.SendMessageAsync($":x: {result.ErrorReason}");
                }
            }
        }
    }
}
