using System;using Shell.WRFM.Global.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Shell.WRFM.Global.Web.DataAccess;
using Shell.WRFM.Global.Web.Error;
using Shell.WRFM.Global.Web.Log;
using Shell.WRFM.Global.Web.Service;
using Shell.WRFM.Global.Web.SingleUI.Infra;
using Shell.WRFM.Global.Business.SharedService;
using Shell.WRFM.Global.Business.WebService;
using Shell.WRFM.Global.Data;
using Shell.WRFM.Global.Data.Entity;
using System.Xml.XPath;
using Shell.WRFM.Global.Business.TaskSchedulerProcess;


namespace Shell.WRFM.Global.Business
{
    public interface ISharedModule
    {

    }
    public sealed partial class WRFMCommon : IDisposable
    {
        public partial class CommonUtility { }
        public partial class DataUtility { }

        

        /// <summary>
        /// Form related common functions
        /// </summary>
        public static class Form
        {
            internal delegate object SessionFillDelegate(string userId);
            //internal delegate object ApplicationFillDelegate();
            public delegate object ApplicationFillDelegate(object param);
            internal static object GetApplicationVariable(string appVariableName)
            {
                object objectValue = null;
                if (HttpContext.Current.Application != null && HttpContext.Current.Application[appVariableName] != null)
                {
                    objectValue = HttpContext.Current.Application[appVariableName];
                }
                return objectValue;
            }
            public static void ClearApplicationVariable(string appVariableName)
            {
                HttpContext.Current.Application.Remove(appVariableName);
            }
            public static void ClearApplicationVariables()
            {
                HttpContext.Current.Application.RemoveAll();
            }
            public static object GetApplicationVariable(string appVariableName, ApplicationFillDelegate appFill, object param)
            {
                object objectValue = GetApplicationVariable(appVariableName);
                if (objectValue == null)
                {
                    objectValue = appFill(param);
                    SetApplicationVariable(appVariableName, objectValue);
                }
                return objectValue;
            }
            public static void SetApplicationVariable(string appVariableName, object appObjectValue)
            {

                //#region Arth SEIC Livelink,Recall and EP Catalog removal
                //if (appVariableName.ToLower().Contains("leftnavigationdata"))
                //{
                //    LeftNavigationData objNav = (LeftNavigationData)appObjectValue;
                //    LeftNavigationNode objNode= objNav.RootNode;
                //    string strUserName = string.Empty;
                //    strUserName = WRFMCommon.DataUtility.GetReorderUserName();
                //    if (!string.IsNullOrEmpty(strUserName))
                //    {
                //        DataTable dtUserType = WRFMCommon.DataUtility.GetUserType(strUserName);
                //        if (dtUserType != null && dtUserType.Rows.Count > 0)
                //        {
                //            string strUserType = dtUserType.Rows[0][DreamConstants.USERTYPE].ToString();
                //            string strRegion = WRFMCommon.Instance.ConfiguationData.PortalConfigurations["Region"];
                //            if (strUserType != "DEFAULT" && string.Equals(strRegion.ToLowerInvariant(), "MAJNOON".ToLowerInvariant()) && strUserType == "SEIC")
                //            {
                //                foreach (SearchElement se in objNode.ContextSearch)
                //                {
                //                    if (se.Name != null)
                //                    {
                //                        if (se.Name.ToLower().Equals("documents") || se.Name.ToLower().Equals("petrophysical") || se.Name.ToLower().Contains("survey"))
                //                        {
                //                            List<SearchElement> delList = new List<SearchElement>();
                //                            List<SearchElement> lstchild = se.ChildElements;
                //                            foreach (SearchElement sChild in lstchild)
                //                            {
                //                                if (sChild.Name != null)
                //                                {
                //                                    if (sChild.Name.ToLower().Equals("well status diagrams cached/dynamic") || sChild.Name.ToLower().Equals("curves") || sChild.Name.ToLower().Equals("cldb/ofdb links") || sChild.Name.ToLower().Contains("recall") || sChild.Name.ToLower().Contains("logs") || sChild.Name.ToLower().Contains("ep catalog") || sChild.Name.ToLower().Contains("well status diagram"))
                //                                    {
                //                                        delList.Add(sChild);

                //                                    }
                //                                }
                //                            }
                //                            //Deletion Code
                //                            foreach (SearchElement temp in delList)
                //                                lstchild.Remove(temp);

                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }

                //}

                //#endregion
                if (HttpContext.Current.Application != null)
                {
                    HttpContext.Current.Application.Add(appVariableName, appObjectValue);
                }
            }
            /// <summary>
            /// Gets the session variable.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            public static object GetSessionVariable(Page pageReference, string sessionVariableName)
            {
                object objectValue = null;
                if (pageReference.Session != null && pageReference.Session[sessionVariableName] != null)
                {
                    objectValue = pageReference.Session[sessionVariableName];
                }
                return objectValue;
            }

            /// <summary>
            /// Gets the session variable.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <param name="name">The name.</param>
            /// <returns></returns>
            internal static object GetSessionVariable(string sessionVariableName, SessionFillDelegate sessionFill)
            {
                ShellSession session = (ShellSession)HttpContext.Current.Session[SEDConstants.SHELLSESSION];
                return GetSessionVariable(sessionVariableName, sessionFill, session.UserName);
            }

            internal static object GetSessionVariable(string sessionVariableName, SessionFillDelegate sessionFill, string userName)
            {
                object objectValue = GetSessionVariable((Page)HttpContext.Current.Handler, sessionVariableName);
                if (objectValue == null)
                {
                    objectValue = sessionFill(userName);
                    SetSessionVariable((Page)HttpContext.Current.Handler, sessionVariableName, objectValue);
                }
                return objectValue;
            }

            /// <summary>
            /// Sets the session variable.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <param name="name">The name.</param>
            /// <param name=CommonConstants.VALUE>The value.</param>
            public static void SetSessionVariable(Page pageReference, string sessionVariableName, object sessionObjectValue)
            {
                if (pageReference.Session != null)
                {
                    pageReference.Session.Add(sessionVariableName, sessionObjectValue);
                }
            }

