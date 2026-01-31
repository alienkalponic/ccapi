using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Project.Domain.Utility
{
    public class DataBaseManagement
    {
        private SqlConnection _ConnObj;
        private readonly IConfiguration _configuration;
        private List<string> _ExceptionError = new List<string>();
        private string connectionString;
        public DataBaseManagement(IConfiguration configuration, string keyName)
        {

            _configuration = configuration;

            if (keyName.Trim() != string.Empty || keyName.Trim() != null)
            {
                
                _ConnObj = new SqlConnection(_configuration.GetConnectionString(keyName));
            }
            else
            {
                _ExceptionError.Add("AppSettings Key name is missing.");
            }
        }

        public string Select(string procedureName, SqlParameter[] parameters)
        {
            DataTable dataTable1 = new DataTable();

            string jsonResult = string.Empty;
            try
            {
                // Fetch JSON result from the stored procedure
                jsonResult = ExecuteStoredProcedure(procedureName, parameters);

                if (!string.IsNullOrEmpty(jsonResult))
                {
                    // Convert JSON string to DataTable

                    //dataTable1 = ConvertJsonToDataTable(jsonResult);
                }
            }
            catch (JsonException ex)
            {
                _ExceptionError.Add("Select_JsonException. Message - " + ex.Message);
            }
            catch (SqlException ex)
            {
                _ExceptionError.Add("Select_SqlException. Message - " + ex.Message);
            }
            catch (Exception ex)
            {
                _ExceptionError.Add("Select_Exception. Message - " + ex.Message);
            }

            return jsonResult;
        }

        public string ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters)
        {
            string jsonResult = string.Empty;

            using (SqlCommand command = new SqlCommand(procedureName, _ConnObj))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters);
                command.CommandTimeout = 36000;

                try
                {
                    Connect();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                jsonResult = reader.GetString(0); // Assuming the JSON result is in the first column
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    _ExceptionError.Add("ExecuteStoredProcedure_SqlException. Message - " + ex.Message);
                }
                catch (Exception ex)
                {
                    _ExceptionError.Add("ExecuteStoredProcedure_Exception. Message - " + ex.Message);
                }
                finally
                {
                    Disconnect();
                }
            }

            return jsonResult;
        }

        public DataTable ConvertJsonToDataTable(string jsonString)
        {
            DataTable dataTable = new DataTable();
            JArray jsonArray = JArray.Parse(jsonString);

            if (jsonArray.Count > 0)
            {
                // Create DataTable columns
                foreach (JProperty property in jsonArray[0].Children<JProperty>())
                {
                    dataTable.Columns.Add(property.Name);
                }

                // Add DataTable rows
                foreach (JObject obj in jsonArray)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (JProperty property in obj.Properties())
                    {
                        row[property.Name] = property.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        private void Connect()
        {
            if (_ConnObj.State == ConnectionState.Closed || _ConnObj.State == ConnectionState.Broken)
            {
                try
                {
                    _ConnObj.Open();
                }
                catch (SqlException ex)
                {
                    _ExceptionError.Add("ConnectModule_SqlException. Message - " + ex.Message.ToString());
                }
                catch (Exception ex2)
                {
                    _ExceptionError.Add("ConnectModule_Exception. Message - " + ex2.Message.ToString());
                }
            }
        }

        private void Disconnect()
        {
            if (_ConnObj.State == ConnectionState.Open || _ConnObj.State == ConnectionState.Connecting)
            {
                try
                {
                    _ConnObj.Close();
                }
                catch (SqlException ex)
                {
                    _ExceptionError.Add("DisconnectModule_SqlException. Message - " + ex.Message.ToString());
                }
                catch (Exception ex2)
                {
                    _ExceptionError.Add("DisconnectModule_Exception. Message - " + ex2.Message.ToString());
                }
            }
        }

    }
}
