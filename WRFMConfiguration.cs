using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;

namespace Shell.WRFM.Global.Business
{
    /// <summary>
    /// ReportType
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// LinkSearch
        /// </summary>
        LinkSearch,
        /// <summary>
        /// BasicSearch
        /// </summary>
        BasicSearch,
        /// <summary>
        /// ContextSearch
        /// </summary>
        ContextSearch
    }
    /// <summary>
    /// WRFMReports
    /// </summary>
    public class WRFMConfiguration :IDisposable
    {
        const string SP_GETLEFTNAVIGATIONDATA = "GetLeftNavigationData";
        const string LINKSEARCH = "LinkSearch";
        const string BASICSEARCH = "BasicSearch";
        const string CONTEXTSEARCH = "ContextSearch";

        LeftNavigationData rConfig;
        string repConfig = "";

        public WRFMConfiguration()
        {
            rConfig = new LeftNavigationData();            
            CreateLeftNavigationData();
        }

        /// <summary>
        /// WRFMReports
        /// </summary>
        /// <param name="path"></param>
        public WRFMConfiguration(string path)
        {
            rConfig = new LeftNavigationData();
            // TODO

            // Satbir: Populate from DB
            CreateLeftNavigationData();

            //repConfig = HttpContext.Current.Server.MapPath(path);
            //using(StreamReader objStrRdr = new StreamReader(repConfig))
            //{
            //    XmlSerializer objXmlSer = new XmlSerializer(typeof(LeftNavigationData));
            //    rConfig = (LeftNavigationData)objXmlSer.Deserialize(objStrRdr);
            //}
        }

        DataTable dtNavigationMaster, dtNavigation;
        public void CreateLeftNavigationData()
        {
            rConfig = new LeftNavigationData();
            dtNavigationMaster = GetDataFromDB();
          
             rConfig.LeftNavigation = new LeftNavigationNode();

            // Link Search
            CreateLinkSearchNavigation();

            // Basic Search
            CreateBasicSearchNavigation();

            // Context Search
            CreateContextSearchNavigation();
        }

        /// <summary>
        /// Creates link search navigation links.
        /// </summary>
        private void CreateLinkSearchNavigation()
        {

            rConfig.LeftNavigation.LinkSearch = new List<SearchElement>();
            var qryLinkSearch = (from data in dtNavigationMaster.AsEnumerable()
                                 where data.Field<string>("ReportType") == ReportType.LinkSearch.ToString()
                                 select data);

            if (qryLinkSearch.Count() != 0)
            {
                dtNavigation = qryLinkSearch.CopyToDataTable();
                CreateNode(LINKSEARCH);
            }
        }

        /// <summary>
        /// Creates basic search navigation links.
        /// </summary>
        private void CreateBasicSearchNavigation()
        {
            rConfig.LeftNavigation.BasicSearch = new List<SearchElement>();
            var qryBasicSearch = (from data in dtNavigationMaster.AsEnumerable()
                                  where data.Field<string>("ReportType") == ReportType.BasicSearch.ToString()
                                  select data);

            if (qryBasicSearch.Count() != 0)
            {
                dtNavigation = qryBasicSearch.CopyToDataTable();
                CreateNode(BASICSEARCH);
            }
        }

        /// <summary>
        /// Creates Context search navigation links.
        /// </summary>
        private void CreateContextSearchNavigation()
        {
            rConfig.LeftNavigation.ContextSearch = new List<SearchElement>();
            var qryContextSearch = (from data in dtNavigationMaster.AsEnumerable()
                                    where data.Field<string>("ReportType") == ReportType.ContextSearch.ToString()
                                    select data);

            if (qryContextSearch.Count() != 0)
            {
                dtNavigation = qryContextSearch.CopyToDataTable();
                CreateNode(CONTEXTSEARCH);
            }
        }

        /// <summary>
        /// Create node for a particular search link.
        /// </summary>
        /// <param name="searchType"></param>
        private void CreateNode(string searchType)
        {
            // Get all root nodes.
            var rootEntities = (from data in dtNavigation.AsEnumerable()
                                where data.Field<string>("GroupName") == "-1"
                                select data).Distinct();

            foreach (DataRow row in rootEntities)
            {
                string entityName = row["Entity"].ToString();
                string fallbackUrl = row["FallbackUrl"].ToString();
                int module = Convert.ToInt32(row["ModuleID"]);
                var childNodes = (from data in dtNavigation.AsEnumerable()
                                  where data.Field<string>("GroupName") == entityName
                                  select data);

                SearchElement objSearch = new SearchElement();
                objSearch.Name = entityName;
                objSearch.Module = module.ToString();
                objSearch.FallbackUrl = fallbackUrl;

                if (childNodes.Count() != 0)
                {
                    // if child nodes are there.
                    objSearch.ChildElements = new List<SearchElement>();
                    foreach (DataRow rowChild in childNodes)
                    {
                        objSearch.ChildElements.Add(CreateChildNodes(rowChild));
                    }
                }

                // Depending on the search type, search element is added to its respective list.
                if (searchType.Equals(ReportType.LinkSearch.ToString()))
                {
                    rConfig.LeftNavigation.LinkSearch.Add(objSearch);
                }
                else if (searchType.Equals(ReportType.BasicSearch.ToString()))
                {
                    rConfig.LeftNavigation.BasicSearch.Add(objSearch);
                }
                else if (searchType.Equals(ReportType.ContextSearch.ToString()))
                {
                    rConfig.LeftNavigation.ContextSearch.Add(objSearch);
                }
            }
        }

        /// <summary>
        /// Creates child nodes recursively.
        /// </summary>
        /// <param name="rowChild"></param>
        /// <returns></returns>
        private SearchElement CreateChildNodes(DataRow rowChild)
        {
            string entityName = rowChild["Entity"].ToString();
            int module = Convert.ToInt32(rowChild["ModuleID"]);
            string fallbackUrl = rowChild["FallbackUrl"].ToString();

            // Get all child nodes for the entity.
            var childNodes = (from data in dtNavigation.AsEnumerable()
                              where data.Field<string>("GroupName") == entityName
                              select data);

            SearchElement objSearch = new SearchElement();
            objSearch.Name = entityName;
            objSearch.Module = module.ToString();
            objSearch.FallbackUrl = fallbackUrl;

            if (childNodes.Count() != 0)
            {
                // if child nodes are there.
                objSearch.ChildElements = new List<SearchElement>();
                foreach (DataRow row in childNodes)
                {
                    objSearch.ChildElements.Add(CreateChildNodes(row));
                }
            }

            return objSearch;
        }

        /// <summary>
        /// Fetches the navigation details from the database.
        /// </summary>
        /// <returns></returns>
        private DataTable GetDataFromDB()
        {
            DataSet dsData;
            WRFMCommon objCommon = WRFMCommon.Instance;
            using (DBManager dbManager = objCommon.DataManager)
            {
                try
                {
                    dbManager.Open();
                    dsData = dbManager.ExecuteDataSet(CommandType.StoredProcedure, SP_GETLEFTNAVIGATIONDATA);
                    return dsData.Tables[0];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// GetReports
        /// </summary>
        /// <returns></returns>
        public LeftNavigationData GetReports()
        {
            return rConfig;
        }
        /// <summary>
        /// GetContextReportGroup
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //public ContextGroup GetContextReportGroup(string name)
        //{
        //    foreach(ContextGroup group in rConfig.LeftNavigation.ContextSearch)
        //    {
        //        if(group.Name.ToLower() == name.ToLower())
        //            return group;
        //    }
        //    return null;
        //}
        /// <summary>
        /// GetContextReport
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        //public ContextElement GetContextReport(string name, string groupName)
        //{
        //    foreach(ContextGroup group in rConfig.LeftNavigation.ContextSearch)
        //    {
        //        if(group.Name.ToLower() == groupName.ToLower())
        //        {
        //            if(group.ReportList == null)
        //                continue;
        //            foreach(ContextElement rep in group.ReportList)
        //            {
        //                if(rep.Name.ToLower() == name.ToLower())
        //                {
        //                    return rep;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// GetBasicReportGroup
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public SearchElement GetBasicReportGroup(string name)
        //{
        //    foreach(SearchElement se in rConfig.LeftNavigation.BasicSearch)
        //    {
        //        if(se.ChildElements != null && se.ChildElements.Count > 0 && se.Name.ToLower() == name.ToLower())
        //            return se;
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// GetBasicReport
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="groupName"></param>
        ///// <returns></returns>
        //public SearchElement GetBasicReport(string name, string groupName)
        //{
        //    foreach(SearchElement se in rConfig.LeftNavigation.BasicSearch)
        //    {
        //        if(se.Name.ToLower() == groupName.ToLower())
        //        {
        //            if(se.ChildElements == null)
        //                continue;
        //            foreach(SearchElement seChild in se.ChildElements)
        //            {
        //                if(seChild.Name.ToLower() == name.ToLower())
        //                {
        //                    return seChild;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}
        ///// <summary>
        ///// GetLinkReport
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public SearchElement GetLinkReport(string name)
        //{
        //    foreach(SearchElement se in rConfig.LeftNavigation.LinkSearch)
        //    {
        //        if(se.Name.ToLower() == name.ToLower())
        //        {
        //            return se;
        //        }
        //    }
        //    return null;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupName"></param>
        /// <param name="reportName"></param>
        /// <returns></returns>
        public bool Remove(ReportType type, string reportName, string groupName)
        {
            //switch(type)
            //{
            //    //case ReportType.LinkSearch:
            //    //    rConfig.LeftNavigation.LinkSearch.Remove(GetLinkReport(reportName));
            //    //    break;
            //    //case ReportType.BasicSearch:
            //    //    SearchElement bGroup = GetBasicReportGroup(groupName);
            //    //    bGroup.ChildElements.Remove(GetBasicReport(reportName, groupName));
            //    //    break;
            //    //case ReportType.ContextSearch:
            //    //    ContextGroup cGroup = GetContextReportGroup(groupName);
            //    //    cGroup.ReportList.Remove(GetContextReport(reportName, groupName));
            //    //    break;
            //}
            return true;
        }
        /// <summary>
        /// AddUpdate
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <param name="moduleName"></param>
        /// <param name="moduleVersion"></param>
        public void AddUpdate(ReportType type, string name, string groupName, string moduleName, string moduleVersion)
        {
            CreateLeftNavigationData();
            //SearchElement element;
            //Regex rgx;
            //string repKey;
            //switch(type)
            //{
            //    case ReportType.LinkSearch:
            //        element = GetLinkReport(name);
            //        rgx = new Regex("[^a-zA-Z0-9 -]");
            //        repKey = rgx.Replace(name, "").Replace(" ", "");
            //        if(element == null)
            //        {
            //            SearchElement repNew = new SearchElement();
            //            repNew.Name = name;
            //            repNew.Module = moduleName + "|" + moduleVersion;
            //            rConfig.LeftNavigation.LinkSearch.Add(repNew);
            //        }
            //        else
            //        {
            //            element.Name = name;
            //            element.Module = moduleName + "|" + moduleVersion;
            //        }
            //        break;
            //    case ReportType.BasicSearch:
            //        SearchElement sGroup = GetBasicReportGroup(groupName);
            //        if(sGroup == null)
            //        {
            //            sGroup = new SearchElement()
            //            {
            //                Name = groupName
            //            };
            //            rConfig.LeftNavigation.BasicSearch.Add(sGroup);
            //        }
            //        element = GetBasicReport(name, groupName);
            //        if(element == null)
            //        {
            //            SearchElement repNew = new SearchElement();
            //            repNew.Name = name;
            //            repNew.Module = moduleName + "|" + moduleVersion;
            //            if(sGroup.ChildElements == null)
            //                sGroup.ChildElements = new List<SearchElement>();
            //            sGroup.ChildElements.Add(repNew);
            //        }
            //        else
            //        {
            //            element.Name = name;
            //            element.Module = moduleName + "|" + moduleVersion;
            //        }
            //        break;
            //    default:
            //    //case ReportType.ContextSearch:
            //    //    ContextGroup rGroup = GetContextReportGroup(groupName);
            //    //    if(rGroup == null)
            //    //    {
            //    //        rGroup = new ContextGroup()
            //    //        {
            //    //            Name = groupName
            //    //        };
            //    //        rConfig.LeftNavigation.ContextSearch.Add(rGroup);
            //    //    }
            //    //    ContextElement reportC = GetContextReport(name, groupName);
            //    //    rgx = new Regex("[^a-zA-Z0-9 -]");
            //    //    repKey = rgx.Replace(moduleName, "").Replace(" ", "");
            //    //    if(reportC == null)
            //    //    {
            //    //        ContextElement repNew = new ContextElement();
            //    //        repNew.Name = name;
            //    //        repNew.Key = repKey;
            //    //        repNew.Module = moduleName + "|" + moduleVersion;
            //    //        if(rGroup.ReportList == null)
            //    //            rGroup.ReportList = new List<ContextElement>();
            //    //        rGroup.ReportList.Add(repNew);
            //    //    }
            //    //    else
            //    //    {
            //    //        reportC.Name = name;
            //    //        reportC.Key = repKey;
            //    //        reportC.Module = moduleName + "|" + moduleVersion;
            //    //    }
            //    //    break;
            //}
            
        }

        void IDisposable.Dispose()
        {
            //using(StreamWriter objStrRdr = new StreamWriter(repConfig))
            //{
            //    XmlSerializer objXmlSer = new XmlSerializer(typeof(LeftNavigationData));
            //    objXmlSer.Serialize(objStrRdr, rConfig);
            //}
        }
    }
}
