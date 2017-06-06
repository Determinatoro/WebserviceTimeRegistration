using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace WebserviceTimeRegistration.Webservice
{
    public class WebserviceObject
    {
        private bool success;
        private object errorMessage;
        private object response;

        public WebserviceObject(bool success, object errorMessage, object response)
        {
            this.success = success;
            this.errorMessage = errorMessage;
            this.response = response;
        }

        public bool Success
        {
            get
            {
                return success;
            }

            set
            {
                success = value;
            }
        }

        public object ErrorMessage
        {
            get
            {
                return errorMessage;
            }

            set
            {
                errorMessage = value;
            }
        }

        public object Response
        {
            get
            {
                return response;
            }

            set
            {
                response = value;
            }
        }
    }

    public class WebserviceHelper
    {
        public static void WriteResponse(HttpContext context, bool success, object response)
        {
            WebserviceObject wsObj;

            if (success)
                wsObj = new WebserviceObject(success, "", response);
            else
                wsObj = new WebserviceObject(success, response, "");

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Write(new JavaScriptSerializer().Serialize(wsObj));
        }

        public static List<string> GetObjectData(SqlDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            List<string> dataList = new List<string>();

            foreach (var item in columns)
                dataList.Add(reader[item].ToString());

            return dataList;
        }

        public static SqlConnection GetDatabaseConnection()
        {
            string conString = ConfigurationManager.ConnectionStrings["DbConnection"].ToString();
            return new SqlConnection(conString);
        }
    }
}