            /// <summary>
            /// Sets the session variable.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <param name="name">The name.</param>
            /// <param name=CommonConstants.VALUE>The value.</param>
            public static void RemoveSessionVariable(Page pageReference, string sessionVariableName)
            {
                if (pageReference.Session != null)
                {
                    pageReference.Session.Remove(sessionVariableName);
                }
            }

            /// <summary>
            /// Gets the form control value.
            /// </summary>
            /// <param name="controlId">The control id.</param>
            /// <returns></returns>
            public static string GetFormControlValue(string controlId)
            {
                #region Decaleration
                string strControlId = string.Empty;
                string strValue = string.Empty;
                string strFirstPart = string.Empty;
                string strSecondPart = string.Empty;
                string strIds = string.Empty;
                string[] arrControlIds = null;
                #endregion

                arrControlIds = HttpContext.Current.Request.Form.AllKeys;

                if (arrControlIds.Length > 0)
                {
                    strIds = string.Join(CommonConstants.PIPESYMBOL, arrControlIds);
                    if (arrControlIds.Length == 1)
                    {
                        strControlId = strIds;
                    }
                    else if (strIds.IndexOf(controlId) != -1)
                    {
                        strFirstPart = strIds.Substring(0, strIds.IndexOf(controlId));
                        strSecondPart = strIds.Substring(strIds.IndexOf(controlId));
                        strControlId = strFirstPart.Substring(strFirstPart.LastIndexOf(CommonConstants.PIPESYMBOL) + 1) + (strSecondPart.Contains(CommonConstants.PIPESYMBOL) ? strSecondPart.Substring(0, strSecondPart.IndexOf(CommonConstants.PIPESYMBOL)) : strSecondPart);
                    }
                    if (HttpContext.Current.Request.Form[strControlId] != null)
                    {
                        strValue = HttpContext.Current.Request.Form[strControlId];
                    }
                }
                else
                {
                    //Http Posting code added 23/02/2015
                    Page page = HttpContext.Current.Handler as Page;
                    object objQueryString = GetSessionVariable(page, controlId);
                    if (objQueryString != null)
                    {
                        strValue = (string)objQueryString;
                        //RemoveSessionVariable(page, controlId);
                    }
                }
                return strValue;
            }

            /// <summary>
            /// Adds the unit attributes.
            /// </summary>
            /// <param name="unitControl">The unit control.</param>
            /// <param name="objDataRow">The obj data row.</param>
            /// <param name="onClickEventName">Name of the on click event.</param>
            public static void AddUnitAttributes(WebControl unitControl, DataRow objDataRow, string onClickEventName)
            {
                unitControl.Attributes.Add("CVA", (string)objDataRow["Conversion_x0020_Value_x0020_A"]);
                unitControl.Attributes.Add("CVB", (string)objDataRow["Conversion_x0020_Value_x0020_B"]);
                unitControl.Attributes.Add("CVC", (string)objDataRow["Conversion_x0020_Value_x0020_C"]);
                unitControl.Attributes.Add("unitlabel", (string)objDataRow["Display_x0020_Label"]);
                unitControl.Attributes.Add("OnClick", onClickEventName);
            }

            /// <summary>
            /// Populates the asset list control.
            /// </summary>
            /// <param name="listControl">The list control.</param>
            /// <param name="arrText">The arr text.</param>
            /// <param name="arrValue">The arr value.</param>
            /// <param name="setFirstItemSeleted">This parameter checks whether 1st item in listBox control need to be selected or not
            /// if setFirstItemSeleted = True then select the first item in listbox, if false don't select anything in listbox</param>
            public static void PopulateAssetListControl(ListControl listControl, string[] arrText, string[] arrValue)
            {
                /* AlphaNumeric sorting implementation */
                //AlphaNumericSorting objAlphaNumericSorting = new AlphaNumericSorting();
                Dictionary<string, string> objDicSort = new Dictionary<string, string>();
                try
                {
                    if (listControl != null && arrText != null)
                    {
                        Array.Sort(arrText, arrValue, StringComparer.Ordinal);//For Ascii Value Sorting
                        //Array.Sort(arrText, arrValue, objAlphaNumericSorting); //Used for alphaNumeric sorting
                        for (int intIndex = 0; intIndex < arrText.Length; intIndex++)
                        {
                            objDicSort.Add(arrText[intIndex], arrValue[intIndex]);
                        }
                        listControl.DataSource = objDicSort;
                        listControl.DataValueField = CommonConstants.DICTIONARYVALUE;
                        listControl.DataTextField = CommonConstants.DICTIONARYKEY;
                        listControl.DataBind();
                    }
                    //Code added for Asset dropdown width
                    listControl.Style.Add(CommonConstants.WIDTH, CommonConstants.AUTO);
                }
                finally
                {
                    //if (objAlphaNumericSorting != null)
                    //    objAlphaNumericSorting = null;
                    if (objDicSort != null)
                        objDicSort = null;
                }
            }


            /// <summary>
            /// Populates the asset list control.
            /// </summary>
            /// <param name="listControl">The list control.</param>
            /// <param name="arrText">The arr text.</param>
            /// <param name="arrValue">The arr value.</param>
            /// <param name="setFirstItemSeleted">This parameter checks whether 1st item in listBox control need to be selected or not
            /// if setFirstItemSeleted = True then select the first item in listbox, if false don't select anything in listbox</param>
            public static void PopulateAssetListControl(ListControl listControl, string[] arrText, string[] arrValue, bool setFirstItemSeleted = true)
            {
                /* AlphaNumeric sorting implementation */
                //AlphaNumericSorting objAlphaNumericSorting = new AlphaNumericSorting();
                Dictionary<string, string> objDicSort = new Dictionary<string, string>();
                try
                {
                    if (listControl != null && arrText != null)
                    {
                        Array.Sort(arrText, arrValue, StringComparer.Ordinal);
                        //Array.Sort(arrText, arrValue, objAlphaNumericSorting); //Used for alphaNumeric sorting
                        for (int intIndex = 0; intIndex < arrText.Length; intIndex++)
                        {
                            objDicSort.Add(arrText[intIndex], arrValue[intIndex]);
                        }
                        listControl.DataSource = objDicSort;
                        listControl.DataValueField = CommonConstants.DICTIONARYVALUE;
                        listControl.DataTextField = CommonConstants.DICTIONARYKEY;
                        listControl.DataBind();

                        string strDefaultValue = arrValue[0];
                        if (setFirstItemSeleted)
                        {
                            if (listControl.Items.FindByValue(strDefaultValue) != null)
                                listControl.Items.FindByValue(strDefaultValue).Selected = true;
                        }
                    }
                    //Code added for Asset dropdown width
                    listControl.Style.Add(CommonConstants.WIDTH, CommonConstants.AUTO);
                }
                finally
                {
                    //if (objAlphaNumericSorting != null)
                    //    objAlphaNumericSorting = null;
                    if (objDicSort != null)
                        objDicSort = null;
                }
            }

