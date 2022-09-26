using System;
using Shell.WRFM.Global.Utilities;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Shell.WRFM.Global.Web.DataAccess;
using Shell.WRFM.Global.Web.Log;
using Shell.WRFM.Global.Business;
using Shell.WRFM.Global.Data;
using Shell.WRFM.Global.Business.Entities;
using System.Text;
using System.Xml;
using System.Web;
using System.Data.SqlClient;
using Shell.WRFM.AWR.DataAccessLayer;
using Shell.WRFM.Global.Web.SingleUI.Infra;

namespace Shell.WRFM.eBooks.DataAccessLayer
{
    public class eBookControlsData
    {
        public IDBManager _manager;
        public Log _log = null;
        const string DWBUSER = "DWB User";
        const string BATCHIMPORTPATH = "BatchImportSharedPath";



        /// <summary>
        /// Initializes a new instance of the <see cref="eBookControlsData"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public eBookControlsData(IDBManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="eBookControlsData"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public eBookControlsData()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="eBookControlsData"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="log">The log.</param>
        public eBookControlsData(IDBManager manager, Log log)
        {
            _manager = manager;
            _log = log;
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="exc">The exc.</param>
        void LogError(Exception exc)
        {
            if (_log != null)
                _log.Error(exc);
        }

        /// <summary>
        /// Execute StoredProcedure to fetch data into table.
        /// </summary>
        /// <returns></returns>
        DataTable ExecuteAndFetch(string procName)
        {
            return ExecuteAndFetch(procName, 0);
        }

        /// <summary>
        /// Executes the and fetch.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="iteration">The iteration.</param>
        /// <returns></returns>
        DataTable ExecuteAndFetch(string procName, int iteration)
        {
            DataTable dt = null;

            try
            {
                using (DBManager mgr = (DBManager)_manager.Clone())
                {
                    mgr.Open();
                    DataSet ds = mgr.ExecuteDataSet(CommandType.StoredProcedure, procName);
                    if (ds.Tables.Count != 0)
                        dt = ds.Tables[0];
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
                if (iteration == 0)
                {
                    return ExecuteAndFetch(procName, 1);
                }
                else
                    throw new WRFMBusinessException("Error in ExecuteAndFetchAsDataSet. " + exc.Message, exc);
            }

            return dt;
        }

        /// <summary>
        /// Execute StoredProcedure to fetch data into Dataset.
        /// </summary>
        /// <returns></returns>
        DataSet ExecuteAndFetchAsDataSet(string procName, Dictionary<string, object> paramList)
        {
            return ExecuteAndFetchAsDataSet(procName, paramList, 0);
        }

        DataSet ExecuteAndFetchAsDataSet(string procName, Dictionary<string, object> paramList, int iteration)
        {
            DataSet ds = null;
            try
            {
                using (DBManager mgr = (DBManager)_manager.Clone())
                {
                    mgr.Open();
                    foreach (string key in paramList.Keys)
                        mgr.AddParameters(key, paramList[key]);
                    ds = mgr.ExecuteDataSet(CommandType.StoredProcedure, procName);
                }
            }
            catch (Exception exc)
            {

                if (iteration == 0)
                {
                    return ExecuteAndFetchAsDataSet(procName, paramList, 1);
                }
                else
                    throw new WRFMBusinessException("Error in ExecuteAndFetchAsDataSet. " + exc.Message, exc);
            }
            return ds;
        }

        /// <summary>
        /// Executes the and fetch.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="paramList">The param list.</param>
        /// <returns></returns>
        DataTable ExecuteAndFetch(string procName, Dictionary<string, object> paramList)
        {
            return ExecuteAndFetch(procName, paramList, 0);
        }

        /// <summary>
        /// Executes the and fetch.
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="paramList">The param list.</param>
        /// <param name="iteration">The iteration.</param>
        /// <returns></returns>
        DataTable ExecuteAndFetch(string procName, Dictionary<string, object> paramList, int iteration)
        {
            DataTable dt = null;

            try
            {
                using (DBManager mgr = (DBManager)_manager)
                {
                    mgr.Open();
                    foreach (string key in paramList.Keys)
                        mgr.AddParameters(key, paramList[key]);
                    DataSet ds = mgr.ExecuteDataSet(CommandType.StoredProcedure, procName);
                    if (ds.Tables.Count != 0)
                        dt = ds.Tables[0];
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
                if (iteration == 0)
                {
                    return ExecuteAndFetch(procName, paramList, 1);
                }
                else
                    throw new WRFMBusinessException("Error in ExecuteAndFetch. " + exc.Message, exc);
            }

            return dt;
        }

        /// <summary>
        /// Executes the proc and fetches the first row's first column value
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <param name="paramList">The param list.</param>
        /// <returns></returns>
        Object ExecuteScalar(string procName, Dictionary<string, object> paramList)
        {
            object obj = null;

            try
            {
                using (DBManager mgr = (DBManager)_manager.Clone())
                {
                    mgr.Open();
                    foreach (string key in paramList.Keys)
                        mgr.AddParameters(key, paramList[key]);

                    obj = mgr.ExecuteScalar(CommandType.StoredProcedure, procName);
                }
            }
            catch (Exception exc)
            {
                LogError(exc);
            }
            return obj;
        }


        /// <summary>
        /// Checks the duplicate entry.
        /// </summary>
        /// <param name=CommonConstants.VALUE>The value.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public DataTable CheckDuplicateEntry(string value, string columnName, string tableName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ColumnName", columnName);
            paramList.Add(CommonConstants.TableNamePARAM, tableName);
            paramList.Add(EBookConstants.ValuePARAM, value);
            return ExecuteAndFetch(EBookConstants.SPDUPLICATECHECK, paramList);
        }

        /// <summary>
        /// Checks the duplicate entry.
        /// </summary>
        /// <param name=CommonConstants.VALUE>The value.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public DataTable CheckDuplicateEntry(string value1, string columnName, string tableName, string ID, string value2)
        {
            DataTable dtDuplicateEntry = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ColumnName", columnName);
            paramList.Add(CommonConstants.TableNamePARAM, tableName);
            paramList.Add("@Value1", value1);
            paramList.Add("@Value2", value2);
            paramList.Add(CommonConstants.PARAMID, ID);
            switch (tableName)
            {
                case EBookConstants.USERTABLE:
                    dtDuplicateEntry = ExecuteAndFetch(EBookConstants.SPDUPLICATEEDIT, paramList);
                    break;

                case EBookConstants.TEAMSTABLE:
                    dtDuplicateEntry = ExecuteAndFetch(EBookConstants.SPDUPLICATEEDITTEAM, paramList);
                    break;
            }
            return dtDuplicateEntry;
        }

        public DataTable LoadTemplateControl(string strSelectedAsset, string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ASSETTYPE", strSelectedAsset);
            paramList.Add("@APPTYPE", value);

            return ExecuteAndFetch("SP_eBooks_GetTemplates", paramList);
        }

        public DataTable GetSelectedBookType(string strSelectedValue)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterPageID", strSelectedValue);

            return ExecuteAndFetch("SP_eBooks_GetMasterBookType", paramList);
        }




        public DataTable GetSelectedTemplateBookType(string strSelectedValue)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplatePageID", strSelectedValue);

            return ExecuteAndFetch("SP_eBooks_GetTemplatePageBookType", paramList);
        }



        public DataTable LoadConnectionType(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@APPNAME", value);

            return ExecuteAndFetch("SP_eBooks_GetComponentType", paramList);
        }

        public DataTable GetMasterPageComponents(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ASSETNAME", value);

            return ExecuteAndFetch("USP_eBooks_GetType5Components", paramList);
        }

        public DataTable GetValidateGenericPlotCount(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ASSETNAME", value);

            return ExecuteAndFetch("SP_eBooks_ValidateGenericplotCount ", paramList);
        }

        public DataTable GetWellEWBComponents(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@DISPLAYNAME", value);

            return ExecuteAndFetch("SP_eBooks_EwbComponents", paramList);
        }

        public DataTable GetWellEWBComponentsDirectional(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@DISPLAYNAME", value);

            return ExecuteAndFetch("SP_eBooks_GetDirectionalSurveyComponent ", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetBreadCrumbXML(string name, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvName", name);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerName"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetAllBooks(string ownerName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Username", ownerName);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetAllAwrBooks(string storProc, string bookIds)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@BookIds", bookIds);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// Updates the list entry.
        /// </summary>
        /// <param name="strUserID">The STR user ID.</param>
        /// <param name="listEntry">The list entry.</param>
        /// <param name="pageType">Type of the page.</param>
        /// <param name="actionPerformed">The action performed.</param>
        /// <returns></returns>

        public int UpdateListEntry(string strUserID, ListEntry listEntry, string pageType, string actionPerformed, int ID, string reportName)
        {
            int intID = 0;
            bool blnTerminated = false;

            switch (pageType)
            {
                case EBookConstants.USERREGISTRATION:
                    {
                        string UserID = listEntry.UserDetails.WindowUserID;
                        string Discipline = listEntry.UserDetails.Discipline;
                        string Team = listEntry.UserDetails.Team;
                        string Privileges = listEntry.UserDetails.PrivilegeCode;
                        string strTerminated = listEntry.UserDetails.Terminated;
                        if (string.Equals(strTerminated, EBookConstants.STATUS_TERMINATED))
                            blnTerminated = true;
                        if (string.Equals(strTerminated, EBookConstants.STATUS_ACTIVE))
                            blnTerminated = false;
                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.DELETEUSER))
                                {
                                    dbManager.AddParameters("@chvOperationType", "REMOVESTAFF");
                                }
                                dbManager.AddParameters(SEDConstants.PARAMUSERID, UserID);
                                dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Discipline);
                                dbManager.AddParameters("@Team", Team);
                                dbManager.AddParameters("@Privileges", Privileges);
                                dbManager.AddParameters(CommonConstants.PARAMID, ID);
                                dbManager.AddParameters(SEDConstants.PARAUSERNAME, UserID);
                                dbManager.AddParameters("@TerminateStatus", blnTerminated);
                                dbManager.AddParameters(CommonConstants.PARAMCREATEDBY, strUserID);
                                dbManager.AddParameters(CommonConstants.PARAMMODIFIEDBY, strUserID);
                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.SPADDUPDATEUSER);
                                if (!string.IsNullOrEmpty(dbManager.Parameters[10].Value.ToString()))
                                    intID = Int32.Parse(dbManager.Parameters[10].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }
                    }
                    break;
                case EBookConstants.MASTERPAGE:
                    {
                        string TemplateTitle = listEntry.MasterPage.TemplateTitle;
                        string MasterPageName = listEntry.MasterPage.Name;
                        string StandardOperarting = listEntry.MasterPage.SOP;
                        string strTerminateStatus = listEntry.MasterPage.Terminated;
                        if (string.Equals(strTerminateStatus, EBookConstants.STATUS_TERMINATED))
                            blnTerminated = true;
                        if (string.Equals(strTerminateStatus, EBookConstants.STATUS_ACTIVE))
                            blnTerminated = false;
                        string PageOwner = strUserID;
                        string pageURL = listEntry.MasterPage.PageURL;
                        string TemplateID = listEntry.MasterPage.Templates;
                        string AssetType = listEntry.MasterPage.AssetTypeText;
                        string ConnectionType = listEntry.MasterPage.ConnectionTypeText;
                        string PageSequence = listEntry.MasterPage.PageSequence.ToString();
                        string SignOffDecipline = listEntry.MasterPage.SignOffDisciplineText;
                        string ToolTip = listEntry.MasterPage.ToolTip;
                        string components = listEntry.MasterPage.MasterPageComponents;
                        string ApplicationName = listEntry.MasterPage.AppName;
                        string WSDParameters = listEntry.MasterPage.WSDAttributes;

                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");
                                }
                                dbManager.AddParameters("@Id", ID);
                                dbManager.AddParameters("@Title_Template", TemplateTitle);
                                dbManager.AddParameters("@MasterPageName", MasterPageName);
                                dbManager.AddParameters("@standardOperarting", StandardOperarting);
                                dbManager.AddParameters("@Terminate_Status", blnTerminated);
                                dbManager.AddParameters("@Page_Owner", PageOwner);
                                dbManager.AddParameters("@Page_URL", pageURL);
                                dbManager.AddParameters("@Template_ID", TemplateID);
                                dbManager.AddParameters("@Asset_Type", AssetType);
                                dbManager.AddParameters("@Connection_Type", ConnectionType);
                                dbManager.AddParameters("@Page_Sequence", PageSequence);
                                dbManager.AddParameters("@Sign_Off_Discipline", SignOffDecipline);
                                dbManager.AddParameters("@ToolTip", ToolTip);
                                // dbManager.AddParameters("@Components", components);
                                dbManager.AddParameters("@Application_Name", ApplicationName);
                                dbManager.AddParameters("@WSD_Parameters", WSDParameters);
                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_SaveUpdateMasterPage");
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                    intID = Int32.Parse(dbManager.Parameters[16].Value.ToString());
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                    intID = Int32.Parse(dbManager.Parameters[16].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }
                    }
                    break;
                case EBookConstants.TEMPLATEREPORT:
                    {

                        int TemplateId = listEntry.TemplateDetails.RowId;
                        string AssetType = listEntry.TemplateDetails.AssetType;
                        string strTerminateStatus = listEntry.TemplateDetails.Terminated;
                        if (string.Equals(strTerminateStatus, EBookConstants.STATUS_TERMINATED))
                            blnTerminated = true;
                        if (string.Equals(strTerminateStatus, EBookConstants.STATUS_ACTIVE))
                            blnTerminated = false;
                        string TemplateName = listEntry.TemplateDetails.Title;
                        string AppName = listEntry.TemplateDetails.AppName;

                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");
                                }
                                dbManager.AddParameters("@Id", TemplateId);
                                dbManager.AddParameters("@Asset_Type", AssetType);
                                dbManager.AddParameters("@Terminate_Status", blnTerminated);
                                dbManager.AddParameters("@Template_Name", TemplateName);
                                dbManager.AddParameters("@Has_MasterPage", 0);
                                dbManager.AddParameters("@Application_Name", AppName);
                                dbManager.AddParameters(CommonConstants.usernamePARAM, strUserID);


                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_InsertUpdateTemplate");
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                    intID = Int32.Parse(dbManager.Parameters[8].Value.ToString());
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                    intID = Int32.Parse(dbManager.Parameters[8].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }
                    }

                    break;
                case EBookConstants.WELLBOOKREPORT:
                    {
                        bool blnSignOffStatus = false;
                        string strTitle = listEntry.WellBookDetails.Title;
                        string strTeam = listEntry.WellBookDetails.Team;
                        string strTeamID = listEntry.WellBookDetails.TeamID;
                        string strBookOwner = listEntry.WellBookDetails.BookOwner;
                        string strTerminated = listEntry.WellBookDetails.Terminated;

                        if (string.Equals(strTerminated, EBookConstants.STATUS_TERMINATED))
                            blnTerminated = true;
                        if (string.Equals(strTerminated, EBookConstants.STATUS_ACTIVE))
                            blnTerminated = false;
                        string strSignOffStatus = listEntry.WellBookDetails.SignOffStatus;
                        if (string.Equals(strSignOffStatus, "Yes"))
                            blnSignOffStatus = true;
                        if (string.Equals(strTerminated, "No"))
                            blnSignOffStatus = false;
                        int intPrivacyStatus = listEntry.WellBookDetails.PrivacyStatus; ;
                        int intRowId = listEntry.WellBookDetails.RowId;
                        int intNoOfActiveChapters = listEntry.WellBookDetails.NoOfActiveChapters;
                        string strAppName = listEntry.WellBookDetails.AppName;
                        string strBookFocalPoint = listEntry.WellBookDetails.BookFocalPoint;
                        string strDefaultSharedPath = string.Empty;
                        strDefaultSharedPath = WRFMCommon.Instance.ConfiguationData.PortalConfigurations[BATCHIMPORTPATH];

                        //DepthReference
                        string strDepthReference = listEntry.WellBookDetails.DepthReference;
                        string strDepthRefID = listEntry.WellBookDetails.DepthReferenceID;

                        //DepthReference

                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");


                                }
                                dbManager.AddParameters(CommonConstants.usernamePARAM, strUserID);
                                dbManager.AddParameters("@BookName ", strTitle);
                                dbManager.AddParameters("@Team ", strTeam);
                                dbManager.AddParameters("@Owner ", strBookOwner);
                                dbManager.AddParameters("@Favorite  ", false);
                                dbManager.AddParameters("@Sign_Off_Status", blnSignOffStatus);
                                dbManager.AddParameters("@Terminate_Status ", blnTerminated);
                                dbManager.AddParameters("@Published  ", false);
                                dbManager.AddParameters("@Id", intRowId);
                                dbManager.AddParameters("@Team_ID", Convert.ToInt32(strTeamID));
                                dbManager.AddParameters("@ReviewCount", 0);
                                dbManager.AddParameters("@NoOfActiveChapters ", intNoOfActiveChapters);
                                dbManager.AddParameters("@ToBeDeleted ", false);
                                dbManager.AddParameters("@Application_Name ", strAppName);
                                dbManager.AddParameters("@Privacy_Status  ", Convert.ToString(intPrivacyStatus));
                                dbManager.AddParameters("@BookFocalPoint  ", strBookFocalPoint);
                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.AddParameters("@BatchImportSharedPath", strDefaultSharedPath);


