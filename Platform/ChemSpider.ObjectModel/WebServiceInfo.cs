using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Data.Database;

/// <summary>
/// Summary description for WebServiceInfo
/// </summary>
public class WebServiceInfo
{
    public static Dictionary<string, string> AccessCodes = new Dictionary<string, string>() { {"R", "Role-based"}, {"A", "Anonymous"}, {"F", "Fine-grained"}, {"T", "Authenticated user"} };

    public int ID { get; private set; }
    public string Name { get; set; }
    public string Access { get; set; }
    public int? RoleID { get; set; }
    public string RoleName { get; private set; }
    public int? Delay { get; set; }

    public bool IsRoleAccess { get { return Access.Equals("R"); } }
    public bool IsFineGrainedAccess { get { return Access.Equals("F"); } }
    public string AccessName { get { return AccessCodeToName(Access); } }

    public static string AccessCodeToName(string code)
    {
        return AccessCodes.ContainsKey(code) ? AccessCodes[code] : "Unknown";
    }

    public WebServiceInfo()
    {
        ID = 0;
        Name = string.Empty;
        Access = string.Empty;
    }

    public WebServiceInfo(int id)
        : this()
    {
        Load(id);
    }

    public void Load(int id)
    {
        ID = id;

        DataRow row = GetWebServiceInfo(ID);

        Name = row["name"].ToString();
        Access = row["access"].ToString();
        RoleID = row["rol_id"] != DBNull.Value ? (int?)Convert.ToInt32(row["rol_id"]) : null;
        RoleName = row["role_name"].ToString();
        Delay = row["delay"] != DBNull.Value ? (int?)Convert.ToInt32(row["delay"]) : null;
    }

    public void Save()
    {
        if (ID == 0)
            ID = RegisterWebService(Name, Access, RoleID, Delay);
        else
            UpdateWebService(ID, Name, Access, RoleID, Delay);
    }

    public void Delete()
    {
        DeleteWebService(ID);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    //  DataBase procedures....
    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    private int RegisterWebService(string name, string access, int? role, int? delay)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            return conn.ExecuteScalar<int>("exec UpdateWebServiceInfo @id, @name, @access, @role, @delay", new { id = DBNull.Value, name = name, access = access, role = role, delay = delay });
        }
    }

    private int UpdateWebService(int id, string name, string access, int? role, int? delay)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            return conn.ExecuteScalar<int>("exec UpdateWebServiceInfo @id, @name, @access, @role, @delay", new { id = id, name = name, access = access, role = role, delay = delay });
        }
    }

    private DataRow GetWebServiceInfo(int id)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            DataTable table = conn.FillDataTable("exec GetWebServiceInfo @id", new { id = id });

            if (table.Rows.Count != 1)
                return null;

            return table.Rows[0];
        }
    }

    private void DeleteWebService(int id)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            conn.ExecuteCommand("exec DeleteWebServiceInfo @id", new { id = id });
        }
    }

    public static Dictionary<int, string> GetAllWebServices()
    {
        Dictionary<int, string> webServices = new Dictionary<int, string>();

        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            DataTable table = conn.FillDataTable("select svc_id, name from web_services");
            foreach (DataRow row in table.Rows)
            {
                webServices.Add(Convert.ToInt32(row["svc_id"]), row["name"].ToString());
            }
        }

        return webServices;
    }

    public static void AddServiceToUser(int svc_id, int usr_id, int granted, int? delay)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            conn.ExecuteCommand("exec AddServiceToUser @svc_id, @usr_id, @granted, @delay", new { svc_id = svc_id, usr_id = usr_id, granted = granted, delay = delay });
        }
    }

    public static void DeleteServiceFromUser(int svc_id, int usr_id)
    {
        using (SqlConnection conn = new SqlConnection(ChemUsersDB.ConnectionString))
        {
            conn.ExecuteCommand("exec DeleteServiceFromUser @svc_id, @usr_id", new { svc_id = svc_id, usr_id = usr_id });
        }
    }
}