using Discord.WebSocket;
using Discord;
using Programm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Discord_Faceit_Bot.Models.Faceit;
using System.Threading.Channels;
using System.Text.RegularExpressions;

namespace Discord_Faceit_Bot.Models
{
    public class DiscordMessage
    {
        public ulong messageID { get; set; }
        public string ticketID { get; set; }
    }
        public class DiscordFunctions
        {

            public async Task<List<ulong>> FindTicketsInDiscord(ulong ulChannelID, DiscordSocketClient client, string match_ID)
            {
                List<ulong> list = new List<ulong>();
                var messages = GetMessagesAsync(ulChannelID, client);
                for (int i = 0; i < messages.Result.Count; i++)
                {

                }

                foreach (var message in messages.Result)
                {
                    if (message.Embeds.ElementAt(0).Footer.Value.Text == match_ID)
                    {
                        list.Add(message.Id);
                    }
                }
                return list;
            }
            public async Task<List<IMessage>> GetMessagesAsync(ulong ulChannelID, DiscordSocketClient client)
            {
                var channel = client.GetChannel(ulChannelID) as IMessageChannel;
                var messages = await channel.GetMessagesAsync(limit: 100).FlattenAsync();
                return messages.ToList();
            }

            public async Task CloseTicket(ulong ulOpenChannelID, ulong ulCloseChannelID, DiscordSocketClient client, Faceit.MatchData match, IMessage msg = null)
            {
                var openChannel = client.GetChannel(ulOpenChannelID) as IMessageChannel;
                var closedChannel = client.GetChannel(ulCloseChannelID) as IMessageChannel;

                var message = await openChannel.GetMessageAsync(match.discordMessageID);
                if(message != null)
                {
                EmbedBuilder embed = new EmbedBuilder();
                embed = message.Embeds.ElementAt(0).ToEmbedBuilder();
                embed.Color = Color.Red;

                var builder = new ComponentBuilder();
                builder.WithButton(label: "Комната матча", url: $"https://www.faceit.com/en/csgo/room/{match.matchID}", style: ButtonStyle.Link);
                builder.WithButton(label: "Closed", customId: "id_Closed", style: ButtonStyle.Danger);

                await closedChannel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                await openChannel.DeleteMessageAsync(match.discordMessageID);
                }

        }
               
            public async Task AlarmAboutTicket(Faceit.MatchData matchData, DiscordSocketClient client, ulong ulChannelID)
            {
            ProgramFaceitBot asd = new ProgramFaceitBot();
                
                
            var embed = new EmbedBuilder()
                    .WithTitle($"Игрок:\t{matchData.ticketCreatedBy}\t зовёт на помощь!\nТикет создан <t:{matchData.ticketCreatedAt / 1000}:R>\nАдминистратор:\t{matchData.ticketTakedByAdmin}")
                    .WithThumbnailUrl(matchData.tournamentAvatarURL)
                    .WithFooter(matchData.matchID, matchData.tournamentAvatarURL);
                //.WithAuthor(Convert.ToString($"<t:{matchData.ticketCreatedAt}:R>"));

                
                int i = 0;
                string strTeam1 = "";
                string strTeam2 = "";

                while (matchData.team1Nicknames[i] != null)
                {
                    strTeam1 = strTeam1 + "\n" + matchData.team1Nicknames[i];
                    strTeam2 = strTeam2 + "\n" + matchData.team2Nicknames[i];
                    i++;
                }

                embed.AddField($"{matchData.team1Name}", $"{strTeam1}", true);
                embed.AddField($"{matchData.team2Name}", $"{strTeam2}", true);
                //embed.AddField("**Комната матча**", "https://www.faceit.com/en/csgo/room/");


                var builder = new ComponentBuilder()
                .WithButton(label: "Комната матча", url: $"https://www.faceit.com/en/csgo/room/{matchData.matchID}", style: ButtonStyle.Link);

                if (matchData.ticketStatus == "inProgress")
                {
                    builder.WithButton(label: "InProgress", customId: "id_InProgress", style: ButtonStyle.Primary);
                    embed.Color = Color.Blue;
                }
                if (matchData.ticketStatus == "open")
                {
                    builder.WithButton(label: "Open", customId: "id_Open", style: ButtonStyle.Success);
                    embed.Color = Color.Green;
                }
                if (matchData.ticketStatus == "closed")
                {
                    builder.WithButton(label: "Closed", customId: "id_Closed", style: ButtonStyle.Danger);
                    embed.Color = Color.Red;
                }

                var channel = client.GetChannel(ulChannelID) as ITextChannel;
                await channel.SendMessageAsync(embed: embed.Build(), components: builder.Build());
                var messages = await channel.GetMessagesAsync(limit: 1).FlattenAsync();
                matchData.discordMessageID = messages.ElementAt(0).Id;

                //var t = new ProgramFaceitBot();
                //t.AddMatchInList(matchData);
            }

            public async Task ChangeTicketStatusToInProgress(DiscordSocketClient client, ulong ulOpenChannelID, ulong ulMsgID, MatchData match)
        {
            var channel = client.GetChannel(ulOpenChannelID) as ITextChannel;
            var message = await channel.GetMessageAsync(ulMsgID) as IUserMessage;

            EmbedBuilder embed = new EmbedBuilder();
            embed = message.Embeds.ElementAt(0).ToEmbedBuilder();

            embed.Color = Color.Blue;

            var builder = new ComponentBuilder();
            builder.WithButton(label: "Комната матча", url: $"https://www.faceit.com/en/csgo/room/{match.matchID}", style: ButtonStyle.Link);
            builder.WithButton(label: "Closed", customId: "id_Closed", style: ButtonStyle.Danger);

            await message.ModifyAsync(msg => msg.Embed = embed.Build());
            await message.ModifyAsync(msg => msg.Components = builder.Build());
        }
        }
    }

