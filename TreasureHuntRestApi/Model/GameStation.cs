


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TreasureHunt.Model
{

  
  // A station at which a game is played.
  public class GameStation
  {
    public string Id;
    public string Name;
    public string Description;   
    public string CurrentAuthCode;    
    public string CurrentAuthToken;    

  }


  public class GameStationDto
  {
    public string Id;
    public string Name;
    public string Description; 

    public string Status; 
    public DateTime? LastContactDate;    
  }
  

}