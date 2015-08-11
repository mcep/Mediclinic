﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections;

public partial class ClaimNumbersAllocation_AllDBsV2 : System.Web.UI.Page
{

    #region Page_Load

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            HideErrorMessage();

            if (!IsPostBack)
            {
                PagePermissions.EnforcePermissions_RequireAny(Session, Response, true, true, true, false, false, true);
                Run();
            }

        }
        catch (CustomMessageException ex)
        {
            if (IsPostBack) SetErrorMessage(ex.Message);
            else HideTableAndSetErrorMessage(ex.Message);
        }
        catch (Exception ex)
        {
            if (IsPostBack) SetErrorMessage("", ex.ToString());
            else HideTableAndSetErrorMessage("", ex.ToString());
        }
    }

    #endregion

    #region Run()

    /*
     * Put this into grid - page and menu only can be seen by stakeholder
     * 
     *           0000-0999          1000-1999          2000-2999          3000-3999          ....
     * A      • Total Podiatry
     * B      • PodiatryClinics
     * C
     * D
     * ..
     * 
     */

    protected void Run()
    {
        string curDbName = Session["DB"].ToString();

        try
        {
            ArrayList claimNbrRangeList = new ArrayList();
            for (char l = 'A'; l <= 'Z'; l++)
                for (int i = 0; i <= 9; i++)
                    claimNbrRangeList.Add("" + l + i + "000" + "_" + l + i + "999");


            ArrayList dbList          = new ArrayList();
            ArrayList siteNameList    = new ArrayList();
            ArrayList allocationsList = new ArrayList();

            System.Data.DataTable tblDBs = DBBase.ExecuteQuery("EXEC sp_databases;", "master").Tables[0];
            for (int i = 0; i < tblDBs.Rows.Count; i++)
            {
                string databaseName = tblDBs.Rows[i][0].ToString();

                if (!Regex.IsMatch(databaseName, @"Mediclinic_\d{4}"))
                    continue;
                //if (databaseName == "Mediclinic_0001")
                //    continue;


                Session["DB"]              = databaseName;
                Session["SystemVariables"] = SystemVariableDB.GetAll();

                string sql = "SELECT COUNT(*) FROM InvoiceHealthcareClaimNumbers WHERE last_date_used IS NOT NULL OR is_active = 1";
                int countUsed = Convert.ToInt32(DBBase.ExecuteSingleResult(sql));
                if (countUsed > 0)
                {
                    dbList.Add(databaseName);
                    siteNameList.Add(((SystemVariables)Session["SystemVariables"])["Site"].Value);
                    allocationsList.Add(GetClaimsAllocated(claimNbrRangeList));
                }

                Session.Remove("DB");
                Session.Remove("SystemVariables");
            }



            string outputHeaderRow = string.Empty;
            string outputBody      = string.Empty;

            DataTable results = new DataTable();
            for (int i = 0; i < 11; i++)
                results.Columns.Add("");

            DataRow dtRow = results.NewRow();
            dtRow[0] = "";
            for (int i = 0; i < 10; i++)
                dtRow[i+1] = i.ToString() + "000-" + i.ToString() + "999";
            results.Rows.Add(dtRow);

            for (int i = 0; i <= 9; i++)  // each row
            {
                for (char l = 'A'; l <= 'Z'; l++)  // each col
                {
                    DataRow dtNewRow = results.NewRow();
                    results.Rows.Add(dtNewRow);
                }
            }


            int r=1;
            for (char l = 'A'; l <= 'Z'; l++)  // each row
            {
                string row = "<tr><th>" + l + "</th>";

                for (int i = 0; i <= 9; i++)  // each col
                {
                    if (l == 'A')
                        outputHeaderRow += "<th style=\"text-align:center;\">" + "" + i + "000" + "-" + i + "999" + "</th>";

                    if (i == 0)
                        results.Rows[r][i] = l;

                    string range      = "" + l + i + "000" + "_" + l + i + "999";
                    string startRange = range.Split('_')[0];
                    string endRange   = range.Split('_')[1];
                    string firstChar  = range[0].ToString();


                    string sites = string.Empty;
                    for (int j = 0; j < siteNameList.Count; j++)
                    {
                        DataTable tbl = (DataTable)allocationsList[j];
                        bool allocated = Convert.ToBoolean(tbl.Rows[0][range]);
                        if (allocated)
                            sites += (sites.Length > 0 ? "<br />" : "") + "• " + siteNameList[j];
                    }

                    row += "<td" + (sites.Length > 0 ? " class=\"nowrap\"" : "") + ">" + sites + "</td>";
                    results.Rows[r][i + 1] = sites.Replace("• ", "").Replace("<br />", Environment.NewLine);
                }

                
                row += "</tr>";
                outputBody += row;

                r++;
            }

            outputHeaderRow = "<tr><th></th>" + outputHeaderRow + "<tr>";

            outputBody = "<table class=\"table table-bordered table-striped table-grid table-grid-top-bottum-padding-normal auto_width block_center\" border=\"1\">" + outputHeaderRow + outputBody + "</table>";
            lblOutput.Text = outputBody;


            ViewState["results"] = results;

        }
        finally
        {
            Session["DB"] = curDbName;
            Session["SystemVariables"] = SystemVariableDB.GetAll();
        }

    }

    #endregion

    #region GetClaimsAllocated

    protected DataTable GetClaimsAllocated(ArrayList claimNbrRangeList)
    {
        System.Text.StringBuilder sql = new System.Text.StringBuilder();
        sql.AppendLine("SELECT ");

        for(int i=0; i<claimNbrRangeList.Count; i++)
        {
            string range      = (string)claimNbrRangeList[i];
            string startRange = range.Split('_')[0];
            string endRange   = range.Split('_')[1];

            string _sql = @"SELECT CASE WHEN EXISTS (SELECT * FROM InvoiceHealthcareClaimNumbers WHERE claim_number >= '" + startRange + @"' AND claim_number <= '" + endRange + @"' AND is_active = 1) THEN 1 ELSE 0 END";
            sql.AppendLine("(" + _sql + ") AS " + range + (i == claimNbrRangeList.Count-1 ? " " : ","));
        }

        DataTable tbl = DBBase.ExecuteQuery(sql.ToString()).Tables[0];
        return tbl;
    }

    #endregion
    

    #region btnExport_Click

    protected void btnExport_Click(object sender, EventArgs e)
    {
        DataTable dt = ViewState["results"] as DataTable;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            for (int j = 0; j < dt.Columns.Count; j++)
                sb.Append("\"" + dt.Rows[i][j] + "\"").Append(",");
            sb.AppendLine();
        }

        ExportCSV(Response, sb.ToString(), "Claim Numbers Allocated.csv");
    }
    protected static void ExportCSV(HttpResponse response, string fileText, string fileName)
    {
        byte[] buffer = GetBytes(fileText);

        try
        {
            response.Clear();
            response.ContentType = "text/plain";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.AddHeader("Content-Disposition", "attachment;filename=\"" + fileName + "\"");
            response.End();
        }
        catch (System.Web.HttpException ex) 
        {
            // ignore exception where user closed the download box
            if (!ex.Message.StartsWith("The remote host closed the connection. The error code is"))
                throw;
        }
    }
    protected static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    #endregion


    #region SetErrorMessage, HideErrorMessage

    private void HideTableAndSetErrorMessage(string errMsg = "", string details = "")
    {
        SetErrorMessage(errMsg, details);
    }
    private void SetErrorMessage(string errMsg = "", string details = "")
    {
        if (errMsg.Contains(Environment.NewLine))
            errMsg = errMsg.Replace(Environment.NewLine, "<br />");

        // double escape so shows up literally on webpage for 'alert' message
        string detailsToDisplay = (details.Length == 0 ? "" : " <a href=\"#\" onclick=\"alert('" + details.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("'", "\\'").Replace("\"", "\\'") + "'); return false;\">Details</a>");

        lblErrorMessage.Visible = true;
        if (errMsg != null && errMsg.Length > 0)
            lblErrorMessage.Text = errMsg + detailsToDisplay + "<br />";
        else
            lblErrorMessage.Text = "An error has occurred. Plase contact the system administrator. " + detailsToDisplay + "<br />";
    }
    private void HideErrorMessage()
    {
        lblErrorMessage.Visible = false;
        lblErrorMessage.Text = "";
    }

    #endregion

}