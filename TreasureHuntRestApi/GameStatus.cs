


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TreasureHunt.Model;

namespace TreasureHunt.Data
{
 
  public class GameStateFuncs
  {

    public static bool IsValidGameState(string s)
    {
      if(s == "Initialised")
        return true;
      if (s == "Activated")
        return true;
      if (s == "Online_Ready")
        return true;
      if (s == "Online_Dormant")
        return true;
      if (s == "Online_Demo")
        return true;
      if (s == "Authenticating")
        return true;
      if (s == "PreGame")
        return true;
      if (s == "GamePlaying")
        return true;
      if (s == "PostGame")
        return true;
      if (s == "Deactivated")
        return true;

      return false;
    }

    /*
     * 
     * 
     *   public enum GameManagerState
  {
    Initialised = 0,  // Started
    Activated,        // Registered at server, but not available for clients
    Online_Ready,     // Registered at server, and available for clients
    Online_Dormant,   // Registered at server, and available for clients, low activity mode
    Online_Demo,      // Registered at server, and available for clients, high activity mode
    Authenticating,   // Access Code generated, waiting for client to send code
    PreGame,          // |
    GamePlaying,      // |  - Client connected
    PostGame,         // |
    Deactivated       // 
  } 


    public string GetStationStatus(string StationHubId)
    {
      string status = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT Status FROM TStation where HubDeviceId = '{0}' ", StationHubId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                status = reader["Status"].ToString();  
              }
            }
            
            reader.Close();
          }
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }

      return status;
    }
    */


  }



}