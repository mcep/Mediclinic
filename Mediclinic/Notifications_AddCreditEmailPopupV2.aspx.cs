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

public partial class Notifications_AddCreditEmailPopupV2 : System.Web.UI.Page
{


    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            HideErrorMessage();
            Utilities.UpdatePageHeaderV2(Page.Master, true);

            if (!IsPostBack)
            {
                PagePermissions.EnforcePermissions_RequireAny(Session, Response, true, true, true, false, false, false);
                SetupGUI();
                FillEmptyForm();
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


    #region GetUrlParamType()

    private enum UrlParamType { Call, ChargeCreditCard, None };
    private UrlParamType GetUrlParamType()
    {
        string type = Request.QueryString["type"];
        if (type != null && type.ToLower() == "call")
            return UrlParamType.Call;
        else if (type != null && type.ToLower() == "charge_credit_card")
            return UrlParamType.ChargeCreditCard;
        else
            return UrlParamType.None;
    }

    #endregion

    #region SetupGUI()

    public void SetupGUI()
    {
        bool editable = true;
        Utilities.SetEditControlBackColour(txtName,                   editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtPhoneNumber,            editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtMobileNumber,           editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCreditCardNumber,       editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCreditCarddExpiryDate,  editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtCCV,                    editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtAmountToTopUp,          editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
        Utilities.SetEditControlBackColour(txtEmailMessage,           editable, System.Drawing.Color.LightGoldenrodYellow, System.Drawing.Color.Empty);
    }

    #endregion


    private void FillEmptyForm()
    {
        lblHeading.Text = "SMS Credit Request";

        btnSubmit.Text = "Send Request";
        btnCancel.Visible = true;
    }



    protected void btnCancel_Click(object sender, EventArgs e)
    {
        // close this window
        Page.ClientScript.RegisterStartupScript(this.GetType(), "close", "<script language=javascript>window.returnValue=false;self.close();</script>");
    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        string htmlMessage = @"
<table>
  <tr>
    <td>Name</td>
    <td>&nbsp;&nbsp;<b>" + txtName.Text + @"</b></td>
  </tr>
  <tr>
    <td>Phone Number</td>
    <td>&nbsp;&nbsp;<b>" + txtPhoneNumber.Text + @"</b></td>
  </tr>
  <tr>
    <td>Mobile Number</td>
    <td>&nbsp;&nbsp;<b>" + txtMobileNumber.Text + @"</b></td>
  </tr>

  <tr>
    <td colspan=""2""><br /><br /></td>
  </tr>
";

if (radioType.SelectedValue == "charge_credit_card")
    htmlMessage += @"
  <tr>
    <td>Credit Card Number</td>
    <td>&nbsp;&nbsp;<b>" + txtCreditCardNumber.Text + @"</b></td>
  </tr>
  <tr>
    <td>CC Expiry Date</td>
    <td>&nbsp;&nbsp;<b>" + txtCreditCarddExpiryDate.Text + @"</b></td>
  </tr>
  <tr>
    <td>CCV</td>
    <td>&nbsp;&nbsp;<b>" + txtCCV.Text + @"</b></td>
  </tr>

  <tr>
    <td colspan=""2""><br /><br /></td>
  </tr>
";


htmlMessage += @"
  <tr>
    <td>Amount To Top-Up</td>
    <td>&nbsp;&nbsp;<b>" + txtAmountToTopUp.Text + @"</b></td>
  </tr>

  <tr>
    <td colspan=""2""><br /><br /></td>
  </tr>

  <tr>
    <td colspan=""2"">Extra Information From Sender:</td>
  </tr>
  <tr>
    <td colspan=""2"">&nbsp;&nbsp;<b>" + txtEmailMessage.Text.Trim().Replace(Environment.NewLine, "<br />") + @"</b></td>
  </tr>

  <tr>
    <td colspan=""2""><br /><br /><br /></td>
  </tr>

  <tr>
    <td style=""height:12px"" colspan=""2""><hr /></td>
  </tr>

  <tr>
    <td>Machine</td>
    <td>&nbsp;&nbsp;<b>" + Environment.MachineName + @"</b></td>
  </tr>
  <tr>
    <td>Database</td>
    <td>&nbsp;&nbsp;<b>" + Session["DB"] + @"</b></td>
  </tr>
  <tr>
    <td>User</td>
    <td>&nbsp;&nbsp;<b>" + Session["StaffFullnameWithoutMiddlename"] + @" (StaffID: " + Session["StaffID"] + @")</b></td>
  </tr>
  <tr>
    <td>Site</td>
    <td>&nbsp;&nbsp;<b>" + Session["SiteName"] + @" (SiteID: " + Session["SiteID"] + @")</b></td>
  </tr>


  <tr>
    <td style=""height:12px"" colspan=""2""><hr /></td>
  </tr>

</table>";


        Emailer.SimpleEmail(
            System.Configuration.ConfigurationManager.AppSettings["TopupEmail_To"],
            radioType.SelectedValue == "charge_credit_card" ? "SMS Credit Topup - Credit Card" : "SMS Credit Topup - Call Required",
            htmlMessage,
            true,
            null,
            null);

        // close this window
        Page.ClientScript.RegisterStartupScript(this.GetType(), "close", "<script language=javascript>window.returnValue=false;self.close();</script>");
    }


    #region SetErrorMessage, HideErrorMessage

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