                                //DepthReference
                                dbManager.AddParameters("@DepthReference", strDepthRefID);
                                //DepthReference
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_InsertUpdateBooks");
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                    intID = Int32.Parse(dbManager.Parameters[17].Value.ToString());
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                    intID = Int32.Parse(dbManager.Parameters[17].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }
                    }
                    break;
                case EBookConstants.TEAMREGISTRATION:
                    {
                        string TeamName = listEntry.TeamDetails.TeamName;
                        string AssetOwner = listEntry.TeamDetails.AssetOwner;
                        string strTerminated = listEntry.TeamDetails.Terminated;
                        if (string.Equals(strTerminated, EBookConstants.STATUS_TERMINATED))
                            blnTerminated = true;
                        if (string.Equals(strTerminated, EBookConstants.STATUS_ACTIVE))
                            blnTerminated = false;

                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");
                                }
                                dbManager.AddParameters("@ID ", ID);
                                dbManager.AddParameters(DreamConstants.TeamNamePARAM, listEntry.TeamDetails.TeamName);
                                dbManager.AddParameters(CommonConstants.PARAMASSETOWNER, listEntry.TeamDetails.AssetOwner);
                                dbManager.AddParameters("@TerminateStatus", blnTerminated);
                                dbManager.AddParameters(EBookConstants.PARAMGAINUNITS, listEntry.TeamDetails.GainUnits);
                                dbManager.AddParameters(EBookConstants.PARAMCOSTSCALE, listEntry.TeamDetails.CostScale);
                                dbManager.AddParameters(EBookConstants.PARAMCOSTUNIT, listEntry.TeamDetails.CostUnit);
                                dbManager.AddParameters(CommonConstants.PARAMCREATEDBY, strUserID);
                                dbManager.AddParameters(CommonConstants.PARAMMODIFIEDBY, strUserID);
                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.SPADDUPDATETEAM);
                                if (!string.IsNullOrEmpty(dbManager.Parameters[10].Value.ToString()))
                                    intID = Int32.Parse(dbManager.Parameters[10].Value.ToString());
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }
                        break;
                    }

                case EBookConstants.STAFFREGISTRATION:
                    {
                        if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                        {
                            string strUserName = ((StaffDetails)listEntry.Staffs[0]).UserName;
                            int strUser = listEntry.UserDetails.RowId;
                            string strUserRank = ((StaffDetails)listEntry.Staffs[0]).UserRank;
                            string strDiscipline = listEntry.UserDetails.Discipline;
                            string strPrivilege = listEntry.UserDetails.PrivilegeCode;
                            Dictionary<string, object> paramList = new Dictionary<string, object>();
                            paramList.Add("@chvOperationType", EBookConstants.AUDITACTIONUPDATION);
                            paramList.Add(SEDConstants.PARAUSERNAME, strUserName);
                            paramList.Add(CommonConstants.TeamIDPARAM, ID);
                            paramList.Add(SEDConstants.PARAMUSERID, strUser);
                            paramList.Add("@UserRank", strUserRank);
                            paramList.Add(CommonConstants.PARAMDISCIPLINE, strDiscipline);
                            paramList.Add(CommonConstants.PARAMID, ID);
                            paramList.Add(SEDConstants.PARAPRIVILEGE, strPrivilege);
                            paramList.Add(CommonConstants.PARAMCREATEDBY, strUserID);
                            paramList.Add(CommonConstants.PARAMMODIFIEDBY, strUserID);
                            ExecuteAndFetch(EBookConstants.SPADDREMOVETEAMSTAFF, paramList);
                        }
                        else if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONTERMINATE))
                        {
                            int strTeamID = ID;
                            string strUserName = ((StaffDetails)listEntry.Staffs[0]).UserName;
                            Dictionary<string, object> paramList = new Dictionary<string, object>();
                            paramList.Add("@chvOperationType", EBookConstants.AUDITACTIONTERMINATE);
                            paramList.Add(SEDConstants.PARAUSERNAME, strUserName);
                            paramList.Add(CommonConstants.TeamIDPARAM, strTeamID);
                            paramList.Add(SEDConstants.PARAMUSERID, strTeamID);
                            paramList.Add("@UserRank", strUserID);
                            paramList.Add(CommonConstants.PARAMDISCIPLINE, strUserID);
                            paramList.Add(CommonConstants.PARAMID, ID);
                            paramList.Add(SEDConstants.PARAPRIVILEGE, strUserID);
                            paramList.Add(CommonConstants.PARAMCREATEDBY, strUserID);
                            paramList.Add(CommonConstants.PARAMMODIFIEDBY, strUserID);
                            ExecuteAndFetch(EBookConstants.SPADDREMOVETEAMSTAFF, paramList);
                        }
                        break;
                    }

                case EBookConstants.CHANGEPAGEOWNER:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();
                        paramList.Add("@NewOwner", reportName);
                        paramList.Add(CommonConstants.PARAMID, ID);
                        ExecuteAndFetch(EBookConstants.SPCHANGEPAGEOWNER, paramList);
                        break;
                    }
                case EBookConstants.WELLBOOKPAGEVIEW:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();
                        string strSignOffStatus = listEntry.WellBookDetails.SignOffStatus;
                        paramList.Add("@Status", strSignOffStatus);
                        paramList.Add(SEDConstants.PARAMUSERID, strUserID);
                        paramList.Add("@PagesID", reportName);
                        ExecuteAndFetch(EBookConstants.SPBOOKPAGESUMMARYUPDATESIGNOFFSTATUS, paramList);
                        break;
                    }

                case EBookConstants.UPDATEPAGEOWNER:
                    {
                        Dictionary<string, object> paramlist = new Dictionary<string, object>();
                        string strTeamID = listEntry.StaffDetails.TeamID;
                        string strDiscipline = listEntry.StaffDetails.Discipline;
                        paramlist.Add(EBookConstants.PARAMTEAMID, strTeamID);
                        paramlist.Add(EBookConstants.PARAMDELDISCIPLINE, strDiscipline);
                        ExecuteAndFetch(EBookConstants.SPEBOOKSDELETEDISCIPLINE, paramlist);

                        break;
                    }
                case EBookConstants.ACTIVATERECORD:
                case EBookConstants.ARCHIVERECORD:
                case EBookConstants.DELETERECORD:
                case EBookConstants.DELETETEAM:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();

                        switch (reportName)
                        {
                            case EBookConstants.MASTERPAGE:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                paramList.Add("@user", strUserID);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEMASTER, paramList);
                                break;
                            case EBookConstants.USERREGISTRATION:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEUSER, paramList);
                                break;
                            case EBookConstants.TEAMREGISTRATION:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVETEAM, paramList);
                                break;
                            case EBookConstants.STAFFREGISTRATION:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPDELETETEAMSTAFFTEAM, paramList);
                                break;
                            case EBookConstants.TEMPLATEREPORT:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                paramList.Add("@user", strUserID);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVETEMPLATE, paramList);
                                break;
                            case EBookConstants.WELLBOOKREPORT:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEBOOK, paramList);
                                break;
                            case EBookConstants.CHAPTERPAGEMAPPINGREPORT:
                                string strID = listEntry.StaffDetails.RowID;
                                if (listEntry.StaffDetails.RowID.ToLowerInvariant().Contains("bookpages"))
                                {
                                    if (listEntry.StaffDetails.RowID.Split(CommonConstants.PIPESYMBOL.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 1)
                                    {
                                        Dictionary<string, string> dicnryPagesDetails = new Dictionary<string, string>();
                                        string strBookPagesIDs = listEntry.StaffDetails.RowID.Split(CommonConstants.PIPESYMBOL.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                                        string strChapterPagesIDs = listEntry.StaffDetails.RowID.Split(CommonConstants.PIPESYMBOL.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                                        dicnryPagesDetails.Add(strBookPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], (strBookPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 1) ? strBookPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1] : string.Empty);
                                        dicnryPagesDetails.Add(strChapterPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], (strChapterPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 1) ? strChapterPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1] : string.Empty);
                                        paramList.Add("@BookPagesIds", dicnryPagesDetails[strBookPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0]]);
                                        paramList.Add("@ChapterPagesIds", dicnryPagesDetails[strChapterPagesIDs.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0]]);
                                        paramList.Add("@chvOperationType", pageType);
                                        ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEBOOKPAGES, paramList);
                                    }
                                    //strBookPagesIDs.Split("-".ToCharArray(),StringSplitOptions.RemoveEmptyEntries)[1]
                                }
                                //else
                                //{
                                //    paramList.Add(EBookConstants.ValuePARAM, strID);
                                //    paramList.Add("@chvOperationType", pageType);
                                //    ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEBOOKPAGES, paramList);
                                //}
                                break;
                            case EBookConstants.CHAPTERREPORT:
                                string strRowID = listEntry.StaffDetails.RowID;
                                string strBookID = listEntry.StaffDetails.UserID;
                                paramList.Add(EBookConstants.ValuePARAM, strRowID);
                                paramList.Add("@chvOperationType", pageType);
                                paramList.Add(EBookConstants.PARM_BOOK_ID, strBookID);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREOVECHAPTERS, paramList);
                                break;
                            case EBookConstants.CHAPTERPAGEREPORT:
                                string strIDValue = listEntry.StaffDetails.RowID;
                                paramList.Add(EBookConstants.ValuePARAM, strIDValue);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVECHAPTERPAGES, paramList);
                                break;
                            //WRFMMigration_Release [4.2] <start>
                            case EBookConstants.GENERICPLOT:
                                paramList.Add(EBookConstants.ValuePARAM, ID);
                                paramList.Add("@chvOperationType", pageType);
                                ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVEGENERICPLOT, paramList);
                                break;
                            //WRFMMigration_Release [4.2] <end>
                        }
                        break;
                    }

            }

            return intID;
        }

        //DepthReference
        public DataTable GetMasterDepthRefById(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Id", Convert.ToInt32(value));

            return ExecuteAndFetch(EBookConstants.SPGETDEPTHREFBYID, paramList);
        }
        //DepthReference

        //DepthReference for AWR
        public DataTable GetAWRMasterDepthRefById(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AWRId", Convert.ToInt32(value));

            return ExecuteAndFetch(EBookConstants.SPGETAWRDEPTHREFBYID, paramList);
        }
        //DepthReference for AWR


        public int UpdateTemplatePageMapping(string strUserID, ListEntry listEntry, string actionPerformed, int intTemplateID, int ID)
        {
            int intID = 0;
            bool blnTerminated = false;
            string pageURL = "";
            string components = string.Empty;
            string WSD_Parameters = string.Empty;
            string ToolTip = string.Empty;
            int intNoOfMasterPages = 0;
            // int templateId = selectedTemplateID;
            //DataTable dtListItemCollection = null;
            //DataTable dtresult = null;
            if (actionPerformed == "1")
            {
                if (listEntry != null && listEntry.TemplateConfiguration != null)
                {
                    foreach (TemplateConfiguration objTemplateConfiguration in listEntry.TemplateConfiguration)
                    {
                        string Page_Title_Template = objTemplateConfiguration.TemplateTitle;
                        int PageSequence = objTemplateConfiguration.PageSequence;
                        //string Title = objTemplateConfiguration.MasterPageTitle;
                        string Master_Page_ID = objTemplateConfiguration.LinkedMasterPageId;
                        string MasterPageName = objTemplateConfiguration.MasterPageTitle;
                        string AssetType = objTemplateConfiguration.AssetType;
                        string Decipline = objTemplateConfiguration.Discipline;
                        string ConnectionType = objTemplateConfiguration.ConnectionType;
                        if (!string.IsNullOrEmpty(objTemplateConfiguration.ToolTip))
                            ToolTip = objTemplateConfiguration.ToolTip;
                        string PageOwner = objTemplateConfiguration.PageOwner;
                        if (string.IsNullOrEmpty(PageOwner))
                        {
                            PageOwner = strUserID;
                        }
                        if (!string.IsNullOrEmpty(objTemplateConfiguration.PageURL))
                        {
                            pageURL = objTemplateConfiguration.PageURL;
                        }
                        //if (!string.IsNullOrEmpty(objTemplateConfiguration.MasterPageComponents))
                        //{
                        //    components = objTemplateConfiguration.MasterPageComponents;
                        //}

                        if (!string.IsNullOrEmpty(objTemplateConfiguration.WSD_Parameters))
                        {
                            WSD_Parameters = objTemplateConfiguration.WSD_Parameters;
                        }
                        string StandardOperarting = objTemplateConfiguration.StandardOperatingProcedure;
                        //string TemplateID = objTemplateConfiguration.TemplateID;
                        int TemplateID = intTemplateID;
                        string ApplicationName = objTemplateConfiguration.AppName;

                        WRFMCommon objCommon = WRFMCommon.Instance;
                        using (DBManager dbManager = objCommon.DataManager)
                        {
                            try
                            {
                                dbManager.Open();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "INSERT");
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    dbManager.AddParameters("@chvOperationType", "UPDATE");
                                }
                                dbManager.AddParameters("@Id", ID);
                                dbManager.AddParameters("@Master_Page_ID ", Master_Page_ID);
                                dbManager.AddParameters("@Page_Sequence", PageSequence);
                                dbManager.AddParameters("@Template_ID", TemplateID);
                                dbManager.AddParameters("@Terminate_Status", blnTerminated);
                                dbManager.AddParameters("@Master_Page_Name", MasterPageName);
                                dbManager.AddParameters("@Page_Title_Template", Page_Title_Template);
                                dbManager.AddParameters("@Page_Owner", PageOwner);
                                dbManager.AddParameters("@Page_URL", pageURL);
                                dbManager.AddParameters("@Standard_Operating_Procedure", StandardOperarting);
                                dbManager.AddParameters("@Asset_Type", AssetType);
                                dbManager.AddParameters("@Connection_Type", ConnectionType);

                                dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Decipline);
                                dbManager.AddParameters("@ToolTip", ToolTip);
                                //dbManager.AddParameters("@Components", components);
                                dbManager.AddParameters("@Application_Name", ApplicationName);
                                dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);
                                dbManager.AddParameters("@usrname", strUserID);
                                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_SaveTemplatePageMapping");
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                    intID = Int32.Parse(dbManager.Parameters[18].Value.ToString());
                                // blnIsUpdateSuccess = true;
                                intNoOfMasterPages++;
                                UpdateListTemplateMappingAuditHistory(intID, strUserID, EBookConstants.AUDITACTIONCREATION);
                            }
                            catch (Exception ex)
                            {
                                WRFMCommon.Instance.Error.Handle(ex);
                            }
                        }


                    }
                }

            }

            return intNoOfMasterPages;

        }



        public int UpdateTemplatePageMappingTable(string strUserID, ListEntry listEntry, string actionPerformed, int intTemplateID, int ID)
        {
            int intID = 0;
            bool blnTerminated = false;
            string pageURL = "";
            //string components = string.Empty;
            string WSD_Parameters = string.Empty;
            // string ToolTip = string.Empty;
            int intNoOfMasterPages = 0;
            DataTable dtFetchTemplateMapDetail = null;
            DataTable dtListItemCollection = null;
            DataTable dtresult = null;
            dtFetchTemplateMapDetail = GetLinkedTemplateMappingDetails(intTemplateID);

            List<string> itemTobeAddedOrDeleted = new List<string>();
            // List<string> itemToAdded = new List<string>();
            bool blnMasterPageFound = false;


            /// If the entry in List Box is not in dtFetchTemplateMapDetail, add it
            /// If the entry in List Box is in dtFetchTemplateMapDetail, update it
            /// If the entry is not in ListBox but present in dtFetchTemplateMapDetail delete it
            /// 

            if (dtFetchTemplateMapDetail != null && dtFetchTemplateMapDetail.Rows.Count > 0)
            {
                string strMasterPageID = string.Empty;
                foreach (DataRow row in dtFetchTemplateMapDetail.Rows)
                {
                    blnMasterPageFound = false;
                    strMasterPageID = string.Empty;
                    strMasterPageID = row[CommonConstants.ID].ToString();

                    if (listEntry != null)
                    {
                        if (listEntry.TemplateConfiguration != null && listEntry.TemplateConfiguration.Count > 0)
                        {
                            foreach (TemplateConfiguration objTemplateConfiguration in listEntry.TemplateConfiguration)
                            {
                                if (string.Compare(strMasterPageID, objTemplateConfiguration.LinkedMasterPageId) == 0)
                                {
                                    blnMasterPageFound = true;
                                    break;
                                }

                            }

                        }

                        if (!blnMasterPageFound && !itemTobeAddedOrDeleted.Contains(row[CommonConstants.ID].ToString()))
                        {
                            itemTobeAddedOrDeleted.Add(row[CommonConstants.ID].ToString());
                        }
                    }
                }
            }

            for (int index = 0; index < itemTobeAddedOrDeleted.Count; index++)
            {
                DataTable dtDeleteTemplatemapping = null;
                int intItemToBeDeleted = Convert.ToInt32(itemTobeAddedOrDeleted[index]);

                dtDeleteTemplatemapping = DeleteTemplateMappingDetails(intItemToBeDeleted);
            }

            dtFetchTemplateMapDetail = GetLinkedTemplateMappingDetails(intTemplateID);
            if (dtFetchTemplateMapDetail != null)
            {
                dtListItemCollection = dtFetchTemplateMapDetail;
            }

            if (listEntry != null && listEntry.TemplateConfiguration != null)
            {
                // DataTable dtTable = null;



                string[] splitter = { "New" };
                string strRowID = string.Empty;
                string strMasterPageID = string.Empty;
                string[] strSplitted = null;
                DataRow[] dtRow = null;
                // string strCAMLQuery = string.Empty;
                //                string strfieldsToView = string.Empty;
                foreach (TemplateConfiguration objTemplateConfiguration in listEntry.TemplateConfiguration)
                {
                    //DataRow row = dtTable.NewRow();
                    strRowID = string.Empty;
                    strMasterPageID = string.Empty;
                    /// Check the LinkedMasterPageId contains "New" string
                    /// If contains split the string and assign the value at index 1
                    /// Else the assign the LinkedMasterPageId value to 
                    /// 

                    if (objTemplateConfiguration.LinkedMasterPageId.IndexOf("New") != -1)
                    {
                        strSplitted = null;
                        strSplitted = objTemplateConfiguration.LinkedMasterPageId.Split(splitter, StringSplitOptions.None);
                        if (strSplitted != null && strSplitted.Length >= 2)
                        {
                            strMasterPageID = strSplitted[1];
                        }
                    }
                    else
                    {
                        strRowID = objTemplateConfiguration.LinkedMasterPageId;
                        strMasterPageID = objTemplateConfiguration.LinkedMasterPageId;
                    }
                    dtRow = null;
                    if (dtListItemCollection != null && !string.IsNullOrEmpty(strRowID))
                    {
                        dtRow = dtListItemCollection.Select("ID = " + strRowID);
                    }
                    if (dtRow != null && dtRow.Length > 0)
                    {
                        int intTemplateMapID = (Convert.ToInt32(dtRow[0][CommonConstants.ID]));

                        UpdateListTemplateMappingAuditHistory(intTemplateMapID, strUserID, "3");

                    }
                    else
                    {
                        int intMasterID = Convert.ToInt32(strMasterPageID);
                        dtresult = FetchMasterDetails(intMasterID);
                        if (dtresult != null && dtresult.Rows.Count > 0)
                        {
                            foreach (DataRow objdataRow in dtresult.Rows)
                            {
                                string ApplicationName = "";
                                string Page_Title_Template = objdataRow["Title_Template"].ToString();
                                int PageSequence = Int32.Parse(objdataRow["Page_Sequence"].ToString());
                                //string Title = objdataRow["Title_Template"].ToString();
                                int Master_Page_ID = Int32.Parse(objdataRow[CommonConstants.ID].ToString());
                                string MasterPageName = objdataRow["MasterPageName"].ToString();
                                string AssetType = objdataRow["Asset_Type"].ToString();
                                string Decipline = objdataRow["Sign_Off_Discipline"].ToString();
                                string ConnectionType = objdataRow["Connection_Type"].ToString();
                                string Tooltip = objdataRow["ToolTip"].ToString();
                                string PageOwner = objdataRow["Page_Owner"].ToString();
                                if (string.IsNullOrEmpty(PageOwner))
                                {
                                    PageOwner = strUserID;
                                }
                                //if (!string.IsNullOrEmpty(objdataRow["Components"].ToString()))
                                //{
                                //    components = objdataRow["Components"].ToString();
                                //}
                                if (!string.IsNullOrEmpty(objdataRow["WSD_Parameters"].ToString()))
                                {
                                    WSD_Parameters = objdataRow["WSD_Parameters"].ToString();
                                }
                                if (!string.IsNullOrEmpty(objdataRow["Page_URL"].ToString()))
                                {
                                    pageURL = objdataRow["Page_URL"].ToString();
                                }
                                string StandardOperarting = objdataRow["Standard_Operating_Procedure"].ToString();
                                int TemplateID = intTemplateID;
                                if (!string.IsNullOrEmpty(objdataRow["Application_Name"].ToString()))
                                {
                                    ApplicationName = objdataRow["Application_Name"].ToString();
                                }

                                WRFMCommon objCommon = WRFMCommon.Instance;
                                using (DBManager dbManager = objCommon.DataManager)
                                {
                                    try
                                    {
                                        dbManager.Open();
                                        if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                        {
                                            dbManager.AddParameters("@chvOperationType", "INSERT");
                                        }
                                        if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                        {
                                            dbManager.AddParameters("@chvOperationType", "UPDATE");
                                        }
                                        dbManager.AddParameters("@Id", ID);
                                        dbManager.AddParameters("@Master_Page_ID ", Master_Page_ID);
                                        dbManager.AddParameters("@Page_Sequence", PageSequence);
                                        dbManager.AddParameters("@Template_ID", TemplateID);
                                        dbManager.AddParameters("@Terminate_Status", blnTerminated);
                                        dbManager.AddParameters("@Master_Page_Name", MasterPageName);
                                        dbManager.AddParameters("@Page_Title_Template", Page_Title_Template);
                                        dbManager.AddParameters("@Page_Owner", PageOwner);
                                        dbManager.AddParameters("@Page_URL", pageURL);
                                        dbManager.AddParameters("@Standard_Operating_Procedure", StandardOperarting);
                                        dbManager.AddParameters("@Asset_Type", AssetType);
                                        dbManager.AddParameters("@Connection_Type", ConnectionType);

                                        dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Decipline);
                                        dbManager.AddParameters("@ToolTip", Tooltip);
                                        //dbManager.AddParameters("@Components", components);
                                        dbManager.AddParameters("@Application_Name", ApplicationName);
                                        dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);
                                        dbManager.AddParameters("@usrname", strUserID);
                                        dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_SaveTemplatePageMapping");
                                        //if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                        intID = Int32.Parse(dbManager.Parameters[18].Value.ToString());
                                        // blnIsUpdateSuccess = true;
                                        intNoOfMasterPages++;
                                        //objCommonDAL.UpdateListAuditHistory(siteUrl, auditListName, Convert.ToInt32(objListItem[CommonConstants.ID])
                                        UpdateListTemplateMappingAuditHistory(intID, strUserID, EBookConstants.AUDITACTIONUPDATION);
                                    }
                                    catch (Exception ex)
                                    {
                                        WRFMCommon.Instance.Error.Handle(ex);
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return intID;
        }






        /// <summary>
        /// Updates the details of the Master Page whiled edited
        /// Called from MasterPage.ascx
        /// </summary>
        /// <param name="siteUrl">Site URL.</param>
        /// <param name="listName">List Name.</param>
        /// <param name="auditListName">Audit List Name.</param>
        /// <param name="objListEntry">List Entry object.</param>
        /// <param name="actionPerformed">Audit Action.</param>
        /// <param name="userName">User Name.</param>
        /// <returns>returns the TemplateID associated with the master page.</returns>
        /// <exception cref="">Handled in calling method.</exception>
        //public int UpdateMasterPageDetail(ListEntry objListEntry, string actionPerformed, string userName)
        public int UpdateMasterPageDetail(ListEntry objListEntry, string strUserID)
        {
            int intTemplateID = 0;

            if (objListEntry != null && objListEntry.MasterPage != null)
            {
                int ID = (objListEntry.MasterPage.RowId);

                string Decipline = objListEntry.MasterPage.SignOffDiscipline;//Discipline
                string Standard_Operating_Procedure = objListEntry.MasterPage.SOP;//Standard_Operating_Procedure 
                string Page_Title_Template = objListEntry.MasterPage.TemplateTitle;//Page_Title_Template 
                string Master_Page_Name = objListEntry.MasterPage.Name;//Master_Page_Name
                string ToolTip = objListEntry.MasterPage.ToolTip;
                string WSD_Parameters = objListEntry.MasterPage.WSDAttributes;  // issue 177263
                WRFMCommon objCommon = WRFMCommon.Instance;
                using (DBManager dbManager = objCommon.DataManager)
                {
                    try
                    {
                        dbManager.Open();

                        dbManager.AddParameters("@Id", ID);
                        dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Decipline);
                        dbManager.AddParameters("@Standard_Operating_Procedure", Standard_Operating_Procedure);
                        dbManager.AddParameters("@Page_Title_Template", Page_Title_Template);
                        dbManager.AddParameters("@Master_Page_Name", Master_Page_Name);
                        dbManager.AddParameters("@ToolTip", ToolTip);
                        dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);  // issue 177263
                        dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_UpdateTemplateMappingPages");
                        if (!string.IsNullOrEmpty(dbManager.Parameters[7].Value.ToString()))
                            intTemplateID = Int32.Parse(dbManager.Parameters[7].Value.ToString());

                    }
                    catch (Exception ex)
                    {
                        WRFMCommon.Instance.Error.Handle(ex);
                    }
                }

                UpdateListTemplateMappingAuditHistory(ID, strUserID, EBookConstants.AUDITACTIONUPDATION);
                //objCommonDAL.UpdateListAuditHistory(siteUrl, auditListName, objListEntry.MasterPage.RowId,
                //    userName, actionPerformed);
            }

            return intTemplateID;
        }



        public DataTable GetTeamNames(string strTeamID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, strTeamID);
            return ExecuteAndFetch(strProc, paramList);

        }

        public DataTable FetchMasterDetails(int intMasterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, intMasterID);
            return ExecuteAndFetch("SP_eBooks_FetchMasterDetails", paramList);
        }

        public void UpdatePageSequenceForArchive(DataTable dt, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@tblPageSequence", dt);
            ExecuteAndFetch(storProc, paramList);
        }

        public DataTable DeleteTemplateMappingDetails(int intTemplateID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, intTemplateID);
            return ExecuteAndFetch("SP_eBooks_DeleteTemplatePageMappingDetails", paramList);
        }

        public DataTable GetLinkedTemplateMappingDetails(int intTemplateID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedTemplateID", intTemplateID);
            return ExecuteAndFetch("SP_eBooks_GetTemplatePageMappingDetails", paramList);
        }


        public DataTable GetMasterDetailsForTemplateUpdate(string AssetName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AssetName", AssetName);
            return ExecuteAndFetch("SP_eBooks_GetMasterDetailsForUpdate", paramList);
        }

        public DataTable GetTemplateMappingDetails(string selectedTemplate)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedTemplate", selectedTemplate);
            return ExecuteAndFetch("SP_eBooks_GetTemplateMappingDetails", paramList);
        }



        public DataTable UpdateTemplateIDInMasterPages(string selectedTemplateIDs, int masterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", selectedTemplateIDs);
            paramList.Add("@MasterID", masterId);
            return ExecuteAndFetch("SP_eBooks_UpdateTemplateColumn", paramList);
        }


        /// <summary>
        /// Reads the table.
        /// </summary>
        /// <param name=CommonConstants.VALUE>The value.</param>
        /// <param name="StorProc">The stor proc.</param>
        /// <returns></returns>
        public DataTable ReadTable(string value, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            if (string.Equals(StorProc, EBookConstants.SPUSERREGISTRATION) || string.Equals(StorProc, EBookConstants.SPTEAMREGISTRATION) || string.Equals(StorProc, EBookConstants.SPDUALLISTLEFTBOX))
            {
                if (value == EBookConstants.STATUS_TERMINATED)
                    value = CommonConstants.TRUE;
                if (value == EBookConstants.STATUS_ACTIVE)
                    value = CommonConstants.FALSE;
            }
            paramList.Add(EBookConstants.ValuePARAM, value);
            return ExecuteAndFetch(StorProc, paramList);
        }

        public DataTable GetValuefromtable(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            return ExecuteAndFetch("SP_eBooks_ChapterPagesMapping_Proc", paramList);
        }

        public DataTable GetLastDatefromtable(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            return ExecuteAndFetch("SP_eBooks_GetLastdate", paramList);
        }

        public DataTable GetDWBuserdata(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@userid ", value);
            return ExecuteAndFetch("SP_eBooks_GetDWBUserdata", paramList);
        }
        public DataTable GetTeamstaffdata(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intUserId", value);
            return ExecuteAndFetch("SP_eBooks_GetDWBTeamStaffdata", paramList);
        }
        public DataTable GetDWBBookprevilage(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.PARAPRIVILEGE, value);
            return ExecuteAndFetch("SP_eBooks_DWBBookprevilage", paramList);
        }
        public DataTable Updatechapterpagesmapping(string value, string signoff, string lastsodate)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            paramList.Add("@Sign_Off_Status", signoff);
            paramList.Add("@Last_SO_Date", lastsodate);
            return ExecuteAndFetch("SP_eBooks_UpdateChapterPagesmapping_Proc", paramList);
        }
        public DataTable UpdateeWBreportPageLibrary(string value, string Type, string User, DateTime Modified)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            paramList.Add("@Type", Type);
            paramList.Add(CommonConstants.ModifiedPARAM, Modified);
            paramList.Add(DreamConstants.UserPARAM, User);
            return ExecuteAndFetch("SP_eBooks_UpdateeWBreportPageLibrary", paramList);
        }

        //Added for WRFMMigration 4.2 requirment-95764<start>
        public void CleareWBreportPageLibrary(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            ExecuteAndFetch("SP_eBooks_CleareWBreportPageLibrary", paramList);
        }
        //Added for WRFMMigration 4.2 requirment-95764<end>

        public DataTable GetRadedtorContent(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            return ExecuteAndFetch("SP_eBooks_GetRadedtorContent", paramList);
        }
        public DataTable GetChapterPagesMappingAuditrail(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            return ExecuteAndFetch("SP_eBooks_ChapterPagesMappingAuditrail", paramList);
        }
        public DataTable UpdateListAuditHistory(string value, string User, DateTime Date, int actionPerformed)
        {

            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add("@actionPerformed", actionPerformed);
            paramList.Add("@PageID", int.Parse(value));
            paramList.Add(DreamConstants.UserPARAM, User);
            paramList.Add("@Date", Date);
            return ExecuteAndFetch("SP_eBooks_UpdateChapterAuditTrial", paramList);

        }
        public DataTable UpdateDocumentStatus(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", value);
            return ExecuteAndFetch("SP_eBooks_UpdateDocumentStatus", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="content"></param>
        /// <param name="userName"></param>
        /// <param name="auditAction"></param>
        /// <param name="isEmpty"></param>
        /// <param name="storProc"></param>
        public void UpdateType4andPageAuditTrail(int pageId, string content, string userName, int auditAction, bool isEmpty, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageID", pageId);
            paramList.Add("@chvType", content);
            paramList.Add("@chvUser", userName);
            paramList.Add("@intAuditAction", auditAction);
            paramList.Add("@blIsEmpty", isEmpty);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="StorProc"></param>
        /// <returns></returns>
        public DataTable GetGenericPlotBasedOnID(string id, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intIdValue", id);
            return ExecuteAndFetch(StorProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetNames"></param>
        /// <param name="StorProc"></param>
        /// <returns></returns>
        public DataTable GetGenericPlotMappedColumns(string assetNames, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvAssetNames", assetNames);
            return ExecuteAndFetch(StorProc, paramList);
        }

        /// <summary>
        /// Inserts a new row to Generic Plot
        /// </summary>
        /// <param name="chartName"></param>
        /// <param name="firstPrimaryBottomColumn"></param>
        /// <param name="firstPrimaryLeftColumn"></param>
        /// <param name="dtSeries"></param>
        /// <param name="genericChartName"></param>
        /// <param name="viewName"></param>
        /// <param name="assets"></param>
        /// <param name="mappedColumns"></param>
        /// <param name="columnSelected"></param>
        /// <param name="unitColumnSelected"></param>
        /// <param name="storedProc"></param>
        /// <returns></returns>
        public DataTable SaveGenericPlot(string chartName, string firstPrimaryBottomColumn, string firstPrimaryLeftColumn, DataTable dtSeries, string genericChartName, string viewName, string assets, string mappedColumns, string columnSelected, string unitColumnSelected, string userName, bool blIsZoomApplicable, string strLegendBoxPosition, int intWidth, int intHeight, int intDS_ID, string strDataRangeColumn, bool blShowNearByAssets, bool blGroupAssets, string strIdentifierColumn, string storedProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvTitle", chartName);
            paramList.Add("@chvXAxisText", firstPrimaryBottomColumn);
            paramList.Add("@chvYAxisText", firstPrimaryLeftColumn);
            paramList.Add(DreamConstants.tblSeriesLevelPropPARAM, dtSeries);
            paramList.Add("@chvChartName", genericChartName);
            paramList.Add("@chvReportName", viewName);
            paramList.Add(SEDConstants.PARAMASSETNAME, assets);
            paramList.Add("@chvMappedColumnName", mappedColumns);
            paramList.Add("@chvGenericViewColumns", columnSelected);
            paramList.Add("@chvGenericViewColumnUnits", unitColumnSelected);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add(DreamConstants.IsZoomApplicablePARAM, blIsZoomApplicable);
            paramList.Add(CommonConstants.ChartLegendBoxPositionPARAM, strLegendBoxPosition);
            paramList.Add(CommonConstants.WidthPARAM, intWidth);
            paramList.Add(CommonConstants.HeightPARAM, intHeight);
            paramList.Add("@DS_ID", intDS_ID);
            paramList.Add(DreamConstants.DateRangeColumnPARAM, strDataRangeColumn);
            paramList.Add(DreamConstants.ShowNearByAssetsPARAM, blShowNearByAssets);
            paramList.Add(DreamConstants.GroupAssetsPARAM, blGroupAssets);
            paramList.Add(DreamConstants.IdentifierColumnPARAM, strIdentifierColumn);
            return ExecuteAndFetch(storedProc, paramList);
        }

        /// <summary>
        /// Updates generic plot table
        /// </summary>
        /// <param name="chartName"></param>
        /// <param name="firstPrimaryBottomColumn"></param>
        /// <param name="firstPrimaryLeftColumn"></param>
        /// <param name="dtSeries"></param>
        /// <param name="genericChartName"></param>
        /// <param name="viewName"></param>
        /// <param name="assets"></param>
        /// <param name="mappedColumns"></param>
        /// <param name="columnSelected"></param>
        /// <param name="unitColumnSelected"></param>
        /// <param name="storedProc"></param>
        /// <returns></returns>
        public DataTable UpdateGenericPlot(string chartName, string firstPrimaryBottomColumn, string firstPrimaryLeftColumn, DataTable dtSeries, string genericChartName, string viewName,
            string assets, string mappedColumns, string columnSelected, string unitColumnSelected, int ewbComponentId, string isNameChanged, string userName, bool blIsZoomApplicable, string strLegendBoxPosition, int intWidth, int intHeight, int intDS_ID, string strDataRangeColumn, bool blShowNearByAssets, bool blGroupAssets, string strIdentifierColumn, string storedProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvTitle", chartName);
            paramList.Add("@chvXAxisText", firstPrimaryBottomColumn);
            paramList.Add("@chvYAxisText", firstPrimaryLeftColumn);
            paramList.Add(DreamConstants.tblSeriesLevelPropPARAM, dtSeries);
            paramList.Add("@chvChartName", genericChartName);
            paramList.Add("@chvReportName", viewName);
            paramList.Add(SEDConstants.PARAMASSETNAME, assets);
            paramList.Add("@chvMappedColumnName", mappedColumns);
            paramList.Add("@chvGenericViewColumns", columnSelected);
            paramList.Add("@chvGenericViewColumnUnits", unitColumnSelected);
            paramList.Add("@chvIsNameChanged", isNameChanged);
            paramList.Add("@intChartID", ewbComponentId);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add(DreamConstants.IsZoomApplicablePARAM, blIsZoomApplicable);
            paramList.Add(CommonConstants.ChartLegendBoxPositionPARAM, strLegendBoxPosition);
            paramList.Add(CommonConstants.WidthPARAM, intWidth);
            paramList.Add(CommonConstants.HeightPARAM, intHeight);
            paramList.Add("@DS_ID", intDS_ID);
            paramList.Add(DreamConstants.DateRangeColumnPARAM, strDataRangeColumn);
            paramList.Add(DreamConstants.ShowNearByAssetsPARAM, blShowNearByAssets);
            paramList.Add(DreamConstants.GroupAssetsPARAM, blGroupAssets);
            paramList.Add(DreamConstants.IdentifierColumnPARAM, strIdentifierColumn);
            return ExecuteAndFetch(storedProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="genericChartName"></param>
        /// <param name="StorProc"></param>
        /// <returns></returns>
        public DataTable GetChartSeriesForGenericPlot(string genericChartName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvChartName", genericChartName);
            return ExecuteAndFetch(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="StorProc"></param>
        /// <returns></returns>
        public DataTable ReadTable(string storProc)
        {
            return ExecuteAndFetch(storProc);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="storProc"></param>
        /// <param name="bookType"></param>
        /// <param name="activeTeam"></param>
        /// <param name="isFavouriteSelected"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public DataSet GetDwbHomePageData(string storProc, string bookType, string activeTeam, bool isFavouriteSelected, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookType", bookType);
            paramList.Add("@chvActiveTeam", activeTeam);
            paramList.Add("@bitIsFavouritesSelected", isFavouriteSelected);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storProc"></param>
        /// <param name="country"></param>
        /// <param name="assetType"></param>
        /// <param name="identifier"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public DataSet GetDwbHomePageDataForContextSearchMenu(string storProc, string country, string assetType, string identifier, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@country", country);
            paramList.Add(DreamConstants.ASSETTYPEPARAM, assetType);
            paramList.Add("@identifier", identifier);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="bookId"></param>
        /// <param name="actionPerformed"></param>
        /// <param name="storProc"></param>
        public void UpdateListBookAuditHistory(string username, int bookId, string actionPerformed, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, username);
            paramList.Add("@intBookId", bookId);
            paramList.Add("@chvAuditAction", actionPerformed);
            ExecuteAndFetch(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="printLevel"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetAlertMessage(string printLevel, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvPrintLevel", printLevel);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storProc"></param>
        /// <param name="bookIds"></param>
        /// <param name="userName"></param>
        public void SaveOrUpdateFavouriteBooks(string storProc, string bookIds, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookIds", bookIds);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            ExecuteAndFetchAsDataSet(storProc, paramList);
        }
        public DataTable ReadTable(bool value, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);
            return ExecuteAndFetch(StorProc, paramList);
        }

        public bool UpdateTeamStaffList(string UserId, ListEntry listEntry, int ID, string auditAction)
        {
            bool blnUpdateSuccess = false;
            string strUserName = listEntry.UserDetails.WindowUserID;
            string strDiscipline = listEntry.UserDetails.Discipline;
            string strPrivilege = listEntry.UserDetails.PrivilegeCode;
            string strTeam = listEntry.UserDetails.Team;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            if (string.Equals(auditAction, EBookConstants.AUDITACTIONCREATION))
                paramList.Add("@chvOperationType", "INSERT");
            else if (string.Equals(auditAction, EBookConstants.AUDITACTIONUPDATION))
                paramList.Add("@chvOperationType", "UPDATE");
            paramList.Add(SEDConstants.PARAUSERNAME, strUserName);
            paramList.Add("@Team_ID", strTeam);
            paramList.Add(SEDConstants.PARAMUSERID, ID);
            paramList.Add("@UserRank", 0);
            paramList.Add(CommonConstants.PARAMID, 0);
            paramList.Add(CommonConstants.PARAMDISCIPLINE, strDiscipline);
            paramList.Add("@Created", "0");
            paramList.Add(CommonConstants.ModifiedPARAM, "0");
            paramList.Add(SEDConstants.PARAPRIVILEGE, strPrivilege);
            paramList.Add(CommonConstants.PARAMCREATEDBY, UserId);
            paramList.Add(CommonConstants.PARAMMODIFIEDBY, UserId);
            ExecuteAndFetch(EBookConstants.SPINSERTUPDATETEAMSTAFFLIST, paramList);
            blnUpdateSuccess = true;
            return blnUpdateSuccess;
        }
        #region Release 6.0

        //public DataTable GetStoryBoardInfo(string value)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@PageID", value);
        //    return ExecuteAndFetch("SP_eBooks_DWBStoryBoardInfo", paramList);
        //}
        #endregion Release 6.0
        /// <summary>
        /// Update Audit History.
        /// </summary>
        /// <param name="siteURL">Site URL.</param>
        /// <param name="listName">List Name.</param>
        /// <param name="auditListName">Audit List Name.</param>
        /// <param name="rowId">ID of the item which audit needs to be updated.</param>
        /// <param name="pageTitle">Master Page Title</param>
        /// <param name="userName">Name of the user updating.</param>
        /// <param name="actionPerformed">Action Performed.</param>
        public void UpdateListAuditHistory(string userName, int intRowID, string title, string actionPerformed, int ID, string pageType, string pageCategory = "")
        {


            DataTable dtList = null;
            DataRow objDataRow;
            //int ID = 0;

            string strSelectedID = string.Empty;
            string TerminateStatus = string.Empty;
            //string UserID = listEntry.UserDetails.WindowUserID;
            switch (pageType)
            {
                case EBookConstants.MASTERPAGE:
                    {
                        try
                        {

                            Dictionary<string, object> paramList = new Dictionary<string, object>();

                            paramList.Add("@Master_ID", intRowID);
                            paramList.Add(CommonConstants.PARAMTITLE, title);
                            dtList = ExecuteAndFetch("SP_eBooks_GetMasterPageDetailsForAudit", paramList);

                            for (int intIndex = 0; intIndex < dtList.Rows.Count; intIndex++)
                            {
                                objDataRow = dtList.Rows[intIndex];
                                strSelectedID = objDataRow[CommonConstants.ID].ToString();
                                TerminateStatus = objDataRow["Terminate_Status"].ToString();


                                Dictionary<string, object> paramListInsert = new Dictionary<string, object>();
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                {
                                    //paramListInsert.Add("@chvOperationType", "INSERT");
                                    actionPerformed = "Created";
                                }
                                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                                {
                                    actionPerformed = "Updated";
                                    //paramListInsert.Add("@chvOperationType", "UPDATE");
                                }
                                paramListInsert.Add("@Id", ID);
                                paramListInsert.Add("@Master_ID", strSelectedID);
                                paramListInsert.Add("@Terminate_Status", TerminateStatus);
                                paramListInsert.Add("@Audit_Action", actionPerformed);
                                paramListInsert.Add(DreamConstants.UserPARAM, userName);
                                // ExecuteAndFetch("SP_eBooksInsertUpdateMasterAudit", paramListInsert);
                                ExecuteAndFetch("SP_eBooks_InsertMasterAuditTable", paramListInsert);


                            }

                        }
                        finally
                        {
                            if (dtList != null) dtList.Dispose();
                        }
                    }
                    break;
                case EBookConstants.TEMPLATEREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();

                        paramList.Add("@Temp_ID", intRowID);

                        dtList = ExecuteAndFetch("SP_eBooks_GetTemplateDetailsForAudit", paramList);

                        for (int intIndex = 0; intIndex < dtList.Rows.Count; intIndex++)
                        {
                            objDataRow = dtList.Rows[intIndex];
                            strSelectedID = objDataRow[CommonConstants.ID].ToString();
                            TerminateStatus = objDataRow["Terminate_Status"].ToString();


                            Dictionary<string, object> paramListInsert = new Dictionary<string, object>();
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                            {
                                actionPerformed = "Created";
                            }
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                            {
                                actionPerformed = "Updated";
                            }
                            paramListInsert.Add("@Id", ID);
                            paramListInsert.Add("@Temp_ID", strSelectedID);
                            paramListInsert.Add("@Terminate_Status", TerminateStatus);
                            paramListInsert.Add("@Audit_Action", actionPerformed);
                            paramListInsert.Add(DreamConstants.UserPARAM, userName);
                            paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                            ExecuteAndFetch("SP_eBooks_InsertTemplateAudit", paramListInsert);

                        }
                    }
                    break;
                case EBookConstants.WELLBOOKREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);

                        paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertBookAuditTrail", paramListInsert);

                    }
                    break;

                case EBookConstants.TEMPLATEPAGESREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);

                        paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertTemplatePagesAudit", paramListInsert);


                    }
                    break;
                case EBookConstants.WELLBOOK:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);

                        paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertBookAuditTrail", paramListInsert);


                    }
                    break;
                case EBookConstants.CHAPTERPAGEMAPPINGREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);
                        paramListInsert.Add("@PageType", pageCategory);
                        //paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertBookPagesAuditTrail", paramListInsert);


                    }
                    break;

                case EBookConstants.CHAPTERREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);

                        //paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        // ExecuteAndFetch("SP_eBooks_InsertChapterPagesAuditTrail", paramListInsert);
                        ExecuteAndFetch("SP_eBooks_InsertChapterAuditTrail", paramListInsert);
                    }
                    break;

                case EBookConstants.CHAPTERPAGEREPORT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);

                        //paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertChapterPagesAuditTrail", paramListInsert);

                    }
                    break;
                //WRFMMigration_Release [4.2] <End>
                case EBookConstants.GENERICPLOT:
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@GenericPlot_ID", intRowID);

                        //paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("SP_eBooks_InsertGenericPlotAuditTrail", paramListInsert);

                    }
                    break;
                //WRFMMigration_Release [4.2] <End>

                //WRFMMigration_Release [5.1] <Start>
                case "GenericTable":
                    {
                        Dictionary<string, object> paramList = new Dictionary<string, object>();



                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();

                        //paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@GenericComponent_ID", intRowID);

                        //paramListInsert.Add(CommonConstants.PARAMTITLE, title);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);

                        ExecuteAndFetch("USP_eBooks_InsertGenericComponentAuditTrail", paramListInsert);

                    }
                    break;
                //WRFMMigration_Release [5.1] <End>
                case EBookConstants.BOOKLEVELPAGES:
                    {
                        Dictionary<string, object> paramListInsert = new Dictionary<string, object>();
                        paramListInsert.Add("@Id", ID);
                        paramListInsert.Add("@Master_ID", intRowID);
                        paramListInsert.Add("@Audit_Action", actionPerformed);
                        paramListInsert.Add(DreamConstants.UserPARAM, userName);
                        ExecuteAndFetch("USPeBooksInsertBookPagesAuditTrail", paramListInsert);
                    }
                    break;
            }

        }



        public DataTable CreateCommonListCAMLQuery(string key, string description)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@KEY", key);
            paramList.Add("@DESCRIPTION", description);

            return ExecuteAndFetch("SP_eBooks_ItemTemplate", paramList);
        }

        public DataTable ReadMasterTable(bool value, string booktype, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);
            paramList.Add("@BookType", booktype);
            return ExecuteAndFetch(StorProc, paramList);
        }

        public DataTable GetMasterTable(string selectedID, string booktype, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, selectedID);
            paramList.Add("@BookType", booktype);
            return ExecuteAndFetch(StorProc, paramList);
        }


        public DataSet GetAuditDetails(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetails", paramList);
            return dtAudit;
        }
        public DataSet GetRecordAuditDetailsForTemplate(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForTemplate", paramList);

            return dtAudit;
        }

        public DataSet GetRecordAuditDetailsForTemplatePages(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForTemplatePages", paramList);

            return dtAudit;
        }


        public DataTable ReadMasterTableForEdit(bool value, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);

            return ExecuteAndFetch(StorProc, paramList);
        }

        public DataTable GetMasterTableForEdit(string value, string StorProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);

            return ExecuteAndFetch(StorProc, paramList);
        }

        public DataTable CheckUserIsAdmin(string value)
        {

            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.usernamePARAM, value);
            return ExecuteAndFetch("SP_eBooks_CheckISAdmin", paramList);

        }

        public DataTable ReadTableForSequence(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@appname", value);
            return ExecuteAndFetch("SP_eBooks_AlterPageSequenceMaster", paramList);
        }

        public DataTable ReadTableForTemplatePagesSequence(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", value);
            return ExecuteAndFetch("SP_eBooks_AlterPageSequenceTemplatePages", paramList);
        }

        public DataTable ReadTableForChapterSequence(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, value);
            return ExecuteAndFetch("SP_eBooks_AlterPageChapterSequence", paramList);
        }

        public DataTable ReadTableForChapterPageSequence(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", value);
            return ExecuteAndFetch("SP_eBooks_AlterPageChapterPageSequence", paramList);
        }

        public DataTable UpdateListItemSequence(int pageseq, string masterpagename, int ID, string actionperformed, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@pagesequence", pageseq);
            paramList.Add("@masterpagename", masterpagename);
            paramList.Add(CommonConstants.PARAMID, ID);
            paramList.Add("@ActionPerformed", actionperformed);
            return ExecuteAndFetch(strProc, paramList);
        }

        //Template Code Start//

        public DataTable GetTemplateNames(string value, string strBookType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ASSETNAME", value);
            paramList.Add("@BOOKTYPE", strBookType);
            return ExecuteAndFetch("SP_eBooks_GetTemplateNames", paramList);
        }

        public DataTable GetCreatedTemplateID(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TEMPLATE_NAME", value);
            return ExecuteAndFetch("SP_eBooks_GetTemplateId", paramList);
        }

        public DataTable GetMasterPageID(int value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TEMPLATEID", value);
            return ExecuteAndFetch("SP_eBooks_GetMasterIDForTemplate", paramList);
        }

        public DataTable GetMasterDetailsForMasterPageID(int value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterID", value);
            return ExecuteAndFetch("SP_eBooks_GetMasterDetails", paramList);
        }


        public DataTable GetDetailsFromTemplatePageMapTable(int value, int tempId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterId", value);
            paramList.Add("@TemplateID", tempId);

            return ExecuteAndFetch("SP_eBooks_GetTemplatePageMapDetails", paramList);
        }


        public DataTable ReadTemplateDetails(int intSelectedID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intSelectedID);


            return ExecuteAndFetch("SP_eBooks_GetTemplateDetails", paramList);
        }

        public DataTable GetTemplateBookType(int intSelectedID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intSelectedID);


            return ExecuteAndFetch("SP_eBooks_GetTemplateBookType", paramList);
        }

        public string AddTemplateMasterPageMapping(ListEntry listEntry, string actionPerformed, string strUserID, int intMasterID, int ID, int PageSeq)
        {
            string[] strTemplateIds = null;
            string Tooltip = string.Empty;
            string components = string.Empty;
            string WSD_Parameters = string.Empty;
            bool blnTerminated = false;
            StringBuilder strTemplatePageMappingRowId = new StringBuilder();
            string pageURL = string.Empty;
            int intID = 0;

            if (!string.IsNullOrEmpty(listEntry.MasterPage.Templates))
            {
                strTemplateIds = listEntry.MasterPage.Templates.Split(';');
            }
            if (strTemplateIds != null)
            {
                for (int i = 0; i < strTemplateIds.Length - 1; i++)
                {
                    int Master_Page_ID = intMasterID;
                    int TemplateID = int.Parse(strTemplateIds[i]);
                    int PageSequence = PageSeq;
                    string MasterPageName = listEntry.MasterPage.Name;
                    string PageTitleTemplate = listEntry.MasterPage.TemplateTitle;
                    string AssetType = listEntry.MasterPage.AssetTypeText;
                    string Decipline = listEntry.MasterPage.SignOffDisciplineText;
                    string StandardOperarting = listEntry.MasterPage.SOP;
                    string ConnectionType = listEntry.MasterPage.ConnectionTypeText;
                    if (!string.IsNullOrEmpty(listEntry.MasterPage.ToolTip))
                        Tooltip = listEntry.MasterPage.ToolTip;
                    //if (!string.IsNullOrEmpty(listEntry.MasterPage.MasterPageComponents))
                    //{
                    //    components = listEntry.MasterPage.MasterPageComponents;
                    //}

                    if (!string.IsNullOrEmpty(listEntry.MasterPage.WSDAttributes))
                    {
                        WSD_Parameters = listEntry.MasterPage.WSDAttributes;
                    }
                    if (!string.IsNullOrEmpty(listEntry.MasterPage.PageURL))
                    {
                        pageURL = listEntry.MasterPage.PageURL;
                    }

                    string pageOwner = listEntry.MasterPage.PageOwner;

                    if (string.IsNullOrEmpty(pageOwner))
                    {
                        pageOwner = strUserID;
                    }


                    string ApplicationName = listEntry.MasterPage.AppName;

                    WRFMCommon objCommon = WRFMCommon.Instance;
                    using (DBManager dbManager = objCommon.DataManager)
                    {
                        try
                        {
                            dbManager.Open();
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "INSERT");
                            }
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "UPDATE");
                            }
                            dbManager.AddParameters("@Id", ID);
                            dbManager.AddParameters("@Master_Page_ID ", Master_Page_ID);
                            dbManager.AddParameters("@Page_Sequence", PageSequence);
                            dbManager.AddParameters("@Template_ID", TemplateID);
                            dbManager.AddParameters("@Terminate_Status", blnTerminated);
                            dbManager.AddParameters("@Master_Page_Name", MasterPageName);
                            dbManager.AddParameters("@Page_Title_Template", PageTitleTemplate);
                            dbManager.AddParameters("@Page_Owner", pageOwner);
                            dbManager.AddParameters("@Page_URL", pageURL);
                            dbManager.AddParameters("@Standard_Operating_Procedure", StandardOperarting);
                            dbManager.AddParameters("@Asset_Type", AssetType);
                            dbManager.AddParameters("@Connection_Type", ConnectionType);

                            dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Decipline);
                            dbManager.AddParameters("@ToolTip", Tooltip);
                            //dbManager.AddParameters("@Components", components);
                            dbManager.AddParameters("@Application_Name", ApplicationName);
                            dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);
                            dbManager.AddParameters("@usrname", strUserID);
                            dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_SaveTemplatePageMapping");
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                intID = Int32.Parse(dbManager.Parameters[18].Value.ToString());
                            // blnIsUpdateSuccess = true;

                            //objCommonDAL.UpdateListAuditHistory(siteUrl, auditListName, Convert.ToInt32(objListItem[CommonConstants.ID])
                        }
                        catch (Exception ex)
                        {
                            WRFMCommon.Instance.Error.Handle(ex);
                        }
                    }

                    UpdateHasMasterPageColumn(Convert.ToInt32(strTemplateIds[i]), 1);
                    strTemplatePageMappingRowId.Append(Convert.ToString(intID) + ";");

                }
            }
            return strTemplatePageMappingRowId.ToString();
        }



        /// <summary>
        /// Updates the Has_MasterPage column in DWB Template list
        /// </summary>
        /// <param name="siteUrl">Site URL.</param>
        /// <param name="listName">List Name(DWB Template).</param>
        /// <param name="templateID">TemplateID.</param>
        /// <param name="noOfMasterPages">No of MasterPages in a template.</param>
        public bool UpdateHasMasterPageColumn(int templateID, int noOfMasterPages)
        {
            bool blnUpdateSuccess = false;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@templateID", templateID);
            paramList.Add("@noOfMasterID", noOfMasterPages);
            ExecuteAndFetch("SP_eBooks_UpdateHasMasterPageColumn", paramList);
            blnUpdateSuccess = true;
            return blnUpdateSuccess;
        }

        public DataTable GetPageSequence(int rowId)
        {
            DataTable dtPageSeq = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@rowid", rowId);
            dtPageSeq = ExecuteAndFetch("SP_eBooks_GetPageSequence", paramList);

            return dtPageSeq;
        }


        public DataTable GetMasterPageDetail(string strAssetType, string strAppName)
        {
            DataTable dtMasterDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.ASSETTYPE, strAssetType);
            paramList.Add("@AppName", strAppName);
            dtMasterDetail = ExecuteAndFetch("SP_eBooks_GetMasterDetail", paramList);

            return dtMasterDetail;
        }



        public DataTable GetMappedMasterPageDetail(int intTemplateID)
        {
            DataTable dtMasterDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intTemplateID);
            dtMasterDetail = ExecuteAndFetch("SP_eBooks_GetMappedMasterDetails", paramList);
            return dtMasterDetail;
        }

        public DataTable GetLinkedMasterDetails(string strMasterID)
        {
            DataTable dtMasterDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@masterID", strMasterID);
            dtMasterDetail = ExecuteAndFetch("SP_eBooks_GetLinkedMasterPageDetail", paramList);
            return dtMasterDetail;
        }



        public DataTable GetTemplatePagesRecords(int intTemplateID, string strAppName, bool value)
        {
            DataTable dtMasterDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intTemplateID);
            paramList.Add("@BookType", strAppName);
            paramList.Add("@value", value);
            dtMasterDetail = ExecuteAndFetch("SP_eBooks_MaintainTemplatePages", paramList);
            return dtMasterDetail;
        }

        public DataTable GetAllBooksRecords(bool value, bool tobeDeleted, string strBookType, UserPrivilege Privilege)
        {
            DataTable dtMasterDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);
            paramList.Add("@ToBeDeleted", tobeDeleted);
            paramList.Add("@BookType", strBookType);
            if (Privilege == UserPrivilege.Normal || Privilege == UserPrivilege.FirstLineAdmin)
            {
                //paramList.Add("@Username", AWR_Utility.DisplayName);
                paramList.Add("@CreatedBy", AWR_Utility.UserName);
                dtMasterDetail = ExecuteAndFetch("SP_eBooks_MaintainBooks_ByUsername", paramList);
            }
            else
            {
                dtMasterDetail = ExecuteAndFetch("SP_eBooks_MaintainBooks", paramList);
            }
            return dtMasterDetail;
        }
        public DataTable GetFavBooks(string strFavBooks, bool value, bool tobeDeleted, string strBookType)
        {
            DataTable dtFavBooks = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, value);
            paramList.Add("@ToBeDeleted", tobeDeleted);


            paramList.Add("@BookType", strBookType);
            paramList.Add(EBookConstants.PARM_BOOK_ID, strFavBooks);
            dtFavBooks = ExecuteAndFetch("SP_eBooks_FavMaintainBooks", paramList);
            return dtFavBooks;
        }



        /// <summary>
        /// Gets the Master Pages for the selected Template
        /// </summary>
        /// <param name="siteUrl">Site URL.</param>
        /// <param name="listName">List Name (DWB Template).</param>
        /// <param name="queryString">CAML Query.</param>
        /// <param name="viewFields">View Fields.</param>
        /// <returns>List Entry object with TemplateConfiguration values assigned.</returns>
        /// <exception cref="">Handled in calling method.</exception>
        public ListEntry GetMasterTemplatePageDetail(string strTemplateID)
        {
            ListEntry objListEntry = null;

            DataTable dtGetTemplatePageDetail = null;
            dtGetTemplatePageDetail = GetTemplatePageDetails(strTemplateID);
            if (dtGetTemplatePageDetail != null && dtGetTemplatePageDetail.Rows.Count > 0)
            {
                objListEntry = new ListEntry();
                MasterPageDetails objMasterPageDetails = new MasterPageDetails();
                foreach (DataRow objdataRow in dtGetTemplatePageDetail.Rows)
                {
                    objMasterPageDetails.RowId = Convert.ToInt32(objdataRow[CommonConstants.ID]);//ID
                    objMasterPageDetails.AssetType = Convert.ToString(objdataRow["Asset_Type"]);//Asset_Type 
                    objMasterPageDetails.ConnectionType = Convert.ToString(objdataRow["Connection_Type"]);// Connection_Type 
                    objMasterPageDetails.PageOwner = Convert.ToString(objdataRow["Page_Owner"]); //Page_Owner 
                    objMasterPageDetails.PageSequence = Convert.ToInt32(objdataRow["Page_Sequence"]);//Page_Sequence
                    objMasterPageDetails.PageURL = Convert.ToString(objdataRow["Page_URL"]);//Page_URL 
                    objMasterPageDetails.SignOffDiscipline = Convert.ToString(objdataRow[EBookConstants.DISCIPLINE]);//Discipline
                    objMasterPageDetails.SOP = Convert.ToString(objdataRow["Standard_Operating_Procedure"]);//Standard_Operating_Procedure 
                    objMasterPageDetails.TemplateTitle = Convert.ToString(objdataRow["Page_Title_Template"]);//Page_Title_Template 
                    objMasterPageDetails.Templates = Convert.ToString(objdataRow["Template_ID"]);//Template_ID 
                    objMasterPageDetails.Name = Convert.ToString(objdataRow["Master_Page_Name"]);//Master_Page_Name 
                    objMasterPageDetails.MasterPageID = Convert.ToString(objdataRow["Master_Page_ID"]);//Master_Page_ID
                    // objMasterPageDetails.MasterPageComponents = Convert.ToString(objdataRow["Components"]);
                    objMasterPageDetails.WSDAttributes = Convert.ToString(objdataRow["WSD_Parameters"]);
                    objMasterPageDetails.ToolTip = Convert.ToString(objdataRow["ToolTip"]);

                    objListEntry.MasterPage = objMasterPageDetails;
                }
            }

            return objListEntry;
        }

        public DataTable GetTemplatePageDetails(string strTemplateID)
        {
            DataTable dtTemplatePageDetail = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", strTemplateID);

            dtTemplatePageDetail = ExecuteAndFetch("SP_eBooks_FetchTemplatePagesDetails", paramList);
            return dtTemplatePageDetail;
        }

        /// <summary>
        /// Updates the template mapping lite item audit history.
        /// </summary>
        /// <param name="siteURL">The site URL.</param>
        /// <param name="auditListName">Name of the audit list.</param>
        /// <param name="rowId">The row id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="actionPerformed">The action performed.</param>
        public void UpdateListTemplateMappingAuditHistory(int rowId, string userName, string actionPerformed)
        {
            try
            {

                int ID = 0;

                Dictionary<string, object> paramListInsert = new Dictionary<string, object>();
                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                {
                    paramListInsert.Add("@chvOperationType", "INSERT");
                }
                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                {
                    paramListInsert.Add("@chvOperationType", "UPDATE");
                }
                if (string.Equals(actionPerformed, "3"))
                {
                    paramListInsert.Add("@chvOperationType", "MODIFY");
                }
                paramListInsert.Add("@Id", ID);
                paramListInsert.Add(DreamConstants.UserPARAM, userName);
                // paramListInsert.Add("@TemplatePageID", intTemplateID);
                paramListInsert.Add("@Master_ID", rowId);

                paramListInsert.Add("@Audit_Action", actionPerformed);



                ExecuteAndFetch("SP_eBooks_InsertUpdateTemplatePagesAudit", paramListInsert);



            }
            finally
            {

            }
        }

        public DataTable GetEBooksList()
        {
            DataTable dtEBooksList = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            dtEBooksList = ExecuteAndFetch(EBookConstants.SPGETBOOKSLIST, paramList);
            return dtEBooksList;
        }
        public DataTable GetPagesList()
        {
            DataTable dtPagesList = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            dtPagesList = ExecuteAndFetch(EBookConstants.SPGETPAGESLIST, paramList);
            return dtPagesList;
        }

        public DataTable GetUserPrevilage(string username)
        {

            DataTable dtUserPrevilage = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.usernamePARAM, username);

            dtUserPrevilage = ExecuteAndFetch("SP_eBooks_GetUserPrevilage", paramList);
            return dtUserPrevilage;

        }

        public DataTable GetTeamNameForAdmin(bool blTerminatedStatus)
        {

            DataTable dtTeamName = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@blTerminatedStatus", blTerminatedStatus);

            dtTeamName = ExecuteAndFetch("SP_eBooks_GetTeamNameForAdmin", paramList);
            return dtTeamName;

        }

        public DataTable GetUserID(string strUsrID)
        {

            DataTable dtTeamName = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.usernamePARAM, strUsrID);

            dtTeamName = ExecuteAndFetch("SP_eBooks_GetUser", paramList);
            return dtTeamName;

        }

        public DataTable GetAllTeams(string strUsrID)
        {

            DataTable dtTeamName = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@userID", strUsrID);

            dtTeamName = ExecuteAndFetch("SP_eBooks_GetAllTeamsForLoggedInUser", paramList);
            return dtTeamName;

        }

        public DataTable GetOwner(int intTeamID)
        {

            DataTable dtOwnerName = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", intTeamID);

            dtOwnerName = ExecuteAndFetch("SP_eBooks_GetOwner", paramList);
            return dtOwnerName;

        }

        /// <summary>
        /// Checks the duplicate entry.
        /// </summary>
        /// <param name=CommonConstants.VALUE>The value.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public DataTable CheckDuplicateDataEntry(string value, string columnName, string tableName, string ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ColumnName", columnName);
            paramList.Add(CommonConstants.TableNamePARAM, tableName);
            paramList.Add("@Value1", value);
            //paramList.Add("@Disc", Disc);
            paramList.Add(CommonConstants.PARAMID, ID);
            return ExecuteAndFetch("SP_eBooks_CheckDuplicate_Edit_Book", paramList);
        }

        public DataTable GetUser(string strUsrName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.usernamePARAM, strUsrName);
            return ExecuteAndFetch("SP_eBooks_GetUser", paramList);
        }



        public DataTable GetUserDetails(bool Terminated, string strUsername)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Terminated", Terminated);
            paramList.Add(CommonConstants.usernamePARAM, strUsername);
            return ExecuteAndFetch("SP_eBooks_GetUserDetails", paramList);
        }

        public DataTable SaveOrUpdateFavouriteBookIDs(string SelectedAsFavorite, string strUserID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookIds", SelectedAsFavorite);
            paramList.Add(CommonConstants.PARAMUSERNAME, strUserID);
            return ExecuteAndFetch("SP_eBooks_UpdateUserFavouriteBooks", paramList);
        }


        public bool UpdateFavouriteinBookTable(int SelectedAsFavorite)
        {
            bool blnSuccess = false;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookIds", SelectedAsFavorite);
            // paramList.Add(CommonConstants.PARAMUSERNAME, strUserID);
            ExecuteAndFetch("SP_eBooks_UpdateFavouriteBookForUser", paramList);
            blnSuccess = true;
            return blnSuccess;
        }


        public bool UpdateFavouriteBookToFalse(string strUsername)
        {
            bool blnSuccess = false;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.usernamePARAM, strUsername);
            // paramList.Add(CommonConstants.PARAMUSERNAME, strUserID);
            ExecuteAndFetch("SP_eBooks_UnCheckFav", paramList);
            blnSuccess = true;
            return blnSuccess;
        }
        public DataTable GetBookDetailsForEdit(string strSelectedID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@SelectedID", strSelectedID);
            //paramList.Add(CommonConstants.PARAMUSERNAME, strProc);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetChartLevelProperties(string chartComponent, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChartComponent ", chartComponent);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetChartSeriesLevelProperties(string chartComponent, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChartComponent ", chartComponent);
            //paramList.Add(CommonConstants.PARAMUSERNAME, strProc);
            return ExecuteAndFetch(strProc, paramList);
        }

        public void UpdatePageSeq(int intSelectedID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", intSelectedID);
            //paramList.Add(CommonConstants.PARAMUSERNAME, strProc);
            ExecuteAndFetch(strProc, paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storProc"></param>
        /// <param name="bookId"></param>
        /// <param name="disciplines"></param>
        /// <param name="pageNames"></param>
        /// <returns></returns>
        public DataSet GetBookFilterValues(string storProc, int bookId, string disciplines, string pageNames, string chapterTitles, string strUserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMINTBOOKID, bookId);
            paramList.Add(EBookConstants.PARAMCHVSELECTEDDISCIPLINE, disciplines);
            paramList.Add(EBookConstants.PARAMCHVSELECTEDPAGENAME, pageNames);
            paramList.Add(EBookConstants.PARAMCHVSELECTEDCHAPTERTITLE, chapterTitles);
            paramList.Add(EBookConstants.PARAMUSERID, strUserName);
            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataSet GetTreeViewData(int bookId, string storProc, string UserName, string strPostBackControlId, string strSelectedView)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            Dictionary<string, object> paramListview = new Dictionary<string, object>();
            Dictionary<string, object> dctSavedFilterParameters = new Dictionary<string, object>();
            paramListview.Add(EBookConstants.PARAMINTBOOKID, bookId);
            paramListview.Add(CommonConstants.PARAMUSERNAME, UserName);
            paramListview.Add(EBookConstants.PARM_VIEW_NAME, strSelectedView.Trim());
            DataTable dtSavedView = null;

            dtSavedView = ExecuteAndFetch(EBookConstants.USPEBOOKSGETSAVEDVIEWS, paramListview);

            if ((dtSavedView.Rows.Count > 0) && (string.IsNullOrEmpty(strPostBackControlId) || !EBookConstants.BUTTONS.Contains(CommonConstants.PIPESYMBOL + strPostBackControlId)))
            {
                string strDiscipline = dtSavedView.Columns[0].ColumnName;
                string strPageName = dtSavedView.Columns[1].ColumnName;
                string strChpaterTitle = dtSavedView.Columns[2].ColumnName;
                dctSavedFilterParameters.Add(EBookConstants.PARAMINTBOOKID, bookId);
                dctSavedFilterParameters.Add(strDiscipline, dtSavedView.Rows[0][strDiscipline].ToString());
                dctSavedFilterParameters.Add(strPageName, dtSavedView.Rows[0][strPageName].ToString());
                dctSavedFilterParameters.Add(strChpaterTitle, dtSavedView.Rows[0][strChpaterTitle].ToString());
                return ExecuteAndFetchAsDataSet(storProc, dctSavedFilterParameters);
            }
            else
            {
                #region 5.1 implementation
                paramList.Add(EBookConstants.PARAMINTBOOKID, bookId);
                if (HttpContext.Current.Session["SqlFilterParameters"] != null)
                {
                    Dictionary<string, string> dctFilterParameters = (Dictionary<string, string>)HttpContext.Current.Session["SqlFilterParameters"];
                    foreach (KeyValuePair<string, string> dict in dctFilterParameters)
                        paramList.Add(dict.Key, dict.Value);
                }

                return ExecuteAndFetchAsDataSet(storProc, paramList);
            }

                #endregion 5.1 implementation

            //paramList.Add("@chvSelectedChapterTitle", "matt_test 2");
            //paramList.Add("@chvSelectedPageName", "001_Test_Delete");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataSet GetAWRTreeViewData(int bookId, string storProc, string UserName, string strPostBackControlId, string strSelectedView)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);

            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }

        public DataSet GetAWRFreezeData(int bookId, string storProc, string UserName, string strPostBackControlId, string strSelectedView)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intAWRId", bookId);

            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetChapterPreferenceXML(string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            return ExecuteAndFetch(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wellBookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetWellBookDetails(int wellBookId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", wellBookId);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public DataTable GetPageDetails(int pageId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageId", pageId);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable CheckIsPageOwnerForTheBook(int bookId, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetPagesInTheBook(int bookId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);
            return ExecuteAndFetch(storProc, paramList);
        }
        public DataTable GetComponentsInTheBook(int bookId, string storProc)
        {
            //Dictionary<string, object> paramList = new Dictionary<string, object>();
            //paramList.Add("@intBookId", bookId);
            ////paramList.Add("@chvComponentIndices", dtcomponentindex);
            //return ExecuteAndFetch(storProc, paramList);


            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);
            return ExecuteAndFetch(storProc, paramList);

            //using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[EBookConstants.LOCALSQLSERVER].ConnectionString))
            //{
            //    connection.Open();
            //    SqlCommand command = new SqlCommand(storProc, connection);
            //    command.CommandType = CommandType.StoredProcedure;
            //    command.Parameters.AddWithValue("@intBookId", bookId);
            //    command.CommandTimeout = 0;
            //    DataTable dt = new DataTable();
            //    dt.Load(command.ExecuteReader());
            //    connection.Close();
            //    return dt;
            //}
        }

        public DataTable GetEmptyComponentsInTheBook(int bookId, string storProc)
        {
            //Dictionary<string, object> paramList = new Dictionary<string, object>();
            //paramList.Add("@intBookId", bookId);
            //paramList.Add("@chvComponentIndices", dtcomponentindex);
            //return ExecuteAndFetch(storProc, paramList);

            //return ExecuteAndFetch(storProc, paramList);


            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);
            return ExecuteAndFetch(storProc, paramList);


            //using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[EBookConstants.LOCALSQLSERVER].ConnectionString))
            //{
            //    connection.Open();
            //    SqlCommand command = new SqlCommand(storProc, connection);
            //    command.CommandType = CommandType.StoredProcedure;
            //    command.Parameters.AddWithValue("@intBookId", bookId);
            //    command.CommandTimeout = 0;
            //    DataTable dt = new DataTable();
            //    dt.Load(command.ExecuteReader());
            //    connection.Close();
            //    return dt;
            //}
        }
        public DataTable GetBookName(int bookId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, bookId);
            return ExecuteAndFetch(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="ChapterPreferenceXML"></param>
        /// <param name="storProc"></param>
        public void UpdateChapterPreferenceXML(string userName, string ChapterPreferenceXML, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add("@xmlChapterPreference", ChapterPreferenceXML);
            ExecuteAndFetch(storProc, paramList);
        }
        #region Release 6.0
        //public DataTable UpdateStoryBoardData(string value, string Source, string ApplicationPage, string ApplicationTemplate)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@PageID", value);
        //    paramList.Add("@Source", Source);
        //    paramList.Add("@ApplicationPage", ApplicationPage);
        //    paramList.Add("@ApplicationTemplate", ApplicationTemplate);
        //    return ExecuteAndFetch("SP_eBooks_UpdateDWBStoryBoardTable", paramList);
        //}

        //public DataTable GetNarrativeInfo(string value)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@PageID", value);
        //    return ExecuteAndFetch("SP_eBooks_GetNarrativeInfo", paramList);
        //}
        //public DataTable UpdateNarrativeTable(string value, string Narrative, int ID)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@PageID", value);
        //    paramList.Add("@Narrative", Narrative);
        //    paramList.Add(CommonConstants.PARAMID, ID);
        //    return ExecuteAndFetch("SP_eBooks_UpdateNarrativeTable", paramList);
        //}
        #endregion Release 6.0
        public DataTable GetUserDesicipline(string strUserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strUserName", strUserName);
            return ExecuteAndFetch("SP_eBooks_GetWindowsUserID", paramList);

        }
        #region Release 6.0
        //public DataTable InsertToDWBComment(string value, string UserName, string Comment, string Discipline, bool Shared, int ID)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();

        //    paramList.Add("@PageID", value);
        //    paramList.Add("@Comment", Comment);
        //    paramList.Add(CommonConstants.PARAMDISCIPLINE, Discipline);
        //    paramList.Add(SEDConstants.PARAUSERNAME, UserName);
        //    paramList.Add("@Shared", Shared);
        //    paramList.Add(CommonConstants.PARAMID, ID);
        //    return ExecuteAndFetch("SP_eBooks_InsertToDWBComment", paramList);

        //}

        //public DataTable BindDataFromDWBComment(string strPageID, string discipline, string userName)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();

        //    paramList.Add("@strPageID", strPageID);
        //    paramList.Add("@discipline", discipline);
        //    paramList.Add("@userName", userName);
        //    return ExecuteAndFetch("SP_eBooks_BindDataFromDWBComments", paramList);

        //}
        #endregion Release 6.0

        public DataTable GetSelectiveSearchResultHeader(int chapterId, int pageId)
        {
            DataTable dtSSRHeader = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Chapter_Id", chapterId);
            paramList.Add("@Page_Id", pageId);
            dtSSRHeader = ExecuteAndFetch(EBookConstants.SPGETSELECTIVESEARCHHEADER, paramList);
            return dtSSRHeader;
        }

        public DataTable GetChaptersByEBookId(int eBookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@BookId", eBookId);
            return ExecuteAndFetch(EBookConstants.SPGETCHAPTERSBYEBOOK, paramList);
        }

        public Int64 CreateSSR(string strUserName, int isSingleBook)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Created_By", strUserName);
            paramList.Add("@IsSingleBook", isSingleBook);
            return (Int64)ExecuteScalar(EBookConstants.SPCREATESSRRECORD, paramList);
        }

        public int SaveSSRTreeStructure(Int64 ssrID, DataTable ssrTreeDataSource)
        {
            int ssrTreeDataRows = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters("@SSR_ID", ssrID);
                dbManager.AddParameters("@SSR_TreeData", ssrTreeDataSource);
                ssrTreeDataRows = dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.SPSAVESSRTREE);
            }
            return ssrTreeDataRows;
        }

        public int SaveSSRBookData(Int64 ssrID, DataTable ssrBookData)
        {
            int ssrBookDataRows = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters("@SSR_ID", ssrID);
                dbManager.AddParameters("@SSR_BookData", ssrBookData);
                ssrBookDataRows = dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.SPSAVESSRBOOKDATA);
            }
            return ssrBookDataRows;
        }
       
        public void SaveStoredSearch( string searchName, string url, string bookName , string userName)
        {
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters("@SearchName", searchName);
                dbManager.AddParameters("@Url", url);
                dbManager.AddParameters("@BookName", bookName);
                dbManager.AddParameters("@UserName", userName);

                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.SPSAVESTOREDSEARCH);
            }
        }
        public DataTable GetSSRTreeStructure(Int64 ssrId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@SSR_ID", ssrId);
            return ExecuteAndFetch(EBookConstants.SPGETSSRTREE, paramList);
        }

        public DataTable GetStoredSearch(string username)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@UserName", username);
            return ExecuteAndFetch(EBookConstants.SPGETSTOREDSEARCH, paramList);
        }

        public DataTable GetSharedSearch(string username)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@UserName", username);
            return ExecuteAndFetch(EBookConstants.SPGETSHAREDSEARCH, paramList);
        }

        public DataTable GetSSRData(Int64 ssrId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@SSR_ID", ssrId);
            return ExecuteAndFetch(EBookConstants.SPDELETESTOREDSEARCH, paramList);
        }

        public void DeleteStoredSearch(int id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@SSR_ID", id);
            ExecuteAndFetch(EBookConstants.SPDELETESTOREDSEARCH, paramList);
        }

        public void ShareLink(string ShareTo, int SearchId, string SharedBy)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@SharedTo", ShareTo);
            paramList.Add("@UserName", SharedBy);
            paramList.Add("@SearchId", SearchId);
            ExecuteAndFetch(EBookConstants.SPSAVESHAREDSEARCH, paramList);
        }

        public DataTable GetSSRMailData(string mailTemplateId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Subject", mailTemplateId);
            return ExecuteAndFetch(EBookConstants.SPCOMMONGETEMAILTEMPLATE, paramList);
        }

        public DataTable GetDataFromDWBChapter(string selectedID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedID", selectedID);
            return ExecuteAndFetch("SP_eBooks_GetChapterDetails", paramList);

        }
        public DataTable GetDataFromDWBTEmplate(string selectedID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedID", selectedID);
            return ExecuteAndFetch("SP_eBooks_GetTemplate", paramList);

        }

        /// <summary>
        /// Gets the type of the file.
        /// </summary>
        /// <returns></returns>
        public DataTable GetFileType()
        {
            return ExecuteAndFetch("SP_eBooks_DWBFileType");
        }

        /// <summary>
        /// Uploads the file to database.
        /// </summary>
        /// <param name="paramList">The param list.</param>
        /// <returns></returns>
        public bool UploadFileToDatabase(Dictionary<string, object> paramList)
        {
            bool blnUpdateSuccess = false;
            //ExecutesAndFetchs("SP_eBooks_DWBUpdateUserDefinedDocument", paramList);
            ExecuteAndFetch(EBookConstants.SP_UPDATE_USER_DEINED_DOCUMENTS, paramList);
            blnUpdateSuccess = true;
            return blnUpdateSuccess;
        }

        /// <summary>
        /// Uploads the fileto local file server.
        /// </summary>
        /// <param name="strPageID">The STR page ID.</param>
        /// <param name="fileUploader">The file uploader.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="strLocalStoragePath">The STR local storage path.</param>

        public void UploadFiletoLocalFileServer(string strPageID, System.Web.UI.HtmlControls.HtmlInputFile fileUploader, string fileName, string strLocalStoragePath)
        {
            string[] filePaths = Directory.GetFiles(strLocalStoragePath);
            foreach (string strFilepath in filePaths)
            {
                if (strFilepath.Contains("\\" + strPageID + "-"))
                {
                    System.IO.File.Delete(strFilepath);
                    break;
                }
            }
            strLocalStoragePath = strLocalStoragePath + strPageID + "-" + fileName;
            fileUploader.PostedFile.SaveAs(strLocalStoragePath);
        }

        /// <summary>
        /// Updates the type3 document status.
        /// </summary>
        /// <param name="intPageID">The int page ID.</param>
        /// <param name="strStorageOption">The STR storage option.</param>
        public void UpdateType3DocumentStatus(int intPageID, string strStorageOption)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", intPageID);
            paramList.Add("@Save_Location", strStorageOption);
            ExecuteAndFetch("SP_eBooks_DWBUpdateSaveLocation", paramList);
        }

        /// <summary>
        /// Gets the save location.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable getSaveLocation(int PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_getSaveLocation", paramList);
        }

        /// <summary>
        /// Gets the uploaded document URL.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable GetUploadedDocumentUrl(string PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_GetUserDefinedDocuments", paramList);
        }


        /// <summary>
        /// Gets the WSDPDF data.
        /// </summary>
        /// <param name="PageId">The page id.</param>
        /// <returns></returns>
        public DataTable GetWSDPDFData(string PageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Page_ID", PageId);
            return ExecuteAndFetch("SP_GetWSDPDFDocuments", paramList);
        }


        /// <summary>
        /// Gets the WSDPDF bytes.
        /// </summary>
        /// <param name="PageId">The page id.</param>
        /// <returns></returns>
        public DataTable GetWSDPDFBytes(string PageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Page_ID", PageId);
            return ExecuteAndFetch("SP_GetWSDPDFBytes", paramList);
        }

        /// <summary>
        /// Gets the uploaded chart level properties.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable GetChartProperties(string value, string listname)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", value);
            return ExecuteAndFetch(listname, paramList);
        }

        /// <summary>
        /// Gets the uploaded eWbComponent  properties.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable GetEwbComponentData(string value, string listname)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", value);
            return ExecuteAndFetch(listname, paramList);
        }


        /// <summary>
        /// Gets the uploaded eWbComponent  properties.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        //public DataTable GetAllUnitConversion(string listname, string value)
        //{
        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@value", value);
        //    return ExecuteAndFetch(listname, paramList);
        //}

        /// <summary>
        /// Gets the uploaded DefaultPreferences result.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        //public DataTable GetallDefaultPreferences( string listname)
        //{

        //    return ExecuteAndFetch(listname);
        //}



        /// <summary>
        /// Gets the uploaded eWbComponent  properties.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable ReturnZoomDate(string value, string listname)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", value);
            return ExecuteAndFetch(listname, paramList);
        }
        /// <summary>
        /// Gets the uploaded document URL. 
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable GetUploadedReportPageLibraryUrl(string PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_GetReportPageLibraryUrl", paramList);
        }


        public DataTable GetWellStatusDiagramByte(string PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_GetWellStatusDiagramByte", paramList);
        }
        /// <summary>
        /// Clears the content of the type3 page.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable ClearType3PageContent(int PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_DeleteUserDefinedDocument", paramList);
        }

        /// <summary>
        /// Updates the list item noto yes.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        /// <returns></returns>
        public DataTable UpdateListItemNotoYes(int PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            return ExecuteAndFetch("SP_eBooks_UpdateListItemNotoYes", paramList);
        }
        public DataTable GetTemplateName(string strSelectedID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", strSelectedID);
            //paramList.Add(CommonConstants.PARAMUSERNAME, strProc);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable BindDropdown(string ID, string Book_ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, ID);
            paramList.Add("@Book_ID", Book_ID);
            return ExecuteAndFetch("SP_eBooks_BindingDataToDropdown", paramList);
        }

        public DataTable BindDropdownFromBook(string Book_ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Book_ID", Book_ID);
            return ExecuteAndFetch("Sp_eBooks_BindDataFromBooks", paramList);

        }
        public DataTable GetChapterPagesDetails(string chvChapterIDs)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvChapterIDs", chvChapterIDs);
            return ExecuteAndFetch("SP_eBooks_GetChapterPagesDetails", paramList);
        }
        public DataTable SplitValueToBind(string chvChapterIDs, string strMode)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvChapterIDs", chvChapterIDs);
            paramList.Add("@chvIsMode", strMode);
            return ExecuteAndFetch("SP_eBooks_SplitValueToBind", paramList);
        }
        public DataTable SplitValueForPageId(string PageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageId", PageId);
            return ExecuteAndFetch("SP_eBooks_SplitValueForPageId", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strImageURLStarting"></param>
        /// <param name="strPageName"></param>
        /// <returns></returns>
        public DataTable GetInternalURL(string entryPoint)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvEntryPoint", entryPoint);
            return ExecuteAndFetch("SP_eBooks_GetModulesPath", paramList);
        }
        public DataTable GetPrintDetails(string chvuserID, string PageOwner, string PrintMyPages, string includeFilter, string Discipline, string Pagename, string PageType, string ConnectionType, string SignOff, string Empty, string ID)
        {
            try
            {
                Dictionary<string, object> paramList = new Dictionary<string, object>();
                string blnEmpty = string.Empty;
                string blnSignOff = string.Empty;
                if (string.Equals(Empty, EBookConstants.STATUS_TERMINATED))
                    blnEmpty = CommonConstants.TRUE;
                if (string.Equals(Empty, EBookConstants.STATUS_ACTIVE))
                    blnEmpty = CommonConstants.FALSE;
                if (string.Equals(SignOff, EBookConstants.STATUS_TERMINATED))
                    blnSignOff = CommonConstants.TRUE;
                if (string.Equals(SignOff, EBookConstants.STATUS_ACTIVE))
                    blnSignOff = CommonConstants.FALSE;
                paramList.Add("@chvuserID", chvuserID);
                paramList.Add("@ChapterID", ID);
                paramList.Add("@PageOwner", PageOwner);
                paramList.Add("@PrintMyPages", PrintMyPages);
                paramList.Add("@includeFilter", includeFilter);
                paramList.Add(CommonConstants.PARAMDISCIPLINE, Discipline);
                paramList.Add(CommonConstants.PARAMPAGENAME, Pagename);
                paramList.Add("@PageType", PageType);
                paramList.Add("@ConnectionType", ConnectionType);
                if (!string.Equals(SignOff.ToLowerInvariant(), "both"))
                    paramList.Add("@SignOff", blnSignOff);
                else
                    paramList.Add("@SignOff", "both");
                paramList.Add("@Empty ", blnEmpty);
                return ExecuteAndFetch("SP_eBooks_PrintPageDetails", paramList);
            }
            catch
            {
                throw;
            }

        }

        public DataTable GetStatusOfChapter(int chapterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterId", chapterId);
            return ExecuteAndFetch("SP_eBooks_GetDetailsFromTable", paramList);

        }
        public DataTable GetIdOfChapter(int chapterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterId", chapterId);
            return ExecuteAndFetch("SP_eBooks_GetIDFromChapterTable", paramList);
        }


        public DataTable GetChapterPrintDetails(string results)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@results", results);
            return ExecuteAndFetch("SP_eBooks_GetChapterPrintDetails", paramList);

        }


        /// <summary>
        /// Gets the data for BO users.
        /// </summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns></returns>
        public DataTable GetDataForBOUsers(string username, bool blnTerminateStatus, string strApp, UserPrivilege Privilege)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TerminateStatus", blnTerminateStatus);
            paramList.Add("@App", strApp);
            if (Privilege == UserPrivilege.Normal || Privilege == UserPrivilege.FirstLineAdmin)
            {
                paramList.Add("@userId", AWR_Utility.UserName);
                return ExecuteAndFetch("SP_eBooks_GetDetailsForBookOwner_Username", paramList);
            }
            else
            {
                paramList.Add("@userId", username);
                return ExecuteAndFetch("SP_eBooks_GetDetailsForBookOwner", paramList);
            }
        }

        /// <summary>
        /// Gets the data for BO users.
        /// </summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns></returns>
        public DataTable GetFavDataForBOUsers(string strFav, string username, bool blnTerminateStatus, string strApp)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@userId", username);
            paramList.Add("@TerminateStatus", blnTerminateStatus);
            paramList.Add(EBookConstants.PARM_BOOK_ID, strFav);
            paramList.Add("@App", strApp);
            return ExecuteAndFetch("SP_eBooks_GetFavBookDetailsForBookOwner", paramList);
        }





        /// <summary>
        /// Gets the data for BO users.
        /// </summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns></returns>
        public DataTable GetDataForPageOwner(string username, bool blnTerminateStatus, string app)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@usrname", username);
            paramList.Add("@terminateStatus", blnTerminateStatus);
            paramList.Add("@app", app);
            return ExecuteAndFetch("SP_eBooks_GetBookDetailsForPageOwner", paramList);
        }

        public DataSet GetRecordAuditDetailsForBook(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForBooks", paramList);
            return dtAudit;
        }

        public DataSet GetRecordAuditDetailsforAdmin(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETDetailsForAdminAuditTrail", paramList);
            return dtAudit;
        }

        public DataTable UpdateBookPrintDetails(XmlDocument xml, string userName, string requestID, string documentURL, bool IsPublish, string liveBookName, string PrintStatus, string eMailID, string BookId, string ModulePath, string printRequest)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            try
            {
                paramList.Add("@xml", xml.InnerXml);
                paramList.Add("@userName", userName);
                paramList.Add(EBookConstants.REQUESTIDPARAM, requestID);
                paramList.Add("@documentURL", documentURL);
                paramList.Add("@IsPublish", IsPublish);
                paramList.Add("@liveBookName", liveBookName);
                paramList.Add("@PrintStatus", PrintStatus);
                paramList.Add("@eMailID", eMailID);
                paramList.Add(EBookConstants.PARM_BOOK_ID, BookId);
                paramList.Add("@ModulePath", ModulePath);
                paramList.Add("@PrintRequest", printRequest);
            }
            catch
            {
                throw;
            }
            return ExecuteAndFetch("SP_eBooks_UpdateBookPrintDetails", paramList);

        }
        public DataTable UpdateBookdetailsLibraryAfterPrint(string userName, string requestID, string PrintStatus)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add("@userName", userName);
            paramList.Add(EBookConstants.REQUESTIDPARAM, requestID);
            paramList.Add("@PrintStatus", PrintStatus);
            return ExecuteAndFetch("SP_eBooks_UpdateBookdetailsLibraryAfterPrint", paramList);

        }

        public DataTable GetPrintedDocumentUrl(int PrintedId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PrintedId", PrintedId);
            return ExecuteAndFetch("SP_eBooks_GetPrintedDocumentUrl", paramList);
        }

        public DataTable GetUserEmailId(string strUserID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(DreamConstants.STRUSERIDPARAM, strUserID);
            return ExecuteAndFetch("SP_eBooks_GetUserEmailID", paramList);
        }


        public DataTable GetBookIdFromTable(string BookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, BookId);
            return ExecuteAndFetch("SP_eBooks_GetBookIdFromTable", paramList);

        }
        public DataTable GetbookIdFromChapter(string bookid, string terminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            bool blnTerminated = false;
            if (string.Equals(terminated, EBookConstants.STATUS_TERMINATED))
                blnTerminated = true;
            if (string.Equals(terminated, EBookConstants.STATUS_ACTIVE))
                blnTerminated = false;
            paramList.Add(EBookConstants.PARM_BOOK_ID, bookid);
            paramList.Add("@terminated", blnTerminated);
            return ExecuteAndFetch("SP_eBooks_GetBookIdFromChapterTable", paramList);

        }

        public DataTable GetValueForChapter(string ChapterId, string terminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            bool blnTerminated = false;
            if (string.Equals(terminated, EBookConstants.STATUS_TERMINATED))
                blnTerminated = true;
            if (string.Equals(terminated, EBookConstants.STATUS_ACTIVE))
                blnTerminated = false;

            paramList.Add("@ChapterId", ChapterId);
            paramList.Add("@terminated", blnTerminated);
            return ExecuteAndFetch("SP_eBooks_ValueForChapterId", paramList);

        }


        public DataTable GetValueFromChapterMapping(string chapterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chapterId", chapterId);
            return ExecuteAndFetch("SP_eBooks_GetValueFromChapterMapping", paramList);
        }

        public DataTable TerminateLiveBook(int rowid)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@rowid", rowid);
            return ExecuteAndFetch("SP_eBooks_TerminateLiveBook", paramList);

        }




        public DataTable GetChapterIDs(string strBookId)
        {
            DataTable dtChapterID = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Book_ID", strBookId);
            dtChapterID = ExecuteAndFetch("SP_eBooks_GetChapterId", paramList);

            return dtChapterID;
        }

        public DataSet GetBookPagesRecords(string strChapterId, bool blnTerminated)
        {
            DataSet dsBookPages = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strChapterId);
            paramList.Add("@blTerminated", blnTerminated);
            dsBookPages = ExecuteAndFetchAsDataSet("SP_eBooks_GetBookPages", paramList);

            return dsBookPages;
        }

        public DataTable GetOwnerName(string strUserID)
        {
            DataTable dtOwner = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@usrId", strUserID);
            dtOwner = ExecuteAndFetch("SP_eBooks_GetOwnerName", paramList);

            return dtOwner;
        }

        public DataTable GetChapterPageMapping(string strSelectedID)
        {
            DataTable dtChapterMappingDetails = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedID", strSelectedID);
            dtChapterMappingDetails = ExecuteAndFetch("SP_eBooks_GetChapterPageMappingDetails", paramList);

            return dtChapterMappingDetails;
        }

        public DataTable GetBookPageMapping(string strSelectedID)
        {
            DataTable dtChapterMappingDetails = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageId", Convert.ToInt32(strSelectedID));
            dtChapterMappingDetails = ExecuteAndFetch("UspeBookGetBookLevelPageDetails", paramList);

            return dtChapterMappingDetails;
        }

        public DataTable GetChapterDetail(string strChapterID)
        {

            DataTable dtChapterMappingDetails = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedID", strChapterID);
            dtChapterMappingDetails = ExecuteAndFetch("SP_eBooks_GetChapterDetailsForMasterPage", paramList);

            return dtChapterMappingDetails;

        }


        public DataTable GetSelectedBookPageBookType(string strSelectedValue)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@BookPageID", strSelectedValue);

            return ExecuteAndFetch("SP_eBooks_GetBookPageBookType", paramList);
        }


        /// <summary>
        /// Updates the Pages details in a Well Book.
        /// </summary>
        /// <param name="siteURL">The site URL.</param>
        /// <param name="listEntry">The list entry.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="auditListName">Audit List Name.</param>
        /// <param name="userName">User Name.</param>
        /// <param name="actionPerformed">Audit Action.</param>
        /// <exception cref="">Handled in calling method.</exception>

        public void UpdateBookPage(ListEntry listEntry, string userName, string actionPerformed, string PageCategory)
        {
            DataTable dtGetTable = null;
            int intPageSequence = 0;
            DataView dvResultView;
            int ID = 0;
            int intChapterId = 0;
            int intBookId = 0;
            try
            {
                if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                {
                    ID = listEntry.MasterPage.RowId;
                }
                string Page_Name = listEntry.MasterPage.Name;
                string Page_Actual_Name = listEntry.MasterPage.TemplateTitle;
                int Page_Sequence = listEntry.MasterPage.PageSequence;
                string Standard_Operating_Procedure = listEntry.MasterPage.SOP;
                string Discipline = listEntry.MasterPage.SignOffDiscipline;
                string ToolTip = listEntry.MasterPage.ToolTip;
                string WSD_Parameters = listEntry.MasterPage.WSDAttributes;  // issue 177263
                WRFMCommon objCommon = WRFMCommon.Instance;
                using (DBManager dbManager = objCommon.DataManager)
                {
                    dbManager.Open();
                    dbManager.AddParameters("@Id", ID);
                    dbManager.AddParameters("@Page_Name", Page_Name);
                    dbManager.AddParameters("@Page_Actual_Name", Page_Actual_Name);
                    dbManager.AddParameters("@Standard_Operating_Procedure", Standard_Operating_Procedure);
                    dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Discipline);
                    dbManager.AddParameters("@ToolTip", ToolTip);
                    dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);  // issue 177263
                    dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                    if (!PageCategory.ToLowerInvariant().Equals(EBookConstants.BOOKLEVELPAGES.ToLowerInvariant()))
                    {
                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_UpdateChapterMappingPages");
                        if (!string.IsNullOrEmpty(dbManager.Parameters[7].Value.ToString()))      // issue 177263
                            intChapterId = Int32.Parse(dbManager.Parameters[7].Value.ToString());   // issue 177263

                        /// start

                        dtGetTable = GetPageSequenceForChapters(intChapterId);

                        var results = from myRow in dtGetTable.AsEnumerable()
                                      where myRow.Field<int>("Id") == ID
                                      select myRow;

                        if (dtGetTable != null && results.AsDataView().Count <= 0)
                        {
                            DataColumn dc = new DataColumn("IDAuto", typeof(int));
                            dtGetTable.Columns.Add(dc);
                            //dtGetTable.Rows[0]["IDAuto"] = 1;
                            //dtGetTable.Rows[0]["Page_Sequence"] = 10;
                            for (int i = 0; i < dtGetTable.Rows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    intPageSequence = int.Parse(dtGetTable.Rows[i]["Page_Sequence"].ToString());
                                    dtGetTable.Rows[i]["Page_Sequence"] = intPageSequence + 10;
                                    dtGetTable.Rows[i]["IDAuto"] = i + 1;
                                }
                                else
                                {
                                    intPageSequence = int.Parse(dtGetTable.Rows[i]["Page_Sequence"].ToString());
                                    dtGetTable.Rows[i]["Page_Sequence"] = intPageSequence + 10;
                                    dtGetTable.Rows[i]["IDAuto"] = i + 1;
                                }
                            }
                            dvResultView = new DataView(dtGetTable);
                            string[] paramas = { CommonConstants.ID, "Page_Sequence", "IDAuto" };
                            DataTable dt = dvResultView.ToTable("dt", true, paramas);
                            UpdatePageSequenceForArchive(dt, "SP_eBooks_updatePageSequenceForChapters");
                        }

                        /// end
                        //web.AllowUnsafeUpdates = false;
                        //objCommonDAL = new CommonDAL();
                        //string userName, int intRowID, string title, string actionPerformed, int ID, string pageType
                        UpdateListAuditHistory(userName, listEntry.MasterPage.RowId, "", actionPerformed, 0, EBookConstants.CHAPTERPAGEMAPPINGREPORT);
                        ///// Update the values to StoryBoard list
                        //WellBookChapterDAL objWellBookChapterDAL = new WellBookChapterDAL();
                        StoryBoard objStoryBoard = new StoryBoard();
                        DataTable dtListItem = GetChapterDeatail(listEntry.MasterPage.RowId);
                        //objListItem = list.GetItemById(listEntry.MasterPage.RowId);

                        if (dtListItem != null)
                        {
                            objStoryBoard.PageId = Int32.Parse(Convert.ToString(dtListItem.Rows[0][CommonConstants.ID]));
                            objStoryBoard.SOP = Convert.ToString(dtListItem.Rows[0][EBookConstants.STANDARDOPERATINGPROCEDURE]);
                            objStoryBoard.PageTitle = Convert.ToString(dtListItem.Rows[0][EBookConstants.PAGEACTUALNAME]);
                            objStoryBoard.Discipline = Convert.ToString(dtListItem.Rows[0][EBookConstants.DISCIPLINE]);
                            objStoryBoard.MasterPageName = Convert.ToString(dtListItem.Rows[0][EBookConstants.PAGENAMECOLUMN]);

                        }
                        // UpdateStoryBoard(objStoryBoard);
                        #region Release 6.0
                        // UpdateStoryBoardInEdit(objStoryBoard);
                        //if (string.Compare(actionPerformed, EBookConstants.AUDITACTIONSTORYBOARDUPDATED, true) == 0)
                        //{
                        //    UpdateListAuditHistory(objStoryBoard.PageId.ToString(), userName, DateTime.Now, 11);
                        //}
                        #endregion Release 6.0
                    }
                    else
                    {
                        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "USPeBooksUpdateBookMappingPages");
                        if (!string.IsNullOrEmpty(dbManager.Parameters[7].Value.ToString()))
                            intBookId = Int32.Parse(dbManager.Parameters[7].Value.ToString());
                        UpdateListAuditHistory(userName, listEntry.MasterPage.RowId, "", actionPerformed, 0, EBookConstants.BOOKLEVELPAGES, PageCategory);
                    }
                }
            }

            finally
            {
                if (dtGetTable != null)
                    dtGetTable.Dispose();
            }
        }

        public DataTable GetChapterDeatail(int intRowId)
        {
            DataTable dtPageSeq = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@rowid", intRowId);
            dtPageSeq = ExecuteAndFetch("SP_eBooks_GetChapterDetail", paramList);

            return dtPageSeq;
        }
        #region Release 6.0
        ///// <summary>
        ///// Updates the Story board.
        ///// </summary>
        ///// <param name="parentSiteURL">The site URL.</param>
        ///// <param name="listName">Name of the list.</param>
        ///// <param name="auditListName">Audit List Name.</param>
        ///// <param name="camlQuery">CAML Query.</param>
        ///// <param name="pageID">Page ID.</param>
        ///// <param name="pageStoryBoard">StoryBoard object.</param>
        ///// <param name="actionPerformed">Audit action.</param>
        ///// <param name="userName">User Name.</param>
        ///// <exception cref="">Handled in calling method.</exception>
        //
        //public void UpdateStoryBoard(StoryBoard pageStoryBoard)
        //{

        //    int Page_ID = 0;
        //    string Page_Title = string.Empty;
        //    string Connection_Type = string.Empty;
        //    string Source = string.Empty;
        //    string Discipline = string.Empty;
        //    string Master_Page = string.Empty;
        //    string Application_Template = string.Empty;
        //    string Application_Page = string.Empty;
        //    string SOP = string.Empty;
        //    string Created_By = string.Empty;
        //    string Page_Owner = string.Empty;
        //    string Page_Type = string.Empty;

        //    if (pageStoryBoard.PageId > 0)
        //        Page_ID = pageStoryBoard.PageId;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.PageTitle))
        //        Page_Title = pageStoryBoard.PageTitle;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.ConnectionType))
        //        Connection_Type = pageStoryBoard.ConnectionType;
        //    if (pageStoryBoard.Source != null)
        //        Source = pageStoryBoard.Source;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.Discipline))
        //        Discipline = pageStoryBoard.Discipline;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.MasterPageName))
        //        Master_Page = pageStoryBoard.MasterPageName;
        //    if (pageStoryBoard.ApplicationTemplate != null)
        //        Application_Template = pageStoryBoard.ApplicationTemplate;
        //    if (pageStoryBoard.ApplicationPage != null)
        //        Application_Page = pageStoryBoard.ApplicationPage;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.SOP))
        //        SOP = pageStoryBoard.SOP;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.CreatedBy))
        //        Created_By = pageStoryBoard.CreatedBy;
        //    //if (!string.IsNullOrEmpty(pageStoryBoard.CreationDate))
        //    //    Creation_Date = Convert.ToDateTime(pageStoryBoard.CreationDate).ToString("yyyy-MM-ddTHH:mm:ssZ");
        //    if (!string.IsNullOrEmpty(pageStoryBoard.PageOwner))
        //        Page_Owner = pageStoryBoard.PageOwner;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.PageType))
        //        Page_Type = pageStoryBoard.PageType;
        //    WRFMCommon objCommon = WRFMCommon.Instance;
        //    using (DBManager dbManager = objCommon.DataManager)
        //    {
        //        dbManager.Open();

        //        dbManager.AddParameters("@Page_ID", Page_ID);
        //        dbManager.AddParameters("@Page_Title", Page_Title);
        //        dbManager.AddParameters("@Connection_Type", Connection_Type);
        //        dbManager.AddParameters("@Source", Source);
        //        dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Discipline);
        //        dbManager.AddParameters("@Master_Page", Master_Page);
        //        dbManager.AddParameters("@Application_Template", Application_Template);
        //        dbManager.AddParameters("@Application_Page", Application_Page);
        //        dbManager.AddParameters("@SOP", SOP);
        //        dbManager.AddParameters("@Created_By", Created_By);
        //        dbManager.AddParameters("@Page_Owner", Page_Owner);
        //        dbManager.AddParameters("@Page_Type", Page_Type);

        //        //dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
        //        dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_UpdateStoryBoardData");
        //    }

        //}

        ///// <summary>
        ///// Updates the story board in edit.
        ///// </summary>
        ///// <param name="pageStoryBoard">The page story board.</param>
        //public void UpdateStoryBoardInEdit(StoryBoard pageStoryBoard)
        //{
        //    int Page_ID = 0;
        //    string Page_Title = string.Empty;
        //    string Discipline = string.Empty;
        //    string SOP = string.Empty;
        //    string strMasterPageName = string.Empty;

        //    if (pageStoryBoard.PageId > 0)
        //        Page_ID = pageStoryBoard.PageId;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.PageTitle))
        //        Page_Title = pageStoryBoard.PageTitle;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.Discipline))
        //        Discipline = pageStoryBoard.Discipline;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.SOP))
        //        SOP = pageStoryBoard.SOP;
        //    if (!string.IsNullOrEmpty(pageStoryBoard.MasterPageName))
        //        strMasterPageName = pageStoryBoard.MasterPageName;

        //    Dictionary<string, object> paramList = new Dictionary<string, object>();
        //    paramList.Add("@Page_ID", Page_ID);
        //    paramList.Add("@Page_Title", Page_Title);
        //    paramList.Add(CommonConstants.PARAMDISCIPLINE, Discipline);
        //    paramList.Add("@SOP", SOP);
        //    paramList.Add("@MasterPage", strMasterPageName);
        //    ExecuteAndFetch("SP_eBooks_UpdateStoryBoardDataEditMode", paramList);
        //}
        #endregion Release 6.0

        public DataTable GetPageSequenceForChapters(int rowId)
        {
            DataTable dtPageSeq = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@rowid", rowId);
            dtPageSeq = ExecuteAndFetch("SP_eBooks_GetPageSequenceForChapters", paramList);

            return dtPageSeq;
        }

        public DataSet GetRecordAuditDetailsForBookPages(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForBookPages", paramList);

            return dtAudit;
        }
        public DataSet GetRecordAuditDetailsForBookLevelPages(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForBookLevelPages", paramList);

            return dtAudit;
        }
        public DataSet GetRecordAuditDetailsForChapters(string strAuditId)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet("SP_eBooks_GETAuditDetailsForChapter", paramList);

            return dtAudit;
        }

        #region AddChapter

        /// <summary>
        /// Gets the country active yes.
        /// </summary>
        /// <returns></returns>
        public DataTable GetCountryActiveYes()
        {
            return ExecuteAndFetch(EBookConstants.GetCountryActiveYes);
        }

        /// <summary>
        /// Gets the asset type columns list.
        /// </summary>
        /// <param name="SelectedAssetType">Type of the selected asset.</param>
        /// <returns></returns>
        public DataTable GetAssetTypeColumnsList(string SelectedAssetType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.ASSETTYPE, SelectedAssetType);
            return ExecuteAndFetch(EBookConstants.GETASSETTYPECOLUMNSLIST, paramList);
        }

        public DataTable GetAssetDetails(string Identifier, string AssetType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvIdentifier", Identifier);
            paramList.Add("@chvAssetName", AssetType);
            return ExecuteAndFetch("USP_SED_GetAssetDetails", paramList);
        }

        public DataSet GetIncludedAssets(string strSelectedIdentifier, string strAssetType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AssetIdentifier", strSelectedIdentifier.Replace(CommonConstants.PIPESYMBOL, ""));
            paramList.Add("@AssetType", strAssetType);
            //paramList.Add("@IncludeWell", IncludeWell);
            //paramList.Add("@IncludeWellbore", IncludeWellbore);
            //paramList.Add("@IncludeConduit", IncludeConduit);
            //paramList.Add("@ShowParentWellbore", blShowParentWellbore);
            return ExecuteAndFetchAsDataSet("uspSEDGetAssetDetails", paramList);
        }

        /// <summary>
        /// Gets the type of the DWB asset.
        /// </summary>
        /// <returns></returns>
        public DataTable GetDWBAssetType()
        {
            return ExecuteAndFetch(EBookConstants.GetDWBAssetType);
        }

        /// <summary>
        /// Gets the conditional_ DWB templates.
        /// </summary>
        /// <param name="AssetType">Type of the asset.</param>
        /// <param name="ApplicationName">Name of the application.</param>
        /// <returns></returns>
        public DataTable GetConditional_DWBTemplates(string AssetType, string ApplicationName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Asset_Type", AssetType);
            paramList.Add("@Application_Name", ApplicationName);
            return ExecuteAndFetch(EBookConstants.GetConditional_DWBTemplates, paramList);
        }

        /// <summary>
        /// Gets the conditional well list.
        /// </summary>
        /// <param name="FieldIdentifier">Type of the FieldIdentifier number.</param>
        /// <returns></returns>
        public DataTable GetConditional_WellList(string FieldIdentifier)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Field_Identifier", FieldIdentifier);
            return ExecuteAndFetch(EBookConstants.GetConditional_WellList, paramList);
        }

        /// <summary>
        /// Gets the conditional wellbore list.
        /// </summary>
        /// <param name="FieldIdentifier">Type of the FieldIdentifier number.</param>
        /// <returns></returns>
        public DataTable GetConditional_WellBoreList(string FieldIdentifier)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Field_Identifier", FieldIdentifier);
            return ExecuteAndFetch(EBookConstants.GetConditional_WellBoreList, paramList);
        }
        
        /// <summary>
        /// Gets the conditional wellbore list.
        /// </summary>
        /// <param name="UWI">Type of the UWI number.</param>
        /// <returns></returns>
        public DataTable GetConditional_WellBoreListWithUWI(string UWI)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@UWI", UWI);
            return ExecuteAndFetch(EBookConstants.GetConditional_WellBoreListWithUWI, paramList);
        }




        /// <summary>
        /// Gets the name of the template.
        /// </summary>
        /// <param name="intTemplateId">The int template id.</param>
        /// <returns></returns>
        public string GetTemplateName(int intTemplateId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            string strTemplateName = string.Empty;
            paramList.Add("@TemplateId", intTemplateId);
            DataTable dt = ExecuteAndFetch("eBook_SP_GetTemplateName", paramList);
            if (dt != null && dt.Rows.Count > 0)
                strTemplateName = dt.Rows[0][0].ToString();
            return strTemplateName;
        }



        /// <summary>
        /// Gets the template pages count.
        /// </summary>
        /// <param name="TempId">The temp id.</param>
        /// <returns></returns>
        public int GetTemplatePagesCount(int TempId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateId", TempId);
            DataTable dtResult = ExecuteAndFetch("eBook_GetMasterPagesCount", paramList);
            int intResult = dtResult != null ? Int32.Parse(dtResult.Rows[0][0].ToString()) : -1;
            return (intResult);
        }

        /// <summary>
        /// Checks the duplicate entry for chapter.
        /// </summary>
        /// <param name="ChapterTitle">The chapter title.</param>
        /// <param name="BookID">The book ID.</param>
        /// <returns></returns>
        public DataTable CheckDuplicateEntryForChapter(string ChapterTitle, string BookID, string Mode)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterTitle", ChapterTitle.Replace("'", "''"));
            paramList.Add(EBookConstants.PARM_BOOK_ID, BookID.Replace("'", "''"));
            paramList.Add("@Mode", Mode);
            return ExecuteAndFetch(EBookConstants.CHECKDUPLICATEENTRYFORCHAPTER, paramList);
        }

        public DataTable CheckChapterEntryForTemplate(int intRowID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TEMPLATEID", intRowID);
            return ExecuteAndFetch(EBookConstants.SPGETCHAPTERFORTEMPLATEID, paramList);
        }

        /// <summary>
        /// Gets the SQL tables data.
        /// </summary>
        /// <param name="TableName">Name of the table.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <param name="WhereCondition">The where condition.</param>
        /// <returns></returns>
        public DataTable GetSqlTablesData(string TableName, string ColumnName, string WhereCondition)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.TableNamePARAM, TableName);
            paramList.Add("@Columns", ColumnName);
            paramList.Add(SEDConstants.WHERECONDITION, WhereCondition);
            return ExecuteAndFetch(EBookConstants.GET_TABLE_WITH_OR_WITHOUT_CONDITION, paramList);
        }

        /// <summary>
        /// Saves the chapters and chapter page mapping.
        /// </summary>
        /// <param name="BookId">The book id.</param>
        /// <param name="ChapterTable">The chapter table.</param>
        /// <param name="ChapterPagesMapping">The chapter pages mapping.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="auditAction">The audit action.</param>
        /// <returns></returns>
        public DataTable SaveChaptersAndChapterPageMapping(string BookId, DataTable ChapterTable, DataTable ChapterPagesMapping, string userName, string auditAction)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookId", BookId.Replace("'", "''"));
            paramList.Add("@tblChapter", ChapterTable);
            paramList.Add("@tblChapterPagesMapping", ChapterPagesMapping);
            paramList.Add("@chvUsername", userName.Replace("'", "''"));
            paramList.Add("@intAuditAction", Convert.ToInt32(auditAction));

            return ExecuteAndFetch(EBookConstants.SAVECHAPTERSANDCHAPTERPAGEMAPPING, paramList);
        }

        /// <summary>
        /// Chapters the edit.
        /// </summary>
        /// <param name="ChapterId">The chapter id.</param>
        /// <param name="ChapterTitle">The chapter title.</param>
        /// <param name="ChapterDescription">The chapter description.</param>
        /// <param name="AuditAction">The audit action.</param>
        /// <param name=CommonConstants.USERNAME>Name of the user.</param>
        public void ChapterEdit(int ChapterId, string ChapterTitle, string ChapterDescription, string strDefaultColumnName, int ChapterOrder, int AuditAction, string UserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", ChapterId);
            paramList.Add("@ChapterTitle", ChapterTitle.Replace("'", "''"));
            paramList.Add("@ChapterDescription", ChapterDescription.Replace("'", "''"));
            paramList.Add(DreamConstants.DefaultColumnPARAM, strDefaultColumnName.Replace("'", "''"));
            paramList.Add("@ChapterOrder", ChapterOrder);
            paramList.Add("@AuditAction", AuditAction);
            paramList.Add(SEDConstants.PARAUSERNAME, UserName.Replace("'", "''"));
            ExecuteAndFetch(EBookConstants.ChapterEdit, paramList);
        }
        /// <summary>
        /// Default Chapters Edit.
        /// </summary>
        /// <param name="ChapterId">The chapter id.</param>
        /// <param name=CommonConstants.USERNAME>Default Chapter.</param>
        public void DefaultChapterUpdate(int BookId, bool DefaultChapter)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, BookId);
            paramList.Add("@DefaultChapter", DefaultChapter);
            ExecuteAndFetch(EBookConstants.DefaultChapterEdit, paramList);
        }

        /// <summary>
        /// Loads the controls on chapter edit.
        /// </summary>
        /// <param name="Asset_Type_ID">The asset_ type_ ID.</param>
        /// <param name="Template_ID">The template_ ID.</param>
        /// <returns></returns>
        public DataTable LoadControlsOnChapterEdit(string Template_ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Template_ID", Convert.ToInt32(Template_ID));
            return ExecuteAndFetch(EBookConstants.LOADCONTROLSONCHAPTEREDIT, paramList);
        }
        #endregion

        /// <summary>
        /// Gets the data for BO users.
        /// </summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns></returns>
        public DataTable LoadControlForBOUsers(string username, bool blnTerminateStatus, bool value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@userId", username);
            paramList.Add("@TerminateStatus", blnTerminateStatus);
            paramList.Add("@App", value);
            return ExecuteAndFetch("SP_eBooks_LoadTeam", paramList);
        }

        public DataTable GetChaptersDetails(string strBookId, bool boolTerminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookId);
            paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch("SP_eBooks_GetChaptersForBook", paramList);
        }

        public DataTable GetTemplateForChapter(string strTemplateID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", strTemplateID);
            //  paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch("SP_eBooks_GetTemplateTitle", paramList);
        }

        public DataTable GetChapterDetailsForChapterID(string strChapterID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chapterId", strChapterID);
            //  paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetMasterPageDetailForChapter(string strAppName, string strAssetType, string sortColumn)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AppName", strAppName);
            paramList.Add(SEDConstants.ASSETTYPE, strAssetType);
            paramList.Add("@SortCol", sortColumn);
            //  paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch("SP_eBooks_GetMasterDetailsForChapter", paramList);
        }

        public DataTable GetMasterPageDetailForChapter(string strMasterPageId, string strAppName, string strAssetType, string sortColumn)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterID", strMasterPageId);
            paramList.Add("@AppName", strAppName);
            paramList.Add(SEDConstants.ASSETTYPE, strAssetType);
            paramList.Add("@SortCol", sortColumn);


            return ExecuteAndFetch("SP_eBooks_GetChapterPageDetailsForMasterID", paramList);
        }

        public DataTable GetAppName(string strBookId, string strproc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookId);
            return ExecuteAndFetch(strproc, paramList);
        }
        public DataTable GetAppName(string BookIds, string SplitCharachter, string proc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@BookIDs", BookIds);
            paramList.Add("@SplitCharacter", SplitCharachter);
            return ExecuteAndFetch(proc, paramList);
        }
        public DataTable GetChapter(string strChapterID, string strproc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", strChapterID);
            return ExecuteAndFetch(strproc, paramList);
        }

        public DataTable GetDetails(string strId, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Id", strId);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetMasterPageDetailForChapter(string strApplicationName, string strAssetType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@app", strApplicationName);
            paramList.Add("@Asset_Name", strAssetType);
            return ExecuteAndFetch("SP_eBooks_GetDetailsFromMasterPage", paramList);
        }

        public DataTable GetMasterIDForPageStatus(string masterPageID, string selectedID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", selectedID);
            paramList.Add("@MasterID", masterPageID);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetUserDetail(string strSelectedValue, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Id", strSelectedValue);

            return ExecuteAndFetch(strProc, paramList);
        }
        /// <summary>
        /// Adds the Pages to the Book Directly.
        /// </summary>
        /// <param name="siteURL">The site URL.</param>
        /// <param name="listEntry">The list entry.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="auditListName">Audit List Name.</param>
        /// <param name="userName">User Name.</param>
        /// <param name="actionPerformed">Audit Action.</param>
        /// <returns>ID of the Chapter Page Created.</returns>
        /// <exception cref="">Handled in calling method.</exception>
        public string AddPageToBook(ListEntry listEntry, string username, string actionPerformed, string bookID)
        {
            string strPageID = string.Empty;
            int intRowId = 0;
            DataTable dtlistItem = null;
            string Sign_Off_Status = string.Empty;
            //string Components = string.Empty;
            string ToolTip = string.Empty;
            string WSD_Parameters = string.Empty;
            string Page_URL = string.Empty;
            bool blnEmpty = false;
            string Empty = string.Empty;
            try
            {

                for (int i = 0; i < listEntry.BookPageMapping.Count; i++)
                {

                    int Master_Page_ID = listEntry.BookPageMapping[i].MasterPageID;
                    int Actual_MasterPage_Id = listEntry.BookPageMapping[i].ActualMasterPageId;
                    //string Page_Actual_Name = listEntry.BookPageMapping[i].PageActualName;
                    string Page_Name = listEntry.BookPageMapping[i].PageName;
                    string Owner = listEntry.BookPageMapping[i].PageOwner;
                    if (string.IsNullOrEmpty(Owner))
                    {
                        Owner = username;
                    }
                    string Discipline = listEntry.BookPageMapping[i].Discipline;
                    int Book_ID = Convert.ToInt32(bookID);
                    string Asset_Type = listEntry.BookPageMapping[i].AssetType;
                    if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].Empty))
                    {
                        Empty = listEntry.BookPageMapping[i].Empty;
                        if (Empty == "Yes")
                        {
                            blnEmpty = true;
                        }
                        else
                        {
                            blnEmpty = false;
                        }
                    }
                    int Page_Sequence = listEntry.BookPageMapping[i].PageSequence;
                    if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].SignOffStatus))
                    {
                        Sign_Off_Status = listEntry.BookPageMapping[i].SignOffStatus;
                    }
                    //if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].MasterPageComponents))
                    //{
                    //    Components = listEntry.BookPageMapping[i].MasterPageComponents;
                    //}
                    if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].ToolTip))
                    {
                        ToolTip = listEntry.BookPageMapping[i].ToolTip;
                    }

                    if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].WSD_Parameters))
                    {
                        WSD_Parameters = listEntry.BookPageMapping[i].WSD_Parameters;
                    }
                    string Connection_Type = listEntry.BookPageMapping[i].ConnectionType;
                    string Standard_Operating_Procedure = listEntry.BookPageMapping[i].StandardOperatingProc;
                    if (!string.IsNullOrEmpty(listEntry.BookPageMapping[i].PageURL))
                    {
                        Page_URL = listEntry.BookPageMapping[i].PageURL;
                    }
                    WRFMCommon objCommon = WRFMCommon.Instance;
                    using (DBManager dbManager = objCommon.DataManager)
                    {
                        try
                        {
                            dbManager.Open();
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "INSERT");
                            }
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "UPDATE");
                            }
                            dbManager.AddParameters("@Id", 0);
                            dbManager.AddParameters("@Master_Page_ID ", Master_Page_ID);
                            dbManager.AddParameters("@Actual_MasterPage_Id", Actual_MasterPage_Id);
                            dbManager.AddParameters("@Page_Name", Page_Name);
                            dbManager.AddParameters("@Page_Actual_Name", Page_Name);
                            dbManager.AddParameters("@Owner", Owner);
                            dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Discipline);
                            dbManager.AddParameters("@Book_ID", Book_ID);
                            dbManager.AddParameters("@Asset_Type", Asset_Type);
                            dbManager.AddParameters("@Empty", blnEmpty);
                            dbManager.AddParameters("@Page_Sequence", Page_Sequence);
                            dbManager.AddParameters("@Sign_Off_Status", Sign_Off_Status);

                            //dbManager.AddParameters("@Components", Components);
                            dbManager.AddParameters("@ToolTip", ToolTip);

                            dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);
                            dbManager.AddParameters("@Connection_Type", Connection_Type);
                            dbManager.AddParameters("@Standard_Operating_Procedure", Standard_Operating_Procedure);
                            dbManager.AddParameters("@Page_URL", Page_URL);

                            dbManager.AddParameters("@usrname", username);
                            dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "USPeBooksInsertUpdateBookPage");
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                intRowId = Int32.Parse(dbManager.Parameters[19].Value.ToString());
                            strPageID = Convert.ToString(intRowId);
                            UpdateListAuditHistory(username, intRowId, "", actionPerformed, 0, EBookConstants.BOOKLEVELPAGES);
                        }
                        catch (Exception ex)
                        {
                            WRFMCommon.Instance.Error.Handle(ex);
                        }
                    }

                }

            }
            finally
            {
                if (dtlistItem != null) dtlistItem.Dispose();
            }
            return strPageID;
        }
        /// <summary>
        /// Adds the Pages to the Chapter Directly.
        /// </summary>
        /// <param name="siteURL">The site URL.</param>
        /// <param name="listEntry">The list entry.</param>
        /// <param name="listName">Name of the list.</param>
        /// <param name="auditListName">Audit List Name.</param>
        /// <param name="userName">User Name.</param>
        /// <param name="actionPerformed">Audit Action.</param>
        /// <returns>ID of the Chapter Page Created.</returns>
        /// <exception cref="">Handled in calling method.</exception>
        public string AddPageToChapter(ListEntry listEntry, string username, string actionPerformed)
        {
            string strPageID = string.Empty;
            int intRowId = 0;
            DataTable dtlistItem = null;
            string Sign_Off_Status = string.Empty;
            //string Components = string.Empty;
            string ToolTip = string.Empty;
            string WSD_Parameters = string.Empty;
            string Page_URL = string.Empty;
            bool blnEmpty = false;
            string Empty = string.Empty;
            try
            {

                for (int i = 0; i < listEntry.ChapterPagesMapping.Count; i++)
                {

                    int Master_Page_ID = listEntry.ChapterPagesMapping[i].MasterPageID;
                    int Actual_MasterPage_Id = listEntry.ChapterPagesMapping[i].ActualMasterPageId;
                    //string Page_Actual_Name = listEntry.ChapterPagesMapping[i].PageActualName;
                    string Page_Name = listEntry.ChapterPagesMapping[i].PageName;
                    string Owner = listEntry.ChapterPagesMapping[i].PageOwner;
                    if (string.IsNullOrEmpty(Owner))
                    {
                        Owner = username;
                    }
                    string Discipline = listEntry.ChapterPagesMapping[i].Discipline;
                    int Chapter_ID = listEntry.ChapterDetails.RowID;
                    string Asset_Type = listEntry.ChapterPagesMapping[i].AssetType;
                    if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].Empty))
                    {
                        Empty = listEntry.ChapterPagesMapping[i].Empty;
                        if (Empty == "Yes")
                        {
                            blnEmpty = true;
                        }
                        else
                        {
                            blnEmpty = false;
                        }
                    }
                    int Page_Sequence = listEntry.ChapterPagesMapping[i].PageSequence;
                    if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].SignOffStatus))
                    {
                        Sign_Off_Status = listEntry.ChapterPagesMapping[i].SignOffStatus;
                    }
                    //if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].MasterPageComponents))
                    //{
                    //    Components = listEntry.ChapterPagesMapping[i].MasterPageComponents;
                    //}
                    if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].ToolTip))
                    {
                        ToolTip = listEntry.ChapterPagesMapping[i].ToolTip;
                    }

                    if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].WSD_Parameters))
                    {
                        WSD_Parameters = listEntry.ChapterPagesMapping[i].WSD_Parameters;
                    }
                    string Connection_Type = listEntry.ChapterPagesMapping[i].ConnectionType;
                    string Standard_Operating_Procedure = listEntry.ChapterPagesMapping[i].StandardOperatingProc;
                    if (!string.IsNullOrEmpty(listEntry.ChapterPagesMapping[i].PageURL))
                    {
                        Page_URL = listEntry.ChapterPagesMapping[i].PageURL;
                    }
                    WRFMCommon objCommon = WRFMCommon.Instance;
                    using (DBManager dbManager = objCommon.DataManager)
                    {
                        try
                        {
                            dbManager.Open();
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "INSERT");
                            }
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONUPDATION))
                            {
                                dbManager.AddParameters("@chvOperationType", "UPDATE");
                            }
                            dbManager.AddParameters("@Id", 0);
                            dbManager.AddParameters("@Master_Page_ID ", Master_Page_ID);
                            dbManager.AddParameters("@Actual_MasterPage_Id", Actual_MasterPage_Id);
                            dbManager.AddParameters("@Page_Name", Page_Name);
                            dbManager.AddParameters("@Page_Actual_Name", Page_Name);
                            dbManager.AddParameters("@Owner", Owner);
                            dbManager.AddParameters(CommonConstants.PARAMDISCIPLINE, Discipline);
                            dbManager.AddParameters("@Chapter_ID", Chapter_ID);
                            dbManager.AddParameters("@Asset_Type", Asset_Type);
                            dbManager.AddParameters("@Empty", blnEmpty);
                            dbManager.AddParameters("@Page_Sequence", Page_Sequence);
                            dbManager.AddParameters("@Sign_Off_Status", Sign_Off_Status);

                            //dbManager.AddParameters("@Components", Components);
                            dbManager.AddParameters("@ToolTip", ToolTip);

                            dbManager.AddParameters("@WSD_Parameters", WSD_Parameters);
                            dbManager.AddParameters("@Connection_Type", Connection_Type);
                            dbManager.AddParameters("@Standard_Operating_Procedure", Standard_Operating_Procedure);
                            dbManager.AddParameters("@Page_URL", Page_URL);

                            dbManager.AddParameters("@usrname", username);
                            dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                            dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_InsertUpdateChapterPage");
                            if (string.Equals(actionPerformed, EBookConstants.AUDITACTIONCREATION))
                                intRowId = Int32.Parse(dbManager.Parameters[19].Value.ToString());
                            strPageID = Convert.ToString(intRowId);
                            UpdateListAuditHistory(username, intRowId, "", actionPerformed, 0, EBookConstants.CHAPTERPAGEREPORT);

                        }
                        catch (Exception ex)
                        {
                            WRFMCommon.Instance.Error.Handle(ex);
                        }
                    }

                }

            }
            finally
            {
                if (dtlistItem != null) dtlistItem.Dispose();
            }
            return strPageID;
        }
        public DataTable GetBookPages(string strBookId, bool boolTerminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookId);
            paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch("USPeBooksGetBookPagesForBook", paramList);
        }
        public DataTable GetChapterPages(string strChapterId, bool boolTerminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterId", strChapterId);
            paramList.Add("@TerminateStatus", boolTerminated);

            return ExecuteAndFetch("SP_eBooks_GetChapterPagessForChapter", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BookID"></param>
        /// <param name="FromDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="storProc"></param>
        public void UpdateBookChartZoomDates(string BookID, string FromDate, string EndDate, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookId", BookID);
            paramList.Add("@chvFromDate", FromDate);
            paramList.Add("@chvToDate", EndDate);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="unitSettingsForBook"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        public void SaveUnitPreferenceForBook(string bookId, string unitSettingsForBook, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookId", bookId);
            paramList.Add("@xmlUnitSettings", unitSettingsForBook);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// Gets the type of the file content.
        /// </summary>
        /// <param name="strfileextnName">Name of the strfileextn.</param>
        /// <returns></returns>
        public DataTable GetFileContentType(string strfileextnName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@FileExtension", strfileextnName);
            return ExecuteAndFetch("SP_eBooks_GetFileContentType", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentIndices"></param>
        /// <returns></returns>
        public DataTable GetEWBComponentsBasedOnComponentName(string componentName, string AssetType, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvComponentName", componentName);
            paramList.Add(SEDConstants.ASSETTYPE, AssetType);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="componentIndices"></param>
        /// <returns></returns>
        public DataTable GetEWBComponentsBasedOnComponentIndex(string componentIndices)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvComponentIndices", componentIndices);
            return ExecuteAndFetch("SP_eBooks_GetEWBComponents", paramList);
        }

        /// <summary>USP_eBooks_UploadBatchUpdatePathXMLToTable
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetDefaultPreferences(string entityType, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvTitle", entityType);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public DataTable GetChapterPagesDetails(int pageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@selectedID", pageId);
            return ExecuteAndFetch("SP_eBooks_GetChapterPageMappingDetails", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetBookUnitPreferences(string bookId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookId", bookId);
            return ExecuteAndFetch(storProc, paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable GetDWBURLComponent(int pageId, int intComponentPropertyId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageId", pageId);
            paramList.Add("@intCompPropertyID", intComponentPropertyId);
            return ExecuteAndFetch(storProc, paramList);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="storProc"></param>
        public DataTable GetUnitsBasedOnCriteria(string title, string unitSystemGroup, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvTitle", title);
            paramList.Add("@chvUnitSystemGroup", unitSystemGroup);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetAllUnits(string storeProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetch(storeProc, paramList);
        }

        /// <summary>
        /// Gets the chapters for book.
        /// </summary>
        /// <param name="strBookID">The string book identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public DataTable GetChaptersForBook(string strBookID, bool status)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookID);
            paramList.Add("@TerminateStatus", status);
            return ExecuteAndFetch("SP_eBooks_GetChaptersForBook", paramList);
        }

        /// <summary>
        /// Gets the type iii pages for book.
        /// </summary>
        /// <param name="strChapterId">The string chapter identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public DataSet GetTypeIIIPagesForBook(string strChapterId, bool status)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", strChapterId);
            paramList.Add("@TerminateStatus", status);
            return ExecuteAndFetchAsDataSet("SP_eBooks_GetTypeIIIPagesForBook", paramList);
        }

        /// <summary>
        /// Gets the document naming convention.
        /// </summary>
        /// <returns></returns>
        public DataTable getDocumentNamingConvention()
        {
            return ExecuteAndFetch("SP_eBooks_GetDocumentNamingConvention");
        }

        /// <summary>
        /// Uploads the batch import XML to table.
        /// </summary>
        /// <param name="bookID">The book identifier.</param>
        /// <param name="finalDocument">The final document.</param>
        public void UploadBatchImportXMLToTable(string bookID, System.Xml.XmlDocument finalDocument)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, bookID);
            paramList.Add("@xmlDocument", finalDocument.InnerXml.ToString());
            ExecuteAndFetch("SP_eBooks_UploadBatchImportXMLToTable", paramList);
        }

        /// <summary>
        /// Gets the only batch import XML.
        /// </summary>
        /// <param name="bookID">The book identifier.</param>
        /// <returns></returns>
        public DataTable GetOnlyBatchImportXML(string bookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, bookID);
            return ExecuteAndFetch("SP_ebooks_GetBatchImportXML", paramList);
        }

        /// <summary>
        /// Gets the book name by book identifier.
        /// </summary>
        /// <param name="strWellBookId">The string well book identifier.</param>
        /// <returns></returns>
        public DataTable GetBookNameByBookId(string strWellBookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@value", strWellBookId);
            return ExecuteAndFetch("SP_eBooks_GetBookName", paramList);
        }

        /// <summary>
        /// Gets the name of the page ids by page.
        /// </summary>
        /// <param name="strPageName">Name of the string page.</param>
        /// <returns></returns>
        public DataTable GetPageIdsByPageName(string strPageName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMPAGENAME, strPageName);
            return ExecuteAndFetch("SP_eBooks_GetPageIdsByPageName", paramList);
        }

        /// <summary>
        /// Gets the page owner details.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="pageName">Name of the page.</param>
        /// <param name="strTerminateStatus">The string terminate status.</param>
        /// <param name="strSelectedChapters">The string selected chapters.</param>
        /// <returns></returns>
        public DataTable GetPageOwnerDetails(string userName, string pageName, string strTerminateStatus, StringBuilder strSelectedChapters)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(DreamConstants.UserPARAM, userName);
            paramList.Add(CommonConstants.PARAMPAGENAME, pageName);
            paramList.Add("@Chapters", strSelectedChapters);
            paramList.Add("TerminateStatus", strTerminateStatus);
            return ExecuteAndFetch("", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataTable SavedAngleDWBChapterPagesMapping(int pageId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageId", pageId);
            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="pageId"></param>
        /// <param name="auditAction"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public void UpdateConfigurationOfPage(string configuration, int pageId, int auditAction, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvConfigurtion", configuration);
            paramList.Add("@intPageID", pageId);
            paramList.Add("@intAuditAction", auditAction);
            paramList.Add("@chvUser", userName);
            ExecuteAndFetch(storProc, paramList);
        }
        /// <summary>
        /// Gets the chapter book identifier.
        /// </summary>
        /// <param name="strChapterId">The string chapter identifier.</param>
        /// <param name="storProc">The stor proc.</param>
        /// <returns></returns>
        public DataTable GetChapterBookId(string strChapterId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", strChapterId);

            return ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="urlName"></param>
        /// <param name="url"></param>
        /// <param name="auditAction"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        public void UpdateURLContentForPages(int pageId, string urlName, string url, int auditAction, string userName, string storProc, int intComponentPropID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvURLName", urlName);
            paramList.Add("@intPageID", pageId);
            paramList.Add("@intAuditAction", auditAction);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add("@chvURL", url);
            paramList.Add("@intComponentPropID", intComponentPropID);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// Updates the chapter print details.
        /// </summary>
        /// <param name="requestID">The request ID.</param>
        /// <param name="documentURL">The document URL.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public DataTable UpdateChapterPrintDetails(string requestID, string documentURL, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.REQUESTIDPARAM, requestID);
            paramList.Add("@documentURL", documentURL);
            paramList.Add("@userName", userName);
            return ExecuteAndFetch("SP_eBooks_UpdateChapterPrintDetails", paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="documentURL"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        public void UpdateChapterPrintDetails(string requestID, string documentURL, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvRequestId", requestID);
            paramList.Add("@chvDocumentURL", documentURL);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PageId"></param>
        /// <param name="documentName"></param>
        /// <param name="documentContent"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        public void UploadFileToDWBWellStatusDiagrams(int PageId, string documentName, Byte[] documentContent, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageID", PageId);
            paramList.Add("@chvDocumentName", documentName);
            paramList.Add("@chvDocumentContent", documentContent);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            ExecuteAndFetch(storProc, paramList);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="PageId"></param>
        /// <param name="userName"></param>
        /// <param name="signOffStatus"></param>
        /// <param name="auditAction"></param>
        /// <param name="storProc"></param>
        public void SignOffPageandUpdateAuditHistory(int PageId, string userName, string signOffStatus, int auditAction, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intPageID", PageId);
            paramList.Add("@chvSignOffStatus", signOffStatus);
            paramList.Add("@intAuditAction", auditAction);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            ExecuteAndFetch(storProc, paramList);
        }

        /// <summary>
        /// Gets the chapter identifier by book identifier.
        /// </summary>
        /// <param name="strWellBookId">The string well book identifier.</param>
        /// <returns></returns>
        public DataTable GetChapterIDByBookID(string strWellBookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Book_ID", strWellBookId);
            return ExecuteAndFetch("SP_eBooks_GetChapterIdbyBookID", paramList);
        }

        /// <summary>
        /// Gets the chapter name by chapter identifier.
        /// </summary>
        /// <param name="ChapterId">The chapter identifier.</param>
        /// <returns></returns>
        public DataTable GetChapterNameByChapterId(string ChapterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Chapter_ID", ChapterId);
            return ExecuteAndFetch("SP_eBooks_GetChapterNameByChapterId", paramList);
        }

        /// <summary>
        /// Saves the batch import logs.
        /// </summary>
        /// <param name="strBookID">The string book identifier.</param>
        /// <param name="strPageName">Name of the string page.</param>
        /// <param name="strChapterName">Name of the string chapter.</param>
        /// <param name="strStatus">The string status.</param>
        /// <param name="strError">The string error.</param>
        public void SaveBatchImportLogs(string strBookID, string strPageName, string strChapterName, string strStatus, string strError)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookID);
            paramList.Add(CommonConstants.PARAMPAGENAME, strPageName);
            paramList.Add("@ChapterName", strChapterName);
            paramList.Add("@Status", strStatus);
            paramList.Add("@Error", strError);
            // ExecutesAndFetchs("SP_eBooks_SaveBatchImportLogs", paramList);
            ExecuteAndFetch("SP_eBooks_SaveBatchImportLogs", paramList);
        }

        /// <summary>
        /// Gets the batch import logs.
        /// </summary>
        /// <returns></returns>
        public DataTable GetBatchImportLogs()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetch("SP_eBooks_GetBatchImportLogs", paramList);
        }

        /// <summary>
        /// Clears the batch import logs.
        /// </summary>
        public void ClearBatchImportLogs()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            ExecuteAndFetch("SP_eBooks_ClearBatchImportLogs", paramList);
        }

        /// <summary>
        /// Updates the batch import audit history.
        /// </summary>
        /// <param name="BookdID">The bookd identifier.</param>
        /// <param name="strUserName">Name of the string user.</param>
        /// <param name="Date">The date.</param>
        /// <param name="actionPerformed">The action performed.</param>
        /// <returns></returns>

        public DataTable UpdateBatchImportAuditHistory(string BookdID, string strUserName, DateTime Date, int actionPerformed)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add("@Id", 0);
            paramList.Add("@Master_ID", BookdID);
            paramList.Add(CommonConstants.PARAMTITLE, strUserName);
            paramList.Add("@Audit_Action", actionPerformed);
            paramList.Add(DreamConstants.UserPARAM, strUserName);
            return ExecuteAndFetch("SP_eBooks_InsertBookAuditTrail", paramList);
        }


        /// <summary>
        /// Gets the chapter details for template.
        /// </summary>
        /// <param name="intChapterID">The int chapter identifier.</param>
        /// <returns></returns>
        public DataTable GetChapterDetailsForTemplate(int intChapterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", intChapterID);

            return ExecuteAndFetch("SP_eBooks_GetChapterpageDetailForTemplate", paramList);
        }

        /// <summary>
        /// Gets the details from chapter page map table.
        /// </summary>
        /// <param name=CommonConstants.VALUE>The value.</param>
        /// <param name="masterId">The master identifier.</param>
        /// <returns></returns>
        public DataTable GetDetailsFromChapterPageMapTable(int value, int masterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", value);
            paramList.Add("@masterPageID", masterId);
            return ExecuteAndFetch("SP_eBooks_GetChapterPageMapDetailForTmplt", paramList);
        }

        public DataTable GetERchapterList(int BookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, BookID);
            return ExecuteAndFetch("USPeBooksGetERchapterList", paramList);
        }
        /// <summary>
        /// Gets the published files.
        /// </summary>
        /// <param name="BookID">The book identifier.</param>
        /// <returns></returns>
        public DataTable GetPublishedFiles(int BookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, BookID);
            return ExecuteAndFetch("SP_eBooks_GetPublishedBooks", paramList);
        }

        /// <summary>
        /// Gets the request identifier.
        /// </summary>
        /// <param name="strRequestID">The string request identifier.</param>
        /// <param name="strLibraryName">Name of the string library.</param>
        /// <returns></returns>
        public DataTable GetRequestID(string strRequestID, string strLibraryName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.REQUESTIDPARAM, strRequestID);
            paramList.Add("@LibraryName", strLibraryName);
            return ExecuteAndFetch("SP_eBooks_GetRequestID", paramList);
        }
        public DataTable GetUnitConversion()
        {
            return ExecuteAndFetch("SP_eBooks_UnitConversion");
        }
        //UOM changes
        public DataTable GetUnitConversionUOM()
        {
            return ExecuteAndFetch("SP_eBooks_UnitConversionUOM");
        }

        public bool SaveNewRanks(string rowID, string rankDetails)
        {
            bool blnUpdateSuccess = false;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, rowID);
            paramList.Add("@Rank", rankDetails);
            ExecuteAndFetch(EBookConstants.SPUPDATENEWRANK, paramList);
            blnUpdateSuccess = true;
            return blnUpdateSuccess;
        }
        public DataTable GetSelectedDisciplineStaff(string discipline, string teamID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMDISCIPLINE, discipline);
            paramList.Add(CommonConstants.TeamIDPARAM, teamID);
            return ExecuteAndFetch(EBookConstants.SPGETSTAFFDETAILSRANKSTAFF, paramList);
        }


        /// <summary>
        /// Gets the book page summary details.
        /// </summary>
        /// <param name="listEntry">GetChartZoomOptionsForBookThe list entry.</param>
        /// <param name="ChapterID">The chapter ID.</param>
        /// <returns></returns>
        public DataTable GetBookPageSummaryDetails(ListEntry listEntry, string ChapterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            bool blnTerminate = false;
            string strPageStatus = listEntry.WellBookDetails.SignOffStatus;
            string strTerminated = listEntry.WellBookDetails.Terminated;
            if (string.Equals(strTerminated, EBookConstants.STATUS_TERMINATED))
                blnTerminate = true;
            else
                blnTerminate = false;
            string strPageOwner = listEntry.WellBookDetails.BookOwner;
            paramList.Add("@PageStatus", strPageStatus);
            paramList.Add("@TerminateStatus", blnTerminate);
            paramList.Add("@Owner", strPageOwner);
            paramList.Add("@ChapterID", ChapterID);
            return ExecuteAndFetch(EBookConstants.SPBOOKPAGESUMMARY, paramList);
        }

        /// <summary>
        /// Gets the book page summary filter.
        /// </summary>
        /// <param name="listEntry">The list entry.</param>
        /// <param name="ChapterID">The chapter ID.</param>
        /// <returns></returns>
        public DataTable GetBookPageSummaryFilter(string PageName, string ChapterName, string PageType, bool SignOffStatus)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMPAGENAME, PageName);
            paramList.Add("@ChapterName", ChapterName);
            paramList.Add("@PageType", PageType);
            paramList.Add("@SignOffStatus", SignOffStatus);
            return ExecuteAndFetch(EBookConstants.SPBOOKPAGESUMMARYFILTER, paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public DataTable GetChartZoomOptionsForBook(int bookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", bookId);
            return ExecuteAndFetch("SP_eBooks_GetChartZoomOptionsForBook", paramList);
        }

        public DataTable UpdatePagePrintDetails(string requestID, string documentURL, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.REQUESTIDPARAM, requestID);
            paramList.Add("@documentURL", documentURL);
            paramList.Add("@userName", userName);
            return ExecuteAndFetch("SP_eBooks_GetPagePrintDetails", paramList);
        }

        public DataTable UpdateChapterPrintDetailsForPrint(string requestID, string documentURL, string userName, string strUserEmail, string strPath, string strChapterTitle)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.REQUESTIDPARAM, requestID);
            paramList.Add("@documentURL", documentURL);
            paramList.Add("@userName", userName);
            paramList.Add("@strUserEmail", strUserEmail);
            paramList.Add("@strPath", strPath);
            paramList.Add("@strChapterTitle", strChapterTitle);

            return ExecuteAndFetch("SP_eBooks_UpdateChapterPrintDetails", paramList);
        }

        public void UpdateAWRPDFPath(int awrid, string documentURL)
        {
            try
            {
                Dictionary<string, object> paramList = new Dictionary<string, object>();
                paramList.Add("@AWR_ID", awrid);
                paramList.Add("@PDFPath", documentURL);

                ExecuteAndFetch("AWR_UpdatePDFPath", paramList);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw ex;
            }
        }

        public DataTable GetEmailTemplateDetails(string value)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.SubjectPARAM, value);
            return ExecuteAndFetch("SP_Common_GetEmailTemplate", paramList);
        }
        public DataTable GetChapterDetailsforEmail(string StrRequestId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.REQUESTIDPARAM, StrRequestId);
            return ExecuteAndFetch("SP_eBooks_GetchapterDetailsforEmail", paramList);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userRole"></param>
        /// <returns></returns>
        public DataTable GetDWBReorderXML(string userName, string userRole)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add("@chvUserRole", userRole);
            return ExecuteAndFetch("SP_eBooks_GetDWBReorderXML", paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userRole"></param>
        /// <param name="reorderXML"></param>
        public void SaveOrUpdateDWBReorderXML(string userName, string userRole, string reorderXML)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add("@chvUserRole", userRole);
            paramList.Add("@chvReorderXML", reorderXML);
            ExecuteAndFetch("SP_eBooks_InsertOrUpdateDWBReorderXML", paramList);
        }


        /// <summary>
        /// Gets the user mail id.
        /// </summary>
        /// <param name="currentUser">The current user.</param>
        /// <returns></returns>
        public DataTable GetUserMailId(string currentUser)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.PARAMUSERID, currentUser);
            return ExecuteAndFetch("SP_eBooks_GetEmailID", paramList);
        }

        /// <summary>
        /// Updates the is empty status.
        /// </summary>
        /// <param name="PageID">The page ID.</param>
        public void UpdateIsEmptyStatus(string PageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@PageID", PageID);
            ExecuteAndFetch("SP_eBooks_UpdateIsEmptyStatus", paramList);
        }

        /// <summary>
        /// Gets all asset type columns list.
        /// </summary>
        public DataTable GetAllAssetTypeColumnsList()
        {
            return ExecuteAndFetch("SP_eBooks_GetAllAssetTypeColumnsList");
        }


        //WRFMMigration_Release [4.2] <start>
        public DataTable GetComponentMasterIDs(string strRowId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@InputComponent", strRowId);
            return ExecuteAndFetch("SP_eBooks_GetMasterIDsOfSelectedComponent", paramList);
        }


        public DataTable GetTemBookMasterIDs(string MasterID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterID", MasterID);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetComponentValue(string MasterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@MasterID", MasterID);
            return ExecuteAndFetch("SP_eBooks_GetComponentForMasterID", paramList);
        }



        public int UpdateComponentForMaster(string strUpdateComponent, string strMasterID, string strUserName)
        {
            int intComponentUpdated = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();

                dbManager.AddParameters(CommonConstants.PARAMCOMPONENT, strUpdateComponent);
                dbManager.AddParameters("@rowID", strMasterID);
                dbManager.AddParameters(SEDConstants.PARAMUSERID, strUserName);
                dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_UpdateComponentForMaster");
                intComponentUpdated = Int32.Parse(dbManager.Parameters[3].Value.ToString());

            }

            return intComponentUpdated;
        }

        public DataSet GetRecordAuditDetailsForGeneric(string strAuditId, string strProcGenericAudit)
        {
            DataSet dtAudit = null;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AUDITID", strAuditId);
            dtAudit = ExecuteAndFetchAsDataSet(strProcGenericAudit, paramList);

            return dtAudit;
        }


        public int InsertMasterTempTableForDel(string strMasterID, string strUpdateComponent, string strUserName)
        {
            int intComponentUpdated = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();

                dbManager.AddParameters("@MasterID", strMasterID);
                dbManager.AddParameters(CommonConstants.PARAMCOMPONENT, strUpdateComponent);
                dbManager.AddParameters(SEDConstants.PARAMUSERID, strUserName);
                // dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_InsertTempMasterForDelete");
                //intComponentUpdated = Int32.Parse(dbManager.Parameters[3].Value.ToString());
                intComponentUpdated = 1;
            }

            return intComponentUpdated;
        }


        public int InsertMasterTempTableForUpdate(string strMasterID, string strUpdateComponent, string strUserName)
        {
            int intComponentUpdated = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters("@MasterID", strMasterID);
                dbManager.AddParameters(CommonConstants.PARAMCOMPONENT, strUpdateComponent);
                dbManager.AddParameters(SEDConstants.PARAMUSERID, strUserName);
                // dbManager.AddParameters("@OutputID", 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_InsertTempMasterForUpdate");
                //intComponentUpdated = Int32.Parse(dbManager.Parameters[3].Value.ToString());
                intComponentUpdated = 1;

            }

            return intComponentUpdated;
        }

        public DataTable GetMasterPagesToDelete(string strUserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Username", strUserName);
            return ExecuteAndFetch("SP_eBooks_GetMasterPagesForDelete", paramList);
        }

        public DataTable GetMasterPagesToUpdate(string strUserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Username", strUserName);
            return ExecuteAndFetch("SP_eBooks_GetMasterPagesForUpdate", paramList);
        }


        public void TruncateTempTables()
        {
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_eBooks_TruncateTempTables");
            }
        }

        //WRFMMigration_Release [4.2] <End>

        //WRFMMigration_Release [5.1] Req  <Start>

        public int GetTemplateID(string strChapterIds)
        {
            int intTemplateID = 0;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters("@ChapterID", strChapterIds);
                dbManager.AddParameters("@OutputTemplateID", 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "USP_eBooks_GetTemplateID");
                intTemplateID = Int32.Parse(dbManager.Parameters[1].Value.ToString());
            }

            return intTemplateID;
        }

        public int RefreshChapterDAL(string strUserName, string strChapterIds, int intTemplateID)
        {

            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                //dbManager.AddParameters("@tbltem_tbl_TemplatePages", dtTemplatePages);
                //dbManager.AddParameters("@tbltem_tbl_ChapterPages", dtChapterPages);
                dbManager.AddParameters(SEDConstants.PARAUSERNAME, strUserName);
                dbManager.AddParameters("@ChapterIDs", strChapterIds);
                dbManager.AddParameters("@TemplateID", intTemplateID);
                //dbManager.AddParameters("@OutputTemplateID", 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, "USP_eBooks_RefreshChapter");
                //intRefreshChapter = Int32.Parse(dbManager.Parameters[3].Value.ToString());
            }

            return intTemplateID;
        }

        public DataTable GetTemplatePages(int intTemplateID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intTemplateID);

            return ExecuteAndFetch("USP_eBooks_GetTemplatePages", paramList);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userRole"></param>
        /// <returns></returns>
        public DataTable RefreshChapterDAL1(DataTable dtTemplatePages, string strUserName, string strChapterIds, int intTemplateID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@tbltem_tbl_ChapterPages", dtTemplatePages);
            paramList.Add(SEDConstants.PARAUSERNAME, strUserName);
            paramList.Add("@ChapterIDs", strChapterIds);
            paramList.Add("@TemplateID", intTemplateID);
            return ExecuteAndFetch("USP_eBooks_RefreshChapters_Test", paramList);
        }

        public DataTable GetChapterPages(int intTemplateID, string strChapterIds)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TemplateID", intTemplateID);
            paramList.Add("@ChapterIDs", strChapterIds);
            return ExecuteAndFetch("USP_eBooks_GetChapterPages", paramList);
        }

        private DataTable GetChapterPageSequence(int ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@rowID", ID);

            return ExecuteAndFetch("USP_eBooks_GetChapterPageSequence", paramList);
        }

        public DataTable GetViewsForDataSource(int ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intDS_ID", ID);
            return ExecuteAndFetch("USP_GTC_GetViewNames", paramList);
        }

        public DataTable GetDataSourceDetails(int ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intDS_ID", ID);
            return ExecuteAndFetch("USP_GTC_GetDataSourceDetails", paramList);
        }
        public DataTable InsertTabularComponentDetails(string TabularComponentName, string AssetTypes, int DataSourceID, string ListOrTopicName, string DateRangeColumn, DataTable dtColumnsData, string UserName, string storedProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@TabCompName", TabularComponentName);
            paramList.Add("@AssetTypes", AssetTypes);
            paramList.Add("@DataSourceID", DataSourceID);
            paramList.Add("@ListOrTopicName", ListOrTopicName);
            paramList.Add("@dtColumnsData", dtColumnsData);
            if (DateRangeColumn != null)
                paramList.Add(DreamConstants.DateRangeColumnPARAM, DateRangeColumn);
            paramList.Add(SEDConstants.PARAUSERNAME, UserName);
            return ExecuteAndFetch(storedProc, paramList);
        }


        public DataTable GetGenericRecords(string strVIEW_MODE, string strNotTerminated)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strVIEW_MODE", strVIEW_MODE);
            paramList.Add("@strACTIVE_STATUS", strNotTerminated);
            return ExecuteAndFetch("USP_eBooks_GetGenericComponents", paramList);
        }

        public DataTable GetColumnsToBeAdded(string strGetComponentID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strCOMPONENT_ID", strGetComponentID);

            return ExecuteAndFetch("USP_eBooks_GetComponentDeatils", paramList);
        }

        public DataTable GetDataSourceName(int ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intDS_ID", ID);
            return ExecuteAndFetch("USP_GTC_GetDSNameForSelectedComponent", paramList);
        }

        public DataTable UpdateTabularComponentDetails(int intComponentID, string strComponentName, string strAssetType, int intDataSourceID, string strViewOrTopic, string DateRangeColumn, DataTable dtGridData, string strUserName, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@GenericComponentID", intComponentID);
            paramList.Add("@TabCompName", strComponentName);
            paramList.Add("@varAssetType", strAssetType);
            paramList.Add("@intDataSourceID", intDataSourceID);
            paramList.Add("@varViewOrTopic", strViewOrTopic);
            paramList.Add(DreamConstants.DateRangeColumnPARAM, DateRangeColumn);
            paramList.Add("@dtColumnsData", dtGridData);
            paramList.Add(SEDConstants.PARAUSERNAME, strUserName);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable InsertGTCMasterPageComponents(string strComponentID, bool blnIsVertical, int intMasterPageID, string strAction)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strComponentID", strComponentID);
            paramList.Add("@blnIsVertical", blnIsVertical);
            paramList.Add("@intMasterPageID", intMasterPageID);
            paramList.Add("@strAction", strAction);
            return ExecuteAndFetch(EBookConstants.GTC_INSERTMASTERPAGECOMPONENT, paramList);
        }

        public DataSet GetWellSummaryConfig(int componentId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ComponentID", componentId);
            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }

        public DataTable GetGenericTableSortOrder(int intComponentIndex, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ComponentId", intComponentIndex);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetGenericTableSortColDataTypes(int intComponentIndex, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ComponentId", intComponentIndex);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetGenericTableColumns(int intComponentIndex, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ComponentId", intComponentIndex);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetGenericTableProperties(int intDSId, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@DS_ID", intDSId);
            return ExecuteAndFetch(storProc, paramList);
        }

        public DataTable GetEWBGenericViews(string strViewName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strViewName", strViewName);
            return ExecuteAndFetch("USP_GTC_GetAssetIdentifier", paramList);
        }

        public DataTable GetDataSourceName_BV(int intDataSourceId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intDataSourceId", intDataSourceId);
            return ExecuteAndFetch("USP_GTC_GetDataSourceName", paramList);
        }

        public DataTable GetVerticalProperty(int intPageMappingID, int intComponetId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, intPageMappingID);
            paramList.Add("@ComponentId", intComponetId);
            return ExecuteAndFetch(EBookConstants.USP_VERTICALDETAILS, paramList);
        }

        public DataTable GetVerticalProperty_MP(int intPageMappingID, int intComponetId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMID, intPageMappingID);
            paramList.Add("@ComponentId", intComponetId);
            return ExecuteAndFetch(EBookConstants.USP_VERTICALDETAILS_MP, paramList);
        }

        /// <summary>
        /// UPDATES THE PAGE OWNER BASED ON USER RANK
        /// </summary>
        /// <param name=DreamConstants.TeamID></param>
        public void UpdatePageOwner(string TeamID, string StaffDiscipline = null)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMTEAMID, TeamID);
            paramList.Add(EBookConstants.STAFFDISCIPLINE, StaffDiscipline);
            ExecuteAndFetch(EBookConstants.SPEBOOKSUPDATEPAGEOWNER, paramList);

        }

        /// <summary>
        /// CHECKS IF THE REGION HAS THE SELECTED COMPONENT
        /// OR SELECTED COMPONENT IS APPLICABLE FOR THE REGION
        /// </summary>
        /// <param name="strRegion"></param>
        /// <param name="strComponent"></param>
        /// <returns></returns>
        public DataTable HasComponent(string strRegion, string strComponent)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMREGION, strRegion);
            paramList.Add(EBookConstants.PARAMCOMPONENT, strComponent);
            return ExecuteAndFetch(EBookConstants.SPEBOOKSHASCOMPONENTS, paramList);
        }

        #region Changes done for TFS#:113862
        /// <summary>
        /// This method is used to get the Team staff list using Book Id
        /// </summary>
        /// <param name="intBookID"></param>
        /// <returns></returns>
        public DataTable GetTeamStaffListUsingBookID(int intBookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", intBookID);
            return ExecuteAndFetch("USP_eBooks_GetTeamStaffListUsingBookID", paramList);
        }
        #endregion Changes done for TFS#:113862

        /// <summary>
        /// GETS THE REDIRECTION URL FOR RENDERING SPECIFIC COMPONENTS FOR A REGION
        /// </summary>
        /// <param name="strRegion"></param>
        /// <param name="strReportName"></param>
        /// <returns></returns>
        public DataTable GetComponentRenderingRedirection(string strRegion, string strReportName, string strPageType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMREGION, strRegion);
            paramList.Add(EBookConstants.PARAMREPORTNAME, strReportName);
            paramList.Add(EBookConstants.PARAMPAGETYPE, strPageType);
            return ExecuteAndFetch(EBookConstants.SPEBOOKSGETCOMPONENTRENDERINGREDIRECTION, paramList);
        }

        /// <summary>
        /// Gets the ValidateChapter.
        /// </summary>
        /// <param name="AssetType">Type of the asset.</param>
        /// <returns></returns>
        public DataTable GetValidateChapter(string AssetValue)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AssetValues", AssetValue);
            return ExecuteAndFetch(EBookConstants.GetValidateChapter, paramList);
        }

        #region Release 6.0

        public DataTable BindDatatoAuditTrial(string strPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@strPageID", strPageId);
            return ExecuteAndFetch("SP_eBooks_BindDataToAuditTrial", paramList);
        }

        public DataTable GetChapterDefaultPage(int intChapterId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMCHAPTERID, intChapterId);
            return ExecuteAndFetch(EBookConstants.GETDEFAULTCHAPTERPAGE, paramList);
        }

        public DataTable GetChapterPageComponents(int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PAGEID, intPageId);
            return ExecuteAndFetch(EBookConstants.GETPAGECOMPONENTDETAILS, paramList);
        }
        public DataTable GetBookPageComponents(int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PAGEID, intPageId);
            return ExecuteAndFetch(EBookConstants.USPEBOOKGETBBOOKPAGECOMPONENTDETAILS, paramList);
        }

        /// <summary>
        /// GETS THE GAIN UNIT,COST UNIT AND COST SCALE VALUES TO LOAD RESPECTIVE DROPDOWNS
        /// </summary>
        /// <returns></returns>
        public DataSet GetTeamUnits()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetchAsDataSet(EBookConstants.USPEBOOKSGETTEAMUNITS, paramList);
        }

        /// <summary>
        /// Get the component position and print details
        /// </summary>
        /// <returns></returns>
        public DataSet GetComponentPositionAndPrint()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetchAsDataSet(EBookConstants.GETPAGECOMPONENTPOSITION, paramList);
        }


        /// <summary>
        ///Insert the page component properties details
        /// </summary>
        /// <returns></returns>
        public int SaveComponentProperties(ComponentProperties componentProperties)
        {
            WRFMCommon objCommon = WRFMCommon.Instance;
            int outputParameter = 0;
            using (DBManager dbManager = objCommon.DataManager)
            {
                dbManager.Open();
                dbManager.AddParameters(CommonConstants.PARAMID, componentProperties.ID);
                dbManager.AddParameters(EBookConstants.MasterPageIdParameter, componentProperties.MasterPageId);
                dbManager.AddParameters(EBookConstants.ComponentIdParameter, componentProperties.ComponentId);
                dbManager.AddParameters(EBookConstants.NameParameter, componentProperties.Name);
                if (componentProperties.ViewMode.Equals(EBookConstants.GENERIC_TABLE) || componentProperties.ViewMode.Equals(EBookConstants.GENERICPLOT))
                    dbManager.AddParameters(EBookConstants.IsGenericParameter, "True");
                else
                    dbManager.AddParameters(EBookConstants.IsGenericParameter, "False");
                dbManager.AddParameters(EBookConstants.HeightParameter, componentProperties.Height);
                dbManager.AddParameters(EBookConstants.WidthParameter, componentProperties.Width);
                dbManager.AddParameters(EBookConstants.PositionIdParameter, componentProperties.PositionId);
                dbManager.AddParameters(EBookConstants.PrintOptioIdParameter, componentProperties.PrintOptionId);
                dbManager.AddParameters(EBookConstants.DisplayContentParameter, componentProperties.DisplayContent);
                dbManager.AddParameters(EBookConstants.CreatedByParameter, componentProperties.CreatedBy);
                dbManager.AddParameters(EBookConstants.PotraitParameter, componentProperties.IsPotrait);
                dbManager.AddParameters(EBookConstants.IsActiveParameter, componentProperties.IsActive);
                dbManager.AddParameters(EBookConstants.ComponentOrderParameter, componentProperties.ComponentOrder);
                dbManager.AddParameters(EBookConstants.MaxCharactersParameter, componentProperties.MaxCharacters);
                dbManager.AddParameters(EBookConstants.ProductTypeParameter, componentProperties.ProductType);  //145321 - ER Functionalities Implementation (EP Catalog)
                if (componentProperties.Name == EBookConstants.AWRWEPH)
                {
                    dbManager.AddParameters(EBookConstants.EVNTGRPPARAM, componentProperties.EventGroups);
                    dbManager.AddParameters(EBookConstants.EVNTTYPPARAM, componentProperties.EventTypes);
                    dbManager.AddParameters(EBookConstants.EVNTOWNPARAM, componentProperties.Owners);
                    if (componentProperties.EventsFrom != null && componentProperties.EventsTo != null)
                    {
                        dbManager.AddParameters(EBookConstants.EVNTFRMPARAM, componentProperties.EventsFrom);
                        dbManager.AddParameters(EBookConstants.EVNTTOPARAM, componentProperties.EventsTo);
                    }
                    dbManager.AddParameters(EBookConstants.EVNTSORTPARAM, componentProperties.EventSortOrder);
                }
                dbManager.AddParameters(EBookConstants.IDExistParameter, 0, ParameterDirection.Output);
                dbManager.ExecuteNonQuery(CommandType.StoredProcedure, EBookConstants.UspInsertPageComponentProperties);
                outputParameter = Int32.Parse(dbManager.Parameters[13].Value.ToString()); ;

            }
            return outputParameter;

        }
        /// <summary>
        /// Get page component properties information based on master page id
        /// </summary>
        /// <param name="intMasterPageId"></param>
        /// <returns></returns>
        public DataTable GetPageComponentsDetails(int intMasterPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.MasterPageIdParameter, intMasterPageId);
            return ExecuteAndFetch(EBookConstants.UspGetComponentPropertiesDetails, paramList);
        }

        /// <summary>
        /// DISPLAYS FEW BUTTONS TO TEAM MEMBERS IN BOOK VIEWER SCREEN
        /// </summary>
        /// <param name="bookTeamID"></param>
        /// <param name="strUserID"></param>
        /// <returns></returns>
        public bool DisplayButtonsForTeamStaff(int bookTeamID, string strUserID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            DataTable dtResult = null;
            paramList.Add(EBookConstants.PARAMBOOKTEAMID, bookTeamID);
            paramList.Add(EBookConstants.PARAMUSERID, strUserID);
            dtResult = ExecuteAndFetch(EBookConstants.USPEBOOKSDISPLAYBUTTONSFORTEAMSTAFF, paramList);
            if (dtResult != null && dtResult.Rows.Count > 0)
                return true;
            else
                return false;
        }

        public DataTable GetColumnDetails(string strViewName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.CHVVIEWNAME, strViewName);
            return ExecuteAndFetch(EBookConstants.USPGETCOLUMNDETAILS, paramList);
        }

        /// <summary>
        /// To fetch the team members for respected book
        /// </summary>
        /// <param name="BookTeamID"></param>
        /// <returns></returns>
        public DataTable GetBookTeamStaff(string BookTeamID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMBOOKID, BookTeamID);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSGETBOOKTEAMSTAFF, paramList);
        }

        //AWR Review Team
        public DataTable GetAWRBookTeamStaff(string awrID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMAWRID, awrID);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSGETAWRBOOKTEAMSTAFF, paramList);
        }

        public DataTable GetAWRTeamStaff(string awrID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMAWRID, awrID);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSGETAWRTEAMSTAFF, paramList);
        }
        //AWR Review Team

        /// <summary>
        /// adds External review members
        /// </summary>
        /// <param name="BookTeamID"></param>
        /// <returns></returns>
        public void AddExternalReviewMembers(XmlDocument MemberXml, string BookTeamID, int BookID)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings[EBookConstants.LOCALSQLSERVER].ConnectionString;
            SqlCommand cmd = new SqlCommand
            {
                Connection = new SqlConnection(connection),
                CommandText = EBookConstants.USPEBOOKSADDEXTERNALREVIEWMEMBERS,
                CommandType = CommandType.StoredProcedure
            };
            cmd.Connection.Open();
            cmd.Parameters.AddWithValue(EBookConstants.PARAMBOOKTEAMID, BookTeamID);
            cmd.Parameters.AddWithValue(EBookConstants.PARAMBOOKID, BookID);
            cmd.Parameters.Add(EBookConstants.MEMBERXML, SqlDbType.Xml).Value = MemberXml.InnerXml;
            cmd.ExecuteNonQuery();
        }

        //AWR Review Team
        public void AddAWRExternalReviewMembers(XmlDocument MemberXml, int awrId)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings[EBookConstants.LOCALSQLSERVER].ConnectionString;
            SqlCommand cmd = new SqlCommand
            {
                Connection = new SqlConnection(connection),
                CommandText = EBookConstants.USPEBOOKSAWRADDEXTERNALREVIEWMEMBERS,
                CommandType = CommandType.StoredProcedure
            };
            cmd.Connection.Open();
            cmd.Parameters.AddWithValue(EBookConstants.PARAMAWRID, awrId);
            //cmd.Parameters.AddWithValue(EBookConstants.PARAMBOOKTEAMID, BookTeamID);
            //cmd.Parameters.AddWithValue(EBookConstants.PARAMBOOKID, BookID);
            cmd.Parameters.Add(EBookConstants.MEMBERXML, SqlDbType.Xml).Value = MemberXml.InnerXml;
            cmd.ExecuteNonQuery();
        }


        //AWR Review Team


        //AWR Review Outcome Summary

        public void AddAWRReviewOutcome(string Comments, int awrId)
        {
            string connection = System.Configuration.ConfigurationManager.ConnectionStrings[EBookConstants.LOCALSQLSERVER].ConnectionString;
            SqlCommand cmd = new SqlCommand
            {
                Connection = new SqlConnection(connection),
                CommandText = EBookConstants.USPAWRREVIEWOUTCOME,
                CommandType = CommandType.StoredProcedure
            };
            cmd.Connection.Open();
            cmd.Parameters.AddWithValue(EBookConstants.PARAMAWRID, awrId);

            cmd.Parameters.AddWithValue(EBookConstants.REVIEWCOMMENTS, Comments);
            cmd.ExecuteNonQuery();
        }

        //AWR Review Outcome Summary

        //AWR Review Outcome Summary
        public DataTable GetReviewOutComeData(int awr_id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add(EBookConstants.PARAMAWRID, awr_id);

            return ExecuteAndFetch(EBookConstants.USPGETAWRREVOUTCOME, paramList);
        }

        //AWR Review Outcome Summary


        //AWR Review Outcome Summary
        public DataTable GetReviewOutComeChapters(int awr_id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add(EBookConstants.PARAMAWRID, awr_id);

            return ExecuteAndFetch(EBookConstants.USPGETREVOUTCHAPTERS, paramList);
        }

        //AWR Review Outcome Summary



        /// <summary>
        /// to remove external members
        /// 
        /// </summary>
        /// <param name="BookTeamID"></param>
        public void RemoveExternalMembers(int BookID, string BookTeamID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMBOOKID, BookID);
            paramList.Add(EBookConstants.PARAMBOOKTEAMID, BookTeamID);
            ExecuteAndFetch(EBookConstants.USPEBOOKSREMOVEEXTERNALREVIEWMEMBERS, paramList);

        }

        /// <summary>
        /// To initiate review mode
        /// </summary>
        /// <param name="BookTeamID"></param>
        public void InitiateReviewMode(string BookID, StringBuilder strSelectedChapterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            string strSelectedChapterIDs = strSelectedChapterID.ToString();
            paramList.Add(EBookConstants.PARAMBOOKID, BookID);
            paramList.Add(EBookConstants.SELECTEDCHAPTERID, strSelectedChapterIDs);
            ExecuteAndFetch(EBookConstants.USPEBOOKSINITIATEREVIEWMODE, paramList);

        }

        public DataTable GetPageComponents(int intMasterID, int intComponentID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.MASTERPAGEID, intMasterID);
            paramList.Add(EBookConstants.COMPONENTID, intComponentID);
            return ExecuteAndFetch(EBookConstants.GET_PAGECOMPONENT, paramList);
        }


        /// <summary>
        /// This method is used to get the chapter asset details in a book
        /// </summary>
        /// <param name="strAssetType"></param>
        /// <param name="intBookId"></param>
        /// <param name="strSearchType"></param>
        /// <returns></returns>
        public DataTable GetBookAssets(string strAssetType, int intBookId, string strSearchType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMASSETTYPE, strAssetType);
            paramList.Add(EBookConstants.PARAMINTBOOKID, intBookId);
            paramList.Add(EBookConstants.PARAMSEARCHTYPE, strSearchType);
            return ExecuteAndFetch(EBookConstants.SPEBOOKSGETALLCHAPTERASSETSOFBOOK, paramList);
        }

        /// <summary>
        /// SAVES THE BOOK FILTER VIEWS
        /// </summary>
        /// <param name="objBookFilter"></param>
        /// <returns></returns>
        public DataTable TransactViewBookFilter(BookFilter objBookFilter)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMINTBOOKID, objBookFilter.BookID);
            paramList.Add(EBookConstants.PARAMUSERID, objBookFilter.UserName);
            paramList.Add(EBookConstants.PARAMVIEWNAME, objBookFilter.ViewName);
            paramList.Add(EBookConstants.PARAMCHAPTERTITLE, objBookFilter.ChapterTitle);
            paramList.Add(EBookConstants.PARAMDISCIPLINE, objBookFilter.Discipline);
            paramList.Add(EBookConstants.PARAMPAGENAME, objBookFilter.PageName);
            paramList.Add(EBookConstants.PARAMISDEFAULT, objBookFilter.IsDefault);
            paramList.Add(EBookConstants.PARAMISSHARED, objBookFilter.IsShared);
            paramList.Add(EBookConstants.PARAMUSERPRIVILEGE, objBookFilter.UserPrivilege);
            paramList.Add(EBookConstants.PARAMACTION, objBookFilter.Action);
            paramList.Add(EBookConstants.PARAMBFSSID, objBookFilter.BFSSID);
            paramList.Add(EBookConstants.PARAMSELUSERNAME, objBookFilter.SelUserName);
            paramList.Add(EBookConstants.ISEMERGENCY, objBookFilter.IsEmergency);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSTRANSACTVIEWBOOKFILTER, paramList);
        }

        public DataTable GetUserUploadedDocuments(int intComponentPropertyID, int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_COMPONENT_PROPERTY_ID, intComponentPropertyID);
            paramList.Add(EBookConstants.PAGEID, intPageId);
            return ExecuteAndFetch(EBookConstants.USP_GET_USER_UPLOADED_DOCUMENTS, paramList);
        }

        public bool UploadType5FileToDatabase(Dictionary<string, object> paramList)
        {
            bool blnUpdateSuccess = false;
            try
            {
                ExecuteAndFetch(EBookConstants.USP_UPDATE_TYPE5_USER_DOCUMENTS, paramList);
                blnUpdateSuccess = true;
            }
            catch
            {
                blnUpdateSuccess = false;
                throw;
            }
            return blnUpdateSuccess;
        }

        public bool RemoveUploadedType5Content(int intComponentPropertyID, string strUserName, int intPageId)
        {
            bool blnUpdateSuccess = false;
            try
            {
                Dictionary<string, object> paramList = new Dictionary<string, object>();
                paramList.Add(EBookConstants.PARM_COMPONENT_PROPERTY_ID, intComponentPropertyID);
                paramList.Add(EBookConstants.PARM_USER_NAME, strUserName);
                paramList.Add(EBookConstants.PAGEID, intPageId);
                ExecuteAndFetch(EBookConstants.USP_REMOVE_UPLOAD_TYPE5_CONTENT, paramList);
                blnUpdateSuccess = true;
            }
            catch
            {
                blnUpdateSuccess = false;
                throw;
            }
            return blnUpdateSuccess;
        }

        public bool ValidateUserForUploadControl(string strUserName, int intTeamId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            DataTable dtResult = null;
            bool boolResult = false;
            try
            {

                paramList.Add(EBookConstants.PARM_USER_NAME, strUserName);
                paramList.Add(EBookConstants.PARAMTEAMID, intTeamId);
                dtResult = ExecuteAndFetch(EBookConstants.USP_EBOOKS_VALIDATE_USER, paramList);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0][0] != DBNull.Value && !string.IsNullOrEmpty(dtResult.Rows[0][0].ToString()))
                    {
                        if (!bool.TryParse(dtResult.Rows[0][0].ToString(), out boolResult))
                            boolResult = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (dtResult != null)
                    if (dtResult != null) dtResult.Dispose();
            }
            return boolResult;
        }

        public void SaveTextBoxData(int strComponentPropertyId, string strTextBoxData, string user, int pageid)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.COMPONENTPROPERTYIDPARAMETER, strComponentPropertyId);
            paramList.Add(EBookConstants.COMPONENTDATAPARAMETER, strTextBoxData);
            paramList.Add(EBookConstants.PARM_USER_NAME, user);
            paramList.Add(EBookConstants.PAGEID, pageid);
            ExecuteAndFetch(EBookConstants.USPEBOOKSAVETEXTBOXCOMPONENT, paramList);
        }

        public DataTable GetTextBoxData(int componentPropertyId, int pageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.COMPONENTPROPERTYIDPARAMETER, componentPropertyId);
            paramList.Add(EBookConstants.PAGEID, pageId);

            return ExecuteAndFetch(EBookConstants.USPEBOOKGETTEXTBOXDATA, paramList);
        }


        public DataTable UpdateTextBoxDataAuditTrial(string strAuditAction, int componentPropertyId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.AUDITACTIONPARAMETER, strAuditAction);
            paramList.Add(EBookConstants.COMPONENTPROPERTYIDPARAMETER, componentPropertyId);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSUPDATETEXTBOXCOMPONENTAUDITTRAIL, paramList);
        }
        /// <summary>
        /// Gets the published chapters.
        /// </summary>
        /// <param name="ChapterID">The book identifier.</param>
        /// <returns></returns>
        public DataTable GetPublishedChapters(int ChapterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ChapterID", ChapterID);
            return ExecuteAndFetch("SP_eBooks_GetPublishedChapters", paramList);
        }

        /// <summary>
        /// Gets the published List.
        /// </summary>
        /// <param name="ChapterID">The book identifier.</param>
        /// <returns></returns>
        public DataTable GetPublishedList(int BookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@BookID", BookId);
            return ExecuteAndFetch("SP_eBooks_GetPublishedList", paramList);
        }

        /// <summary>
        /// Gets the PageName per Discipline.
        /// </summary>
        /// <param name="ChapterID">The book identifier.</param>
        /// <param name=EBookConstants.DISCIPLINE>The book identifier.</param>
        /// <returns></returns>
        public DataTable GetPageNamePerDiscipline(string BookID, string ChapterID, string Discipline)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", BookID);
            paramList.Add("@intChapterId", ChapterID);
            paramList.Add("@chvSelectedDiscipline", Discipline);
            return ExecuteAndFetch("usp_eBooks_getDiscipline", paramList);
        }

        public DataTable GetPageIdsByPageID(int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_PROP_ID, intPageId);
            return ExecuteAndFetch(EBookConstants.USP_GET_PAGE_DETAILS_BY_ID, paramList);
        }

        public DataTable ValidateBookFilterViewName(string strViewName, int intBookId, bool blnUserPrivilege)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMVIEWNAME, strViewName);
            paramList.Add(EBookConstants.PARAMINTBOOKID, intBookId);
            paramList.Add(EBookConstants.PARAMUSERPRIVILEGE, blnUserPrivilege);
            return ExecuteAndFetch(EBookConstants.USP_VALIDATE_BOOK_FILTER_NAME, paramList);
        }
        public void UpdateAddedNearbyAssets(int intPageID, int eWBComponentIndex, string strAddedAssets, string strColumnName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.INTPARAMETERPAGEID, intPageID);
            paramList.Add(EBookConstants.EWBCOMPONENTINDEXPARAMETER, eWBComponentIndex);
            paramList.Add(EBookConstants.VARADDEDASSETS, strAddedAssets);
            paramList.Add(EBookConstants.VARCOLUMNNAME, strColumnName);
            ExecuteAndFetch(storProc, paramList);
        }

        public void UpdateType5ReportContent(int intComponentPropId, string content, string userName, int auditAction, bool isEmpty, int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_COMPONENT_PROPERTY_ID, intComponentPropId);
            paramList.Add(EBookConstants.PARM_REPORT_CONTENT, content);
            paramList.Add(CommonConstants.PARAMUSERNAME, userName);
            paramList.Add(EBookConstants.PARM_REPORT_ISEMPTY, isEmpty);
            paramList.Add(EBookConstants.PARM_AUDIT_ACTION, auditAction);
            paramList.Add(EBookConstants.PARM_PAGE_ID, intPageId);
            ExecuteAndFetch(EBookConstants.USP_UPDATE_TYPE5_REPORT_COMPONENT, paramList);
        }

        public DataTable GetUploadedType5ReportPageContent(int intComponentPropId, int intPageId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_COMPONENT_PROPERTY_ID, intComponentPropId);
            paramList.Add(EBookConstants.PAGEID, intPageId);
            return ExecuteAndFetch(EBookConstants.USP_GET_TYPE5_REPORT_COMPONENT, paramList);
        }


        public DataTable GetEventDateContent(string strIdentifierValue)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ASSETIDENTIFIER, strIdentifierValue);
            return ExecuteAndFetch(EBookConstants.USP_EBOOKS_GETEVENTDATECONTENT, paramList);
        }

        public DataTable GetChaptetLastUpadated(string strChapterID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.CHAPTERID, strChapterID);
            return ExecuteAndFetch(EBookConstants.USP_EBOOKS_CHAPTERLASTUPDATED, paramList);
        }

        public DataTable GetActualMasterPageID(string strListType, int intMasterPageID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.LISTTYPE, strListType);
            paramList.Add(EBookConstants.ACTUALMASTERPAGEID, intMasterPageID);
            return ExecuteAndFetch(EBookConstants.USP_EBOOKS_GETACTUALMASTERPAGEID, paramList);
        }


        public void InsertWellSummaryH2S(string strSelectedH2sValue, int intPageID, int intCompPropID, string strRecordNo, string strH2SValueHidden, string strDestnUnit, string strAssetIdenfier)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.STRH2SVALUE, strSelectedH2sValue);
            paramList.Add(EBookConstants.INTPAGEID, intPageID);
            paramList.Add(EBookConstants.INTCOMPPROPID, intCompPropID);
            paramList.Add(EBookConstants.INTRECORDNO, strRecordNo);
            paramList.Add(EBookConstants.PARAMH2SVALUE, strH2SValueHidden);
            paramList.Add(EBookConstants.PARAMDESTUNIT, strDestnUnit);
            paramList.Add(EBookConstants.PARAMASSETIDENTIFIER, strAssetIdenfier);
            ExecuteAndFetch(EBookConstants.USP_EBOOKS_INSERTWELLSUMMARYH2S, paramList);
        }

        public DataTable getH2SValueFromDB(string spanID, int intPageID, int intCopmPropID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.SPANID, spanID);
            paramList.Add(EBookConstants.GETPAGEID, intPageID);
            paramList.Add(EBookConstants.INTCOMPPROPID, intCopmPropID);
            return ExecuteAndFetch(EBookConstants.USP_EBOOKS_GETH2SVALUE, paramList);
        }

        public DataTable GetPageDetailsByPageName(string strPageName, int intBookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMPAGENAME, strPageName);
            paramList.Add(EBookConstants.PARM_BOOK_ID, intBookId);
            return ExecuteAndFetch(EBookConstants.USP_EBOOOK_GETPAGEDETAILS_BY_PAGENAME, paramList);
        }

        public DataTable GetPageNameByComponentName(string strComponentName, int intBookId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMCOMPONENTNAME, strComponentName);
            paramList.Add(EBookConstants.PARM_BOOK_ID, intBookId);
            return ExecuteAndFetch(EBookConstants.USP_BATCHIMPORT_GETPAGENAME, paramList);
        }

        public void UpdateChapterPublishDetails(string strUserName, string strRequestID, string strDocumentURL, string strUserEmail, string strPath, string strChapterID, string strPublishedDate, string strChapterName, string strPublishedChapterName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAM_USERNAME, strUserName);
            paramList.Add(EBookConstants.PARAM_REQUESTID, strRequestID);
            paramList.Add(EBookConstants.PARAM_DOCUMENTURL, strDocumentURL);
            paramList.Add(EBookConstants.PARAM_USEREMAIL, strUserEmail);
            paramList.Add(EBookConstants.PARAM_PATH, strPath);
            paramList.Add(EBookConstants.PARAM_CHAPTERID, strChapterID);
            paramList.Add(EBookConstants.PARAM_PUBLISHEDDATE, strPublishedDate);
            paramList.Add(EBookConstants.PARAM_CHAPTERNAME, strChapterName);
            paramList.Add(EBookConstants.PARAM_PUBLISHEDCHAPTERNAME, strPublishedChapterName);
            ExecuteAndFetch(EBookConstants.USP_EBOOKS_UPDATECHAPTERPUBLISHDETAILS, paramList);

        }

        public void AWRUpdateChapterPublishDetails(string strTitle, string strUserName, string strRequestID, string strDocumentURL, string strUserEmail, string strPath, string strChapterID, string strPublishedDate, string strChapterName, string strPublishedChapterName, string AwrId)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAM_TITLE, strTitle);
            paramList.Add(EBookConstants.PARAM_USERNAME, strUserName);
            paramList.Add(EBookConstants.PARAM_REQUESTID, strRequestID);
            paramList.Add(EBookConstants.PARAM_DOCUMENTURL, strDocumentURL);
            paramList.Add(EBookConstants.PARAM_USEREMAIL, strUserEmail);
            paramList.Add(EBookConstants.PARAM_PATH, strPath);
            paramList.Add(EBookConstants.PARAM_CHAPTERID, strChapterID);
            paramList.Add(EBookConstants.PARAM_PUBLISHEDDATE, strPublishedDate);
            paramList.Add(EBookConstants.PARAM_CHAPTERNAME, strChapterName);
            paramList.Add(EBookConstants.PARAM_PUBLISHEDCHAPTERNAME, strPublishedChapterName);
            paramList.Add(EBookConstants.AWRIDPARAM, AwrId);
            ExecuteAndFetch(EBookConstants.AWR_UPDATECHAPTERPUBLISHDETAILS, paramList);
        }


        public DataTable GetPublishedChapterNames(string requestID, string userName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAM_REQUESTID, requestID);
            paramList.Add(EBookConstants.PARAM_USERNAME, userName);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSGETPUBLISHEDCHAPTERNAME, paramList);
        }
        public DataTable GetH2SIdentifierValue(int intPageID, int intCopmPropID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.GETPAGEID, intPageID);
            paramList.Add(EBookConstants.INTCOMPPROPID, intCopmPropID);
            return ExecuteAndFetch(EBookConstants.USPGETIDENTIFIERVALUE, paramList);
        }

        public DataTable GetDescriptionOfWellStatusCode(string strCode, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvStatusCode", strCode);
            return ExecuteAndFetch(strProc, paramList);
        }

        #endregion RELEASE 6.0

        #region Release 7.0
        public DataTable GetChapterIDByChapterName(string strChapterTitle, string strBookId, string strproc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@Book_ID", strBookId);
            paramList.Add("@Chapter_Title", strChapterTitle);
            return ExecuteAndFetch(strproc, paramList);
        }

        public DataTable GetChapterDetailsByAssetValue(string strAssetValue, string username, string strproc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AssetValue", strAssetValue);
            paramList.Add("@User_Name", username);
            return ExecuteAndFetch(strproc, paramList);
        }

        public DataTable GetUserAccess(string username, string strproc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.PARAMUSERID, username);
            return ExecuteAndFetch(strproc, paramList);
        }

        #endregion RELEASE 7.0

        //Release 7.0
        public DataTable GetDefaultBatchImportPath(string strBookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARM_BOOK_ID, strBookID);
            return ExecuteAndFetch(EBookConstants.GetDefaultBatchImportPath, paramList);
        }
        public void UpdateBatchImportDefaultPath(string strOldDefaultPath, string strNewDefaultPath)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvOldPath", strOldDefaultPath);
            paramList.Add(DreamConstants.CHVNEWPATHPARAM, strNewDefaultPath);
            ExecuteAndFetch(EBookConstants.UpdateBatchImportDefaultPath, paramList);
        }
        public DataTable GetDWBBooksForImportPath(string strUserName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(CommonConstants.PARAMUSERNAME, strUserName);
            return ExecuteAndFetch(EBookConstants.GetDWBBooksForImportPath, paramList);
        }
        public DataTable UpdateBatchImportNewPath()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetch(EBookConstants.UpdateBatchImportNewPath, paramList);
        }
        public void UploadBatchUpdatePathXMLToTable(XmlDocument xmlDoc, string bookID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@xmlDocument", xmlDoc.InnerXml.ToString());
            paramList.Add(EBookConstants.PARM_BOOK_ID, bookID);
            ExecuteAndFetch(EBookConstants.UploadBatchUpdatePathXMLToTable, paramList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="storProc"></param>
        /// <returns></returns>
        public DataSet GetTreeViewDataForPublish(int bookId, string storProc, string UserName, string strPostBackControlId, string strSelectedView, string strFilter)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            Dictionary<string, object> paramListview = new Dictionary<string, object>();
            Dictionary<string, object> dctSavedFilterParameters = new Dictionary<string, object>();
            paramListview.Add(EBookConstants.PARAMINTBOOKID, bookId);
            paramListview.Add(CommonConstants.PARAMUSERNAME, UserName);
            paramListview.Add(EBookConstants.PARM_VIEW_NAME, strSelectedView.Trim());
            DataTable dtSavedView = null;

            dtSavedView = ExecuteAndFetch(EBookConstants.USPEBOOKSGETSAVEDVIEWS, paramListview);

            if ((dtSavedView.Rows.Count > 0) && (string.IsNullOrEmpty(strPostBackControlId) || !EBookConstants.BUTTONS.Contains(CommonConstants.PIPESYMBOL + strPostBackControlId)))
            {
                string strDiscipline = dtSavedView.Columns[0].ColumnName;
                string strPageName = dtSavedView.Columns[1].ColumnName;
                string strChpaterTitle = dtSavedView.Columns[2].ColumnName;
                dctSavedFilterParameters.Add(EBookConstants.PARAMINTBOOKID, bookId);
                dctSavedFilterParameters.Add(strDiscipline, dtSavedView.Rows[0][strDiscipline].ToString());
                dctSavedFilterParameters.Add(strPageName, dtSavedView.Rows[0][strPageName].ToString());
                dctSavedFilterParameters.Add(strChpaterTitle, dtSavedView.Rows[0][strChpaterTitle].ToString());
                return ExecuteAndFetchAsDataSet(storProc, dctSavedFilterParameters);
            }
            else
            {

                paramList.Add(EBookConstants.PARAMINTBOOKID, bookId);
                if (strFilter.Equals("yes"))
                {
                    if (HttpContext.Current.Session["SqlFilterParameters"] != null)
                    {
                        Dictionary<string, string> dctFilterParameters = (Dictionary<string, string>)HttpContext.Current.Session["SqlFilterParameters"];
                        foreach (KeyValuePair<string, string> dict in dctFilterParameters)
                            paramList.Add(dict.Key, dict.Value);
                    }
                }
                return ExecuteAndFetchAsDataSet(storProc, paramList);
            }
        }

        /// <summary>
        /// Checks if customize view is enabled for a component
        /// </summary>
        /// <param name="componentID"></param>
        /// <returns></returns>
        public bool CheckCustomizeViewEnabled(int componentID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@componentID", componentID);
            return (bool)ExecuteScalar("USP_eBooks_CheckCustomizeViewEnabled", paramList);
        }
        public DataSet GetChartPropertiesforPlot(string strComponentTitle, string AssetType, string storedProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@ComponentName ", strComponentTitle);
            paramList.Add(SEDConstants.ASSETTYPE, AssetType);
            return ExecuteAndFetchAsDataSet(storedProc, paramList);
        }
        public DataTable GetAssetValueForH2S(string strAssetType, string strBookID, string strAddedAssetName)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvAssetType", strAssetType);
            paramList.Add("@intBookId", strBookID);
            paramList.Add("@chapterTitle", strAddedAssetName);
            return ExecuteAndFetch("UspeBooksGetAssetsValueOfBook", paramList);
        }
        public DataTable GetBookPages(string BookPagesIDs)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@chvBookPagesIDs", BookPagesIDs);
            return ExecuteAndFetch("USPeBooksGetBookPages", paramList);
        }
        //Release 6.0.4 - 140648 - Start 
        public DataTable RemoveTeamplate(string strUserID, string pageType, int ID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ValuePARAM, ID);
            paramList.Add("@chvOperationType", pageType);
            paramList.Add("@user", strUserID);
            return ExecuteAndFetch(EBookConstants.SPACTIVEARCHIVEREMOVETEMPLATE, paramList);
        }
        public DataSet GetBookFilterValuesEmpty(string storProc, string disciplines, string pageNames, string chapterTitles, DataTable dtRecords)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();

            paramList.Add(EBookConstants.PARAMCHVSELECTEDDISCIPLINE, disciplines);
            paramList.Add(EBookConstants.PARAMCHVSELECTEDPAGENAME, pageNames);
            paramList.Add(EBookConstants.PARAMCHVSELECTEDCHAPTERTITLE, chapterTitles);
            paramList.Add(EBookConstants.PARAMDTRECORDS, dtRecords);
            return ExecuteAndFetchAsDataSet(storProc, paramList);
        }
        //Release 6.0.4 - 140648 - End 
        public void SaveEmergencyResponseXML(int BookID, string BookName, XmlDocument xml)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", BookID);
            paramList.Add("@varBookName", BookName);
            paramList.Add("@XmlTitle", xml.InnerXml);
            ExecuteAndFetch(EBookConstants.USPEBOOKSSAVEEMERGENCYRESPONSEXML, paramList);

        }
        //Release 7.0.1 -Data Availability - Start
        public DataTable GetDataAvailabilityInfo(int strBookID, string strPageList, string strChapterList)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intBookId", strBookID);
            paramList.Add("@varPageList", strPageList);
            paramList.Add("@varChapterList", strChapterList);
            return ExecuteAndFetch(EBookConstants.USPEBOOKSGETDATAAVAILABILITYINFO, paramList);
        }
        //Release 7.0.1 -Data Availability - End

        public DataTable GetConfigDetailsRW(string strViewName, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ViewnamePARAM, strViewName);
            return ExecuteAndFetch(strProc, paramList);
        }

        // Start - Issue 183970:5072 - Problem with Book Viewer page selection option
        public DataTable GetDisplayNameForUser(string strUserName, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@stringUserId", strUserName);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetTeamMemberDetails(int teamId, string strUserName, string strUserDetails, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@teamId", teamId);
            paramList.Add("@strUserName", strUserName);
            paramList.Add("@strUserDetails", strUserDetails);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetBookDetails(int strBookID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@stringUserId", strBookID);
            return ExecuteAndFetch(strProc, paramList);

        }
        // End - Issue 183970:5072 - Problem with Book Viewer page selection option

        //eBooks Usability Enhancements(2.5.1 Well summary & Well integrity pages full configurable) ----- Starts
        //EBOOKS -  (AWR – Well Summary) <start>
        public DataTable GetColConfigDetailsRW(string strViewName, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.ViewnamePARAM, strViewName);
            return ExecuteAndFetch(strProc, paramList);
        }
        //EBOOKS -  (AWR – Well Summary) <end>


        internal DataTable GetComponentViewMode(int ComponentID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.COMPONENTID, ComponentID);
            return ExecuteAndFetch(strProc, paramList);
        }
        //eBooks Usability Enhancements(2.5.1 Well summary & Well integrity pages full configurable) ----- Ends
        //Code change May 13 ----- Starts
        internal DataTable GetReviewedOn(string strViewName, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.WS_PARAMNAME, strViewName);
            return ExecuteAndFetch(strProc, paramList);
        }
        //Code change May 13 ----- Ends

        internal DataTable GetConcatenatedValue(int intComponnetID, string strProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@componentID", intComponnetID);
            return ExecuteAndFetch(strProc, paramList);
        }

        public DataTable GetAwrDataAvailabilityInfo(int Awr_Id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@intAwrId", Awr_Id);
            return ExecuteAndFetch(EBookConstants.GETAWRDATAAVAILABILITYINFO, paramList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="AWR id"></param>
        /// <param name="unitSettingsForBook"></param>
        /// <param name="userName"></param>
        /// <param name="storProc"></param>
        public void SaveUnitPreferenceForAWR(string bookId, string unitSettingsForBook, string userName, string storProc)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AWRId", bookId);
            paramList.Add("@Type", unitSettingsForBook);
            paramList.Add("@CreatedBy", userName);
            ExecuteAndFetch(storProc, paramList);
        }

        //AWR PDF Generation
        public DataTable GetAWRPrintPageCount(int awr_id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AWRId", awr_id);
            return ExecuteAndFetch(EBookConstants.GETAWRPAGECOUNT, paramList);
        }
        //AWR PDF Generation

        public DataTable GetAWRRev_Sendmail(int awr_id)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add("@AWRId", awr_id);
            return ExecuteAndFetch("AWR_GET_AWRRev_MAIL", paramList);
        }
        /// <summary>
        /// to remove external members AWR
        /// 
        /// </summary>
        /// <param name="BookTeamID"></param>
        public void RemoveExternalMembersAWR(int awrID)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMAWRID, awrID);
            ExecuteAndFetch(EBookConstants.USPEAWRSREMOVEEXTERNALREVIEWMEMBERS, paramList);

        }


        /// <summary>
        /// Gets all filters data for event group, type, date & owner dropdown list
        /// </summary>
        public DataSet GetAWREventFilters()
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            return ExecuteAndFetchAsDataSet(EBookConstants.SPGETEVENTFILTERS, paramList);
        }

        /// <summary>
        /// Gets event types for the given event groups
        /// </summary>
        public DataTable GetEventTypes(string eventGroups)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMEVENTGROUPS, eventGroups);
            return ExecuteAndFetch(EBookConstants.SPGETEVENTTYPESBYGROUP, paramList);
        }

        #region Migration Of Type 1 WellHistory to Type 5
        public DataSet GetRecordsForWellHistory(string strAssetType, string strSelectedIdentifier, bool blIsParentWellboreSelected, string strSearchType)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(SEDConstants.PARAMASSETIDENTIFIER, strSelectedIdentifier);
            paramList.Add(SEDConstants.PARAMEXPORTASSETTYPE, strAssetType);
            paramList.Add(SEDConstants.PARAMISPARENTWELLBORECHECKED, blIsParentWellboreSelected);
            paramList.Add(SEDConstants.PARAMREPORTNAME, strSearchType);
            return ExecuteAndFetchAsDataSet(SEDConstants.USPSEDGETEVENTSANDACTIONSDETAILS, paramList);
        }
        # endregion

        internal DataTable CurerentGetTeamMembers(string awrID, string bookIds)
        {
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            paramList.Add(EBookConstants.PARAMAWRID, awrID);
            paramList.Add(EBookConstants.PARM_BOOK_ID, bookIds);
            return ExecuteAndFetch(EBookConstants.SPGETREVIEWTEAMMEMBERBYBOOKID, paramList);
        }
    }
}

