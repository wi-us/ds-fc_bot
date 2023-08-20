using Discord.WebSocket;
using System.Net;
using static Discord_Faceit_Bot.Models.Faceit;

namespace Discord_Faceit_Bot.Models
{
    public class Faceit
    {
        public class OrganizersChampionships
        {
            public string strHubName { get; set; }
            public string strHubID { get; set; }
            public string strHubURL { get; set; }

            public string strHubAvatarURL { get; set; }
        }
            public class MatchData
        {
            public string matchID { get; set; } // payload.id
            public string matchURL { get; set; } // $"https://www.faceit.com/en/room/{matchID}"
            public string game { get; set; }    //payload.game
            public string region { get; set; }  //payload.region
            public string providerType { get; set; } //payload.entity.type
            public string providerTypeID { get; set; } //payload.entity.id
            public string providerName { get; set; } //payload.entity.name
            public string team1Name { get; set; } //payload.teams.faction1.name
            public string[] team1Nicknames { get; set; } = new string[5]; //payload.teams.faction1.nickname
            public string team2Name { get; set; } //payload.teams.faction2.name
            public string[] team2Nicknames { get; set; } = new string[5]; //payload.teams.faction2.nickname
            public string matchType { get; set; } //payload.matchCustom.grouping_stats
            public string tournamentAvatarURL { get; set; }

            public string ticketStatus { get; set; } //tickets.status
            public string ticketCreatedBy { get; set; } //tickets.status
            public string ticketTakedByAdmin { get; set; } //tickets.status
            public ulong ticketCreatedAt { get; set; } //tickets
            public string ticketUpdatedAt { get; set; }
            public ulong discordMessageID { get; set; }
            public SocketMessage socketMessage { get; set; }
            public bool isPosted { get; set; }
        }

        public List<OrganizersChampionships> HubsLinks(string strOrgURL, string strToken)
        {
            dynamic events_body = WebRequests.GetJSONResponseFromWebsite(strOrgURL, strToken);
            List<OrganizersChampionships> champs = new List<OrganizersChampionships>();
            foreach (var events in events_body.payload.items)
            {
                var listEvents = new OrganizersChampionships();
                listEvents.strHubName = events.name.ToString();
                listEvents.strHubID = events.guid.ToString();
                listEvents.strHubURL = $"https://www.faceit.com/en/hub/{events.guid}1xBet%20CyberLeague%20%7C%20CS:GO%201x1";
                listEvents.strHubAvatarURL = events.avatar.ToString();
                champs.Add(listEvents);
            }

            return champs;
        }

        public List<OrganizersChampionships> TournamentsLinks(string strOrgURL, string strToken)
        {
            dynamic events_body = WebRequests.GetJSONResponseFromWebsite(strOrgURL, strToken);
            List<OrganizersChampionships> champs = new List<OrganizersChampionships>();
            foreach (var events in events_body.payload.items)
            {
                var listEvents = new OrganizersChampionships();
                listEvents.strHubName = events.name.ToString();
                listEvents.strHubID = events.id.ToString();
                listEvents.strHubURL = $"https://www.faceit.com/en/championship/{events.guid}1xBet%20%7C%201x1%20FAST%20CUP%20%7C%20Headshots%20Only%20%7C%2030%20USD";
                //listEvents.strHubAvatarURL = events.avatar.ToString();
            }
            return champs;
        }

        public List<string> GetTicketsJson(string strHubURL, string strToken)
        {
            dynamic matches_body = WebRequests.GetJSONResponseFromWebsite(strHubURL, strToken);
            var matches = new List<string>();
            foreach (var matchLink in matches_body.tickets)
            {
                matches.Add($"{matchLink.match.guid.ToString()}");
            }
            return matches;
        }

        public MatchData CollectDataAboutFaceitMatch(dynamic jsonResponse, OrganizersChampionships org)
        {
            MatchData data = new MatchData();

            data.matchID = jsonResponse.payload.id;
            data.matchURL = $"https://www.faceit.com/en/room/{data.matchID}";
            data.game = jsonResponse.payload.game;
            data.region = jsonResponse.payload.region;
            data.providerType = jsonResponse.payload.entity.type;
            data.providerTypeID = jsonResponse.payload.entity.id;
            data.providerName = jsonResponse.payload.entity.name;
            data.team1Name = jsonResponse.payload.teams.faction1.name;
            data.team2Name = jsonResponse.payload.teams.faction2.name;
            data.tournamentAvatarURL = org.strHubAvatarURL;
            int i = 0;



            foreach (var players in jsonResponse.payload.teams.faction1.roster)
            {
                data.team1Nicknames[i] = players.nickname;
                i++;
            }
            i = 0;
            foreach (var players in jsonResponse.payload.teams.faction2.roster)
            {
                data.team2Nicknames[i] = players.nickname;
                i++;
            }

            data.matchType = jsonResponse.payload.matchCustom.overview.grouping_stats;

            //matchData.Add(jsonResponse.payload.id.ToString());
            //matchData.Add(jsonResponse.payload.game);
            //matchData.Add(jsonResponse.payload.region);
            //matchData.Add(jsonResponse.payload.entity.type);
            //matchData.Add(jsonResponse.payload.entity.id);
            //matchData.Add(jsonResponse.payload.entity.name);
            //matchData.Add(jsonResponse.payload.teams.faction1.name);
            //matchData.Add(jsonResponse.payload.teams.faction2.name);

                TicketStatus(data);
            return data;
        }

        public void TicketStatus(in MatchData data)
        {
            dynamic wr = WebRequests.GetJSONResponseFromWebsite($"https://api.faceit.com/tickets/v1/ticket?competitionGuid={data.providerTypeID}&competitionType={data.providerType}&status=open,inProgress&offset=0&limit={100}", "Bearer 024ed3e4-b288-45c3-aff4-229197e86fb3", true);
            
                foreach (var item in wr.tickets)
            {
                if (item.match.guid.ToString() == data.matchID.ToString())
                {
                    if (item.status.ToString() != null)
                    {
                        data.ticketStatus = item.status.ToString();
                        data.ticketCreatedBy = item.createdBy.nickname.ToString();
                        data.ticketCreatedAt = Convert.ToUInt64(item.createdAt);
                        //matchData.ticketCreatedAt = ConvertTime(Convert.ToInt64(matchData.ticketCreatedAt));
                        if (data.ticketStatus.ToString() == "inProgress")
                        {
                            data.ticketTakedByAdmin = item.assignee.nickname.ToString();
                            break;
                        }
                        if (item.assignee == null)
                        {
                            data.ticketTakedByAdmin = "Waitng for admin";
                        }
                        else
                        {
                            data.ticketTakedByAdmin = item.assignee.nickname.ToString();
                        }
                    }
                    else
                    {
                        data.ticketStatus = "Closed";
                        if (item.assignee.ToString() == null)
                        {
                            data.ticketTakedByAdmin = "Closed";
                        }
                    }
                }
            }
        }
    }

}