            /// <summary>
            /// Populates asset dropdown for daily wells reporting
            /// </summary>
            /// <param name="listControl">The list control.</param>
            /// <param name="arrText">The arr text.</param>
            /// <param name="arrValue">The arr value.</param>

            public static void PopulateDWRAssetDropdown(ListControl listControl, string[] arrText, string[] arrValue)
            {
                //ASCII sorting implementation
                Dictionary<string, string> objDicSort = new Dictionary<string, string>();
                try
                {
                    //if (ReportType= edm)
                    if (listControl != null && arrText != null)
                    {
                        //code added for asset dropdown ascii sorting
                        Array.Sort(arrText, arrValue, StringComparer.Ordinal);

                        for (int intIndex = 0; intIndex < arrText.Length; intIndex++)
                        {
                            objDicSort.Add(arrText[intIndex], arrValue[intIndex]);
                        }
                        listControl.DataSource = objDicSort;
                        listControl.DataValueField = CommonConstants.DICTIONARYVALUE;
                        listControl.DataTextField = CommonConstants.DICTIONARYKEY;
                        listControl.DataBind();

                        string strDefaultValue = arrValue[0];
                        if (listControl.Items.FindByValue(strDefaultValue) != null)
                            listControl.Items.FindByValue(strDefaultValue).Selected = true;
                    }
                    //Code added for Asset dropdown width
                    listControl.Style.Add(CommonConstants.WIDTH, CommonConstants.AUTO);
                }

                finally
                {
                    if (objDicSort != null)
                        objDicSort = null;
                }
            }

            /// <summary>
            /// Populates the list control.
            /// </summary>
            /// <param name="control">The control.</param>
            /// <param name="dataSource">The data source.</param>
            public static void PopulateListControl(ListControl control, object dataSource)
            {
                if (dataSource != null)
                {
                    control.DataSource = dataSource;
                    control.DataTextField = CommonConstants.VALUE;
                    control.DataValueField = CommonConstants.VALUE;
                    control.DataBind();
                }
            }
            /// <summary>
            /// Adds the data DDL.
            /// </summary>
            /// <param name="lstcontrol">The lstcontrol.</param>
            public static void AddDataDDL(ListControl lstcontrol, DataTable dtListItems)
            {

                if ((dtListItems != null) && (dtListItems.Rows.Count > 0))
                {
                    lstcontrol.DataSource = dtListItems;
                    lstcontrol.DataTextField = dtListItems.Columns[0].ToString();
                    lstcontrol.DataValueField = dtListItems.Columns[1].ToString();
                    lstcontrol.DataBind();
                    lstcontrol.Items.Insert(0, CommonConstants.DEFAULTDROPDOWNTEXT);
                }
            }

            /// <summary>
            /// Ensures the panel fix.
            /// </summary>
            /// <param name=SEDConstants.TYPE>The type.</param>
            public static void EnsurePanelFix(Page page, Type type)
            {
                if (page.Form != null)
                {
                    string formOnSubmitAtt = page.Form.Attributes["onsubmit"];
                    if (formOnSubmitAtt == "return _spFormOnSubmitWrapper();")
                    {
                        page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
                    }
                }
                ScriptManager.RegisterStartupScript(page, type, "UpdatePanelFixup", "_spOriginalFormAction = document.forms[0].action; _spSuppressFormOnSubmitWrapper=true;", true);
            }

