using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Reactive;
using System.Linq;
using System.Data.Common;
using Discord_Faceit_Bot.Models;
using Discord.Net;
using static Discord_Faceit_Bot.Models.Faceit;
using System.IO;

namespace Programm
{

    public class ProgramFaceitBot
    {
        Faceit faceit = new Faceit();
        public DiscordSocketClient client;
        public ulong openTicketsChannel = 988730504348045355;
        public ulong closedTicketsChannel = 1141842318060695572;
        public static Timer timer;
        public static Timer _timer;
        List<Faceit.MatchData> matchData = new List<Faceit.MatchData>();
        List<Faceit.MatchData> _matchData = new List<Faceit.MatchData>();
        List<Faceit.MatchData> postedMessages = new List<Faceit.MatchData>();
        List<string> tickets = new List<string>();
        public List<OrganizersChampionships> hubs = new List<OrganizersChampionships>();
        public List<OrganizersChampionships> tournaments = new List<OrganizersChampionships>();

        List<string>__matches = new List<string>();
        List<string> __msgs = new List<string>();
        List<DiscordMessage> __message = new List<DiscordMessage>();
        static void Main(string[] args) => new ProgramFaceitBot().MainAsync().GetAwaiter().GetResult();
        string strBearer = "Bearer 024ed3e4-b288-45c3-aff4-229197e86fb3";

