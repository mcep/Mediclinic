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

public partial class NoteListV2 : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            if (!IsPostBack)
                Utilities.SetNoCache(Response);
            HideErrorMessage();
            Utilities.UpdatePageHeaderV2(Page.Master, true);

            if (!IsPostBack)
            {
                Session.Remove("noteinfo_sortexpression");
                Session.Remove("noteinfo_data");
                FillNoteGrid();

                if (IsValidFormScreen() && GetFormScreen() == 16)
                    lblHeading.Text = "Patient Body Chart Notes";

                if (IsValidFormScreen() && GetFormScreen() == 17)
                    lblHeading.Text = "Medications";

                if (IsValidFormScreen() && GetFormScreen() == 18)
                    lblHeading.Text = "Medical Conditions";

                if (IsValidFormScreen() && GetFormScreen() == 20)
                    lblHeading.Text = "Allergies";

                bool IsMobileDevice = Utilities.IsMobileDevice(Request);
                hiddenIsMobileDevice.Value = IsMobileDevice ? "1" : "0";
                body_chart.Visible  = body_chart_space.Visible  = IsValidFormScreen() && GetFormScreen() == 16 && !IsMobileDevice;
                body_chart2.Visible                             = IsValidFormScreen() && GetFormScreen() == 16 &&  IsMobileDevice;


                if (Request.QueryString["refresh_on_close"] != null && Request.QueryString["refresh_on_close"] == "1")
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "close_on_x", "<script language=javascript>window.onbeforeunload = function () { window.opener.location.href = window.opener.location.href; }</script>");
                    btnClose.OnClientClick = "window.opener.location.href = window.opener.location.href;self.close();";
                }

            }

            this.GrdNote.EnableViewState = true;

            // set for use for saving note text in a cookie for when they are logged out and needs to be reset
            // but only for this user and this notes entity
            userID.Value = Session["StaffID"].ToString();
            entityID.Value = GetFormID().ToString();

            if (GrdNote.FooterRow != null)
            {
                string load_saved_note = IsPostBack ? "" : "load_note(document.getElementById('" + ((TextBox)GrdNote.FooterRow.FindControl("txtNewText")).ClientID + "'), document.getElementById('" + userID.ClientID + "').value, document.getElementById('" + entityID.ClientID + "').value);";

                // make sure any selected checkboxes are highlighted on postback
                Page.ClientScript.RegisterStartupScript(this.GetType(), "close", "<script language=javascript>highlight_row();" + load_saved_note + "</script>");


                //Page.ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), Page.ClientID, "load_note(document.getElementById('" + ((TextBox)GrdNote.FooterRow.FindControl("txtFName1")).ClientID + "'), document.getElementById('" + userID.ClientID + "').value, document.getElementById('" + entityID.ClientID + "').value);", true);
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


    #region IsValidFormID, GetFormID, IsValidFormScreen, GetFormScreen

    private bool IsValidFormID()
    {
        string id = Request.QueryString["id"];
        return id != null && Regex.IsMatch(id, @"^\d+$") && EntityDB.IDExists(Convert.ToInt32(Request.QueryString["id"]));
    }
    private int GetFormID()
    {
        if (!IsValidFormID())
            throw new Exception("Invalid url id");
        return Convert.ToInt32(Request.QueryString["id"]);
    }

    private bool IsValidFormScreen()
    {
        string screen = Request.QueryString["screen"];
        return screen != null && Regex.IsMatch(screen, @"^\d+$") && EntityDB.IDExists(Convert.ToInt32(Request.QueryString["screen"]));
    }
    private int GetFormScreen()
    {
        if (!IsValidFormScreen())
            throw new Exception("Invalid url screen");
        return Convert.ToInt32(Request.QueryString["screen"]);
    }

    #endregion

    #region GrdNote

    protected Tuple<string, string, string, string> GetReferrersEmail()
    {
        if (!IsValidFormID())
            return null;

        Booking booking = BookingDB.GetByEntityID(GetFormID());
        if (booking == null || booking.Patient == null)
            return null;

        PatientReferrer[] patientReferrer = PatientReferrerDB.GetActiveEPCPatientReferrersOf(booking.Patient.PatientID);
        if (patientReferrer.Length == 0)
            return null;

        PatientReferrer currentPatRegReferrer = patientReferrer[patientReferrer.Length - 1];
        RegisterReferrer curRegReferrer = currentPatRegReferrer.RegisterReferrer;

        //string refName = curRegReferrer.Referrer.Person.Surname + ", " + curRegReferrer.Referrer.Person.Firstname + " [" + curRegReferrer.Organisation.Name + "]" + " [" + currentPatRegReferrer.PatientReferrerDateAdded.ToString("dd-MM-yyyy") + "]";
        //SetErrorMessage("Name: " + refName);

        string[] emails = ContactDB.GetEmailsByEntityID(currentPatRegReferrer.RegisterReferrer.Organisation.EntityID);
        if (emails.Length == 0)
            return null;

        string refEmail = string.Join(",", emails);
        string refName = (curRegReferrer.Referrer.Person.Title.ID == 0 ? "Dr." : curRegReferrer.Referrer.Person.Title.Descr) + " " + curRegReferrer.Referrer.Person.Surname;
        string bookingOrg = booking.Organisation.Name;
        string bookingPatientName = booking.Patient.Person.FullnameWithoutMiddlename;
        return new Tuple<string, string, string, string>(refEmail, refName, bookingOrg, bookingPatientName);
    }

    protected void DisallowAddEditIfNoPermissions()
    {
        // if its a booking note
        // only allow add/edit if by the provider of the booking, or by a "principle" staff memeber

        UserView userView        = UserView.GetInstance();
        int      loggedInStaffID = Session["StaffID"] == null ? -1 : Convert.ToInt32(Session["StaffID"]);

        Booking booking = BookingDB.GetByEntityID(GetFormID());
        if (booking != null)
        {
            bool canAddEdit = (booking.Provider != null && loggedInStaffID == booking.Provider.StaffID) || userView.IsPrincipal || userView.IsStakeholder;
            if (!canAddEdit)
            {
                GrdNote.FooterRow.Visible = false;
                for (int i = 0; i < GrdNote.Columns.Count; i++)
                    if (GrdNote.Columns[i].HeaderText.Trim() == ".")
                        GrdNote.Columns[i].Visible = false;
            }
        }
    }


    protected void FillNoteGrid()
    {
        if (!IsValidFormID())
        {
            if (!Utilities.IsDev() || Request.QueryString["id"] != null)
            {
                HideTableAndSetErrorMessage();
                return;
            }

            // can still view all if dev and no id set .. but no insert/edit
            GrdNote.Columns[5].Visible = false;
        }

        if (!IsValidFormScreen() && !Utilities.IsDev())
        {
            HideTableAndSetErrorMessage();
            return;
        }


        DataTable dt = IsValidFormID() ? NoteDB.GetDataTable_ByEntityID(GetFormID(), null, -1, true, true) : NoteDB.GetDataTable(true);


        if (IsValidFormScreen())
        {
            Hashtable allowedNoteTypes = new Hashtable();
            DataTable noteTypes = ScreenNoteTypesDB.GetDataTable_ByScreenID(GetFormScreen());
            for(int i=0; i<noteTypes.Rows.Count; i++)
                allowedNoteTypes[Convert.ToInt32(noteTypes.Rows[i]["note_type_id"])] = 1;

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                if (allowedNoteTypes[Convert.ToInt32(dt.Rows[i]["note_type_id"])] == null)
                    dt.Rows.RemoveAt(i);
        }

        UserView userView = UserView.GetInstance();
        bool canSeeModifiedBy = userView.IsStakeholder || userView.IsMasterAdmin;
        dt.Columns.Add("last_modified_note_info_visible", typeof(Boolean));
        for (int i = 0; i < dt.Rows.Count; i++)
            dt.Rows[i]["last_modified_note_info_visible"] = canSeeModifiedBy;


        ViewState["noteinfo_data"] = dt;



        // add note info to hidden field to use when emailing notes

        string emailBodyText = string.Empty;

        Booking booking = BookingDB.GetByEntityID(GetFormID());
        if (booking != null)
        {
            emailBodyText += @"<br /><br />
<u>Treatment Information</u>
<br />
<table border=""0"" cellpadding=""0"" cellspacing=""0"">" +
    (booking.Patient == null ? "" : @"<tr><td>Patient</td><td style=""width:10px;""></td><td>" + booking.Patient.Person.FullnameWithoutMiddlename + @"</td></tr>") +
    (booking.Offering == null ? "" : @"<tr><td>Service</td><td></td><td>" + booking.Offering.Name + @"</td></tr>") + @"
    <tr><td>Date</td><td></td><td>" + booking.DateStart.ToString("dd-MM-yyyy") + @"</td></tr>
    <tr><td>Provider</td><td></td><td>" + booking.Provider.Person.FullnameWithoutMiddlename + @"</td></tr>
</table>";
        }

        for (int i = 0; i < dt.Rows.Count; i++)
        {
            Note n = NoteDB.Load(dt.Rows[i]);
            emailBodyText += "<br /><br /><u>Note (" + n.DateAdded.ToString("dd-MM-yyyy") + ")</u><br />" + n.Text.Replace(Environment.NewLine, "<br />");
        }
        emailText.Value = emailBodyText + "<br /><br />" + SystemVariableDB.GetByDescr("LettersEmailSignature").Value; ;


        bool siteIsGP = Convert.ToInt32(Session["SiteTypeID"]) == 3;


        if (dt.Rows.Count > 0)
        {

            if (IsPostBack && ViewState["noteinfo_sortexpression"] != null && ViewState["noteinfo_sortexpression"].ToString().Length > 0)
            {
                DataView dataView = new DataView(dt);
                dataView.Sort = ViewState["noteinfo_sortexpression"].ToString();
                GrdNote.DataSource = dataView;
            }
            else
            {
                GrdNote.DataSource = dt;
            }


            try
            {
                GrdNote.DataBind();
            }
            catch (Exception ex)
            {
                this.lblErrorMessage.Visible = true;
                this.lblErrorMessage.Text = ex.ToString();
            }
        }
        else
        {
            dt.Rows.Add(dt.NewRow());
            GrdNote.DataSource = dt;
            GrdNote.DataBind();

            int TotalColumns = GrdNote.Rows[0].Cells.Count;
            GrdNote.Rows[0].Cells.Clear();
            GrdNote.Rows[0].Cells.Add(new TableCell());
            GrdNote.Rows[0].Cells[0].ColumnSpan = TotalColumns + (siteIsGP ? -1 : 0);
            GrdNote.Rows[0].Cells[0].Text = "No Record Found";
        }


        GrdNote.Columns[3].Visible = siteIsGP && GetFormScreen() == 6;


        Tuple<string, string, string, string> refsEmailInfo = GetReferrersEmail();
        ImageButton btnEmail = GrdNote.HeaderRow.FindControl("btnEmail") as ImageButton;
        if (refsEmailInfo != null)
        {
            btnEmail.Visible = true;
            ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenRefEmail")).Value = refsEmailInfo.Item1;
            ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenRefName")).Value = refsEmailInfo.Item2;
            ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenBookingOrg")).Value = refsEmailInfo.Item3;
            ((HiddenField)GrdNote.HeaderRow.FindControl("HiddenBookingPatientName")).Value = refsEmailInfo.Item4;
        }
        else
        {
            btnEmail.Visible = false;
        }

        DisallowAddEditIfNoPermissions(); // place this after databinding
    }
    protected void GrdNote_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (!Utilities.IsDev() && e.Row.RowType != DataControlRowType.Pager)
        {
            e.Row.Cells[0].CssClass = "hiddencol";
            e.Row.Cells[1].CssClass = "hiddencol";
        }

        if (e.Row.RowType != DataControlRowType.Pager && !UserView.GetInstance().IsAdminView)
            e.Row.Cells[7].CssClass = "hiddencol";

        if (e.Row.RowType != DataControlRowType.Pager && (IsValidFormScreen() && (GetFormScreen() == 16 || GetFormScreen() == 17 || GetFormScreen() == 18 || GetFormScreen() == 20)))
        {
            foreach (DataControlField col in GrdNote.Columns)
                if (col.HeaderText.ToLower().Trim() == "type")
                    e.Row.Cells[GrdNote.Columns.IndexOf(col)].CssClass = "hiddencol";
        }
        if (e.Row.RowType != DataControlRowType.Pager && (IsValidFormScreen() && GetFormScreen() != 16))
        {
            foreach (DataControlField col in GrdNote.Columns)
                if (col.HeaderText.ToLower().Trim() == "body part")
                    e.Row.Cells[GrdNote.Columns.IndexOf(col)].CssClass = "hiddencol";
        }

        if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
        {
            TextBox lblNote = (TextBox)e.Row.FindControl("lblNote");
            if (lblNote != null)
            {
                DataTable dt = ViewState["noteinfo_data"] as DataTable;
                string text = dt.Rows[e.Row.RowIndex]["text"].ToString();
                string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                lblNote.Rows = lines.Length <= 1 ? 1 : lines.Length - 1;
            }
        }

        if (IsValidFormScreen() && GetFormScreen() == 17 && e.Row.RowType == DataControlRowType.Header)
            e.Row.Cells[6].Text = "Medication";

        if (IsValidFormScreen() && GetFormScreen() == 18 && e.Row.RowType == DataControlRowType.Header)
            e.Row.Cells[6].Text = "Medical Condition";

        if (IsValidFormScreen() && GetFormScreen() == 20 && e.Row.RowType == DataControlRowType.Header)
            e.Row.Cells[6].Text = "Allergy";
    }
    protected void GrdNote_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            UserView userView = UserView.GetInstance();

            // if there exists note with the type not in the list of types offered on this screen, use this to add below with "foundRows"
            DataTable allNoteTypes = DBBase.GetGenericDataTable(null, "NoteType", "note_type_id", "descr");
            DataTable allBodyParts = DBBase.GetGenericDataTable(null, "BodyPart", "body_part_id", "descr");
            DataTable allMedicalServiceTypes = DBBase.GetGenericDataTable(null, "MedicalServiceType", "medical_service_type_id", "descr");

            DataTable noteTypes = IsValidFormScreen() ? ScreenNoteTypesDB.GetDataTable_ByScreenID(GetFormScreen()) : allNoteTypes;
            DataTable sites = SiteDB.GetDataTable();

            DataTable dt = ViewState["noteinfo_data"] as DataTable;
            bool tblEmpty = (dt.Rows.Count == 1 && dt.Rows[0][0] == DBNull.Value);
            if (!tblEmpty && e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblId = (Label)e.Row.FindControl("lblId");
                DataRow[] foundRows = dt.Select("note_id=" + lblId.Text);
                DataRow thisRow = foundRows[0];


                bool isDeleted = thisRow["deleted_by"] != DBNull.Value || thisRow["date_deleted"] != DBNull.Value;


                DropDownList ddlDate_Day = (DropDownList)e.Row.FindControl("ddlDate_Day");
                DropDownList ddlDate_Month = (DropDownList)e.Row.FindControl("ddlDate_Month");
                DropDownList ddlDate_Year = (DropDownList)e.Row.FindControl("ddlDate_Year");
                if (ddlDate_Day != null && ddlDate_Month != null && ddlDate_Year != null)
                {
                    ddlDate_Day.Items.Add(new ListItem("--", "-1"));
                    ddlDate_Month.Items.Add(new ListItem("--", "-1"));
                    ddlDate_Year.Items.Add(new ListItem("----", "-1"));

                    for (int i = 1; i <= 31; i++)
                        ddlDate_Day.Items.Add(new ListItem(i.ToString(), i.ToString()));
                    for (int i = 1; i <= 12; i++)
                        ddlDate_Month.Items.Add(new ListItem(i.ToString(), i.ToString()));
                    for (int i = DateTime.Today.Year - 1; i <= DateTime.Today.Year + 1; i++)
                        ddlDate_Year.Items.Add(new ListItem(i.ToString(), i.ToString()));

                    if (thisRow["date_added"] != DBNull.Value)
                    {
                        DateTime Date = Convert.ToDateTime(thisRow["date_added"]);

                        ddlDate_Day.SelectedValue = Date.Day.ToString();
                        ddlDate_Month.SelectedValue = Date.Month.ToString();

                        int firstYearSelectable = Convert.ToInt32(ddlDate_Year.Items[1].Value);
                        int lastYearSelectable = Convert.ToInt32(ddlDate_Year.Items[ddlDate_Year.Items.Count - 1].Value);
                        if (Date.Year < firstYearSelectable)
                            ddlDate_Year.Items.Insert(1, new ListItem(Date.Year.ToString(), Date.Year.ToString()));
                        if (Date.Year > lastYearSelectable)
                            ddlDate_Year.Items.Add(new ListItem(Date.Year.ToString(), Date.Year.ToString()));

                        ddlDate_Year.SelectedValue = Date.Year.ToString();
                    }
                }


                DropDownList ddlNoteType = (DropDownList)e.Row.FindControl("ddlNoteType");
                if (ddlNoteType != null)
                {
                    ddlNoteType.DataSource     = noteTypes;
                    ddlNoteType.DataTextField  = "descr";
                    ddlNoteType.DataValueField = "note_type_id";
                    ddlNoteType.DataBind();

                    // if this note type is not in the list for this screen, add it to the edit list
                    bool found = false;
                    foreach (ListItem li in ddlNoteType.Items)
                        if (li.Value == thisRow["note_type_id"].ToString())
                            found = true;
                    if (!found)
                        ddlNoteType.Items.Add(new ListItem(thisRow["note_type_descr"].ToString(), thisRow["note_type_id"].ToString()));

                    ddlNoteType.SelectedValue = thisRow["note_type_id"].ToString();
                }

                DropDownList ddlBodyPart = (DropDownList)e.Row.FindControl("ddlBodyPart");
                if (ddlBodyPart != null)
                {
                    ddlBodyPart.Items.Clear();
                    ddlBodyPart.Items.Add(new ListItem("","-1"));
                    for(int i=0; i<allBodyParts.Rows.Count; i++)
                        ddlBodyPart.Items.Add(new ListItem(allBodyParts.Rows[i]["body_part_id"].ToString() + ". " + allBodyParts.Rows[i]["descr"].ToString(), allBodyParts.Rows[i]["body_part_id"].ToString()));

                    ddlBodyPart.SelectedValue = thisRow["body_part_id"].ToString();
                }


                DropDownList ddlMedicalServiceType = (DropDownList)e.Row.FindControl("ddlMedicalServiceType");
                if (ddlMedicalServiceType != null)
                {
                    ddlMedicalServiceType.Items.Clear();
                    ddlMedicalServiceType.Items.Add(new ListItem("", "-1"));
                    for (int i = 0; i < allMedicalServiceTypes.Rows.Count; i++)
                        ddlMedicalServiceType.Items.Add(new ListItem(allMedicalServiceTypes.Rows[i]["descr"].ToString(), allMedicalServiceTypes.Rows[i]["medical_service_type_id"].ToString()));

                    if (thisRow["medical_service_type_id"] != DBNull.Value)
                        ddlMedicalServiceType.SelectedValue = thisRow["medical_service_type_id"].ToString();
                }


                ImageButton lnkEdit = (ImageButton)e.Row.FindControl("lnkEdit");
                if (lnkEdit != null)
                {
                    lnkEdit.Visible = !isDeleted && 
                    (userView.IsAdminView || 
                     (thisRow["added_by_staff_id"]    != DBNull.Value && Convert.ToInt32(thisRow["added_by_staff_id"])    == Convert.ToInt32(Session["StaffID"])) ||
                     (thisRow["modified_by_staff_id"] != DBNull.Value && Convert.ToInt32(thisRow["modified_by_staff_id"]) == Convert.ToInt32(Session["StaffID"])));
                }

                /*
                DropDownList ddlSite = (DropDownList)e.Row.FindControl("ddlSite");
                if (ddlSite != null)
                {
                    ddlSite.Items.Add(new ListItem("--", "-1"));
                    foreach (DataRow row in sites.Rows)
                        ddlSite.Items.Add(new ListItem(row["name"].ToString(), row["site_id"].ToString()));
                    ddlSite.SelectedValue = thisRow["site_id"].ToString();
                }
                */

                
                ImageButton lnkDelete = (ImageButton)e.Row.FindControl("lnkDelete");
                if (lnkDelete != null)
                {
                    lnkDelete.Visible = userView.IsAdminView ||
                    (thisRow["added_by_staff_id"]    != DBNull.Value && Convert.ToInt32(thisRow["added_by_staff_id"])    == Convert.ToInt32(Session["StaffID"])) ||
                    (thisRow["modified_by_staff_id"] != DBNull.Value && Convert.ToInt32(thisRow["modified_by_staff_id"]) == Convert.ToInt32(Session["StaffID"]));

                    if (isDeleted)
                    {
                        lnkDelete.CommandName = "_UnDelete";
                        lnkDelete.ImageUrl = "~/images/tick-24.png";
                        lnkDelete.ToolTip = "Un-Delete";
                    }
                }

                if (isDeleted)
                {
                    e.Row.AddCssClass("deleted_note");
                    e.Row.Style["display"] = "none";
                    e.Row.Style["color"] = "gray";
                }


                Utilities.AddConfirmationBox(e);
                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                    Utilities.SetEditRowBackColour(e, System.Drawing.Color.LightGoldenrodYellow);
            }
            if (e.Row.RowType == DataControlRowType.Footer)
            {

                DropDownList ddlDate_Day = (DropDownList)e.Row.FindControl("ddlNewDate_Day");
                DropDownList ddlDate_Month = (DropDownList)e.Row.FindControl("ddlNewDate_Month");
                DropDownList ddlDate_Year = (DropDownList)e.Row.FindControl("ddlNewDate_Year");
                if (ddlDate_Day != null && ddlDate_Month != null && ddlDate_Year != null)
                {
                    ddlDate_Day.Items.Add(new ListItem("--", "-1"));
                    ddlDate_Month.Items.Add(new ListItem("--", "-1"));
                    ddlDate_Year.Items.Add(new ListItem("----", "-1"));

                    for (int i = 1; i <= 31; i++)
                        ddlDate_Day.Items.Add(new ListItem(i.ToString(), i.ToString()));
                    for (int i = 1; i <= 12; i++)
                        ddlDate_Month.Items.Add(new ListItem(i.ToString(), i.ToString()));
                    for (int i = DateTime.Today.Year - 1; i <= DateTime.Today.Year + 1; i++)
                        ddlDate_Year.Items.Add(new ListItem(i.ToString(), i.ToString()));

                    ddlDate_Day.SelectedValue = DateTime.Today.Day.ToString();
                    ddlDate_Month.SelectedValue = DateTime.Today.Month.ToString();
                    ddlDate_Year.SelectedValue = DateTime.Today.Year.ToString();

                }

                DropDownList ddlNoteType = (DropDownList)e.Row.FindControl("ddlNewNoteType");
                ddlNoteType.DataSource = noteTypes;
                ddlNoteType.DataBind();

                DropDownList ddlBodyPart = (DropDownList)e.Row.FindControl("ddlNewBodyPart");
                ddlBodyPart.Items.Add(new ListItem("", "-1"));
                for (int i = 0; i < allBodyParts.Rows.Count; i++)
                    ddlBodyPart.Items.Add(new ListItem(allBodyParts.Rows[i]["body_part_id"].ToString() + ". " + allBodyParts.Rows[i]["descr"].ToString(), allBodyParts.Rows[i]["body_part_id"].ToString()));

                DropDownList ddlMedicalServiceType = (DropDownList)e.Row.FindControl("ddlNewMedicalServiceType");
                ddlMedicalServiceType.Items.Add(new ListItem("", "-1"));
                for (int i = 0; i < allMedicalServiceTypes.Rows.Count; i++)
                    ddlMedicalServiceType.Items.Add(new ListItem(allMedicalServiceTypes.Rows[i]["descr"].ToString(), allMedicalServiceTypes.Rows[i]["medical_service_type_id"].ToString()));


                // set note text in cookie in case user logged out, to keep note text for this user and this entity
                TextBox txtNewText = (TextBox)e.Row.FindControl("txtNewText");
                txtNewText.Attributes["onkeyup"] = "set_note(document.getElementById('" + txtNewText.ClientID + "'), document.getElementById('" + userID.ClientID + "').value, document.getElementById('" + entityID.ClientID + "').value);";


                /*
                DropDownList ddlSite = (DropDownList)e.Row.FindControl("ddlNewSite");
                ddlSite.Items.Add(new ListItem("--", "-1"));
                foreach (DataRow row in sites.Rows)
                    ddlSite.Items.Add(new ListItem(row["name"].ToString(), row["site_id"].ToString()));
                ddlSite.SelectedValue = Session["SiteID"].ToString();
                */
            }
        }
        catch (Exception ex)
        {
            if (Utilities.IsDev())
                throw;
            else
                HideTableAndSetErrorMessage(ex is CustomMessageException ? ex.Message : "");
        }
    }
    protected void GrdNote_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GrdNote.Columns[9].Visible = true;
        GrdNote.EditIndex = -1;
        FillNoteGrid();
    }
    protected void GrdNote_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        Label lblId = (Label)GrdNote.Rows[e.RowIndex].FindControl("lblId");
        DropDownList ddlNoteType = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlNoteType");
        DropDownList ddlBodyPart = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlBodyPart");
        DropDownList ddlMedicalServiceType = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlMedicalServiceType");
        TextBox txtText = (TextBox)GrdNote.Rows[e.RowIndex].FindControl("txtText");
        //DropDownList ddlSite = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlSite");
        DropDownList ddlDate_Day = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlDate_Day");
        DropDownList ddlDate_Month = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlDate_Month");
        DropDownList ddlDate_Year = (DropDownList)GrdNote.Rows[e.RowIndex].FindControl("ddlDate_Year");

        DataTable dt = ViewState["noteinfo_data"] as DataTable;
        DataRow[] foundRows = dt.Select("note_id=" + lblId.Text);
        Note note = NoteDB.Load(foundRows[0]);




        DateTime date = GetDate(ddlDate_Day.SelectedValue, ddlDate_Month.SelectedValue, ddlDate_Year.SelectedValue);
        NoteDB.Update(Convert.ToInt32(lblId.Text), date, Convert.ToInt32(Session["StaffID"]), Convert.ToInt32(ddlNoteType.SelectedValue), Convert.ToInt32(ddlBodyPart.SelectedValue), Convert.ToInt32(ddlMedicalServiceType.SelectedValue), txtText.Text, note.Site.SiteID);



        // if its a booking note
        // email admin so they know if a provider is sabotaging the system (it has happened before)

        int loggedInStaffID = Session["StaffID"] == null ? -1 : Convert.ToInt32(Session["StaffID"]);

        Booking booking = BookingDB.GetByEntityID(GetFormID());
        if (booking != null)  // if note is for a booking
        {

            int thresholdCharacters = 50;
            int totalCharactersBefore = note.Text.Trim().Length;
            int totalCharactersAfter = txtText.Text.Trim().Length;
            int difference = totalCharactersAfter - totalCharactersBefore;

            if (totalCharactersBefore > thresholdCharacters && totalCharactersAfter < thresholdCharacters && difference < -20)
            {
                string mailText = @"This is an administrative email to notify you that notes for a booking may have been deleted.

<u>Logged-in user performing the udate</u>
" + StaffDB.GetByID(loggedInStaffID).Person.FullnameWithoutMiddlename + @"

<u>Original Text (Characters: " + totalCharactersBefore + @")</u>
<font color=""blue"">" + note.Text.Replace(Environment.NewLine, "<br />") + @"</font>

<u>Updated Text (Characters: " + totalCharactersAfter + @")</u>
<font color=""blue"">" + txtText.Text.Replace(Environment.NewLine, "<br />") + @"</font>

<u>Booking details</u>
<table border=""0"" cellpadding=""2"" cellspacing=""2""><tr><td>Booking ID:</td><td>" + booking.BookingID + @"</td></tr><tr><td>Booking Date:</td><td>" + booking.DateStart.ToString("d MMM, yyyy") + " " + booking.DateStart.ToString("h:mm") + (booking.DateStart.Hour < 12 ? "am" : "pm") + @"</td></tr><tr><td>Organisation:</td><td>" + booking.Organisation.Name + @"</td></tr><tr><td>Provider:</td><td>" + booking.Provider.Person.FullnameWithoutMiddlename + @"</td></tr><tr><td>Patient:</td><td>" + (booking.Patient == null ? "" : booking.Patient.Person.FullnameWithoutMiddlename + " [ID:" + booking.Patient.PatientID + "]") + @"</td></tr><tr><td>Status:</td><td>" + booking.BookingStatus.Descr + @"</td></tr></table>

Regards,
Mediclinic
";
                bool EnableDeletedBookingsAlerts = Convert.ToInt32(SystemVariableDB.GetByDescr("EnableDeletedBookingsAlerts").Value) == 1;

                if (EnableDeletedBookingsAlerts && !Utilities.IsDev())
                    Emailer.AsyncSimpleEmail(
                        ((SystemVariables)Session["SystemVariables"])["Email_FromEmail"].Value,
                        ((SystemVariables)Session["SystemVariables"])["Email_FromName"].Value,
                        ((SystemVariables)Session["SystemVariables"])["AdminAlertEmail_To"].Value,
                        "Notification that booking notes may have been deleted",
                        mailText.Replace(Environment.NewLine, "<br />"),
                        true,
                        null);
            }
        }



        GrdNote.Columns[7].Visible = true;
        GrdNote.EditIndex = -1;
        FillNoteGrid();
    }
    protected void GrdNote_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        /*
        Label lblId = (Label)GrdNote.Rows[e.RowIndex].FindControl("lblId");
        int note_id = Convert.ToInt32(lblId.Text);
        NoteDB.SetDeleted(note_id, Convert.ToInt32(Session["StaffID"]));

        FillNoteGrid();
        */
    }
    protected void GrdNote_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName.Equals("Insert"))
        {
            if (!IsValidFormID())
                throw new CustomMessageException();

            Label lblId = (Label)GrdNote.FooterRow.FindControl("lblId");
            DropDownList ddlNoteType = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewNoteType");
            DropDownList ddlBodyPart = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewBodyPart");
            DropDownList ddlMedicalServiceType = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewMedicalServiceType");
            TextBox txtText = (TextBox)GrdNote.FooterRow.FindControl("txtNewText");
            //DropDownList ddlSite = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewSite");
            DropDownList ddlDate_Day = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewDate_Day");
            DropDownList ddlDate_Month = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewDate_Month");
            DropDownList ddlDate_Year = (DropDownList)GrdNote.FooterRow.FindControl("ddlNewDate_Year");

            if (!IsValidDate(ddlDate_Day.SelectedValue, ddlDate_Month.SelectedValue, ddlDate_Year.SelectedValue))
                return;

            DateTime date = GetDate(ddlDate_Day.SelectedValue, ddlDate_Month.SelectedValue, ddlDate_Year.SelectedValue);
            NoteDB.Insert(GetFormID(), date, Convert.ToInt32(Session["StaffID"]), Convert.ToInt32(ddlNoteType.SelectedValue), Convert.ToInt32(ddlBodyPart.SelectedValue), Convert.ToInt32(ddlMedicalServiceType.SelectedValue), txtText.Text, Convert.ToInt32(Session["SiteID"]));

            FillNoteGrid();

            string clear_saved_note = "clear_note(document.getElementById('" + ((TextBox)GrdNote.FooterRow.FindControl("txtNewText")).ClientID + "'), document.getElementById('" + userID.ClientID + "').value, document.getElementById('" + entityID.ClientID + "').value);";
            ScriptManager.RegisterStartupScript(GrdNote, this.GetType(), "unset_cookie", clear_saved_note, true);
        }
        if (e.CommandName == "_Delete")
        {
            NoteDB.SetDeleted(Convert.ToInt32(e.CommandArgument), Convert.ToInt32(Session["StaffID"]));
            FillNoteGrid();
        }
        if (e.CommandName == "_UnDelete")
        {
            NoteDB.SetNotDeleted(Convert.ToInt32(e.CommandArgument), Convert.ToInt32(Session["StaffID"]));
            FillNoteGrid();
        }

    }
    protected void GrdNote_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GrdNote.Columns[9].Visible = false;
        GrdNote.EditIndex = e.NewEditIndex;
        FillNoteGrid();
    }
    protected void GrdNote_Sorting(object sender, GridViewSortEventArgs e)
    {
        // dont allow sorting if in edit mode
        if (GrdNote.EditIndex >= 0)
            return;

        DataTable dataTable = ViewState["noteinfo_data"] as DataTable;

        if (dataTable != null)
        {
            if (ViewState["noteinfo_sortexpression"] == null)
                ViewState["noteinfo_sortexpression"] = "";

            DataView dataView = new DataView(dataTable);
            string[] sortData = ViewState["noteinfo_sortexpression"].ToString().Trim().Split(' ');
            string newSortExpr = (e.SortExpression == sortData[0] && sortData[1] == "ASC") ? "DESC" : "ASC";
            dataView.Sort = e.SortExpression + " " + newSortExpr;
            ViewState["noteinfo_sortexpression"] = e.SortExpression + " " + newSortExpr;

            GrdNote.DataSource = dataView;
            GrdNote.DataBind();
        }
    }

    #endregion


    protected void DateAllOrNoneCheck(object sender, ServerValidateEventArgs e)
    {
        try
        {
            CustomValidator cv = (CustomValidator)sender;
            GridViewRow grdRow = ((GridViewRow)cv.Parent.Parent);
            DropDownList _ddlDate_Day = (DropDownList)grdRow.FindControl(grdRow.RowType == DataControlRowType.Footer ? "ddlNewDate_Day" : "ddlDate_Day");
            DropDownList _ddlDate_Month = (DropDownList)grdRow.FindControl(grdRow.RowType == DataControlRowType.Footer ? "ddlNewDate_Month" : "ddlDate_Month");
            DropDownList _ddlDate_Year = (DropDownList)grdRow.FindControl(grdRow.RowType == DataControlRowType.Footer ? "ddlNewDate_Year" : "ddlDate_Year");

            e.IsValid = IsValidDate(_ddlDate_Day.SelectedValue, _ddlDate_Month.SelectedValue, _ddlDate_Year.SelectedValue);
        }
        catch (Exception)
        {
            e.IsValid = false;
        }

    }
    public bool IsValidDate(string day, string month, string year)
    {
        bool invalid = ((day == "-1" || month == "-1" || year == "-1") && (day != "-1" || month != "-1" || year != "-1"));

        if ((day == "-1" || month == "-1" || year == "-1"))
            return false;

        try
        {
            DateTime d = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    protected DateTime GetDate(string day, string month, string year)
    {
        if ((day == "-1" && month == "-1" && year == "-1"))
            return DateTime.MinValue;

        return new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
    }

    protected void ValidDateCheck(object sender, ServerValidateEventArgs e)
    {
        try
        {
            CustomValidator cv = (CustomValidator)sender;
            GridViewRow grdRow = ((GridViewRow)cv.Parent.Parent);
            TextBox txtDate = grdRow.RowType == DataControlRowType.Footer ? (TextBox)grdRow.FindControl("txtNewDate") : (TextBox)grdRow.FindControl("txtDate");

            if (!IsValidDate(txtDate.Text))
                throw new Exception();

            DateTime d = GetDate(txtDate.Text);

            e.IsValid = (d == DateTime.MinValue) || (Utilities.IsValidDBDateTime(d));
        }
        catch (Exception)
        {
            e.IsValid = false;
        }
    }
    public DateTime GetDate(string inDate)
    {
        inDate = inDate.Trim();

        if (inDate.Length == 0)
        {
            return DateTime.MinValue;
        }
        else
        {
            string[] DateParts = inDate.Split(new char[] { '-' });
            return new DateTime(Convert.ToInt32(DateParts[2]), Convert.ToInt32(DateParts[1]), Convert.ToInt32(DateParts[0]));
        }
    }
    public bool IsValidDate(string inDate)
    {
        inDate = inDate.Trim();
        try
        {
            if (inDate.Length == 0)
                return true;

            if (!System.Text.RegularExpressions.Regex.IsMatch(inDate, @"^\d{2}\-\d{2}\-\d{4}$"))
                return false;

            string[] parts = inDate.Split('-');
            DateTime d = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }
    

    #region btnPrint_Click, btnEmail_Click

    protected string GetNewTmpDir()
    {
        string tmpLettersDirectory = Letter.GetTempLettersDirectory();
        if (!System.IO.Directory.Exists(tmpLettersDirectory))
            throw new CustomMessageException("Temp letters directory doesn't exist");
        string tmpDir = FileHelper.GetTempDirectoryName(tmpLettersDirectory);
        System.IO.Directory.CreateDirectory(tmpDir);

        return tmpDir;
    }


    protected void btnPrint_Click(object sender, ImageClickEventArgs e)
    {
        try
        {
            string outputFileName = "Notes.pdf";
            byte[] fileContents = GetNoteFileContents(outputFileName);
            Letter.DownloadDocument(Response, fileContents, outputFileName);
        }
        catch (CustomMessageException cmEx)
        {
            SetErrorMessage("Error: " + cmEx.Message);
        }
    }
    protected void btnEmail_Click(object sender, ImageClickEventArgs e)
    {
        string refsEmail = ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenRefEmail")).Value;
        string refsName = ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenRefName")).Value;
        string bookingOrg = ((HiddenField)GrdNote.HeaderRow.FindControl("hiddenBookingOrg")).Value;
        string patientName = ((HiddenField)GrdNote.HeaderRow.FindControl("HiddenBookingPatientName")).Value;

        string tmpDir = GetNewTmpDir();


        //create delegate and invoke it asynchrnously, control passes past this line while this is done in another thread
        //AsyncSendEmailDelegate myAction = new AsyncSendEmailDelegate(SendEmail);
        //myAction.BeginInvoke(refsEmail, refsName, bookingOrg, patientName, tmpDir, null, null);

        // dont send async as the session info is unavailable for a whole lot 
        bool sent = SendEmail(refsEmail, refsName, bookingOrg, patientName, tmpDir);
        if (sent)
            SetErrorMessage("Email sent");
    }

    protected delegate void AsyncSendEmailDelegate(string refsEmail, string refsName, string bookingOrg, string patientName, string tmpDir);
    protected bool SendEmail(string refsEmail, string refsName, string bookingOrg, string patientName, string tmpDir)
    {
        try
        {
            string tmpFilename = tmpDir + "Notes.pdf";
            CreateNoteFile(tmpFilename);

            // email the referrer
            Emailer.SimpleEmail(
                bookingOrg,
                refsEmail,
                "Treatment Notes for " + patientName,
                @"Dear " + refsName + ",<br /><br />Please find booking notes attached for " + patientName + "<br /><br />Regards,<br />" + bookingOrg,
                true,
                new string[] { tmpFilename },
                null
                );

            System.IO.File.Delete(tmpFilename);
            System.IO.Directory.Delete(tmpDir);

            //SetErrorMessage("Email sent");
            return true;
        }
        catch (CustomMessageException cmEx)
        {
            SetErrorMessage(cmEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, false);
            //SetErrorMessage("Error: " + cmEx.Message);
            return false;
        }
    }

    protected byte[] GetNoteFileContents(string outputFileName)
    {
        string tmpDir = GetNewTmpDir();
        string tmpFilename = tmpDir + outputFileName;
        CreateNoteFile(tmpFilename);

        byte[] fileContents = System.IO.File.ReadAllBytes(tmpFilename);
        System.IO.File.Delete(tmpFilename);
        System.IO.Directory.Delete(tmpDir);

        return fileContents;
    }

    protected void CreateNoteFile(string tmpFilename)
    {

        string header = string.Empty;

        Booking booking = BookingDB.GetByEntityID(GetFormID());
        if (booking != null)
        {
            Site site = SiteDB.GetByID(Convert.ToInt32(Session["SiteID"]));

            string[] phNums;
            if (Utilities.GetAddressType().ToString() == "Contact")
                phNums = ContactDB.GetByEntityID(-1, booking.Organisation.EntityID, 34).Select(r => r.AddrLine1).ToArray();
            else if (Utilities.GetAddressType().ToString() == "ContactAus")
                phNums = ContactAusDB.GetByEntityID(-1, booking.Organisation.EntityID, 34).Select(r => r.AddrLine1).ToArray();
            else
                throw new Exception("Unknown AddressType in config: " + Utilities.GetAddressType().ToString().ToString());

            if (phNums.Length == 0)
            {
                if (Utilities.GetAddressType().ToString() == "Contact")
                    phNums = ContactDB.GetByEntityID(-1, site.EntityID, 34).Select(r => r.AddrLine1).ToArray();
                else if (Utilities.GetAddressType().ToString() == "ContactAus")
                    phNums = ContactAusDB.GetByEntityID(-1, site.EntityID, 34).Select(r => r.AddrLine1).ToArray();
                else
                    throw new Exception("Unknown AddressType in config: " + Utilities.GetAddressType().ToString().ToString());
            }

            string numbers = string.Empty;
            if (phNums.Length > 0)
                numbers += " - TEL " + phNums[0];
            if (phNums.Length > 1)
                numbers += ", " + phNums[1];

            header += site.Name + numbers + Environment.NewLine;
            header += "Clinic:  " + booking.Organisation.Name + Environment.NewLine;

            if (booking.Patient != null)
                header += "Patient:  " + booking.Patient.Person.FullnameWithoutMiddlename + Environment.NewLine;
            if (booking.Offering != null)
                header += "Service:  " + booking.Offering.Name + Environment.NewLine;

            header += "Provider:  " + booking.Provider.Person.FullnameWithoutMiddlename + Environment.NewLine;
            header += "Date of Consultation: " + booking.DateStart.ToString("d MMM yyyy") + Environment.NewLine + Environment.NewLine + "Treatment Note:" + Environment.NewLine;
        }


        System.Collections.ArrayList notesList = new System.Collections.ArrayList();
        foreach (GridViewRow row in GrdNote.Rows)
        {
            Label lblId = row.FindControl("lblId") as Label;
            Label lblText = row.FindControl("lblText") as Label;
            CheckBox chkPrint = row.FindControl("chkPrint") as CheckBox;

            if (lblId == null || lblText == null || chkPrint == null)
                continue;

            if (chkPrint.Checked)
                notesList.Add(header + lblText.Text.Replace("<br/>", "\n"));
        }

        if (notesList.Count == 0)
            throw new CustomMessageException("Please select at least one note to print.");

        UserView userView = UserView.GetInstance();
        bool isAgedCare = booking != null && booking.Organisation != null ? booking.Organisation.IsAgedCare : userView.IsAgedCareView;
        string filename = isAgedCare ? "BlankTemplateAC.docx" : "BlankTemplate.docx";
        string originalFile = Letter.GetLettersDirectory() + filename;
        if (!System.IO.File.Exists(originalFile))
            throw new CustomMessageException("Template File '" + filename + "' does not exist.");

        string errorString = string.Empty;
        if (!WordMailMerger.Merge(originalFile, tmpFilename, null, null, 0, false, true, (string[])notesList.ToArray(typeof(string)), false, null, out errorString))
            throw new CustomMessageException("Error:" + errorString);
    }

    #endregion


    #region SetErrorMessage, HideErrorMessage

    private void HideTableAndSetErrorMessage(string errMsg = "", string details = "")
    {
        main_table.Visible = false;
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