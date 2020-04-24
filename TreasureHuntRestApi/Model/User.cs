


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TreasureHunt.Model
{

  
  // A player of a game
  public class UserDto
  {
    public string Id;
    public string Name;
    public string UserAgent; 
    public string IPAddr; 
    public DateTime? LastContactDate;    
  }
  

}