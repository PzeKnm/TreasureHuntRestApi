
using System;
using System.Data.SqlClient;


namespace TreasureHunt.Data
{
  public static class ReaderExtensions {

    public static DateTime? GetNullableDateTime(this SqlDataReader reader, string name){ 
        var col = reader.GetOrdinal(name);
        return reader.IsDBNull(col) ? 
                    (DateTime?)null :
                    (DateTime?)reader.GetDateTime(col);
    }

    public static int GetInt(this SqlDataReader reader, string name){ 
        var col = reader.GetOrdinal(name);
        return reader.GetInt32(col);
    }

  }
}

