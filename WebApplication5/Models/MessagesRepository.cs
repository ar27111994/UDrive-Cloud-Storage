using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication5.Hubs;
using WebApplication5.Models;
namespace WebApplication5.Models
{
    public class MessagesRepository
    {
        readonly string _connString =
        ConfigurationManager.ConnectionStrings["conn"].ConnectionString;

        public IEnumerable<Messages> GetAllMessages()
        {
            var messages = new List<Messages>();
            using (var connection = new SqlConnection(_connString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"SELECT [MessageID], 
                [Message], [EmptyMessage], [MessageDate] FROM [dbo].[Messages]", connection))
                {
                    command.Notification = null;

                    var dependency = new SqlDependency(command);
                    dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {


                        messages.Add(item: new Messages
                        {
                            MessageID = (int)reader["MessageID"],

                            Message = (string)reader["Message"],
                            EmptyMessage = reader["EmptyMessage"] != DBNull.Value ?
                        (string)reader["EmptyMessage"] : "",
                            MessageDate = Convert.ToDateTime(reader["MessageDate"])
                        });
                    }
                }
            }
            
            return messages;
        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                MessagesHub.SendMessages();
            }
        }
    }
}
