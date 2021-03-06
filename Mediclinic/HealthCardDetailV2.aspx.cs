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

public partial class HealthCardDetailV2 : System.Web.UI.Page
{

    #region Page_Load

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            if (!IsPostBack)
                Utilities.SetNoCache(Response);
            HideErrorMessage();

            if (!IsPostBack)
            {
                Session.Remove("epcremaining_sortexpression");
                Session.Remove("epcremaining_data");
                SetupGUI();

                hiddenIsMobileDevice.Value = Utilities.IsMobileDevice(Request) ? "1" : "0";

                UrlParamCard urlParamCard = GetUrlParamCard();
                if ((GetUrlParamType() == UrlParamType.Edit || GetUrlParamType() == UrlParamType.View) && IsValidFormID())
                {
                    HealthCard health_card = HealthCardDB.GetByID(GetFormID());
                    if (health_card != null)
                    {
                        health_card.Organisation = OrganisationDB.GetByID(health_card.Organisation.OrganisationID);
                        SetBackLink(PatientDB.GetByID(health_card.Patient.PatientID));
                    }

                    if ((health_card != null && urlParamCard == UrlParamCard.Medicare) ||
                        (health_card != null && urlParamCard == UrlParamCard.DVA)      ||
                        (health_card != null && urlParamCard == UrlParamCard.Insurance))
                    {
                        FillEditViewForm(health_card, urlParamCard, GetUrlParamType() == UrlParamType.Edit);
                        patientReferrer.SetInfo(health_card.Patient.PatientID, "view");
                        //healthCardInfoControl.SetInfo(patient.PatientID, false, true, true, false, false);
                    }
                    else
                        HideTableAndSetErrorMessage();
                }
                else if (GetUrlParamType() == UrlParamType.Add && IsValidFormID())
                {
                    Patient patient = PatientDB.GetByID(GetFormID());
                    if (patient != null)
                        SetBackLink(patient);

                    if (patient != null && urlParamCard == UrlParamCard.Medicare)
                        FillEmptyAddForm(patient, urlParamCard);
                    else if (patient != null && urlParamCard == UrlParamCard.DVA)
                        FillEmptyAddForm(patient, urlParamCard);
                    else if (patient != null && urlParamCard == UrlParamCard.Insurance)
                        FillEmptyAddForm(patient, urlParamCard);
                    else
                        HideTableAndSetErrorMessage();
                }
                else
                    HideTableAndSetErrorMessage("", "Invalid URL Parameters");
            }
            else
            {
                DataTable dt = Session["hcaction_data"] as DataTable;
                if (dt != null)
                {
                    bool tblEmpty = (dt.Rows.Count == 1 && dt.Rows[0][0] == DBNull.Value);
                    if (tblEmpty)
                    {
                        GrdHealthCardAction.DataSource = dt;
                        GrdHealthCardAction.DataBind();

                        int TotalColumns = GrdHealthCardAction.Rows[0].Cells.Count;
                        GrdHealthCardAction.Rows[0].Cells.Clear();
                        GrdHealthCardAction.Rows[0].Cells.Add(new TableCell());
                        GrdHealthCardAction.Rows[0].Cells[0].ColumnSpan = TotalColumns;
                        GrdHealthCardAction.Rows[0].Cells[0].Text = "No Record Found";
                    }
                }
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

    private void SetBackLink(Patient patient)
    {
        this.lnkThisPatient.NavigateUrl = "~/PatientDetailV2.aspx?type=view&id=" + patient.PatientID;
        this.lnkThisPatient.Text = "Back to " + patient.Person.Firstname + " " + patient.Person.Surname;
    }

    #endregion

    #region GetUrlParamCard(), GetUrlParamType(), IsValidFormID(), GetFormID()

    private bool IsValidFormID()
    {
        string id = Request.QueryString["id"];
        return id != null && Regex.IsMatch(id, @"^\d+$");
    }
    private int GetFormID()
    {
        if (!IsValidFormID())
            throw new Exception("Invalid url id");

        string id = Request.QueryString["id"];
        return Convert.ToInt32(id);
    }

    private enum UrlParamType { Add, Edit, View, None };
    private UrlParamType GetUrlParamType()
    {
        string type = Request.QueryString["type"];
        if (type != null && type.ToLower() == "add")
            return UrlParamType.Add;
        else if (type != null && type.ToLower() == "edit")
            return UrlParamType.Edit;
        else if (type != null && type.ToLower() == "view")
            return UrlParamType.View;
        else
            return UrlParamType.None;
    }

    private enum UrlParamCard { Medicare, DVA, Insurance, None };
    private UrlParamCard GetUrlParamCard()
    {
        string card = Request.QueryString["card"];
        if (card != null && card.ToLower() == "medicare")
            return UrlParamCard.Medicare;
        else if (card != null && card.ToLower() == "dva")
            return UrlParamCard.DVA;
        else if (card != null && card.ToLower() == "ins")
            return UrlParamCard.Insurance;
        else
            return UrlParamCard.None;
    }

    #endregion

    #region SetupGUI()

    public void SetupGUI()
    {
        UrlParamCard urlParamCard = GetUrlParamCard();

        if (urlParamCard == UrlParamCard.Insurance)
        {
            lblCardNbrText.Text = "Claim Nbr";
            this.lnkEditEPC.Visible = false;
            this.lblActions.Visible = false;
            GrdHealthCardAction.Visible = false;
            this.lblEPCInfoText.Visible = false;
        }

        this.lnkEditEPC.Text = "Referral Info";
        this.lblActions.Text = "&nbsp;Referral Letters Sent<br />";
        this.lblEPCInfoText.Text = "Referral Info";

        this.lnkThisPatient.Visible = false;


        lblOrganisationText.Text = (urlParamCard == UrlParamCard.Insurance) ? "Company" : "Organisation";
        DataTable orgs = (urlParamCard == UrlParamCard.Insurance) ? OrganisationDB.GetDataTable_Insurance() : OrganisationDB.GetDataTable_GroupOrganisations();
        if (ddlOrganisation != null)
        {
            ddlOrganisation.DataSource = orgs;
            ddlOrganisation.DataTextField = "name";
            ddlOrganisation.DataValueField = "organisation_id";
            ddlOrganisation.DataBind();
        }

        if (ddlCardFamilyMemberNbr != null)
        {
            ddlCardFamilyMemberNbr.Items.Add(new ListItem("", ""));
            for (int i = 1; i < 10; i++)
                ddlCardFamilyMemberNbr.Items.Add(new ListItem(i.ToString(), i.ToString()));
        }

        ddlExpiry_Month.Items.Add(new ListItem("--", "-1"));
        ddlExpiry_Year.Items.Add(new ListItem("--", "-1"));

        for (int i = 1; i <= 12; i++)
            ddlExpiry_Month.Items.Add(new ListItem(i.ToString(), i.ToString()));
        for (int i = DateTime.Today.Year - 10; i <= DateTime.Today.Year + 10; i++)
            ddlExpiry_Year.Items.Add(new ListItem(i.ToString(), i.ToString()));

        if (Utilities.IsMobileDevice(Request))
        {
            txtCardNbr_Digit_1.Width =
            txtCardNbr_Digit_2.Width =
            txtCardNbr_Digit_3.Width =
            txtCardNbr_Digit_4.Width =
            txtCardNbr_Digit_5.Width =
            txtCardNbr_Digit_6.Width =
            txtCardNbr_Digit_7.Width =
            txtCardNbr_Digit_8.Width =
            txtCardNbr_Digit_9.Width =
            txtCardNbr_Digit_10.Width = new Unit(28, UnitType.Pixel);
        }




        bool editable = GetUrlParamType() == UrlParamType.Add || GetUrlParamType() == UrlParamType.Edit;
        Utilities.SetEditControlBackColour(ddlOrganisation, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardName, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_1, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_2, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_3, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_4, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_5, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_6, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_7, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_8, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_9, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr_Digit_10, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCardNbr, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(ddlCardFamilyMemberNbr, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(ddlExpiry_Month, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(ddlExpiry_Year, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtAreaTreated, editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
    }

    #endregion


    private void UpdateHistory()
    {
        patientReferrer.ReSetInfo();  // update referrer because will disallow removal of referrer when card set

        string new_referral_added_text = "New Referral Added";

        DataTable dt = new DataTable();
        dt.Columns.AddRange(new DataColumn[] { new DataColumn("new_referral_added_text"), new DataColumn("is_new_epc_card_set"), new DataColumn("date"), new DataColumn("staff_id"), new DataColumn("staff_name"), new DataColumn("desc"), new DataColumn("before"), new DataColumn("after") });

        ArrayList list = HealthCard.GetFullEPCChangeHistory(GetFormID());
        foreach (object o in list)
        {
            if (o is HealthCardEPCChangeHistory)
            {
                HealthCardEPCChangeHistory epcChangeHistory = (HealthCardEPCChangeHistory)o;
                if (epcChangeHistory.IsNewEpcCardSet)
                {
                    dt.Rows.Add(new_referral_added_text, true, epcChangeHistory.Date.ToString("dd-MM-yyyy"), epcChangeHistory.Staff.StaffID, "", "Rcvd In Office", epcChangeHistory.PreDateReferralReceivedInOffice == DateTime.MinValue ? "--" : epcChangeHistory.PreDateReferralReceivedInOffice.ToString("dd-MM-yyyy"), epcChangeHistory.PostDateReferralReceivedInOffice == DateTime.MinValue ? "--" : epcChangeHistory.PostDateReferralReceivedInOffice.ToString("dd-MM-yyyy"));
                    dt.Rows.Add(new_referral_added_text, false, epcChangeHistory.Date.ToString("dd-MM-yyyy"), epcChangeHistory.Staff.StaffID, "", "Signed", epcChangeHistory.PreDateReferralSigned == DateTime.MinValue ? "--" : epcChangeHistory.PreDateReferralSigned.ToString("dd-MM-yyyy"), epcChangeHistory.PostDateReferralSigned == DateTime.MinValue ? "--" : epcChangeHistory.PostDateReferralSigned.ToString("dd-MM-yyyy"));
                }
                else
                {
                    if (epcChangeHistory.PreDateReferralReceivedInOffice != epcChangeHistory.PostDateReferralReceivedInOffice)
                        dt.Rows.Add(new_referral_added_text, false, epcChangeHistory.Date.ToString("dd-MM-yyyy"), epcChangeHistory.Staff.StaffID, epcChangeHistory.Staff.Person.FullnameWithoutMiddlename, "Rcvd In Office", epcChangeHistory.PreDateReferralReceivedInOffice == DateTime.MinValue ? "--" : epcChangeHistory.PreDateReferralReceivedInOffice.ToString("dd-MM-yyyy"), epcChangeHistory.PostDateReferralReceivedInOffice == DateTime.MinValue ? "--" : epcChangeHistory.PostDateReferralReceivedInOffice.ToString("dd-MM-yyyy"));
                    if (epcChangeHistory.PreDateReferralSigned != epcChangeHistory.PostDateReferralSigned)
                        dt.Rows.Add(new_referral_added_text, false, epcChangeHistory.Date.ToString("dd-MM-yyyy"), epcChangeHistory.Staff.StaffID, epcChangeHistory.Staff.Person.FullnameWithoutMiddlename, "Signed", epcChangeHistory.PreDateReferralSigned == DateTime.MinValue ? "--" : epcChangeHistory.PreDateReferralSigned.ToString("dd-MM-yyyy"), epcChangeHistory.PostDateReferralSigned == DateTime.MinValue ? "--" : epcChangeHistory.PostDateReferralSigned.ToString("dd-MM-yyyy"));
                }
            }
            else if (o is HealthCardEPCRemainingChangeHistory)
            {
                HealthCardEPCRemainingChangeHistory epcRemainingChangeHistory = (HealthCardEPCRemainingChangeHistory)o;
                if (epcRemainingChangeHistory.PreNumServicesRemaining == -1 ||
                    epcRemainingChangeHistory.PostNumServicesRemaining == -1 ||
                    epcRemainingChangeHistory.PreNumServicesRemaining != epcRemainingChangeHistory.PostNumServicesRemaining)
                    dt.Rows.Add(new_referral_added_text, false, epcRemainingChangeHistory.Date.ToString("dd-MM-yyyy"), epcRemainingChangeHistory.Staff.StaffID, epcRemainingChangeHistory.Staff.Person.FullnameWithoutMiddlename, epcRemainingChangeHistory.HealthCardEpcRemaining.Field.Descr, epcRemainingChangeHistory.PreNumServicesRemaining == -1 ? "Created" : epcRemainingChangeHistory.PreNumServicesRemaining.ToString(), epcRemainingChangeHistory.PostNumServicesRemaining == -1 ? "Deleted" : epcRemainingChangeHistory.PostNumServicesRemaining.ToString());
            }
        }

        tr_epc_change_history.Visible = list.Count > 0;
        tr_epc_change_history_space_row.Visible = list.Count > 0;
        if (list.Count > 0)
        {
            lstEPCChangeHistory.DataSource = dt;
            lstEPCChangeHistory.DataBind();

            Label lblEPCChangeHistoryText = (Label)lstEPCChangeHistory.Controls[0].Controls[0].FindControl("lblEPCChangeHistoryText");
            lblEPCChangeHistoryText.Text = "Referral Change History";
        }

        ClientScript.RegisterStartupScript(Page.ClientScript.GetType(), Page.ClientID, "ScrollEPCHistoryToBottom()", true);
    }

    protected void btnUpdateEPCInfo_Click(object sender, EventArgs e)
    {
        HealthCard health_card = HealthCardDB.GetByID(GetFormID());
        health_card.Organisation = OrganisationDB.GetByID(health_card.Organisation.OrganisationID);
        FillEditViewForm(health_card, GetUrlParamCard(), GetUrlParamType() == UrlParamType.Edit);
    }

    private void FillEditViewForm(HealthCard health_card, UrlParamCard cardType, bool isEditMode)
    {
        health_card.Patient = PatientDB.GetByID(health_card.Patient.PatientID);

        if (!Utilities.IsDev())
            idRow.Attributes["class"] = "hiddencol";

        UpdateHistory();


        if (cardType == UrlParamCard.Medicare)
        {
            if (health_card.Organisation.OrganisationID != -1)
            {
                HideTableAndSetErrorMessage("");
                return;
            }

            lblHeading.Text = "Medicare Card Information For ";
            lnkToEntity.Text = health_card.Patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + health_card.Patient.PatientID;
            lblOrganisation.Text = "Medicare";

            txtCardNbr.Width = Unit.Pixel(89);

            area_treated.Visible = false;
        }
        else if (cardType == UrlParamCard.DVA)
        {
            if (health_card.Organisation.OrganisationID != -2)
            {
                HideTableAndSetErrorMessage("");
                return;
            }

            lblHeading.Text = "DVA Card Information For ";
            lnkToEntity.Text = health_card.Patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + health_card.Patient.PatientID;
            lblOrganisation.Text = "DVA";

            lblCardNbrFamilyNbrSeperator.Visible = false;
            ddlCardFamilyMemberNbr.Visible = false;

            area_treated.Visible = true;
        }
        else if (cardType == UrlParamCard.Insurance)
        {
            lblHeading.Text = "Insurance Card Information For ";
            lnkToEntity.Text = health_card.Patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + health_card.Patient.PatientID;
            lblOrganisation.Text = health_card.Organisation.Name;

            lblCardNbrFamilyNbrSeperator.Visible = false;
            ddlCardFamilyMemberNbr.Visible = false;

            area_treated.Visible = true;
        }
        else
            HideTableAndSetErrorMessage();

        lnkGoToBookingScreen.NavigateUrl = "~/SelectOrganisationsV2.aspx?patient=" + health_card.Patient.PatientID.ToString();

        lblId.Text = health_card.HealthCardID.ToString();


        if (ddlOrganisation.Items.FindByValue(health_card.Organisation.OrganisationID.ToString()) == null)
            ddlOrganisation.Items.Add(new ListItem(health_card.Organisation.Name,  health_card.Organisation.OrganisationID.ToString()));
        ddlOrganisation.SelectedValue = health_card.Organisation.OrganisationID.ToString();
        ddlOrganisation.Visible = isEditMode && cardType == UrlParamCard.Insurance;
        lblLastModBy.Text = health_card.AddedOrLastModifiedBy == null ? string.Empty : health_card.AddedOrLastModifiedBy.Person.FullnameWithoutMiddlename;
        lblLastModDate.Text = health_card.AddedOrLastModifiedDate == DateTime.MinValue ? string.Empty : health_card.AddedOrLastModifiedDate.ToString("dd-MM-yyyy");


        lblDateReferralSigned.Text = health_card.DateReferralSigned == DateTime.MinValue ? "--" : health_card.DateReferralSigned.ToString("dd-MM-yyyy");
        lblDateReferralReceived.Text = health_card.DateReferralReceivedInOffice == DateTime.MinValue ? "--" : health_card.DateReferralReceivedInOffice.ToString("dd-MM-yyyy");

        DataTable dt = HealthCardEPCRemainingDB.GetDataTable_ByHealthCardID(health_card.HealthCardID);
        lstEPCRemaining.DataSource = dt;
        lstEPCRemaining.DataBind();


        bool hasEpcRemainingRows = HealthCardEPCRemainingDB.GetCountByHealthCardID(health_card.HealthCardID) > 0;
        bool hasEPC = health_card.HasEPC();

        tr_epc_space_row.Visible = hasEPC;
        tr_referral_received.Visible = hasEPC;
        tr_referral_signed.Visible = hasEPC;
        lstEPCRemaining.Visible = hasEpcRemainingRows;
        tr_epc_change_remaining_space_row.Visible = hasEPC;


        lnkEditEPC.Visible = true;
        lnkNewEPC.Visible = true;
        tr_epc_heading.Visible = true;
        tr_epc_space_row.Visible = true;

        if (!hasEPC)
        {
            //string allFeatures = "dialogWidth:550px;dialogHeight:550px;center:yes;resizable:no; scroll:no";
            //string js = "javascript:window.showModalDialog('EPCDetailV2.aspx?type=add&id=" + health_card.HealthCardID.ToString() + "', '', '" + allFeatures + "');document.getElementById('btnUpdateEPCInfo').click();return false;";
            string js = "javascript:show_modal_updade_epc(" + health_card.HealthCardID.ToString() + ");document.getElementById('btnUpdateEPCInfo').click();return false;";

            this.lnkNewEPC.Visible = cardType != UrlParamCard.Insurance;
            this.lnkNewEPC.NavigateUrl = "  ";
            this.lnkNewEPC.Text = "Add Referral";
            this.lnkNewEPC.Attributes.Add("onclick", js);

            lnkEditEPC.Visible = false;
        }
        else
        {
            tr_epc_heading.Visible = true;
            tr_epc_space_row.Visible = true;


            string allFeatures = "dialogWidth:550px;dialogHeight:550px;center:yes;resizable:no; scroll:no";
            //string js = "javascript:window.showModalDialog('EPCDetailV2.aspx?type=add&id=" + health_card.HealthCardID.ToString() + "', '', '" + allFeatures + "');document.getElementById('btnUpdateEPCInfo').click();return false;";
            string js = "javascript:show_modal_updade_epc(" + health_card.HealthCardID.ToString() + ");document.getElementById('btnUpdateEPCInfo').click();return false;";

            this.lnkNewEPC.Visible = true;
            this.lnkNewEPC.NavigateUrl = "  ";
            this.lnkNewEPC.Text = "Replace Referral";
            this.lnkNewEPC.Attributes.Add("onclick", js);



            if (!Utilities.IsMobileDevice(Request))
            {
                allFeatures = "dialogWidth:440px;dialogHeight:440px;center:yes;resizable:no; scroll:no";
                js = "javascript:window.showModalDialog('EPCDetailV2.aspx?type=edit&id=" + health_card.HealthCardID.ToString() + "', '', '" + allFeatures + "');document.getElementById('btnUpdateEPCInfo').click();return false;";
            }
            else
            {
                js = "open_new_tab('EPCDetailV2.aspx?type=edit&id=" + health_card.HealthCardID.ToString() + "');return false;";
            }


            this.lnkEditEPC.Visible = true;
            this.lnkEditEPC.NavigateUrl = "  ";
            this.lnkEditEPC.Text = "Edit Referral";
            this.lnkEditEPC.Attributes.Add("onclick", js);
        }


        if (isEditMode)
        {
            txtCardName.Text = health_card.CardName;
            txtCardNbr.Text = health_card.CardNbr;
            if (health_card.CardNbr.Length > 0)
                txtCardNbr_Digit_1.Text = health_card.CardNbr[0].ToString().Trim();
            if (health_card.CardNbr.Length > 1)
                txtCardNbr_Digit_2.Text = health_card.CardNbr[1].ToString().Trim();
            if (health_card.CardNbr.Length > 2)
                txtCardNbr_Digit_3.Text = health_card.CardNbr[2].ToString().Trim();
            if (health_card.CardNbr.Length > 3)
                txtCardNbr_Digit_4.Text = health_card.CardNbr[3].ToString().Trim();
            if (health_card.CardNbr.Length > 4)
                txtCardNbr_Digit_5.Text = health_card.CardNbr[4].ToString().Trim();
            if (health_card.CardNbr.Length > 5)
                txtCardNbr_Digit_6.Text = health_card.CardNbr[5].ToString().Trim();
            if (health_card.CardNbr.Length > 6)
                txtCardNbr_Digit_7.Text = health_card.CardNbr[6].ToString().Trim();
            if (health_card.CardNbr.Length > 7)
                txtCardNbr_Digit_8.Text = health_card.CardNbr[7].ToString().Trim();
            if (health_card.CardNbr.Length > 8)
                txtCardNbr_Digit_9.Text = health_card.CardNbr[8].ToString().Trim();
            if (health_card.CardNbr.Length > 9)
                txtCardNbr_Digit_10.Text = health_card.CardNbr[9].ToString().Trim();


            if (health_card.Organisation.OrganisationID == -1)
            {
                txtCardNbr.Visible = false;
                txtValidateCardNbrRequired.Visible = false;
            }
            else if (health_card.Organisation.OrganisationID == -2)
            {
                txtCardNbr_Digit_1.Visible = false;
                txtCardNbr_Digit_2.Visible = false;
                txtCardNbr_Digit_3.Visible = false;
                txtCardNbr_Digit_4.Visible = false;
                txtCardNbr_Digit_5.Visible = false;
                txtCardNbr_Digit_6.Visible = false;
                txtCardNbr_Digit_7.Visible = false;
                txtCardNbr_Digit_8.Visible = false;
                txtCardNbr_Digit_9.Visible = false;
                txtCardNbr_Digit_10.Visible = false;
                txtValidateCardNbrsRequired.Visible = false;
            }
            else // insurance card
            {
                txtCardNbr_Digit_1.Visible = false;
                txtCardNbr_Digit_2.Visible = false;
                txtCardNbr_Digit_3.Visible = false;
                txtCardNbr_Digit_4.Visible = false;
                txtCardNbr_Digit_5.Visible = false;
                txtCardNbr_Digit_6.Visible = false;
                txtCardNbr_Digit_7.Visible = false;
                txtCardNbr_Digit_8.Visible = false;
                txtCardNbr_Digit_9.Visible = false;
                txtCardNbr_Digit_10.Visible = false;
                txtValidateCardNbrsRequired.Visible = false;
                lblOrganisation.Visible = false;
            }

            try
            {
                if (ddlCardFamilyMemberNbr.Items.FindByValue(health_card.CardFamilyMemberNbr) != null)
                    ddlCardFamilyMemberNbr.SelectedValue = health_card.CardFamilyMemberNbr;
            }
            catch (Exception) { ; }

            chkIsActive.Checked = health_card.IsActive;
            if (health_card.IsActive)
                chkIsActive.Attributes["onclick"] = "return false";


            if (health_card.ExpiryDate != DateTime.MinValue)
            {
                Utilities.AddIfNotExists(ddlExpiry_Month, health_card.ExpiryDate.Month);
                ddlExpiry_Month.SelectedValue = health_card.ExpiryDate.Month.ToString();

                Utilities.AddIfNotExists(ddlExpiry_Year, health_card.ExpiryDate.Year);
                ddlExpiry_Year.SelectedValue = health_card.ExpiryDate.Year.ToString();
            }

            txtAreaTreated.Text = health_card.AreaTreated;

            lblCardName.Visible = false;
            lblCardNbr.Visible = false;
            lblIsActive.Visible = false;
            lblExpiry.Visible = false;
            lblAreaTreated.Visible = false;
        }
        else
        {
            lblCardName.Text = health_card.CardName.Length == 0 ? "--" : health_card.CardName;
            lblCardNbr.Text = health_card.CardNbr.Length == 0 ? "--" : health_card.CardNbr + (health_card.Organisation.OrganisationID == -1 ? " - " + health_card.CardFamilyMemberNbr : "");

            lblOrganisation.Font.Bold = true;


            txtCardName.Visible = false;
            txtCardNbr.Visible = false;

            txtCardNbr.Visible = false;
            txtCardNbr_Digit_1.Visible = false;
            txtCardNbr_Digit_2.Visible = false;
            txtCardNbr_Digit_3.Visible = false;
            txtCardNbr_Digit_4.Visible = false;
            txtCardNbr_Digit_5.Visible = false;
            txtCardNbr_Digit_6.Visible = false;
            txtCardNbr_Digit_7.Visible = false;
            txtCardNbr_Digit_8.Visible = false;
            txtCardNbr_Digit_9.Visible = false;
            txtCardNbr_Digit_10.Visible = false;
            txtValidateCardNbrsRequired.Visible = false;
            lblIsActive.Text = health_card.IsActive ? "Yes" : "No";
            lblExpiry.Text = health_card.ExpiryDate == DateTime.MinValue ? "--" : health_card.ExpiryDate.ToString("MM  '/'  yyyy");
            lblAreaTreated.Text = health_card.AreaTreated.Length == 0 ? "--" : health_card.AreaTreated;

            ddlCardFamilyMemberNbr.Visible = false;
            lblCardNbrFamilyNbrSeperator.Visible = false;
            chkIsActive.Visible = false;
            ddlExpiry_Month.Visible = false;
            ddlExpiry_Year.Visible = false;
            txtAreaTreated.Visible = false;
        }


        FillHealthCardActionGrid();

        btnSubmit.Text = isEditMode ? "Update Details" : "Edit Details";
        btnCancel.Visible = isEditMode;
    }

    private void FillEmptyAddForm(Patient patient, UrlParamCard cardType)
    {
        if (cardType == UrlParamCard.Insurance && ddlOrganisation.Items.Count == 0)
        {
            HideTableAndSetErrorMessage("Please enter an insurance company into the system before adding an insurance card");
        }

        idRow.Visible = false;

        ddlOrganisation.SelectedValue = cardType == UrlParamCard.Medicare ? "1" : "0";
        ddlOrganisation.Visible       = cardType == UrlParamCard.Insurance;
        lblOrganisation.Visible       = cardType != UrlParamCard.Insurance;
        addOrModByRow.Visible                       = false;
        addOrModDateRow.Visible                     = false;
        tr_epc_heading.Visible                      = false;
        tr_epc_space_row.Visible                    = false;
        tr_referral_received.Visible                = false;
        tr_referral_signed.Visible                  = false;
        lstEPCRemaining.Visible                     = false;
        tr_epc_change_remaining_space_row.Visible   = false;
        pnlGrdHealthCardAction.Visible              = false;
        patientReferrer.Visible                     = false;
        lblActions.Visible                          = false;

        lnkGoToBookingScreen.NavigateUrl = "~/SelectOrganisationsV2.aspx?patient=" + patient.PatientID.ToString();
        lnkGoToBookingScreen.Visible = false;

        HealthCard activeHealthCard = HealthCardDB.GetActiveByPatientID(patient.PatientID);

        if (cardType == UrlParamCard.Medicare)
        {
            lblHeading.Text = "Add Medicare Card Information For ";
            lnkToEntity.Text = patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + patient.PatientID;

            lblOrganisation.Text = "Medicare";
            ddlOrganisation.SelectedValue = "-1";
            txtCardNbr.Width = Unit.Pixel(89);
            ddlCardFamilyMemberNbr.SelectedValue = "1";

            txtCardNbr.Visible = false;
            txtValidateCardNbrRequired.Visible = false;

            //chkIsActive.Checked = !HealthCardDB.ActiveCardExistsFor(patient.PatientID, 0, -1, -1);

            // if no active DVA card, check it to set as active.
            // if no active card at all, force it to be active.
            chkIsActive.Checked = (activeHealthCard == null || (activeHealthCard.Organisation.OrganisationID != -2 && activeHealthCard.Organisation.OrganisationID != -3));
            if (activeHealthCard == null)
                chkIsActive.Attributes["onclick"] = "return false";

            area_treated.Visible = false;

            btnSubmit.Text = "Add Medicare Card";
        }
        else if (cardType == UrlParamCard.DVA)
        {
            lblHeading.Text = "Add DVA Card Information For ";
            lnkToEntity.Text = patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + patient.PatientID;

            lblOrganisation.Text = "DVA";
            ddlOrganisation.SelectedValue = "-2";
            ddlCardFamilyMemberNbr.Visible = false;
            lblCardNbrFamilyNbrSeperator.Visible = false;

            txtCardNbr_Digit_1.Visible = false;
            txtCardNbr_Digit_2.Visible = false;
            txtCardNbr_Digit_3.Visible = false;
            txtCardNbr_Digit_4.Visible = false;
            txtCardNbr_Digit_5.Visible = false;
            txtCardNbr_Digit_6.Visible = false;
            txtCardNbr_Digit_7.Visible = false;
            txtCardNbr_Digit_8.Visible = false;
            txtCardNbr_Digit_9.Visible = false;
            txtCardNbr_Digit_10.Visible = false;
            txtValidateCardNbrsRequired.Visible = false;

            //chkIsActive.Checked = !HealthCardDB.ActiveCardExistsFor(patient.PatientID, 0, -1, -2);

            // if no active DVA card, check it to set as active.
            // if no active card at all, force it to be active.
            chkIsActive.Checked = (activeHealthCard == null || (activeHealthCard.Organisation.OrganisationID != -1 && activeHealthCard.Organisation.OrganisationID != -3));
            if (activeHealthCard == null)
                chkIsActive.Attributes["onclick"] = "return false";

            area_treated.Visible = true;


            btnSubmit.Text = "Add DVA Card";
        }
        else if (cardType == UrlParamCard.Insurance)
        {
            lblHeading.Text = "Add Ins. Card Information For ";
            lnkToEntity.Text = patient.Person.FullnameWithoutMiddlename;
            lnkToEntity.NavigateUrl = "PatientDetailV2.aspx?type=view&id=" + patient.PatientID;

            lblOrganisation.Text = "Insurance";
            ddlOrganisation.SelectedValue = "-3";
            ddlCardFamilyMemberNbr.Visible = false;
            lblCardNbrFamilyNbrSeperator.Visible = false;

            txtCardNbr_Digit_1.Visible = false;
            txtCardNbr_Digit_2.Visible = false;
            txtCardNbr_Digit_3.Visible = false;
            txtCardNbr_Digit_4.Visible = false;
            txtCardNbr_Digit_5.Visible = false;
            txtCardNbr_Digit_6.Visible = false;
            txtCardNbr_Digit_7.Visible = false;
            txtCardNbr_Digit_8.Visible = false;
            txtCardNbr_Digit_9.Visible = false;
            txtCardNbr_Digit_10.Visible = false;
            txtValidateCardNbrsRequired.Visible = false;

            //chkIsActive.Checked = !HealthCardDB.ActiveCardExistsFor(patient.PatientID, 0, -1, -2);

            // if no active Insurance card, check it to set as active.
            // if no active card at all, force it to be active.
            chkIsActive.Checked = (activeHealthCard == null || (activeHealthCard.Organisation.OrganisationID != -1 && activeHealthCard.Organisation.OrganisationID != -2));
            if (activeHealthCard == null)
                chkIsActive.Attributes["onclick"] = "return false";

            area_treated.Visible = false;


            btnSubmit.Text = "Add Ins Card";
        }
        else
            HideTableAndSetErrorMessage();
    }


    protected void CardNbrsRequiredCheck(object sender, ServerValidateEventArgs e)
    {
        e.IsValid =
            Regex.IsMatch(txtCardNbr_Digit_1.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_2.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_3.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_4.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_5.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_6.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_7.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_8.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_9.Text, @"^\d{1}$") &&
            Regex.IsMatch(txtCardNbr_Digit_10.Text, @"^\d{1}$");
    }


    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Response.Redirect(UrlParamModifier.AddEdit(Request.RawUrl, "type", "view"));
    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (GetUrlParamType() == UrlParamType.View)
        {
            Response.Redirect(UrlParamModifier.AddEdit(Request.RawUrl, "type", "edit"));
            return;
        }

        if (!txtValidateCardNbrRequired.IsValid)
            return;


        if (GetUrlParamType() == UrlParamType.Edit)
        {

            if ((ddlExpiry_Year.SelectedValue != "-1" && ddlExpiry_Month.SelectedValue == "-1") ||
                (ddlExpiry_Month.SelectedValue != "-1" && ddlExpiry_Year.SelectedValue == "-1"))
            {
                SetErrorMessage("Expiry Must Be Both Set or Both Unset.");
                return;
            }

            DateTime expiryDate = ddlExpiry_Year.SelectedValue == "-1" || ddlExpiry_Month.SelectedValue == "-1" ?
                                  DateTime.MinValue :
                                  new DateTime(Convert.ToInt32(ddlExpiry_Year.SelectedValue), Convert.ToInt32(ddlExpiry_Month.SelectedValue), 1);


            HealthCard hc = HealthCardDB.GetByID(Convert.ToInt32(lblId.Text));

            UrlParamCard urlParamCard = GetUrlParamCard();
            string cardFamilyMemberNbr = (urlParamCard == UrlParamCard.Medicare) ? ddlCardFamilyMemberNbr.SelectedValue : "";



            string cardNbr;
            if (hc.Organisation.OrganisationID == -1)
            {
                cardNbr =
                    txtCardNbr_Digit_1.Text.PadLeft(1, ' ') + txtCardNbr_Digit_2.Text.PadLeft(1, ' ') + txtCardNbr_Digit_3.Text.PadLeft(1, ' ') + txtCardNbr_Digit_4.Text.PadLeft(1, ' ') + txtCardNbr_Digit_5.Text.PadLeft(1, ' ') +
                    txtCardNbr_Digit_6.Text.PadLeft(1, ' ') + txtCardNbr_Digit_7.Text.PadLeft(1, ' ') + txtCardNbr_Digit_8.Text.PadLeft(1, ' ') + txtCardNbr_Digit_9.Text.PadLeft(1, ' ') + txtCardNbr_Digit_10.Text.PadLeft(1, ' ');
            }
            else 
                cardNbr = txtCardNbr.Text;


            if (chkIsActive.Checked)
                HealthCardDB.UpdateAllCardsInactive(hc.Patient.PatientID, hc.HealthCardID);

            HealthCardDB.Update(hc.HealthCardID, hc.Patient.PatientID, hc.Organisation.OrganisationID == -1 || hc.Organisation.OrganisationID == -2 ? hc.Organisation.OrganisationID : Convert.ToInt32(ddlOrganisation.SelectedValue), txtCardName.Text, cardNbr, cardFamilyMemberNbr, expiryDate,
                hc.DateReferralSigned, hc.DateReferralReceivedInOffice, chkIsActive.Checked, Convert.ToInt32(Session["StaffID"]), txtAreaTreated.Text.Trim());


            Response.Redirect(UrlParamModifier.AddEdit(Request.RawUrl, "type", "view"));
        }
        else if (GetUrlParamType() == UrlParamType.Add)
        {

            if ((ddlExpiry_Year.SelectedValue != "-1" && ddlExpiry_Month.SelectedValue == "-1") ||
                (ddlExpiry_Month.SelectedValue != "-1" && ddlExpiry_Year.SelectedValue == "-1"))
            {
                SetErrorMessage("Expiry Must Be Both Set or Both Unset.");
                return;
            }

            DateTime expiryDate = ddlExpiry_Year.SelectedValue == "-1" || ddlExpiry_Month.SelectedValue == "-1" ?
                                  DateTime.MinValue :
                                  new DateTime(Convert.ToInt32(ddlExpiry_Year.SelectedValue), Convert.ToInt32(ddlExpiry_Month.SelectedValue), 1);


            UrlParamCard urlParamCard = GetUrlParamCard();
            UrlParamType urlParamType = GetUrlParamType();
            if (!IsValidFormID() || urlParamCard == UrlParamCard.None || urlParamType == UrlParamType.None)
            {
                HideTableAndSetErrorMessage();
                return;
            }


            int organisation_id = (urlParamCard == UrlParamCard.Medicare) ? -1 : (urlParamCard == UrlParamCard.DVA ? -2 :-3);

            string cardNbr;
            if (organisation_id == -1)
            {
                cardNbr =
                    txtCardNbr_Digit_1.Text.PadLeft(1, ' ') + txtCardNbr_Digit_2.Text.PadLeft(1, ' ') + txtCardNbr_Digit_3.Text.PadLeft(1, ' ') + txtCardNbr_Digit_4.Text.PadLeft(1, ' ') + txtCardNbr_Digit_5.Text.PadLeft(1, ' ') +
                    txtCardNbr_Digit_6.Text.PadLeft(1, ' ') + txtCardNbr_Digit_7.Text.PadLeft(1, ' ') + txtCardNbr_Digit_8.Text.PadLeft(1, ' ') + txtCardNbr_Digit_9.Text.PadLeft(1, ' ') + txtCardNbr_Digit_10.Text.PadLeft(1, ' ');
            }
            else if (organisation_id == -2 || organisation_id == -3)
                cardNbr = txtCardNbr.Text;
            else
                throw new Exception("Unknown organisation id for healthcard card : " + organisation_id);



            if (chkIsActive.Checked)
                HealthCardDB.UpdateAllCardsInactive(GetFormID());

            string cardFamilyMemberNbr = (urlParamCard == UrlParamCard.Medicare) ? ddlCardFamilyMemberNbr.SelectedValue : "";
            int id = HealthCardDB.Insert(GetFormID(), organisation_id == -3 ? Convert.ToInt32(ddlOrganisation.SelectedValue) : organisation_id, txtCardName.Text, cardNbr, cardFamilyMemberNbr, expiryDate, DateTime.MinValue, DateTime.MinValue, chkIsActive.Checked, Convert.ToInt32(Session["StaffID"]), txtAreaTreated.Text.Trim());


            string url = Request.RawUrl;
            url = UrlParamModifier.AddEdit(url, "type", "view");
            url = UrlParamModifier.AddEdit(url, "id", id.ToString());
            Response.Redirect(url);
            //Response.Redirect(Request.RawUrl.Replace("type=add", "type=edit").Replace("id="+GetFormID(), "id="+id));  // CHANGE PARAM TO EDIT, AND CHANGE ID TO HEALTHCARD ID INSTEAD OF PATIENTID
        }
        else
        {
            HideTableAndSetErrorMessage("", "Invalid URL Parameters");
        }


    }


    #region GrdHealthCardAction

    protected void FillHealthCardActionGrid()
    {

        if ((GetUrlParamType() != UrlParamType.Edit && GetUrlParamType() != UrlParamType.View) || !IsValidFormID())
        {
            pnlGrdHealthCardAction.Visible = false;
            return;
        }

        HealthCard health_card = HealthCardDB.GetByID(GetFormID());
        if (health_card == null)
        {
            pnlGrdHealthCardAction.Visible = false;
            return;
        }

        DataTable dt = HealthCardActionDB.GetDataTable_ByHealthCard(health_card.HealthCardID);
        Session["hcaction_data"] = dt;

        if (dt.Rows.Count > 0)
        {
            if (IsPostBack && Session["epcremaining_sortexpression"] != null && Session["epcremaining_sortexpression"].ToString().Length > 0)
            {
                DataView dataView = new DataView(dt);
                dataView.Sort = Session["epcremaining_sortexpression"].ToString();
                GrdHealthCardAction.DataSource = dataView;
            }
            else
            {
                GrdHealthCardAction.DataSource = dt;
            }


            try
            {
                GrdHealthCardAction.DataBind();
            }
            catch (Exception ex)
            {
                HideTableAndSetErrorMessage("", ex.ToString());
            }
        }
        else
        {
            dt.Rows.Add(dt.NewRow());
            GrdHealthCardAction.DataSource = dt;
            GrdHealthCardAction.DataBind();

            int TotalColumns = GrdHealthCardAction.Rows[0].Cells.Count;
            GrdHealthCardAction.Rows[0].Cells.Clear();
            GrdHealthCardAction.Rows[0].Cells.Add(new TableCell());
            GrdHealthCardAction.Rows[0].Cells[0].ColumnSpan = TotalColumns;
            GrdHealthCardAction.Rows[0].Cells[0].Text = "No Record Found";
        }
    }
    protected void GrdHealthCardAction_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (!Utilities.IsDev() && e.Row.RowType != DataControlRowType.Pager)
            e.Row.Cells[0].CssClass = "hiddencol";
    }
    protected void GrdHealthCardAction_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        DataTable types = DBBase.GetGenericDataTable(null, "HealthCardActionType", "health_card_action_type_id", "descr");
        DataTable dt = Session["hcaction_data"] as DataTable;
        bool tblEmpty = (dt.Rows.Count == 1 && dt.Rows[0][0] == DBNull.Value);
        if (!tblEmpty && e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblId = (Label)e.Row.FindControl("lblId");
            DataRow[] foundRows = dt.Select("health_card_action_id=" + lblId.Text);
            DataRow thisRow = foundRows[0];


            DropDownList ddlHealthCardActionType = (DropDownList)e.Row.FindControl("ddlHealthCardActionType");
            if (ddlHealthCardActionType != null)
            {
                ddlHealthCardActionType.DataSource = types;
                ddlHealthCardActionType.DataTextField = "descr";
                ddlHealthCardActionType.DataValueField = "health_card_action_type_id";
                ddlHealthCardActionType.DataBind();
                ddlHealthCardActionType.SelectedValue = thisRow["health_card_action_type_id"].ToString();
            }


            DateTime actionDate = DateTime.MinValue;
            if ((DropDownList)e.Row.FindControl("ddlActionDate_Day") != null)
                actionDate = Convert.ToDateTime((thisRow["action_date"]));

            DropDownList ddlActionDate_Day = (DropDownList)e.Row.FindControl("ddlActionDate_Day");
            if (ddlActionDate_Day != null)
            {
                for (int i = 1; i <= 31; i++)
                    ddlActionDate_Day.Items.Add(new ListItem(i.ToString(), i.ToString()));
                ddlActionDate_Day.SelectedValue = actionDate.Day.ToString();
            }

            DropDownList ddlActionDate_Month = (DropDownList)e.Row.FindControl("ddlActionDate_Month");
            if (ddlActionDate_Month != null)
            {
                for (int i = 1; i <= 12; i++)
                    ddlActionDate_Month.Items.Add(new ListItem(i.ToString(), i.ToString()));
                ddlActionDate_Month.SelectedValue = actionDate.Month.ToString();
            }

            DropDownList ddlActionDate_Year = (DropDownList)e.Row.FindControl("ddlActionDate_Year");
            if (ddlActionDate_Year != null)
            {
                int startYear = 2000;
                if (actionDate.Year < startYear)
                    ddlActionDate_Year.Items.Add(new ListItem(actionDate.Year.ToString(), actionDate.Year.ToString()));
                for (int i = startYear; i <= DateTime.Today.Year + 5; i++)
                    ddlActionDate_Year.Items.Add(new ListItem(i.ToString(), i.ToString()));
                if (actionDate.Year > DateTime.Today.Year)
                    ddlActionDate_Year.Items.Add(new ListItem(actionDate.Year.ToString(), actionDate.Year.ToString()));
                ddlActionDate_Year.SelectedValue = actionDate.Year.ToString();
            }

            Utilities.AddConfirmationBox(e);
            if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                Utilities.SetEditRowBackColour(e, System.Drawing.Color.LightGoldenrodYellow);
        }


        if (e.Row.RowType == DataControlRowType.Footer)
        {
            DropDownList ddlHealthCardActionType = (DropDownList)e.Row.FindControl("ddlNewHealthCardActionType");
            if (ddlHealthCardActionType != null)
            {
                ddlHealthCardActionType.DataSource = types;
                ddlHealthCardActionType.DataTextField = "descr";
                ddlHealthCardActionType.DataValueField = "health_card_action_type_id";
                ddlHealthCardActionType.DataBind();
            }

            DropDownList ddlActionDate_Day = (DropDownList)e.Row.FindControl("ddlNewActionDate_Day");
            if (ddlActionDate_Day != null)
            {
                for (int i = 1; i <= 31; i++)
                    ddlActionDate_Day.Items.Add(new ListItem(i.ToString(), i.ToString()));
                ddlActionDate_Day.SelectedValue = DateTime.Today.Day.ToString();
            }

            DropDownList ddlActionDate_Month = (DropDownList)e.Row.FindControl("ddlNewActionDate_Month");
            if (ddlActionDate_Month != null)
            {
                for (int i = 1; i <= 12; i++)
                    ddlActionDate_Month.Items.Add(new ListItem(i.ToString(), i.ToString()));
                ddlActionDate_Month.SelectedValue = DateTime.Today.Month.ToString();
            }

            DropDownList ddlActionDate_Year = (DropDownList)e.Row.FindControl("ddlNewActionDate_Year");
            if (ddlActionDate_Year != null)
            {
                for (int i = 2000; i <= DateTime.Today.Year + 5; i++)
                    ddlActionDate_Year.Items.Add(new ListItem(i.ToString(), i.ToString()));
                ddlActionDate_Year.SelectedValue = DateTime.Today.Year.ToString();
            }
        }

    }
    protected void GrdHealthCardAction_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GrdHealthCardAction.EditIndex = -1;
        FillHealthCardActionGrid();
    }
    protected void GrdHealthCardAction_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        Label lblId = (Label)GrdHealthCardAction.Rows[e.RowIndex].FindControl("lblId");
        DropDownList ddlActionDate_Day = (DropDownList)GrdHealthCardAction.Rows[e.RowIndex].FindControl("ddlActionDate_Day");
        DropDownList ddlActionDate_Month = (DropDownList)GrdHealthCardAction.Rows[e.RowIndex].FindControl("ddlActionDate_Month");
        DropDownList ddlActionDate_Year = (DropDownList)GrdHealthCardAction.Rows[e.RowIndex].FindControl("ddlActionDate_Year");
        DropDownList ddlHealthCardActionType = (DropDownList)GrdHealthCardAction.Rows[e.RowIndex].FindControl("ddlHealthCardActionType");

        HealthCardAction action = HealthCardActionDB.GetByID(Convert.ToInt32(lblId.Text));
        DateTime date = new DateTime(Convert.ToInt32(ddlActionDate_Year.SelectedValue), Convert.ToInt32(ddlActionDate_Month.SelectedValue), Convert.ToInt32(ddlActionDate_Day.SelectedValue));
        HealthCardActionDB.Update(Convert.ToInt32(lblId.Text), action.HealthCard.HealthCardID, Convert.ToInt32(ddlHealthCardActionType.SelectedValue), date);

        GrdHealthCardAction.EditIndex = -1;
        FillHealthCardActionGrid();
    }
    protected void GrdHealthCardAction_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Label lblId = (Label)GrdHealthCardAction.Rows[e.RowIndex].FindControl("lblId");
        //HealthCardActionDB.Delete(Convert.ToInt32(lblId.Text));