            /// <summary>
            /// Determines whether [is max record exceeds] [the specified result document].
            /// </summary>
            /// <param name="resultDocument">The result document.</param>
            /// <returns>
            /// 	<c>true</c> if [is max record exceeds] [the specified result document]; otherwise, <c>false</c>.
            /// </returns>
            public static bool IsMaxRecordExceeds(XmlDocument resultDocument, bool isClicked)
            {
                bool blnIsMaxRecordExist = false;
                string strValue = string.Empty;
                try
                {
                    if (!isClicked)
                    {
                        if (resultDocument != null)
                        {
                            XmlNodeList xmlNodeList = resultDocument.SelectNodes(CommonConstants.MAXRECORDSXPATH);
                            //Loops through the nodes in XmlDocument.
                            foreach (XmlNode xmlnodeValue in xmlNodeList)
                            {
                                if (xmlnodeValue.Attributes.GetNamedItem(CommonConstants.MAXRECORDSATTRIB) != null)
                                {
                                    strValue = xmlnodeValue.Attributes.GetNamedItem(CommonConstants.MAXRECORDSATTRIB).Value.ToString();
                                    blnIsMaxRecordExist = Convert.ToBoolean(strValue);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                return blnIsMaxRecordExist;
            }

            /// <summary>
            /// Finds the control recursive.
            /// </summary>
            /// <param name="Root">The root control.</param>
            /// <param name="clientId">The client id.</param>
            /// <returns>Control</returns>
            public static Control FindControlRecursive(Control rootControl, string clientId)
            {
                if (string.Equals(rootControl.ClientID, clientId))
                    return rootControl;
                #region "looping the controls"
                foreach (Control objControl in rootControl.Controls)
                {
                    Control ctlFound = FindControlRecursive(objControl, clientId);
                    if (ctlFound != null)
                        return ctlFound;
                }
                #endregion
                return null;
            }

            /// <summary>
            /// Finds the control.
            /// </summary>
            /// <param name="controlName">Name of the control.</param>
            /// <param name="currentPage">The current page.</param>
            /// <returns>Control</returns>
            public static Control FindControl(string controlName, Page currentPage)
            {
                #region "Declaring local variables"
                Control ctlBuddy;
                string strWebPartControl;
                #endregion
                try
                {
                    #region "setting buddy control"
                    if (currentPage == null || controlName == null)
                        return null;
                    ctlBuddy = currentPage.FindControl(controlName);
                    if (ctlBuddy == null)
                    {
                        strWebPartControl = GetControlUniqueID(controlName, currentPage.Controls);
                        if (strWebPartControl != null)
                            ctlBuddy = currentPage.FindControl(strWebPartControl);
                        else
                            ctlBuddy = currentPage.FindControl(controlName);
                    }
                    #endregion
                }
                catch
                {
                    throw;
                }
                return ctlBuddy;
            }

            /// <summary>
            /// Gets the control unique ID.
            /// </summary>
            /// <param name="controlID">The control ID.</param>
            /// <param name="controls">The controls.</param>
            /// <returns>Control Unique ID</returns>
            public static string GetControlUniqueID(string controlID, ControlCollection controls)
            {
                #region "Declaring Local Variables"
                Control control;
                string strUniqueID = null;
                #endregion
                try
                {
                    #region "looping the controls and getting the controls unique ID"
                    //Loop through the controls in control collection
                    for (int intIndex = 0; intIndex < controls.Count; ++intIndex)
                    {
                        control = controls[intIndex];
                        if (string.Equals(control.ID, controlID))
                        {
                            strUniqueID = control.UniqueID;
                            break;
                        }
                        if (control.Controls.Count > 0)
                        {
                            strUniqueID = GetControlUniqueID(controlID, control.Controls);
                            if (strUniqueID.Length > 0)
                                break;
                        }
                    }
                    #endregion
                }
                catch
                {
                    throw;
                }
                return strUniqueID;
            }

            /// <summary>
            /// Cleans the session variable.
            /// </summary>
            /// <param name="isRequired">if set to <c>true</c> [is required].</param>
            /// <param name="currentPage">The current page.</param>
            public static void CleanSessionVariable(bool isRequired, Page currentPage)
            {
                try
                {
                    if (isRequired && currentPage != null)
                    {
                        WRFMCommon.Form.SetSessionVariable(currentPage, SessionVariable.searchType.ToString(), null);
                        WRFMCommon.Form.SetSessionVariable(currentPage, SessionVariable.geometry.ToString(), null);
                        WRFMCommon.Form.SetSessionVariable(currentPage, SessionVariable.whereClause.ToString(), null);
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Gets the paging sorting params.
            /// </summary>
            /// <param name="paramsList">The params list.</param>
            /// <returns></returns>
            public static Hashtable GetPagingSortingParams(string paramsList)
            {
                Hashtable hstblParams = null;
                if ((!string.IsNullOrEmpty(paramsList)) && (paramsList.Contains("&")))
                {
                    hstblParams = new Hashtable();
                    string[] arrParams = paramsList.Split("&".ToCharArray());
                    foreach (string strParam in arrParams)
                    {
                        if (strParam.Contains("="))
                        {
                            string[] arrKeyValue = strParam.Split(new char[] { '=' }, 2);
                            hstblParams.Add(arrKeyValue[0].Trim(), arrKeyValue[1].Trim());
                        }
                    }
                }
                return hstblParams;
            }

            /// <summary>
            /// Renders the busy box.
            /// </summary>
            public static void RenderBusyBox(Page page)
            {
                ScriptManager objScriptManager = ScriptManager.GetCurrent(page);
                if (!objScriptManager.IsInAsyncPostBack)
                {
                    page.Response.BufferOutput = true;
                    page.Response.Write("<script language=\"javascript\" src=\"/_Layouts/DREAM/Javascript/CastleBusyBoxRel2_1.js\"></script>");
                    page.Response.Write("<iframe id=\"BusyBoxIFrame\" style=\"border:3px double #D2D2D2\" name=\"BusyBoxIFrame\" frameBorder=\"0\" scrolling=\"no\" ondrop=\"return false;\" src=\"/_layouts/dream/InitialBusybox.htm\"></iframe>");
                    page.Response.Write("<script>");
                    page.Response.Write("var busyBox = new BusyBox(\"BusyBoxIFrame\", \"busyBox\", 1, \"processing\", \".gif\", 125, 147, 207,\"/_layouts/dream/InitialBusybox.htm\");");
                    page.Response.Write("busyBox.Show();");
                    page.Response.Write("</script>");
                    page.Response.Flush();
                }
            }

            /// <summary>
            /// Registers the on load client script.
            /// </summary>
            /// <param name="script">The script.</param>
            public static void RegisterOnLoadClientScript(Page page, string script)
            {
                StringBuilder strScript = new StringBuilder();
                strScript.Append("<script language=\"javascript\">try{");
                strScript.Append(script);
                strScript.Append("}catch(Ex){}</script>");
                ScriptManager.RegisterStartupScript(page, page.GetType(), "OnLoadClientScript", strScript.ToString(), false);
            }

            /// <summary>
            /// Gets the post back control.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <returns>System.Web.UI.Control</returns>
            public static Control GetPostBackControl(Page page)
            {
                Control objControl = null;
                try
                {
                    string strCtrlname = page.Request.Params[CommonConstants.EVENTTARGET];
                    if (!string.IsNullOrEmpty(strCtrlname))
                    {
                        objControl = page.FindControl(strCtrlname);
                    }
                    // if __EVENTTARGET is null, the control is a button type and we need to 
                    // iterate over the form collection to find it
                    else
                    {
                        string strCtrl = String.Empty;
                        Control objCtrl = null;
                        foreach (string strCtl in page.Request.Form)
                        {
                            // handle ImageButton controls ...
                            if (strCtl.EndsWith(".x") || strCtl.EndsWith(".y"))
                            {
                                strCtrl = strCtl.Substring(0, strCtl.Length - 2);
                                objCtrl = page.FindControl(strCtrl);
                            }
                            else
                            {
                                objCtrl = page.FindControl(strCtl);
                            }
                            if (objCtrl is System.Web.UI.WebControls.Button ||
                                     objCtrl is System.Web.UI.WebControls.ImageButton)
                            {
                                objControl = objCtrl;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //left blank on purpose
                }
                return objControl;
            }

            /// <summary>
            /// Gets the post back control id.
            /// </summary>
            /// <param name="page">The page.</param>
            /// <returns></returns>
            public static string GetPostBackControlId(Page page)
            {
                string strControlId = string.Empty;
                Control objCtrl = GetPostBackControl(page);
                if (objCtrl != null)
                    strControlId = objCtrl.ID;
                return strControlId;
            }

            /// <summary>
            /// Determines whether [is post back] [the specified page].
            /// </summary>
            /// <param name="page">The page.</param>
            /// <returns>
            /// 	<c>true</c> if [is post back] [the specified page]; otherwise, <c>false</c>.
            /// </returns>
            public static bool IsPostBack(Page page)
            {
                string strCtrlname = page.Request.Params[CommonConstants.EVENTTARGET];
                bool blnIsPostBack = false;
                if ((!string.IsNullOrEmpty(strCtrlname)) || (GetPostBackControl(page) != null))
                {
                    blnIsPostBack = true;
                }
                return blnIsPostBack;
            }

            /// <summary>
            /// Initialises the user preference.
            /// </summary>
            /// <returns></returns>
            public static UserPreferences InitialiseUserPreference()
            {
                UserPreferences objUserPreferences = new UserPreferences();
                Page page = HttpContext.Current.Handler as Page;
                if (page != null)
                    objUserPreferences = (UserPreferences)GetSessionVariable(page, "UserPreferences");
                if (objUserPreferences == null)
                {
                    objUserPreferences = WRFMCommon.CommonUtility.GetUserPreferencesValue();
                    SetSessionVariable(page, "UserPreferences", objUserPreferences);
                }
                return objUserPreferences;
            }

            /// <summary>
            /// Gets the feet meter label.
            /// </summary>
            /// <param name="drUnit">The dr unit.</param>
            /// <returns></returns>
            public static string GetFeetMeterLabel(DataRow drUnit)
            {
                string strLabel = string.Empty;
                if (drUnit != null)
                {
                    strLabel = "(" + (string)drUnit["Display Label"] + ")";
                }
                return strLabel;
            }

            /// <summary>
            /// This methods takes xml, xPath & attributeName as input and returns xml node attribute value based on xPath separated by pipeline symbol
            /// This method also helps in fetching unique values from xml if required i.e. if isUniqueVaueRequired = true
            /// example: <attribute contextkey="" dbcolumnname="" display=CommonConstants.TRUE mapuse=CommonConstants.FALSE name="Reservoir Name" referencecolumn="" tablename="" title=""                             type="string" value="ANJ-RO"/>
            ///          xpath = attribute[@name='Reservoir Name'] , attributeName = value
            ///          Return Value is ANJ-RO
            /// </summary>
            /// <param name="xml"></param>
            /// <param name="xPath"></param>
            /// <param name="attributeName"></param>
            /// <param name="isUniqueVaueRequired"></param>
            /// <returns></returns>
            public static string GetRequiredAttributeValueFromXML(XmlDocument xml, string xPath, string attributeName, bool isUniqueVaueRequired = false)
            {
                string strValue = string.Empty;
                XPathNavigator xpathNavigator = xml.CreateNavigator();
                if (isUniqueVaueRequired)
                {
                    foreach (XPathNavigator nav in xpathNavigator.Select(xPath))
                    {
                        if (!strValue.Contains(nav.GetAttribute(attributeName, "")))
                            strValue = strValue + nav.GetAttribute(attributeName, "") + CommonConstants.PIPESYMBOL;
                    }
                }
                else
                {
                    foreach (XPathNavigator nav in xpathNavigator.Select(xPath))
                    {
                        strValue = strValue + nav.GetAttribute(attributeName, "") + CommonConstants.PIPESYMBOL;
                    }
                }
                return strValue;
            }

            /// <summary>
            /// To sort the user selected first column in excel sheet in ascending order along with maintaing customize view setting in export all    
            ///functionality
            /// </summary>
            /// <param name="ColumnNames"></param>
            /// <param name="ColumnDisplayStatus"></param>
            /// <param name="UserSetFirstColumn"></param>
            /// <returns></returns>
            public static void SetUserSelectedFirstColumnForExportAll(string ColumnNames, string ColumnDisplayStatus, HiddenField hidUserSetFirstColumn)
            {
                Dictionary<string, string> dicSetFirstColumn = new Dictionary<string, string>();
                string[] arrColumnNames, arrDisplayStatus;
                arrColumnNames = ColumnNames.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                arrDisplayStatus = ColumnDisplayStatus.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (arrColumnNames.Length == arrDisplayStatus.Length)
                {
                    for (int arrCount = 0; arrCount < arrColumnNames.Length; arrCount++)
                        dicSetFirstColumn.Add(arrColumnNames[arrCount], arrDisplayStatus[arrCount]);
                    foreach (KeyValuePair<string, string> items in dicSetFirstColumn)
                    {
                        if (items.Value == CommonConstants.TRUE)
                        {
                            hidUserSetFirstColumn.Value = items.Key;
                            break;
                        }
                    }
                }
            }
            //107475 - Rel 7.0 - Start
            public static void SetDefaultSelectAllConfiguration(ListBox ddlAssets, string strSearchType)
            {
                string strDefaultSelectAll = string.Empty;
                if (ddlAssets.Items.Count > 0)
                {
                    strDefaultSelectAll = WRFMCommon.DataUtility.GetDefaultAll(strSearchType);
                    if (strDefaultSelectAll.ToLower().Equals("yes"))
                    {
                        foreach (ListItem item in ddlAssets.Items)
                        {
                            item.Selected = true;
                        }
                    }
                    else
                    {
                        ddlAssets.Items[0].Selected = true;
                    }
                }
            }

            public static void CheckListBoxSelection(ListBox ddlAssets)
            {
                if (ddlAssets.Items.Count > 0 && ddlAssets.SelectedIndex == -1)
                {
                    ddlAssets.Items[0].Selected = true;
                }
            }

            public static void PopulateAssetArrayValues(ListBox ddlAssets, ref string[] arrAssetValues)
            {
                int index = 0;
                if (ddlAssets.Items.Count > 0)
                {
                    foreach (ListItem item in ddlAssets.Items)
                    {
                        if (item.Selected == true)
                        {
                            arrAssetValues[index] = item.Value;
                            index++;
                        }
                    }
                }
            }

            public static void PopulateAssetArrayText(ListBox ddlAssets, ref string[] arrAssetValues)
            {
                int index = 0;
                if (ddlAssets.Items.Count > 0)
                {
                    foreach (ListItem item in ddlAssets.Items)
                    {
                        if (item.Selected == true)
                        {
                            arrAssetValues[index] = item.Text;
                            index++;
                        }
                    }
                }
            }

            public static int GetSelectedItemsCount(ListBox ddlAssets)
            {
                int intCount = 0;
                if (ddlAssets.Items.Count > 0)
                {
                    foreach (ListItem item in ddlAssets.Items)
                    {
                        if (item.Selected == true)
                            intCount++;
                    }
                }
                return intCount;
            }
            //107475 - Rel 7.0 - End
        }
        /// <summary>
        /// Library for common functions for WRFM Units
        /// </summary>
        public static class Units
        {
            /// <summary>
            /// Gets the units list.
            /// </summary>
            /// <returns></returns>
            public static DataTable GetUnitsList()
            {
                DataTable objDataTable = null;

                try
                {
                    string strTblUnits = WRFMCommon.Form.GetFormControlValue("hidUnit");
                    if (!string.IsNullOrEmpty(strTblUnits))
                    {
                        string[] arrUnits = strTblUnits.Split('|');
                        if (arrUnits.Length > 0)
                        {
                            objDataTable = new DataTable();
                            objDataTable.TableName = CommonConstants.USERPREFERENCEUNITSLIST;
                            objDataTable.Columns.Add(CommonConstants.TITLE);
                            objDataTable.Columns.Add("UnitValue").ReadOnly = false;
                            for (int intUnit = 0; intUnit < arrUnits.Length; intUnit++)
                            {
                                DataRow objRow = objDataTable.NewRow();
                                objRow[CommonConstants.TITLE] = arrUnits[intUnit].Split(',')[0].ToString();
                                objRow["UnitValue"] = arrUnits[intUnit].Split(',')[1].ToString();
                                objDataTable.Rows.Add(objRow);
                            }
                        }
                    }
                    if (objDataTable == null)
                    {
                        DataTable objTable = null;
                        objDataTable = WRFMCommon.DataUtility.GetUnitsListFromSession();
                        DataTable dtListValues = null;
                        dtListValues = WRFMCommon.DataUtility.GetDefaultPreferenceFromSession();
                        if (dtListValues != null)
                        {
                            foreach (DataRow dtRow in dtListValues.Rows)
                            {
                                if (string.Equals(dtRow[WRFMData.List.DefaultPreferencesColumns.EntityType].ToString(), CommonConstants.UNITPREFERENCE))
                                {
                                    objTable = WRFMCommon.DataUtility.GetUnit_UnitGroup(dtRow[WRFMData.List.DefaultPreferencesColumns.DefaultValue].ToString());
                                    objTable.Columns[EBookConstants.UNIT].ColumnName = "UnitValue";
                                    objTable.Columns["UnitValue"].ReadOnly = false;
                                    objTable.TableName = CommonConstants.USERPREFERENCEUNITSLIST;
                                }
                            }
                        }

                        if (objDataTable == null || (objDataTable != null && objDataTable.Rows.Count <= 0))
                        {
                            objDataTable = objTable;
                        }
                        else
                        {
                            //Combine both tables
                            objDataTable.Merge(objTable);
                            Hashtable hTable = new Hashtable();
                            ArrayList duplicateList = new ArrayList();
                            //Add list of all the unique item value to hashtable, which stores combination of key, value pair.   
                            //And add duplicate item value in arraylist.    
                            foreach (DataRow drow in objDataTable.Rows)
                            {
                                if (hTable.Contains(drow[CommonConstants.TITLE]))
                                    duplicateList.Add(drow);
                                else
                                    hTable.Add(drow[CommonConstants.TITLE], string.Empty);
                            }     //Removing a list of duplicate items from datatable.   
                            foreach (DataRow dRow in duplicateList)
                            {
                                objDataTable.Rows.Remove(dRow);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (objDataTable != null)
                        objDataTable.Dispose();
                }

                return objDataTable;
            }
            /// <summary>
            /// Adds the attributes for unit.
            /// </summary>
            /// <param name="unitControl">The unit control.</param>
            /// <param name="objDataRow">The obj data row.</param>
            public static void AddAttributesForUnit(TableCell unitControl, DataRow objDataRow)
            {
                unitControl.Attributes.Add("CVA", Convert.ToString(objDataRow["Conversion Value A"]));
                unitControl.Attributes.Add("CVB", Convert.ToString(objDataRow["Conversion Value B"]));
                unitControl.Attributes.Add("CVC", Convert.ToString(objDataRow["Conversion Value C"]));
                unitControl.Attributes.Add("unitlabel", Convert.ToString(objDataRow[EBookConstants.UNIT]));
                unitControl.Attributes.Add("displaylbl", Convert.ToString(objDataRow["Display Label"]));
                if (!string.IsNullOrEmpty(objDataRow["No Of Decimal Places"].ToString()))
                {
                    unitControl.Attributes.Add("decimalPlaces", Convert.ToString(objDataRow["No Of Decimal Places"]));
                }
                else
                {
                    unitControl.Attributes.Add("decimalPlaces", "#0.00");
                }
            }

            /// <summary>
            /// Gets the unit conversion row.
            /// </summary>
            /// <param name="objDataTable">The obj data table.</param>
            /// <param name="filterExpression">The filter expression.</param>
            /// <returns></returns>
            public static DataRow GetUnitConversionRow(DataTable objDataTable, string filterExpression)
            {
                DataRow[] arrDR = null;
                if (objDataTable != null)
                {
                    arrDR = objDataTable.Select(filterExpression);
                }
                if (arrDR != null && arrDR.Length > 0)
                    return arrDR[0];
                else
                    return null;
            }

            /// <summary>
            /// Gets the units list label.
            /// </summary>
            /// <returns></returns>
            public static DataTable GetUnitsListLabel()
            {
                DataTable objDataTable = null;
                DataTable objTable = null;
                try
                {
                    objDataTable = WRFMCommon.DataUtility.GetUnitsListFromSession();
                    DataTable dtListValues = null;
                    dtListValues = WRFMCommon.DataUtility.GetDefaultPreferenceFromSession();
                    if (dtListValues != null)
                    {
                        foreach (DataRow dtRow in dtListValues.Rows)
                        {
                            if (string.Equals(dtRow[WRFMData.List.DefaultPreferencesColumns.EntityType].ToString(), CommonConstants.UNITPREFERENCE))
                            {
                                objTable = WRFMCommon.DataUtility.GetUnit_UnitGroup(dtRow[WRFMData.List.DefaultPreferencesColumns.DefaultValue].ToString());
                                objTable.Columns[EBookConstants.UNIT].ColumnName = "UnitValue";
                                objTable.TableName = CommonConstants.USERPREFERENCEUNITSLIST;
                            }
                        }
                    }

                    if (objDataTable == null || (objDataTable != null && objDataTable.Rows.Count <= 0))
                    {
                        objDataTable = objTable;
                    }
                    else
                    {
                        //Combine both tables
                        objDataTable.Merge(objTable);
                        Hashtable hTable = new Hashtable();
                        ArrayList duplicateList = new ArrayList();
                        //Add list of all the unique item value to hashtable, which stores combination of key, value pair.   
                        //And add duplicate item value in arraylist.    
                        foreach (DataRow drow in objDataTable.Rows)
                        {
                            if (hTable.Contains(drow[CommonConstants.TITLE]))
                                duplicateList.Add(drow);
                            else
                                hTable.Add(drow[CommonConstants.TITLE], string.Empty);
                        }     //Removing a list of duplicate items from datatable.   
                        foreach (DataRow dRow in duplicateList)
                        {
                            objDataTable.Rows.Remove(dRow);
                        }
                    }

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (objDataTable != null)
                        objDataTable.Dispose();
                }

                return objDataTable;
            }


        }
        private static volatile WRFMCommon instance;



        private static object syncRoot = new Object();

        private WRFMCommon() { }
        private WRFMCommon(DBManager manager) { _dataManager = manager; }

        public static WRFMCommon Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WRFMCommon();
                    }
                }

                return instance;
            }
        }
        public static WRFMCommon CreateInstance(DBManager manager)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new WRFMCommon(manager);
                }
            }

            return instance;
        }
        LogManager _logManager;
        public LogManager LogManager
        {
            get
            {
                if (_logManager == null)
                {
                    LogLevel level = LogLevel.Info;
                    if (!Enum.TryParse<LogLevel>(ConfiguationData.PortalConfigurations["LogLevel"], out level))
                        level = LogLevel.Info;
                    _logManager = new LogManager(level, ConfiguationData.PortalConfigurations["LogPath"]);
                }
                return _logManager;
            }
        }
        Log _log;
        public Log Logger
        {
            get
            {
                if (_log == null)
                {
                    bool allow = false;
                    if (Boolean.TryParse(ConfiguationData.PortalConfigurations["AllowLogging"], out allow))
                        LogManager.AllowLogging = allow;
                    LogManager.Start();
                    _log = new Log(LogManager);
                }
                return _log;
            }
        }

        IErrorHandler _err;
        public IErrorHandler Error
        {
            get
            {
                if (_err == null)
                {
                    string errPage = ConfiguationData.PortalConfigurations["ErrorPage"];
                    _err = new Error.ErrorHandler(errPage, Logger);
                }
                return _err;
            }
        }

        ServiceManagerBase _wsManager;
        public ServiceManagerBase WebServiceManager
        {
            get
            {
                if (_wsManager == null)
                {
                    _wsManager = new WebServiceManager();
                }
                return _wsManager;
            }
        }

        ServiceManagerBase _sharedModule;
        public ServiceManagerBase SharedModuleManager
        {
            get
            {
                if (_sharedModule == null)
                {
                    _sharedModule = new ServiceManager<ISharedModule>();
                }
                return _sharedModule;
            }
        }

        DBManager _dataManager;
        public DBManager DataManager
        {
            get
            {
                 if (_dataManager == null)
                  {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["provider"]))
                    throw new WRFMBusinessException("Required parameter 'provider' is missing in configuration file.");

                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectionString"]))
                    throw new WRFMBusinessException("Required parameter 'ConnectionString' is missing in configuration file.");

                string providerType = ConfigurationManager.AppSettings["provider"].ToString();
                string strConnection = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                _dataManager = new DBManager(providerType, strConnection, _log);
                return _dataManager;
                  }
                 else
                 {
                     DBManager manager = (DBManager)_dataManager.Clone();
                     if (manager.DBLogger == null)
                         manager.DBLogger = _log;
                     return manager;
                 }
            }
        }

        public DBManager DataManagerForOracle
        {
            get
            {
                _dataManager = null;

                if (_dataManager == null)
                {
                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["providerOracle"]))
                        throw new WRFMBusinessException("Required parameter 'provider' is missing in configuration file.");

                    if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectionStringOracle"]))
                        throw new WRFMBusinessException("Required parameter 'ConnectionString' is missing in configuration file.");

                    string providerType = ConfigurationManager.AppSettings["providerOracle"].ToString();
                    string strConnection = ConfigurationManager.AppSettings["ConnectionStringOracle"].ToString();
                    _dataManager = new DBManager(providerType, strConnection, _log);
                    return _dataManager;
                }
                else
                {
                    DBManager manager = (DBManager)_dataManager.Clone();
                    if (manager.DBLogger == null)
                        manager.DBLogger = _log;
                    return manager;
                }
            }
        }


        ControlsData _cData;
        public ControlsData ControlsData
        {
            get
            {
                if (_cData == null)
                {
                    _cData = new ControlsData(DataManager, WRFMCommon.Instance.Logger);
                }
                return _cData;
            }
        }

        ConfigurationData _configData;
        public ConfigurationData ConfiguationData
        {
            get
            {
                if (_configData == null)
                {
                    _configData = new ConfigurationData(DataManager);
                }
                return _configData;
            }
        }
        #region 283310 : SEIC Home Page Branding Changes
        //public string UserType;
        //public string UserRegion;
        

        private string _userType=null;
        public string UserType
        {
            get
            {
                if (_userType == null)
                {
                    string  strUserName = GetReorderUserName();   
                    if (!string.IsNullOrEmpty(strUserName))
                    {
                            DataTable dtUserType = WRFMCommon.DataUtility.GetUserType(strUserName);
                            if (dtUserType != null && dtUserType.Rows.Count > 0)
                            {
                                _userType = dtUserType.Rows[0]["UserType"].ToString();
                            }
                            
                    }
                }
                return _userType;
             }
            set {
                this._userType = value;
            }
            
                
        }

        private string _UserRegion;

        public string UserRegion
        {
            get
            {
                _UserRegion = WRFMCommon.Instance.ConfiguationData.PortalConfigurations["Region"];
                return _UserRegion;

            }
            set { 
            this._UserRegion=value;
            }
        }

        public string GetReorderUserName()
        {
            string strLoggedinUserName = string.Empty;
            //this will set the current logged in user name.
            if (HttpContext.Current != null)
            {
                strLoggedinUserName = HttpContext.Current.User.Identity.Name;
            }
            string[] arrUserName = new string[3];
            strLoggedinUserName = strLoggedinUserName.Replace("\\", "|");
            arrUserName = strLoggedinUserName.Split('|');
            return arrUserName[arrUserName.Length - 1];
        }

        #endregion

        WRFMData _wrfmData;
        public WRFMData WRFMData
        {
            get
            {
                if (_wrfmData == null)
                {
                    _wrfmData = new WRFMData(WRFMCommon.Instance.DataManager,
                        WRFMCommon.Instance.ConfiguationData.PortalConfigurations["SPUrl"],
                        WRFMCommon.Instance.Logger,
                        WRFMCommon.Instance.ConfiguationData.PortalConfigurations["DWBWebServiceUserName"],
                        WRFMCommon.Instance.ConfiguationData.PortalConfigurations["DWBWebServicePassword"]);
                }
                return _wrfmData;
            }
        }

        PrintPublishConsoleProcess _printPublishConsoleProcess;
        public PrintPublishConsoleProcess PrintPublishConsoleProcess
        {
            get
            {
                if (_printPublishConsoleProcess == null)
                {
                    _printPublishConsoleProcess = new PrintPublishConsoleProcess();
                }
                return _printPublishConsoleProcess;
            }
        }

        /// <summary>
        /// Gets the access data.
        /// </summary>
        
        public UserAccessData AccessData
        {
            get
            {

                if (HttpContext.Current != null)
                {



                    if (HttpContext.Current.Session[SEDConstants.SHELLSESSION] != null)
                    {


                        return (UserAccessData)((ShellSession)HttpContext.Current.Session[SEDConstants.SHELLSESSION]).AccessDetails;
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;

            }
        }
        public bool AllowWebServiceCaching
        {
            get
            {
                bool allow = false;
                if (ConfiguationData.PortalConfigurations.ContainsKey("AllowWebServiceCaching"))
                    if (!Boolean.TryParse(ConfiguationData.PortalConfigurations["AllowWebServiceCaching"], out allow))
                        allow = false;
                return allow;
            }
        }
        public string SPSiteUrl { get { return ConfiguationData.PortalConfigurations["SPUrl"]; } }

        public void Dispose()
        {
           if(_logManager!= null) _logManager.Dispose();
           if (_dataManager != null) _dataManager.Dispose();
            instance = null;
            GC.SuppressFinalize(this);
        }


        public enum enumSessionVariable
        {
            UserDefinedLinks,
            DefaultPreferences,
            UserPreferenceUnits,
            UserPreferences,
            selectedIdentifierForEPCatalog,
            picksFilterValues,
            QSCriteriaValue,
            ContextMenu,
            /// <summary>
            /// Used for Map Identifier column Index.
            /// </summary>
            ColumnIndex,
            /// <summary>
            /// Whether to display Context Search For Map or not.
            /// </summary>
            IsDisplayContextSearch,
            /// <summary>
            /// Map layer name
            /// </summary>
            AssetType,
            // Get all records
            /// <summary>
            /// Whether to fetch all records or not
            /// </summary>
            IsFetchAll,
            /// <summary>
            /// sets the maxRecord count.
            /// </summary>
            maxRecordsCount,
            /// <summary>
            /// sets the search type.
            /// </summary>
            searchType,
            /// <summary>
            /// map geometry
            /// </summary>
            geometry,
            /// <summary>
            /// the where clause used for map search.
            /// </summary>
            whereClause,
            /// <summary>
            /// the map asset value.
            /// </summary>
            dropDownBoxValue,
            /// <summary>
            /// To hold the exact error message.
            /// </summary>
            ErrorMessage,
            /// <summary>
            /// To hold the length measured.
            /// </summary>
            MeasureLength,
            /// <summary>
            /// To hold the units from the preference.
            /// </summary>
            MeasureUnits,
            /// <summary>
            ///cantains all selected rows unique ids for map webpart
            /// </summary>
            ZoomListIds,
            /// <summary>
            /// User rights
            /// </summary>
            UserPrivileges,
            // Dream 4.0 code start
            /// <summary>
            /// Well Events Filter Option
            /// </summary>
            WellEventsFilterOption,
            /// <summary>
            /// Change dEvents
            /// </summary>
            ChangedEvents,
            //Dream 4.0 code end

        }

       
        
    }
}
