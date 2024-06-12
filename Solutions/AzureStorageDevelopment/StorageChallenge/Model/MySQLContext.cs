using CSSTDModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using MySqlConnector;
namespace CSSTDSolution.Models
{
    #region "Instructions (all challenges)"    
    /*     
    *  1. The connection string for the MySQL database is passed into the contructor and the ConnectionString property is set to its value     
    *  2. Create a table to hold VendorData information. Depending on the framework you are using this might not be necessary     
    *  3. Implement LoadData to upload a collection of VendorData instances to the database     
    *  4. Retrieve a collection of VendorData instances from the database     
    *      * 
    */
    #endregion
    #region "Data structures"    
    /*        namespace CSSTDModels        {             
         [Description("Simple class representing basic vendor data for an Azure database for MySQL")]           
          public class VendorData            {                [Description("Arbitrary primary key")]             
             [Key]               
              public int ID { get; set; }                 
               [Description("Vendor name")]           
                  public string Name { get; set; }                
                    [Description("Industry designation, currently 'Training' or 'Swimming'")]               
                     public string Industry { get; set; }            }        }     
                     * */
    #endregion
    public class MySQLContext : IMySQLContext
    {
        public MySQLContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public string ConnectionString { get; set; }
        public void CreateTable(string tableName)
        {
            var SQL = $"DROP TABLE IF EXISTS {tableName};" + $"CREATE TABLE {tableName}(ID INT, Name VARCHAR(100), Industry VARCHAR(100));" + $"DELETE FROM {tableName};";
            using (var conn = new MySqlConnection(this.ConnectionString))
            {
                using (var cmd = new MySqlCommand(SQL, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
        public List<VendorData> GetData(string tableName)
        {
            List<VendorData> results = new List<VendorData>(); var SQL = $"SELECT * FROM {tableName};";
            using (var conn = new MySqlConnection(this.ConnectionString))
            {
                using (var cmd = new MySqlCommand(SQL, conn))
                {
                    conn.Open(); var rdr = cmd.ExecuteReader(); while (rdr.Read())
                    {
                        results.Add(new VendorData
                        {
                            ID = (int)rdr["ID"],
                            Name = rdr["Name"].ToString(),
                            Industry = rdr["Industry"].ToString()
                        });
                    }
                    conn.Close();
                }
            }
            return results;
        }
        public void LoadData(List<VendorData> vendors, string tableName)
        {
            var SQL = $"INSERT INTO {tableName}(ID,Name,Industry) VALUES(@ID,@Name,@Industry);"; using (
                var conn = new MySqlConnection(this.ConnectionString))
            {
                using (var cmd = new MySqlCommand(SQL, conn))
                {
                    conn.Open();
                    cmd.Parameters.Add("@ID", MySqlDbType.Int32);
                    cmd.Parameters.Add("@Name", MySqlDbType.String);
                    cmd.Parameters.Add("@Industry", MySqlDbType.String);
                    foreach (var vendor in vendors)
                    {
                        cmd.Parameters["@ID"].Value = vendor.ID;
                        cmd.Parameters["@Name"].Value = vendor.Name;
                        cmd.Parameters["@Industry"].Value = vendor.Industry;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
}
