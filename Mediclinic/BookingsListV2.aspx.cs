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

public partial class BookingsListV2 : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            if (!IsPostBack)
                Utilities.SetNoCache(Response);
            HideErrorMessage();

            if (!IsPostBack)
            {
                PagePermissions.EnforcePermissions_RequireAny(Session, Response, true, true, true, true, true, false);
                Session.Remove("bookinglist_sortexpression");
                Session.Remove("bookinglist_data");

                if (GetFormType() == FormType.None)
                {
                    HideTableAndSetErrorMessage();
                    return;
                }

                SetupGUI();
                FillGrid();

                txtStartDate_Picker.OnClientClick = "displayDatePicker('txtStartDate', this, 'dmy', '-'); return false;";
                txtEndDate_Picker.OnClientClick   = "displayDatePicker('txtEndDate',   this, 'dmy', '-'); return false;";
            }

            this.GrdBooking.EnableViewState = true;

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

    protected void SetupGUI()
    {
        chkIncCompleted.Checked  = IsValidFormIncCompleted()  ? GetFormIncCompleted(false)  : true;
        chkIncCancelled.Checked  = IsValidFormIncCancelled()  ? GetFormIncCancelled(false)  : true;
        chkIncIncomplete.Checked = IsValidFormIncIncomplete() ? GetFormIncIncomplete(false) : true;
        chkIncDeleted.Checked    = IsValidFormIncDeleted()    ? GetFormIncDeleted(false)    : false;

        txtBookingNbrSearch.Text = GetFormBookingIDSearch();

        FormType formType = GetFormType();
        if (formType == FormType.Patient)
        {
            txtStartDate.Text = IsValidFormStartDate() ? (GetFormStartDate(false) == DateTime.MinValue ? "" : GetFormStartDate(false).ToString("dd-MM-yyyy")) : string.Empty;  //  DateTime.Now.AddYears(-1).ToString("dd-MM-yyyy"); // 
            txtEndDate.Text   = IsValidFormEndDate()   ? (GetFormEndDate(false)   == DateTime.MinValue ? "" : GetFormEndDate(false).ToString("dd-MM-yyyy"))   : string.Empty;  //  DateTime.Now.AddYears(1).ToString("dd-MM-yyyy");  // 
        }
        else if (formType == FormType.Provider)
        {
            txtStartDate.Text = IsValidFormStartDate() ? (GetFormStartDate(false) == DateTime.MinValue ? "" : GetFormStartDate(false).ToString("dd-MM-yyyy")) : DateTime.Now.ToString("dd-MM-yyyy");
            txtEndDate.Text = IsValidFormEndDate()     ? (GetFormEndDate(false)   == DateTime.MinValue ? "" : GetFormEndDate(false).ToString("dd-MM-yyyy"))   : DateTime.Now.ToString("dd-MM-yyyy");
        }
        else if (formType == FormType.Organsiation)
        {
            txtStartDate.Text = IsValidFormStartDate() ? (GetFormStartDate(false) == DateTime.MinValue ? "" : GetFormStartDate(false).ToString("dd-MM-yyyy")) : DateTime.Now.ToString("dd-MM-yyyy");
            txtEndDate.Text   = IsValidFormEndDate()   ? (GetFormEndDate(false)   == DateTime.MinValue ? "" : GetFormEndDate(false).ToString("dd-MM-yyyy"))   : DateTime.Now.ToString("dd-MM-yyyy");
        }
    }

    protected void btnUpdateGrdBooking_Click(object sender, EventArgs e)
    {
        FillGrid();
    }

    #region FormType, GetFormParam, IsValidDate/GetDate

    protected enum FormType { Patient, Provider, Organsiation, None };
    protected FormType GetFormType()
    {
        if (Request.QueryString["patient"] != null)
            return FormType.Patient;
        else if (Request.QueryString["org"] != null)
            return FormType.Organsiation;
        else if (Request.QueryString["staff"] != null)
            return FormType.Provider;
        else
            return FormType.None;
    }

    protected bool IsValidDate(string strDate)
    {
        try
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(strDate, @"^\d{2}\-\d{2}\-\d{4}$"))
                return false;

            string[] parts = strDate.Split('-');
            DateTime d = new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
            return true;
        }
        catch (Exception)
        {
            return false;
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
            string[] parts = inDate.Split(new char[] { '-' });
            return new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
        }
    }


    protected bool IsValidFormPatient()
    {
        string patient_id = Request.QueryString["patient"];
        return patient_id != null && Regex.IsMatch(patient_id, @"^\d+$") && PatientDB.Exists(Convert.ToInt32(patient_id));
    }
    protected int GetFormPatient(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormPatient())
            throw new Exception("Invalid url patient");
        return Convert.ToInt32(Request.QueryString["patient"]);
    }

    protected bool IsValidFormOrganisation()
    {
        string organsiation_id = Request.QueryString["org"];
        return organsiation_id != null && Regex.IsMatch(organsiation_id, @"^\d+$") && OrganisationDB.Exists(Convert.ToInt32(organsiation_id));
    }
    protected int GetFormOrganisation(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormOrganisation())
            throw new Exception("Invalid url organsiation");
        return Convert.ToInt32(Request.QueryString["org"]);
    }

    protected bool IsValidFormProvider()
    {
        string provider_id = Request.QueryString["staff"];
        return provider_id != null && Regex.IsMatch(provider_id, @"^\d+$") && StaffDB.Exists(Convert.ToInt32(provider_id));
    }
    protected int GetFormProvider(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormProvider())
            throw new Exception("Invalid url provider");
        return Convert.ToInt32(Request.QueryString["staff"]);
    }


    protected bool IsValidFormIncCompleted()
    {
        string inc_completed = Request.QueryString["inc_completed"];
        return inc_completed != null && (inc_completed == "0" || inc_completed == "1");
    }
    protected bool GetFormIncCompleted(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormIncCompleted())
            throw new Exception("Invalid url 'inc_completed'");
        return Request.QueryString["inc_completed"] == "1";
    }
    protected bool IsValidFormIncIncomplete()
    {
        string inc_incomplete = Request.QueryString["inc_incomplete"];
        return inc_incomplete != null && (inc_incomplete == "0" || inc_incomplete == "1");
    }
    protected bool GetFormIncIncomplete(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormIncIncomplete())
            throw new Exception("Invalid url 'inc_incomplete'");
        return Request.QueryString["inc_incomplete"] == "1";
    }
    protected bool IsValidFormIncCancelled()
    {
        string inc_cancelled = Request.QueryString["inc_cancelled"];
        return inc_cancelled != null && (inc_cancelled == "0" || inc_cancelled == "1");
    }
    protected bool GetFormIncCancelled(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormIncCancelled())
            throw new Exception("Invalid url 'inc_cancelled'");
        return Request.QueryString["inc_cancelled"] == "1";
    }
    protected bool IsValidFormIncDeleted()
    {
        string inc_deleted = Request.QueryString["inc_deleted"];
        return inc_deleted != null && (inc_deleted == "0" || inc_deleted == "1");
    }
    protected bool GetFormIncDeleted(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormIncDeleted())
            throw new Exception("Invalid url 'inc_deleted'");
        return Request.QueryString["inc_deleted"] == "1";
    }

    protected string GetFormBookingIDSearch()
    {
        return Request.QueryString["booking_nbr_search"] == null ? "" : Request.QueryString["booking_nbr_search"];
    }

    protected bool IsValidFormStartDate()
    {
        string start_date = Request.QueryString["start_date"];
        return start_date != null && (start_date.Length == 0 || Regex.IsMatch(start_date, @"^\d{4}_\d{2}_\d{2}$"));
    }
    protected DateTime GetFormStartDate(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormStartDate())
            throw new Exception("Invalid url 'start date'");
        return Request.QueryString["start_date"].Length == 0 ? DateTime.MinValue : GetDateFromString(Request.QueryString["start_date"], "yyyy_mm_dd");
    }
    protected bool IsValidFormEndDate()
    {
        string end_date = Request.QueryString["end_date"];
        return end_date != null && (end_date.Length == 0 || Regex.IsMatch(end_date, @"^\d{4}_\d{2}_\d{2}$"));
    }
    protected DateTime GetFormEndDate(bool checkIsValid = true)
    {
        if (checkIsValid && !IsValidFormEndDate())
            throw new Exception("Invalid url 'end date'");
        return Request.QueryString["end_date"].Length == 0 ?   DateTime.MinValue : GetDateFromString(Request.QueryString["end_date"],   "yyyy_mm_dd");
    }
    protected DateTime GetDateFromString(string sDate, string format)
    {
        if (format == "yyyy_mm_dd")
        {
            string[] dateparts = sDate.Split('_');
            return new DateTime(Convert.ToInt32(dateparts[0]), Convert.ToInt32(dateparts[1]), Convert.ToInt32(dateparts[2]));
        }
        else if (format == "dd_mm_yyyy")
        {
            string[] dateparts = sDate.Split('_');
            return new DateTime(Convert.ToInt32(dateparts[2]), Convert.ToInt32(dateparts[1]), Convert.ToInt32(dateparts[0]));
        }
        if (format == "yyyy-mm-dd")
        {
            string[] dateparts = sDate.Split('-');
            return new DateTime(Convert.ToInt32(dateparts[0]), Convert.ToInt32(dateparts[1]), Convert.ToInt32(dateparts[2]));
        }
        else if (format == "dd-mm-yyyy")
        {
            string[] dateparts = sDate.Split('-');
            return new DateTime(Convert.ToInt32(dateparts[2]), Convert.ToInt32(dateparts[1]), Convert.ToInt32(dateparts[0]));
        }
        else
            throw new ArgumentOutOfRangeException("Unknown date format");
    }


    #endregion

    #region GrdBooking

    protected void FillGrid()
    {
        DateTime fromDate = IsValidDate(txtStartDate.Text) ? GetDate(txtStartDate.Text)                             : DateTime.MinValue;
        DateTime toDate   = IsValidDate(txtEndDate.Text)   ? GetDate(txtEndDate.Text).Add(new TimeSpan(23, 59, 59)) : DateTime.MinValue;

        ArrayList list = new ArrayList();
        if (chkIncCompleted.Checked)  list.Add("187");
        if (chkIncCancelled.Checked)  list.Add("188");
        if (chkIncIncomplete.Checked) list.Add("0");
        if (chkIncDeleted.Checked)    list.Add("-1");
        string statusIDsToInclude = string.Join(",", (string[])list.ToArray(Type.GetType("System.String")));


        DataTable dt = null;

        /*
        if (IsValidFormPatient())
        {
            int patient_id = GetFormPatient(false);
            Patient patient = PatientDB.GetByID(patient_id);
            if (patient == null)
                throw new CustomMessageException("Invalid patient ID");

            heading.InnerText = "Bookings List for : ";
            lnkToEntity.Text = patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + patient.PatientID;

            dt = BookingDB.GetDataTable_Between(fromDate, toDate, null, null, patient, chkIncDeleted.Checked, statusIDsToInclude, false, txtBookingNbrSearch.Text.Trim());
        }
        else if (IsValidFormOrganisation())
        {
            int org_id = GetFormOrganisation(false);
            Organisation org = OrganisationDB.GetByID(org_id);
            if (org == null)
                throw new CustomMessageException("Invalid organisation ID");

            heading.InnerText = "Bookings List for : ";
            lnkToEntity.Text = org.Name;
            lnkToEntity.NavigateUrl = "OrganisationDetailV2.aspx?type=view&id=" + org.OrganisationID;

            dt = BookingDB.GetDataTable_Between(fromDate, toDate, null, new Organisation[] { org }, null, chkIncDeleted.Checked, statusIDsToInclude, false, txtBookingNbrSearch.Text.Trim());
        }
        else if (IsValidFormProvider())
        {
            int provider_id = GetFormProvider(false);
            Staff provider = StaffDB.GetByID(provider_id);
            if (provider == null)
                throw new CustomMessageException("Invalid provider ID");

            heading.InnerText = "Bookings List for : ";
            lnkToEntity.Text = provider.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "StaffDetailV2.aspx?type=view&id=" + provider.StaffID;

            dt = BookingDB.GetDataTable_Between(fromDate, toDate, new Staff[] { provider }, null, null, chkIncDeleted.Checked, statusIDsToInclude, false, txtBookingNbrSearch.Text.Trim());
        }
        else
            throw new CustomMessageException("No entity to get bookings for");
        */

        Patient      patient  = null;
        Organisation org      = null;
        Staff        provider = null;


        if (IsValidFormPatient())
        {
            int patientID = GetFormPatient(false);
            patient = PatientDB.GetByID(patientID);
            if (patient == null)
                throw new CustomMessageException("Invalid patient ID");
        }
        if (IsValidFormOrganisation())
        {
            int orgID = GetFormOrganisation(false);
            org = OrganisationDB.GetByID(orgID);
            if (org == null)
                throw new CustomMessageException("Invalid organisation ID");
        }
        if (IsValidFormProvider())
        {
            int provID = GetFormProvider(false);
            provider = StaffDB.GetByID(provID);
            if (provider == null)
                throw new CustomMessageException("Invalid provider ID");
        }

            
        if (patient == null && org == null && provider == null)
            throw new CustomMessageException("No entity to get bookings for");

        UserView userView = UserView.GetInstance();

        lblHeading.Text = "Bookings List for:";
        int items = (patient == null ? 0 : 1) + (provider == null ? 0 : 1) + (org == null ? 0 : 1);
        if (patient != null)
            lblHeading.Text += (items > 1 ? "<br />&nbsp;&nbsp;Patient " : " ") + "<a href=\"PatientDetailV2.aspx?type=view&id=" + patient.PatientID + "\">" + patient.Person.FullnameWithoutMiddlename + "</a>";
        if (provider != null && userView.IsAdminView)
            lblHeading.Text += (items > 1 ? "<br />&nbsp;&nbsp;Provider " : " ") + "<a href=\"StaffDetailV2.aspx?type=view&id=" + provider.StaffID + "\">" + provider.Person.FullnameWithoutMiddlename + "</a>";
        if (provider != null && !userView.IsAdminView)
            lblHeading.Text += (items > 1 ? "<br />&nbsp;&nbsp;Provider " : " ") + provider.Person.FullnameWithoutMiddlename;
        if (org != null)
            lblHeading.Text += (items > 1 ? "<br />&nbsp;&nbsp;Clinic " : " ") + "<a href=\"OrganisationDetailV2.aspx?type=view&id=" + org.OrganisationID + "\">" + org.Name + "</a>";


        if (txtBookingNbrSearch.Text.Trim().Length > 0)
        {
            fromDate = DateTime.MinValue;
            toDate  = DateTime.MinValue;
        }

        dt = BookingDB.GetDataTable_Between(fromDate, toDate, provider == null ? null : new Staff[] { provider }, org == null ? null : new Organisation[] { org }, patient, null, chkIncDeleted.Checked, statusIDsToInclude, false, txtBookingNbrSearch.Text.Trim());

        // above query gets for org OR prov .. so remove those
        for (int i = dt.Rows.Count-1; i >= 0; i--)
        {
            Booking booking = BookingDB.LoadFull(dt.Rows[i]);
            
            if ((patient  != null && (booking.Patient      == null || booking.Patient.PatientID           != patient.PatientID))  ||
                (org      != null && (booking.Organisation == null || booking.Organisation.OrganisationID != org.OrganisationID)) ||
                (provider != null && (booking.Provider     == null || booking.Provider.StaffID            != provider.StaffID))   ||
                (booking.BookingTypeID != 34))
                    dt.Rows.RemoveAt(i);
        }


        // if confirmed by email/sms, display booking_confirmed_by_type.descr
        // if confirmed by person, display their name
        dt.Columns.Add("confirmed_by_text", typeof(string));
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            if (dt.Rows[i]["booking_booking_confirmed_by_type_id"] == DBNull.Value)
                dt.Rows[i]["confirmed_by_text"] = DBNull.Value;
            else if (Convert.ToInt32(dt.Rows[i]["booking_booking_confirmed_by_type_id"]) == 2 || Convert.ToInt32(dt.Rows[i]["booking_booking_confirmed_by_type_id"]) == 3)
                dt.Rows[i]["confirmed_by_text"] = dt.Rows[i]["booking_confirmed_by_type_descr"];
            else if (Convert.ToInt32(dt.Rows[i]["booking_booking_confirmed_by_type_id"]) == 1)
                dt.Rows[i]["confirmed_by_text"] = dt.Rows[i]["person_confirmed_by_person_id"] == DBNull.Value ? (object)DBNull.Value : dt.Rows[i]["person_confirmed_by_firstname"] + " " + dt.Rows[i]["person_confirmed_by_surname"];
        }

        Session["bookinglist_data"] = dt;

        if (dt.Rows.Count > 0)
        {
            if (IsPostBack && Session["bookinglist_sortexpression"] != null && Session["bookinglist_sortexpression"].ToString().Length > 0)
            {
                DataView dataView = new DataView(dt);
                dataView.Sort = Session["bookinglist_sortexpression"].ToString();
                GrdBooking.DataSource = dataView;
            }
            else if (Session["bookinglist_sortexpression"] == null || Session["bookinglist_sortexpression"].ToString().Length == 0)
            {
                DataView dataView = new DataView(dt);
                dataView.Sort = "booking_date_start DESC";
                Session["bookinglist_sortexpression"] = dataView.Sort;
                GrdBooking.DataSource = dataView;
            }
            else
            {
                GrdBooking.DataSource = dt;
            }


            try
            {
                GrdBooking.DataBind();
            }
            catch (Exception ex)
            {
                HideTableAndSetErrorMessage("", ex.ToString());
            }
        }
        else
        {
            dt.Rows.Add(dt.NewRow());
            GrdBooking.DataSource = dt;
            GrdBooking.DataBind();

            int TotalColumns = GrdBooking.Rows[0].Cells.Count;
            GrdBooking.Rows[0].Cells.Clear();
            GrdBooking.Rows[0].Cells.Add(new TableCell());
            GrdBooking.Rows[0].Cells[0].ColumnSpan = TotalColumns;
            GrdBooking.Rows[0].Cells[0].Text = "No Record Found";
        }



        bool hasInvoices = false;
        bool hasCompletedInvoices = false;
        foreach (GridViewRow row in GrdBooking.Rows)
        {
            if (row.RowType == DataControlRowType.Pager)
                continue;
            if (row.FindControl("lblViewInvoice") != null && ((Label)row.FindControl("lblViewInvoice")).Text.Length > 0)
                hasInvoices = true;
            if (row.FindControl("lblGeneratedSystemLetters") != null && ((Label)row.FindControl("lblGeneratedSystemLetters")).Text.Length > 0 && ((Label)row.FindControl("lblGeneratedSystemLetters")).Visible)
                hasCompletedInvoices = true;
        }
        GrdBooking.Columns[12].Visible = hasInvoices;
        GrdBooking.Columns[9].Visible  = hasCompletedInvoices;
    }
    protected void GrdBooking_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (!Utilities.IsDev() && e.Row.RowType != DataControlRowType.Pager)
            e.Row.Cells[0].CssClass = "hiddencol";
    }
    protected void GrdBooking_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        UserView userView        = UserView.GetInstance();
        int      loggedInStaffID = Session["StaffID"] == null ? -1 : Convert.ToInt32(Session["StaffID"]);


        DataTable dt = Session["bookinglist_data"] as DataTable;

        bool tblEmpty = (dt.Rows.Count == 1 && dt.Rows[0][0] == DBNull.Value);
        if (!tblEmpty && e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblId = (Label)e.Row.FindControl("lblId");
            DataRow[] foundRows = dt.Select("booking_booking_id=" + lblId.Text);
            DataRow thisRow = foundRows[0];
            Booking booking = BookingDB.LoadFull(thisRow);


            Label lblGeneratedSystemLetters = (Label)e.Row.FindControl("lblGeneratedSystemLetters");
            if (lblGeneratedSystemLetters != null)
                lblGeneratedSystemLetters.Visible = (booking.BookingStatus.ID == 187);

            Label lblNotes = (Label)e.Row.FindControl("lblNotes");
            if (lblNotes != null)
            {
                if (booking.Patient != null)
                {
                    bool IsMobileDevice = Utilities.IsMobileDevice(Request);
                    lblNotes.Text = Note.GetBookingPopupLinkTextV2(booking.EntityID, booking.NoteCount > 0, true, 1425, 700, "images/notes-bw-24.jpg", "images/notes-24.png", "btnUpdateBookingList.click()", !IsMobileDevice);
                }
            }

            Label lblViewInvoice = (Label)e.Row.FindControl("lblViewInvoice");
            bool canSeeInvoiceInfo = userView.IsAdminView || userView.IsPrincipal || (booking.Provider != null && booking.Provider.StaffID == loggedInStaffID && booking.DateStart > DateTime.Today.AddMonths(-2));
            if (canSeeInvoiceInfo && booking.InvoiceCount > 0 && lblViewInvoice != null)
            {
                string onclick = @"onclick=""javascript:window.showModalDialog('Invoice_ViewV2.aspx?booking_id=" + booking.BookingID + @"', '', 'dialogWidth:820px;dialogHeight:860px;center:yes;resizable:no; scroll:no');return false;""";
                string txt = "<a " + onclick + " href=\"\">View Inv.</a>";
                lblViewInvoice.Text = txt;
            }

            LinkButton lnkReverseInvoice = (LinkButton)e.Row.FindControl("lnkReverseInvoice");
            if (lnkReverseInvoice != null)
                lnkReverseInvoice.Visible = canSeeInvoiceInfo && booking.BookingStatus.ID > 0;  // deceased, cancelled w-or-without inv, completed (not deleted or incomplete)


            HyperLink lnkBookingSheetForPatient = (HyperLink)e.Row.FindControl("lnkBookingSheetForPatient");
            if (lnkBookingSheetForPatient != null)
                lnkBookingSheetForPatient.NavigateUrl = booking.GetBookingSheetLinkV2();

            Utilities.AddConfirmationBox(e);
            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                Utilities.SetEditRowBackColour(e, System.Drawing.Color.LightGoldenrodYellow);
        }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
        }
    }
    protected void GrdBooking_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
    }
    protected void GrdBooking_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
    }
    protected void GrdBooking_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
    }
    protected void GrdBooking_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Reverse")
        {
            try
            {
                UserView userView = UserView.GetInstance();

                int booking_id = Convert.ToInt32(e.CommandArgument);
                Booking booking = BookingDB.GetByID(booking_id);

                string errorString = string.Empty;
                if (!booking.CanReverse(userView.IsAdminView, out errorString))
                    throw new CustomMessageException(errorString);

                booking.Reverse(UserView.GetInstance().IsAdminView, Convert.ToInt32(Session["StaffID"]));

                FillGrid();
            }
            catch (CustomMessageException ex)
            {
                SetErrorMessage(ex.Message);
            }
            catch (Exception ex)
            {
                SetErrorMessage("", ex.ToString());
            }
        }
    }
    protected void GrdBooking_RowEditing(object sender, GridViewEditEventArgs e)
    {
    }
    protected void GridView_Sorting(object sender, GridViewSortEventArgs e)
    {
        // dont allow sorting if in edit mode
        if (GrdBooking.EditIndex >= 0)
            return;

        DataTable dataTable = Session["bookinglist_data"] as DataTable;

        if (dataTable != null)
        {
            if (Session["bookinglist_sortexpression"] == null)
                Session["bookinglist_sortexpression"] = "";

            DataView dataView = new DataView(dataTable);
            string[] sortData = Session["bookinglist_sortexpression"].ToString().Trim().Split(' ');
            string newSortExpr = (e.SortExpression == sortData[0] && sortData[1] == "ASC") ? "DESC" : "ASC";
            dataView.Sort = e.SortExpression + " " + newSortExpr;
            Session["bookinglist_sortexpression"] = e.SortExpression + " " + newSortExpr;

            GrdBooking.DataSource = dataView;
            GrdBooking.DataBind();
        }
    }

    #endregion

    #region btnSearch_Click

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        HideErrorMessage();

        if (txtStartDate.Text.Length > 0 && (!Regex.IsMatch(txtStartDate.Text, @"^\d{2}\-\d{2}\-\d{4}$") || !IsValidDate(txtStartDate.Text)))
        {
            SetErrorMessage("Start date must be empty or valid and of the format dd-mm-yyyy");
            return;
        }
        if (txtEndDate.Text.Length   > 0 && (!Regex.IsMatch(txtEndDate.Text,   @"^\d{2}\-\d{2}\-\d{4}$") || !IsValidDate(txtEndDate.Text)))
        {
            SetErrorMessage("End date must be empty or valid and of the format dd-mm-yyyy");
            return;
        }


        DateTime startDate = txtStartDate.Text.Length == 0 ? DateTime.MinValue : GetDateFromString(txtStartDate.Text, "dd-mm-yyyy");
        DateTime endDate   = txtEndDate.Text.Length   == 0 ? DateTime.MinValue : GetDateFromString(txtEndDate.Text, "dd-mm-yyyy");

        string url = ClearSearchesFromUrl(Request.RawUrl);
        url = UrlParamModifier.AddEdit(url, "start_date"    , startDate == DateTime.MinValue ? "" : startDate.ToString("yyyy_MM_dd"));
        url = UrlParamModifier.AddEdit(url, "end_date"      , endDate   == DateTime.MinValue ? "" : endDate.ToString("yyyy_MM_dd"));
        url = UrlParamModifier.AddEdit(url, "inc_completed" , chkIncCompleted.Checked  ? "1" : "0");
        url = UrlParamModifier.AddEdit(url, "inc_incomplete", chkIncIncomplete.Checked ? "1" : "0");
        url = UrlParamModifier.AddEdit(url, "inc_cancelled" , chkIncCancelled.Checked  ? "1" : "0");
        url = UrlParamModifier.AddEdit(url, "inc_deleted"   , chkIncDeleted.Checked    ? "1" : "0");

        url = UrlParamModifier.Update(txtBookingNbrSearch.Text.Trim().Length > 0, url, "booking_nbr_search", txtBookingNbrSearch.Text.Trim());

        Response.Redirect(url);
    }

    protected string ClearSearchesFromUrl(string url)
    {
        url = UrlParamModifier.Remove(url, "start_date");
        url = UrlParamModifier.Remove(url, "end_date");
        url = UrlParamModifier.Remove(url, "inc_completed");
        url = UrlParamModifier.Remove(url, "inc_incomplete");
        url = UrlParamModifier.Remove(url, "inc_cancelled");
        url = UrlParamModifier.Remove(url, "booking_nbr_search");

        return url;
    }

    #endregion

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        // re-fill grid in case two broswer windows are open with 2 different booking lists and only the one on the other page is in the session memory...
        FillGrid();

        try
        {
            string originalFile        = Letter.GetLettersDirectory() + @"BookingList.docx";
            string tmpLettersDirectory = Letter.GetTempLettersDirectory();
            string tmpOutputFile       = FileHelper.GetTempFileName(tmpLettersDirectory + "BookingList." + System.IO.Path.GetExtension(originalFile));


            // create table data to populate

            DataTable dt      = Session["bookinglist_data"] as DataTable;
            string[,] tblInfo = null;
            bool tblEmpty = (dt.Rows.Count == 1 && dt.Rows[0][0] == DBNull.Value);
            if (tblEmpty)
            {
                tblInfo = new string[1, 4];
                tblInfo[0, 0] = "No Bookings Found";
                tblInfo[0, 1] = "";
                tblInfo[0, 2] = "";
                tblInfo[0, 3] = "";
            }
            else
            {
                tblInfo = new string[dt.Rows.Count, 4];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Booking booking = BookingDB.LoadFull(dt.Rows[i]);
                    tblInfo[i, 0] = booking.DateStart.ToString("d MMM") + " " + booking.DateStart.ToString("h:mm") + " - " + booking.DateEnd.ToString("h:mm") + (booking.DateEnd.Hour < 12 ? "am" : "pm");
                    tblInfo[i, 1] = booking.Patient == null ? "" : booking.Patient.Person.FullnameWithoutMiddlename;
                    tblInfo[i, 2] = booking.Provider.Person.FullnameWithoutMiddlename;
                    tblInfo[i, 3] = booking.Organisation.Name;
                }
            }


            // create empty dataset

            System.Data.DataSet sourceDataSet = new System.Data.DataSet();
            sourceDataSet.Tables.Add("MergeIt");


            // merge

            string errorString = null;
            WordMailMerger.Merge(

                originalFile,
                tmpOutputFile,
                sourceDataSet,

                tblInfo,
                1,
                true,

                true,
                null,
                true,
                null,
                out errorString);

            if (errorString != string.Empty)
                throw new CustomMessageException(errorString);

            Letter.FileContents fileContents = new Letter.FileContents(System.IO.File.ReadAllBytes(tmpOutputFile), "BookingList." + System.IO.Path.GetExtension(originalFile));
            System.IO.File.Delete(tmpOutputFile);


            // Nothing gets past the "DownloadDocument" method because it outputs the file 
            // which is writing a response to the client browser and calls Response.End()
            // So make sure any other code that functions goes before this
            Letter.DownloadDocument(Response, fileContents.Contents, fileContents.DocName);
        }
        catch(CustomMessageException cmEx)
        {
            SetErrorMessage(cmEx.Message);
        }
        catch(Exception ex)
        {
            SetErrorMessage(ex.ToString());
        }


    }

    #region SetErrorMessage, HideErrorMessage

    private void HideTableAndSetErrorMessage(string errMsg = "", string details = "")
    {
        GrdBooking.Visible = false;
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