using MySql.Data.MySqlClient;
using Orchard.Logging;
using Orchard.SuiteCRM.Connector.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.Globalization;
using System.Collections.Specialized;
using System.Text;

namespace Orchard.SuiteCRM.Connector.Services
{
    public static class Helper
    {
        public const string ContactsModuleName = "Contacts";

        public static bool IsDatabaseConnectionProvided(IOrchardServices services, ILogger logger)
        {
            bool isConnectionAvailable = false;
            try
            {
                using (var connection = Helper.GetConnection(services, logger))
                {
                    connection.Open();
                    isConnectionAvailable = connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                isConnectionAvailable = false;
            }

            return isConnectionAvailable;
        }

        public static string GetTaskAddressInSuiteCRM(IOrchardServices orchardServices, string taskId, string taskType)
        {
            return GetSuiteCRMLink(orchardServices, taskId, taskType);
        }
       
        public static string GetProjectAddressInSuiteCRM(IOrchardServices orchardServices, string projectId)
        {
            return GetSuiteCRMLink(orchardServices, projectId, "Project");
        }
        
        public static string GetContactAddressInSuiteCRM(IOrchardServices orchardServices, string contactId)  
        {
            return GetSuiteCRMLink(orchardServices, contactId, ContactsModuleName);
        }

        private static string GetSuiteCRMLink(IOrchardServices orchardServices, string id, string module)
        {
            var setting = orchardServices.WorkContext.CurrentSite.As<SuiteCRMSettingPart>();

            if (setting == null || string.IsNullOrEmpty(setting.WebAddress) || string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            string baseAddress = setting.WebAddress.ToLower(CultureInfo.InvariantCulture);

            if (baseAddress.EndsWith("index.php"))
            {
                baseAddress = baseAddress + "?";
            }
            else if (!baseAddress.Contains("index.php?"))
            {
                baseAddress += "/index.php?";
            }

            if (module == "Tasks")
            {
                //http://localhost/suitecrm/index.php?action=ajaxui#ajaxUILoc=index.php%3Fmodule%3DTasks%26action%3DDetailView%26record%3D0b7b60ba-35c4-42c7-9007-b5def821ed04
                return string.Format("{0}action=ajaxui#ajaxUILoc=index.php%3Fmodule%3D{1}%26action%3DDetailView%26record%3D{2}",
                    baseAddress, module, id);
            }

            NameValueCollection queryString = new NameValueCollection();
            queryString.Add("module", module);
            queryString.Add("action", "DetailView");
            queryString.Add("record", id);
            return baseAddress + ToQueryString(queryString);
        }

        /// <summary>
        /// Sync dialog will be added to the layout, because we want to render it once.
        /// </summary>
        public static void RenderSyncDialogs(WorkContext context, dynamic shapeHelper, dynamic model)
        {
            string dialogShapeName = "syncDialogs";
            var layout = context.Layout;
            bool isRendered = false;
            foreach (var item in layout.Body.Items)
            {
                if (item.TokenName == dialogShapeName)
                {
                    isRendered = true;
                    break;
                }
            }

            if (!isRendered)
            {
                var shape = shapeHelper.SyncDialogs(Model: model);
                shape.TokenName = dialogShapeName;
                context.Layout.Body.Add(shape);
            }
        }
        
        public static string ToQueryString(NameValueCollection nvc)
        {
            StringBuilder sb = new StringBuilder("");

            bool first = true;

            foreach (string key in nvc.AllKeys)
            {
                foreach (string value in nvc.GetValues(key))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }

                    sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));

                    first = false;
                }
            }

            return sb.ToString();
        }

        public static DbConnection GetConnection(IOrchardServices orchardServices, ILogger logger)
        {
            var setting = orchardServices.WorkContext.CurrentSite.As<SuiteCRMSettingPart>();

            if (setting == null)
            {
                logger.Error("SuiteCRMSettingPart is null for current site");
                return null;
            }

            DbConnectionStringBuilder builder = null;
            string providerName = string.Empty;
            if (setting.Provider == Constants.SqlServer)
            {
                var sqlConnectionStringBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = setting.Host;
                sqlConnectionStringBuilder.InitialCatalog = setting.Database;
                sqlConnectionStringBuilder.UserID = setting.Username;
                sqlConnectionStringBuilder.Password = setting.Password;
                providerName = "System.Data.SqlClient";

                EntityConnectionStringBuilder mainBuilder = new EntityConnectionStringBuilder();
                mainBuilder.Provider = providerName;
                mainBuilder.ProviderConnectionString = builder.ConnectionString;
                mainBuilder.Metadata =
                            "res://*/Database.SuiteCRMDataContext.csdl|" +
                            "res://*/Database.SuiteCRMDataContext.ssdl|" +
                            "res://*/Database.SuiteCRMDataContext.msl";

                return new EntityConnection(mainBuilder.ConnectionString);
            }
            else if (setting.Provider == Constants.MySql)
            {
                var mySqlConnectionStringBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
                mySqlConnectionStringBuilder.Database = setting.Database;
                mySqlConnectionStringBuilder.UserID = setting.Username;
                mySqlConnectionStringBuilder.Password = setting.Password;
                mySqlConnectionStringBuilder.Port = (uint)setting.Port;
                mySqlConnectionStringBuilder.Server = setting.Host;
                mySqlConnectionStringBuilder.OldGuids = true;
                builder = mySqlConnectionStringBuilder;
                return new MySqlConnection(builder.ConnectionString);
            }
            else
            {
                logger.Error("The provider is not supported");
                return null;
            }
        }
    }
}