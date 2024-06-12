using CSSTDEvaluation;
using CSSTDModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
#region "Instructions (all challenges)"
/* 
*  1. The connection string for the SQL Server database is passed into the contructor and the ConnectionString property is set to its value 
*  2. Create a table to hold CustomerData information. Depending on the framework you are using this might not be necessary 
*  3. Implement LoadData to upload a collection of CustomerData instances to the database 
*  4. Retrieve a collection of CustomerData instances from the database 
*  * 
*/
#endregion
#region "Data structures"
/* namespace CSSTDModels{     
     [Description("Simple class representing basic customer data for an Azure SQL database")]   
      public class CustomerData    {        [Description("Arbitrary primary key")]       
       [Key]        public int ID { get; set; }          [Description("Customer name")]       
        public string Name { get; set; }          [Description("Customer postal/zip code")]      
          public string PostalCode { get; set; }    }}* 
          */
#endregion
namespace CSSTDSolution.Models
{
    public class SQLServerContext : ISQLServerContext
    {
        public SQLServerContext(string connectionString) : base()
        {
            this.ConnectionString = connectionString;
        }
        public string ConnectionString { get; set; }
        public void CreateTable(string tableName)
        {
            var SQL = $"DROP TABLE IF EXISTS {tableName};" + $"CREATE TABLE {tableName}(ID int, Name VARCHAR(500), PostalCode VARCHAR(500));" + $"DELETE FROM {tableName};"; using (var conn = new SqlConnection(this.ConnectionString)) { using (var cmd = new SqlCommand(SQL, conn)) { conn.Open(); cmd.ExecuteNonQuery(); conn.Close(); } }
        }
        public List<CustomerData> GetData(string tableName)
        {
            List<CustomerData> results = new List<CustomerData>(); var SQL = $"SELECT * FROM {tableName};";
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                using (var cmd = new SqlCommand(SQL, conn))
                {
                    conn.Open(); var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        results.Add(new CustomerData
                        {
                            ID = (int)rdr["ID"],
                            Name = rdr["Name"].ToString(),
                            PostalCode = rdr["PostalCode"].ToString()
                        });
                    }
                    conn.Close();
                }
            }
            return results;
        }
        public void LoadData(List<CustomerData> customers, string tableName)
        {
            var SQL = $"INSERT INTO {tableName}(ID,Name,PostalCode) VALUES(@ID,@Name,@PostalCode);";
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                using (var cmd = new SqlCommand(SQL, conn))
                {
                    conn.Open(); cmd.Parameters.Add(new SqlParameter("@ID", System.Data.SqlDbType.Int));
                    cmd.Parameters.Add(new SqlParameter("@Name", System.Data.SqlDbType.VarChar, 500));
                    cmd.Parameters.Add(new SqlParameter("@PostalCode", System.Data.SqlDbType.VarChar, 500));
                    foreach (var customer in customers)
                    {
                        cmd.Parameters["@ID"].Value = customer.ID;
                        cmd.Parameters["@Name"].Value = customer.Name; cmd.Parameters["@PostalCode"].Value = customer.PostalCode;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
}
