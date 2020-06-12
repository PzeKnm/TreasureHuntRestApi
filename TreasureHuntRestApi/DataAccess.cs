


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using TreasureHunt.Model;

namespace TreasureHunt.Data
{
 
  public class DataAccess
  {

    // private static SqlConnection connection = new SqlConnection();
    private static string sConn = "Server=tcp:sandgateth.database.windows.net,1433;Initial Catalog=TreasureHunt;Persist Security Info=False;User ID=TreasureHuntUser;Password=Treasure2Find;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
  //  private static string sConn = "Server=ASWKNM01\\SQL2008;Initial Catalog=TreasureHunt;Persist Security Info=False;User ID=TreasureHuntUser;Password=Treasure2Find;MultipleActiveResultSets=False;Connection Timeout=30;";

    private static int cInactivityTimoutMin = 10;

    private ILogger _logger;
    public DataAccess(ILogger log)
    {
      _logger = log;
    }

    public List<GameStationDto> GetGameStations()
    {
      List<GameStationDto> lst = new List<GameStationDto>();
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT * FROM TStation ");
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                GameStationDto gs = new GameStationDto();
                gs.Id = reader["ID"].ToString();  
                gs.Name= reader["Name"].ToString();  
                gs.Description = reader["Description"].ToString();  
                gs.Status = reader["Status"].ToString(); 
                gs.LastContactDate = reader.GetNullableDateTime("LastContactDate");
                lst.Add(gs);                              
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

      return lst;
    }

    public void UpdateStationStatusFromLastContactTime()
    {
      List<GameStationDto> lst = GetGameStations();
      foreach(GameStationDto gs in lst)
      {
        if(gs.LastContactDate != null)
        {
          TimeSpan ts = DateTime.UtcNow - (DateTime)gs.LastContactDate;
          if(ts.TotalMinutes > cInactivityTimoutMin)
          {
            string sStationId = GetStationHubDeviceId(gs.Id);
            ResetStationToken(gs.Id);
            UpdateStationStatus(sStationId, "Deactivated");
          }          
        }
      }
    }



    public string GetStationAccessCode(string gameId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT CurrentAuthCode FROM TStation where id = '{0}' ", gameId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["CurrentAuthCode"].ToString();  
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

      return code;
    }


    public string GetGameIdFromHubDeviceId(string hubDeviceId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT ID FROM TStation where HubDeviceId = '{0}' ", hubDeviceId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["ID"].ToString();  
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

      return code;
    }


    public string GetStationToken(string gameId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT CurrentToken FROM TStation where id = '{0}' ", gameId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["CurrentToken"].ToString();  
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

      return code;
    }


    public string GenerateNewStationToken(string gameId)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          using (SqlCommand cmd = new SqlCommand("UPDATE TStation SET CurrentToken = NEWID()  WHERE Id = @Id", conn))
          {
            cmd.Parameters.AddWithValue("@Id", gameId);
            int rows = cmd.ExecuteNonQuery();
            if(rows == 1)
              return GetStationToken(gameId);
          }          
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }

