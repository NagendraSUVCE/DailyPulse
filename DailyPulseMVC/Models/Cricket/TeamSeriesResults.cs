using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


public class TeamSeriesResults
{
    public string TeamSeriesMasterLink { get; set; }
    public string SeriesTournamentName { get; set; }
    public string SeriesTournamentLink { get; set; }
    public string Season { get; set; }
    public string Winner { get; set; }
    public string Margin { get; set; }
}

public class BattingInnings
{
    public string PlayerName { get; set; }
    public string PlayerIndexCI { get; set; }
    public string ForTeam { get; set; }
    public string ForTeamIndexCI { get; set; }
    public int Runs { get; set; }
    public string RunsNotOut { get; set; }
    public int Mins { get; set; }
    public int BallsFaced { get; set; }
    public int Four4s { get; set; }
    public int Sixers { get; set; }
    public decimal StrikeRate { get; set; }
    public int Inns { get; set; }
    public string OppTeam { get; set; }
    public string OppTeamIndexCI { get; set; }
    public string Ground { get; set; }
    public string GroundIndexCI { get; set; }
    public string DateString { get; set; }
    public DateTime StartDate { get; set; }
    public string MatchIndexCI { get; set; }
    public string Temp { get; set; }
    public string TempAgain { get; set; }
}


public class BowlingInnings
{
    public string PlayerName { get; set; }
    public string PlayerIndexCI { get; set; }
    public string ForTeam { get; set; }
    public string ForTeamIndexCI { get; set; }
    public string Desc { get; set; }
    public decimal Overs { get; set; }
    public int Maidens { get; set; }
    public int Runs { get; set; }
    public int Wickets { get; set; }
    public decimal Econ { get; set; }
    public int Inns { get; set; }
    public string OppTeam { get; set; }
    public string OppTeamIndexCI { get; set; }
    public string Ground { get; set; }
    public string GroundIndexCI { get; set; }
    public string DateString { get; set; }
    public DateTime StartDate { get; set; }
    public string MatchIndexCI { get; set; }
    public string Temp { get; set; }
    public string TempAgain { get; set; }
}

public class TeamResultScore
{
    public string Team { get; set; }
    public string TeamIndexCI { get; set; }
    public string Score { get; set; }
    public string Overs { get; set; }
    public string RPO { get; set; }
    public int Inns { get; set; }
    public string Result { get; set; }
    public string OppTeam { get; set; }
    public string OppTeamIndexCI { get; set; }
    public string Ground { get; set; }
    public string GroundIndexCI { get; set; }
    public string DateString { get; set; }
    public DateTime StartDate { get; set; }
    public string MatchIndexCI { get; set; }
    public string Temp { get; set; }
    public string TempAgain { get; set; }
}

public class Pagination
{
    public String CurrentPage { get; set; }
    public String TotalPages { get; set; }
    public String PageSize { get; set; }
    public String TotalRecords { get; set; }
}