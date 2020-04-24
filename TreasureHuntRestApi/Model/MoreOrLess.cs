


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TreasureHunt.Model
{

  
  // A station at which a game is played.
  public class MoreOrLessQuestion
  {
    public string Id;
    public int QuestionKey;
    public string Category;
    public string QuestionText;
    public int Answer;
    public int RangeLo;
    public int RangeHi;   

  }

  

}