        FillHealthCardActionGrid();
    }
    protected void GrdHealthCardAction_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName.Equals("Insert"))
        {
            DropDownList ddlActionDate_Day = (DropDownList)GrdHealthCardAction.FooterRow.FindControl("ddlNewActionDate_Day");
            DropDownList ddlActionDate_Month = (DropDownList)GrdHealthCardAction.FooterRow.FindControl("ddlNewActionDate_Month");
            DropDownList ddlActionDate_Year = (DropDownList)GrdHealthCardAction.FooterRow.FindControl("ddlNewActionDate_Year");
            DropDownList ddlHealthCardActionType = (DropDownList)GrdHealthCardAction.FooterRow.FindControl("ddlNewHealthCardActionType");

            DateTime date = new DateTime(Convert.ToInt32(ddlActionDate_Year.SelectedValue), Convert.ToInt32(ddlActionDate_Month.SelectedValue), Convert.ToInt32(ddlActionDate_Day.SelectedValue));
            HealthCardActionDB.Insert(GetFormID(), Convert.ToInt32(ddlHealthCardActionType.SelectedValue), date);

            FillHealthCardActionGrid();
        }
    }
    protected void GrdHealthCardAction_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GrdHealthCardAction.EditIndex = e.NewEditIndex;
        FillHealthCardActionGrid();
    }
    protected void GrdHealthCardAction_Sorting(object sender, GridViewSortEventArgs e)
    {
        // dont allow sorting if in edit mode
        if (GrdHealthCardAction.EditIndex >= 0)
            return;

        GrdHealthCardAction_Sort(e.SortExpression);
    }

    protected void GrdHealthCardAction_Sort(string sortExpression, params string[] sortExpr)
    {
        DataTable dataTable = Session["hcaction_data"] as DataTable;

        if (dataTable != null)
        {
            if (Session["epcremaining_sortexpression"] == null)
                Session["epcremaining_sortexpression"] = "";

            DataView dataView = new DataView(dataTable);
            string[] sortData = Session["epcremaining_sortexpression"].ToString().Trim().Split(' ');

            string newSortExpr = (sortExpr.Length == 0) ?
                (sortExpression == sortData[0] && sortData[1] == "ASC") ? "DESC" : "ASC" :
                sortExpr[0];

            dataView.Sort = sortExpression + " " + newSortExpr;
            Session["epcremaining_sortexpression"] = sortExpression + " " + newSortExpr;

            GrdHealthCardAction.DataSource = dataView;
            GrdHealthCardAction.DataBind();
        }
    }

    #endregion


    #region HideTableAndSetErrorMessage, SetErrorMessage, HideErrorMessag

    private void HideTableAndSetErrorMessage(string errMsg = "", string details = "")
    {
        maintable.Visible = false;
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