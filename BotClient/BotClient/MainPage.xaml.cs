using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DSharpPlus;

namespace BotClient
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window
    {
        string token;
        IReadOnlyDictionary<ulong, DSharpPlus.Entities.DiscordGuild> guilds;
        DiscordClient client;
        ulong currentChannel;
        public MainPage(string token)
        {
            InitializeComponent();
            this.Title = "Discord Bot Client";
            this.token = token;
            serverList.SelectionChanged += ServerList_SelectionChanged;
            channelList.SelectionChanged += ChannelList_SelectionChanged;
            reloadButton.Click += ReloadButton_Click;
            sendMessageButton.Click += SendMessageButton_Click;
            MainAsync().ConfigureAwait(false).GetAwaiter();
        }

        private async Task MainAsync()
        {
            string? botToken = token;
            textBox.Text = token;
            DiscordClient? discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = botToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.None,
                AutoReconnect = true
            });
            try
            {
                await discord.ConnectAsync();
            }
            catch (Exception e)
            {
                textBox.Text = e.Message;
                await Task.Delay(888888);
            }
            textBox.Text = "connected?";
            textBox.Text = $"Successfully logged in as {discord.CurrentUser.Username}#{discord.CurrentUser.Discriminator}!";

            await Task.Delay(1000); // give it some time
            // get guilds
            guilds = discord.Guilds;

            foreach (KeyValuePair<ulong, DSharpPlus.Entities.DiscordGuild> guild in guilds)
            {
                serverList.Items.Add(guild.Value.Name);
            }

            client = discord;

            discord.MessageCreated += Discord_MessageCreated;

            await Task.Delay(-1);
        }

        private void AddMessageToTheBox(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                messagesBox.Text += message;
                messagesBox.ScrollToEnd();
            });
        }

        private Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var channelId = e.Channel.Id;
            if (channelId == currentChannel)
            {
                var message = e.Message;
                string? author = $"{message.Author.Username}#{message.Author.Discriminator}";
                string? content = message.Content;
                if (content == "" || content == null)
                {
                    content = "User sent attachments with no text body.";
                    int i = 0;
                    foreach (var attachment in message.Attachments)
                    {
                        i++;
                        content += $"\nAttachment [{i}]: {attachment.Url}";
                    }
                }
                string? time = $"{message.Timestamp.DateTime.Hour}:{message.Timestamp.DateTime.Minute}:{message.Timestamp.DateTime.Second}-{message.Timestamp.DateTime.Day}/{message.Timestamp.DateTime.Month}/{message.Timestamp.DateTime.Year}";
                string? full = $"[{author}]-[{time}] - {content}";
               AddMessageToTheBox($"{full}\n");
            }

            return Task.CompletedTask;
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            channelList.Items.Clear();
            int guildIndex = serverList.SelectedIndex;
            try
            {
                KeyValuePair<ulong, DSharpPlus.Entities.DiscordGuild> guild = guilds.ElementAt(guildIndex);
                DSharpPlus.Entities.DiscordGuild? server = guild.Value;
                IReadOnlyDictionary<ulong, DSharpPlus.Entities.DiscordChannel>? channels = server.Channels;
                foreach (KeyValuePair<ulong, DSharpPlus.Entities.DiscordChannel> channel in channels)
                {
                    if (channel.Value.Type == ChannelType.Voice)
                    {

                    }
                    else
                    {
                        channelList.Items.Add(channel.Value.Name);
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            var index = serverList.SelectedIndex;
            var cIndex = channelList.SelectedIndex;
            IReadOnlyDictionary<ulong, DSharpPlus.Entities.DiscordGuild>? guilds = client.Guilds;
            serverList.Items.Clear();
            foreach (KeyValuePair<ulong, DSharpPlus.Entities.DiscordGuild> guild in guilds)
            {
                serverList.Items.Add(guild.Value.Name);
            }
            serverList.SelectedIndex = index;
            channelList.SelectedIndex = cIndex;
        }
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string? message = messageBox.Text;
            int guildIndex = serverList.SelectedIndex;
            KeyValuePair<ulong, DSharpPlus.Entities.DiscordGuild> guild = guilds.ElementAt(guildIndex);
            DSharpPlus.Entities.DiscordGuild? server = guild.Value;
            IReadOnlyDictionary<ulong, DSharpPlus.Entities.DiscordChannel>? channels = server.Channels;
            int channelIndex = channelList.SelectedIndex;
            KeyValuePair<ulong, DSharpPlus.Entities.DiscordChannel> channel = channels.ElementAt(channelIndex);
            channel.Value.SendMessageAsync(message);
            messageBox.Text = "";
        }

        private void ChannelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            messagesBox.Text = "";
            int guildIndex = serverList.SelectedIndex;
            try
            {
                KeyValuePair<ulong, DSharpPlus.Entities.DiscordGuild> guild = guilds.ElementAt(guildIndex);
                DSharpPlus.Entities.DiscordGuild? server = guild.Value;
                IReadOnlyDictionary<ulong, DSharpPlus.Entities.DiscordChannel>? channels = server.Channels;
                int channelIndex = channelList.SelectedIndex;
                KeyValuePair<ulong, DSharpPlus.Entities.DiscordChannel> channel = channels.ElementAt(channelIndex);
                try
                {
                    currentChannel = channel.Value.Id;
                    IReadOnlyList<DSharpPlus.Entities.DiscordMessage>? messages = channel.Value.GetMessagesAsync(100).Result;
                    var reversedMessages = messages.Reverse();
                    foreach (DSharpPlus.Entities.DiscordMessage? message in reversedMessages)
                    {
                        string? author = $"{message.Author.Username}#{message.Author.Discriminator}";
                        string? content = message.Content;
                        if (content == "" || content == null)
                        {
                            content = "User sent attachments with no text body.";
                            int i = 0;
                            foreach (var attachment in message.Attachments)
                            {
                                i++;
                                content += $"\nAttachment [{i}]: {attachment.Url}";
                            }
                        }
                        string? time = $"{message.Timestamp.DateTime.Hour}:{message.Timestamp.DateTime.Minute}:{message.Timestamp.DateTime.Second}-{message.Timestamp.DateTime.Day}/{message.Timestamp.DateTime.Month}/{message.Timestamp.DateTime.Year}";
                        string? full = $"[{author}]-[{time}] - {content}";
                        AddMessageToTheBox($"{full}\n");
                    }

                    messagesBox.ScrollToEnd();
                }
                catch (AggregateException)
                {
                    messagesBox.Text = "Bot does not have access to this channel!";
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
        }
    }
}