        private async Task MainAsync()
        {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent;
            client = new DiscordSocketClient(config);
            client.MessageReceived += CommandHandler;
            client.Log += Log;
            //client.LoggedIn += Start;
            client.Ready += Start;



            var token = "MTEyMjU3MjU3MDY1MTI4MzY3Nw.GqqR8q.ekjGg4aahpO3UlfgpVdurKxjpG-QBEv8Q7AXqU";


            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            Console.ReadLine();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
        private Task CommandHandler(SocketMessage msg)
        {

            if (!msg.Author.IsBot)
            {
                switch (msg.Content)
                {
                    case "!ticket":
                        {
                            //DoMessage(msg);
                            //MessageAboutTicket(msg);

                            break;
                        }
                    case "!test":
                        {
                            //SendTickets(msg);
                            break;
                        }
                    case "!msg":
                        {
                            //Foo(msg);
                            break;
                        }
                }

            }
            return Task.CompletedTask;
        }
        private async Task Start()
        {
            UpdateOrganizerChampionships(1);
            //DoSmth();
            Timer();
        }
        private async Task Timer()
        {
            int n = 0;
            int k = 0;

            TimerCallback tm = new TimerCallback(UpdateTicketsStatus);
            TimerCallback _tm = new TimerCallback(UpdateOrganizerChampionships);
            //каждые 10 секунд вызывается функция UpdateTicketsStatus()
            timer = new Timer(tm, n, 0, 10000);
            _timer = new Timer(tm, k, 0, 86400000);
        }


        private void UpdateOrganizerChampionships(object obj)
        {
            Faceit faceit = new Faceit();
            hubs = faceit.HubsLinks("https://api.faceit.com/hubs/v1/hub?organizerGuid=131082c7-db11-4b05-b393-68c3c2abc596&sort=asc&offset=0&limit=100", strBearer);
            tournaments = faceit.TournamentsLinks("https://api.faceit.com/championships/v1/championship?organizerId=131082c7-db11-4b05-b393-68c3c2abc596&status=started,created,join,checkin,seeding,adjustment,scheduling&sort=asc&offset=0&limit=10", strBearer);

        }
        private void UpdateTicketsStatus(object obj)
        {
            CollectTickets();
        }
        private async Task CollectTickets()
        {

            DiscordFunctions ds = new DiscordFunctions();
            await Task.Run(() =>
            {
                foreach (var hub in hubs)
                {
                    string strHubTicketsLink = $"https://api.faceit.com/tickets/v1/ticket?competitionGuid={hub.strHubID}&competitionType=hub&status=open,inProgress&offset=0&limit=100";
                    foreach (var match in faceit.GetTicketsJson(strHubTicketsLink, strBearer))
                    {
                        //для каждого матча запрашиваем его JSON для получения всей информации о нём
                        string matchUrl = $"https://api.faceit.com/match/v2/match/{match}";
                        dynamic match_body = Discord_Faceit_Bot.Models.WebRequests.GetJSONResponseFromWebsite(matchUrl, strBearer, true);
                        var mtchData = faceit.CollectDataAboutFaceitMatch(match_body, hub);

                        if (matchData.Count != 0)
                        {
                            Console.WriteLine("\n" + mtchData.matchID);
                            for (int i = 0; i < matchData.Count; i++)
                            {
                                if (matchData.ElementAt(i).matchID == mtchData.matchID)
                                {
                                    Console.WriteLine("\t" + matchData.ElementAt(i).matchID);
                                    //matchData[i] = mtchData;
                                    if(matchData.ElementAt(i).ticketStatus == "inProgress")
                                    {
                                        ds.ChangeTicketStatusToInProgress(client, 1142428911645491270, matchData.ElementAt(i).discordMessageID, matchData.ElementAt(i));
                                    }
                                    break;
                                }
                                else if (i == matchData.Count - 1)
                                {
                                    Console.WriteLine("\t~" + matchData.ElementAt(i).matchID);
                                    matchData.Add(mtchData);
                                }


                            }
                        }
                        else
                        {
                            matchData.Add(mtchData);
                        }

                        //создаём экземпляр класса DiscordFunctions и отправляем Embed сообщение в канал
                        //var ticketMessage = new DiscordFunctions();
                    }
                }
            });
            foreach (var champ in tournaments)
            {
                string strHubTicketsLink = $"https://api.faceit.com/tickets/v1/ticket?competitionGuid={champ.strHubID}&competitionType=hub&status=open,inProgress&offset=0&limit=100";
                foreach (var match in faceit.GetTicketsJson(strHubTicketsLink, strBearer))
                {
                    //для каждого матча запрашиваем его JSON для получения всей информации о нём
                    string matchUrl = $"https://api.faceit.com/match/v2/match/{match}";
                    dynamic match_body = Discord_Faceit_Bot.Models.WebRequests.GetJSONResponseFromWebsite(matchUrl, strBearer, true);
                    var mtchData = faceit.CollectDataAboutFaceitMatch(match_body, champ);

                    if (matchData.Count != 0)
                    {
                        Console.WriteLine("\n" + mtchData.matchID);
                        for (int i = 0; i < matchData.Count; i++)
                        {
                            if (matchData.ElementAt(i).matchID == mtchData.matchID)
                            {
                                Console.WriteLine("\t" + matchData.ElementAt(i).matchID);
                                //matchData[i] = mtchData;
                                if (matchData.ElementAt(i).ticketStatus == "inProgress")
                                {
                                    ds.ChangeTicketStatusToInProgress(client, 1142428911645491270, matchData.ElementAt(i).discordMessageID, matchData.ElementAt(i));
                                }
                                break;
                            }
                            else if (i == matchData.Count - 1)
                            {
                                Console.WriteLine("\t~" + matchData.ElementAt(i).matchID);
                                matchData.Add(mtchData);
                            }


                        }
                    }
                    else
                    {
                        matchData.Add(mtchData);
                    }

                    //создаём экземпляр класса DiscordFunctions и отправляем Embed сообщение в канал
                    //var ticketMessage = new DiscordFunctions();
                }
            
            }
        
            

            var channel = client.GetChannel(1142428911645491270) as ITextChannel;
            var messages = await channel.GetMessagesAsync(100).FlattenAsync();

            foreach(var message in messages)
            {
                DiscordMessage dm = new DiscordMessage()
                {
                    messageID = message.Id,
                    ticketID = message.Embeds.ElementAt(0).Footer.Value.Text
                };
                __message.Add(dm);
                __msgs.Add(message.Embeds.ElementAt(0).Footer.Value.Text);
            }
            foreach(var matches in matchData)
            {
                __matches.Add(matches.matchID);
            }
            var newTickets = __matches.Except(__msgs);
            var closedTickets = __msgs.Except(__matches);

            if(newTickets.Count() != 0)
            {
                foreach (var tickets in newTickets)
                {
                    Console.WriteLine($"new: {tickets}");
                    var data = await GetMatchDataByMatchID(tickets);

                    ds.AlarmAboutTicket(data, client, 1142428911645491270);
                }
            }
            if(closedTickets.Count() != 0)
            {
                foreach (var tickets in closedTickets)
                {
                    Console.WriteLine($"closed: {tickets}");
                    MatchData data = new MatchData();
                    //IMessage im;
                    data.matchID = tickets;
                    //var data = await GetMatchDataByMatchID(tickets);
                    foreach (var msg in messages)
                    {
                        if(msg.Embeds.ElementAt(0).Footer.Value.Text == tickets)
                        {
                            //EmbedBuileder embed = msg.Embeds.ElementAt(0).ToEmbedBuilder();
                            data.discordMessageID = msg.Id;
                            //im = msg;
                            break;
                        }
                    }
                    ds.CloseTicket(1142428911645491270, 1141842318060695572, client, data);

                }
            }
            matchData.Clear();
            __message.Clear();
            __matches.Clear();
            __msgs.Clear(); 

        }

        private async Task<MatchData> GetMatchDataByMatchID(string strMatch)
        {
            //List<MatchData>data = new List<MatchData>();
            List<string>str = new List<string>();
            MatchData data = new MatchData();
            data = matchData.Find(
            delegate (MatchData md)
            {
                return md.matchID == strMatch;
            });


            if(data == null)
            {
                return null;
            }
            else
            {
                return data;
            }
        }
    }
}