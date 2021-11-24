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
        public MainPage(string token)
        {
            InitializeComponent();
            this.token = token;
            channelList.Visibility = Visibility.Hidden;
            serverList.SelectionChanged += ServerList_SelectionChanged;
            MainAsync().ConfigureAwait(false).GetAwaiter();
        }

        private async Task MainAsync()
        {
            var botToken = token;
            textBox.Text = token;
                var discord = new DiscordClient(new DiscordConfiguration()
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

            await Task.Delay(3000); // give it some time
            // get guilds
            guilds = discord.Guilds;

            foreach (var guild in guilds)
            {
                serverList.Items.Add(guild.Value.Name);
            }
            
            await Task.Delay(-1);
        }

        private void ServerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            channelList.Items.Clear();
            var guildIndex = serverList.SelectedIndex;
            var guild = guilds.ElementAt(guildIndex);
            var server = guild.Value;
            var channels = server.Channels;
            foreach (var channel in channels)
            {
                channelList.Items.Add(channel.Value.Name);
            }
            channelList.Visibility = Visibility.Visible;
        }
    }
}