      return "";
    }


    public string ResetStationToken(string gameId)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          using (SqlCommand cmd = new SqlCommand("UPDATE TStation SET CurrentToken = null  WHERE Id = @Id", conn))
          {
            cmd.Parameters.AddWithValue("@Id", gameId);
            int rows = cmd.ExecuteNonQuery();
            if(rows == 1)
              return GetStationToken(gameId);
          }          
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }

      return "";
    }


    // Returns the HubDeviceId for a station
    public string GetStationHubDeviceId(string GameId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT HubDeviceId FROM TStation where id = '{0}' ", GameId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["HubDeviceId"].ToString();  
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

      return code;
    }


    public string GetStationHubDeviceKey(string GameId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT HubDeviceKey FROM TStation where id = '{0}' ", GameId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["HubDeviceKey"].ToString();  
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

      return code;
    }


    public string GetStationHubDeviceKeyFromStationHubId(string StationHubId)
    {
      string code = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT HubDeviceKey FROM TStation where HubDeviceId = '{0}' ", StationHubId);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                code = reader["HubDeviceKey"].ToString();  
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

      return code;
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

    public bool UpdateStationStatus(string stationId, string status)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("UPDATE TStation SET Status = @Status WHERE HubDeviceId = @Id", conn))
            {
              cmd.Parameters.AddWithValue("@Id", stationId);
              cmd.Parameters.AddWithValue("@Status", status);
              int rows = cmd.ExecuteNonQuery();
              return (rows == 1);
            }
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }
      return false;
    }  

    public bool UpdateStationAccessCode(string stationId, string authCode)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("UPDATE TStation SET CurrentAuthCode = @AuthCode WHERE HubDeviceId = @Id", conn))
            {
              cmd.Parameters.AddWithValue("@Id", stationId);
              cmd.Parameters.AddWithValue("@AuthCode", authCode);
              int rows = cmd.ExecuteNonQuery();
              return (rows == 1);
            }
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }
      return false;
    }  


    public bool UpdateStationHeartbeat(string stationId)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand("UPDATE TStation SET LastContactDate = GetDate() WHERE HubDeviceId = @Id", conn))
            {
              cmd.Parameters.AddWithValue("@Id", stationId);
              int rows = cmd.ExecuteNonQuery();
              return (rows == 1);
            }
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }
      return false;
    }  



    public MoreOrLessQuestion GetMoreOrLessQuestion(List<int> lstExclude)
    {
      MoreOrLessQuestion q = new MoreOrLessQuestion();
      
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT top 1 * FROM TMoreOrLessQuestions  ");

          if(lstExclude.Count > 0)
          {
            sQry += "where QuestionKey not in (";
            bool bAddComma = false;
            foreach(int n in lstExclude)
            {
              if(bAddComma)
                sQry += ", ";
              sQry += n.ToString();
              bAddComma = true;
            }

            sQry += ") ";

          }
          sQry += " ORDER BY NEWID()";
          

          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                q.Id = reader["ID"].ToString();  
                q.QuestionKey = reader.GetInt("QuestionKey"); 
                q.Category = reader["Category"].ToString();  
                q.QuestionText = reader["QuestionText"].ToString();
                q.Answer = reader.GetInt("Answer");  
                q.RangeLo = reader.GetInt("RangeLo");  
                q.RangeHi = reader.GetInt("RangeHi");                                             
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
           
      return q; 
    }
  

    
    
    public string GetSetting(string SettingName)
    {
      string value = "";
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT Value FROM TSettings where name = '{0}' ", SettingName);
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                value = reader["Value"].ToString();  
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

      return value;
    }
    
    public List<UserDto> GetUsers()
    {
      List<UserDto> lst = new List<UserDto>();
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = string.Format("SELECT * FROM TUser ");
          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
              while (reader.Read())
              {
                UserDto u = new UserDto();
                u.Id = reader["ID"].ToString(); 
                u.Name= reader["Name"].ToString();  
                u.UserAgent = reader["UserAgent"].ToString();  
                u.IPAddr = reader["IPAddr"].ToString(); 
                u.LastContactDate = reader.GetNullableDateTime("LastContactDate");
                lst.Add(u);                              
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

      return lst;
    }

    
    public void AddOrUpdateUser(string UserId, string UserAgent, string ipaddr)
    {
      try
      {
        using (SqlConnection conn = new SqlConnection(sConn))
        {
          conn.Open();
          string sQry = "";
          sQry += " IF NOT EXISTS (SELECT 1 FROM TUser WHERE Id=@Id)   ";
          sQry += "   INSERT INTO TUser (id, useragent, IPAddr, LastContactDate) VALUES(@Id, @UserAgent, @IPAddr, @LastContactDate)  ";
          sQry += " ELSE  ";
          sQry += "   UPDATE TUser SET useragent=@UserAgent, IPAddr = @IPAddr, LastContactDate = @LastContactDate WHERE Id=@Id ";

          using (SqlCommand cmd = new SqlCommand(sQry, conn))
          {
            cmd.Parameters.AddWithValue("@Id", UserId);
            cmd.Parameters.AddWithValue("@UserAgent", UserAgent);
            cmd.Parameters.AddWithValue("@IPAddr", ipaddr);
            cmd.Parameters.AddWithValue("@LastContactDate", DateTime.Now);
            int rows = cmd.ExecuteNonQuery();
            return;
          }
        }
      }
      catch (SqlException ex)
      {
        _logger.LogInformation(ex.Message);
      }
      return;
    }  
  }



}