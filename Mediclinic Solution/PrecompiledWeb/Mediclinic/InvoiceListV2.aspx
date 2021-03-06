﻿<%@ page title="Invoices" language="C#" masterpagefile="~/SiteV2.master" autoeventwireup="true" inherits="InvoiceListV2, App_Web_q34p1rur" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script type="text/javascript">

        function open_new_window(URL) {
            NewWindow = window.open(URL, "_blank", "toolbar=no,menubar=0,status=0,copyhistory=0,scrollbars=yes,resizable=1,location=0,Width=1600,Height=960");
            NewWindow.location = URL;
        }

        function open_new_tab(URL) {
            var win = window.open(URL, '_blank');
            win.focus();
        }

        function clear_span_text(eid) {
            var span = document.getElementById(eid);
            while (span.firstChild) {
                span.removeChild(span.firstChild);
            }
            span.appendChild(document.createTextNode(""));
        }

        function doClick(buttonName, e) {
            //the purpose of this function is to allow the enter key to 
            //point to the correct button to click.
            var key;

            if (window.event)
                key = window.event.keyCode;     //IE
            else
                key = e.which;     //firefox

            if (key == 13) {
                //Get the button the user wants to have clicked
                var btn = document.getElementById(buttonName);
                if (btn != null) { //If we find the button click it
                    btn.click();
                    event.keyCode = 0
                }
            }

            return (key != 13);
        }

    </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <div class="clearfix">
        <div class="page_title">

            <table class="block_center">
                <tr>
                    <td><asp:Label ID="lblHeading" runat="server">Invoices</asp:Label></td>
                    <td style="min-width:10px;">&nbsp;&nbsp;</td>
                    <td style="text-align:left;"><asp:Label ID="lnkToEntity" runat="server"></asp:Label></td>
                </tr>
            </table>

        </div>
        <div class="main_content_with_header">
            <div class="user_login_form_no_width" style="width:1000px;">

                <div id="div_search_section" runat="server" class="border_top_bottom user_login_form_no_width_div">
                    <center>

                        <asp:Panel ID="pnlDefaultButton" runat="server" DefaultButton="btnDefaultButton_NoSubmit" style="margin:6px auto;">
                            <asp:Button ID="btnDefaultButton_NoSubmit" runat="server" CssClass="hiddencol" OnClientClick="javascript:return false;" />
                            <table>
                                <tr>
                                    <td id="td_search_dates" runat="server">
                                        <table>
                                            <tr>
                                                <td class="nowrap"><asp:Label ID="lblSearchDate" runat="server">Start Date: </asp:Label></td>
                                                <td class="nowrap"><asp:TextBox ID="txtStartDate" runat="server" Columns="10"/></td>
                                                <td class="nowrap"><asp:ImageButton ID="txtStartDate_Picker" runat="server" ImageUrl="~/images/Calendar-icon-24px.png" /></td>
                                                <td class="nowrap"><button type="button" onclick="javascript:document.getElementById('txtStartDate').value = '';return false;">Clear</button></td>
                                            </tr>
                                            <tr>
                                                <td class="nowrap"><asp:Label ID="lblEndDate" runat="server">End Date: </asp:Label></td>
                                                <td class="nowrap"><asp:TextBox ID="txtEndDate" runat="server" Columns="10"></asp:TextBox></td>
                                                <td class="nowrap"><asp:ImageButton ID="txtEndDate_Picker" runat="server" ImageUrl="~/images/Calendar-icon-24px.png" /></td>
                                                <td class="nowrap"><button type="button" onclick="javascript:document.getElementById('txtEndDate').value = '';return false;">Clear</button></td>
                                            </tr>
                                        </table>
                                    </td>

                                    <td id="td_search_space2" runat="server" style="width:50px"></td>

                                    <td id="td_search_inv_types" runat="server" >
                                        <table style="line-height:10px;">
                                            <tr style="vertical-align:top;">
                                                <td class="nowrap">
                                                    <asp:CheckBox ID="chkIncMedicare"  runat="server" Text="&nbsp;Inc Medicare"/> 
                                                    <br />
                                                    <asp:CheckBox ID="chkIncDVA"       runat="server" Text="&nbsp;Inc DVA"/>
                                                    <br />
                                                    <asp:CheckBox ID="chkIncPrivate"   runat="server" Text="&nbsp;Inc PT Payable"/>
                                                </td>

                                                <td style="width:15px"></td>

                                                <td class="nowrap">
                                                    <asp:CheckBox ID="chkIncPaid"   runat="server" Text="&nbsp;Inc Paid"/>
                                                    <br />
                                                    <asp:CheckBox ID="chkIncUnpaid" runat="server" Text="&nbsp;Inc Unpaid"/>
                                                    <span style="height:8px;display:block"></span>
                                                    <asp:CheckBox ID="chkViewReversed"  runat="server" Text="&nbsp;View Reversed"/>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>

                                    <td id="td_search_space3" runat="server" style="width:50px"></td>

                                    <td id="td_search_inv_nbr_search" runat="server">
                                        <table>
                                            <tr>
                                                <td class="nowrap">Invoice Nbr Search</td>
                                                <td class="nowrap" style="width:10px"></td>
                                                <td class="nowrap"><asp:TextBox ID="txtInvoiceNbrSearch" runat="server" Width="80" onkeydown="return doClick('btnSearch',event);" /></td>
                                            </tr>
                                            <tr>
                                                <td class="nowrap">Booking Nbr Search</td>
                                                <td class="nowrap"></td>
                                                <td class="nowrap"><asp:TextBox ID="txtBookingNbrSearch" runat="server" Width="80" onkeydown="return doClick('btnSearch',event);" /></td>
                                            </tr>
                                            <tr>
                                                <td class="nowrap">Claim Nbr Search</td>
                                                <td class="nowrap"></td>
                                                <td class="nowrap"><asp:TextBox ID="txtClaimNbrSearch" runat="server" Width="80" onkeydown="return doClick('btnSearch',event);" /></td>
                                            </tr>
                                        </table>
                                    </td>

                                    <td id="td_search_space4" runat="server" style="width:30px"></td>

                                    <td id="td_search_button" runat="server"><asp:Button ID="btnSearch" runat="server" Text="Update" OnClick="btnSearch_Click" /></td>

                                </tr>
                            </table>
                        </asp:Panel>
                        
                    <center>
                </div>

            </div>

            <div class="text-center">
                <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="failureNotification" ValidationGroup="ValidationSummary"/>
                <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" CssClass="failureNotification"></asp:Label>
            </div>

            <br />

            <div id="divReinvoice" runat="server" class="text-center">
                <div style="line-height:7px;">&nbsp;</div>
                <asp:Label ID="lblReinvoiceMsg" runat="server">Invoice XXX closed and new invoice created: YYYY<br /></asp:Label>
                &nbsp;
                <asp:LinkButton ID="lnkInvoice" runat="server" Text="View"/> | 
                <asp:LinkButton ID="lnkPrint" runat="server" Text="Print" OnCommand="lnkPrint_Command" CommandName="ReInvoiced" CommandArgument='<%# Bind("inv_invoice_id") %>' ></asp:LinkButton> | 
                <asp:LinkButton ID="lnkEmail" runat="server" Text="Email" OnCommand="lnkEmail_Command" CommandName="ReInvoiced" CommandArgument='<%# Bind("inv_invoice_id") %>' OnClientClick="clear_span_text('lblErrorMessage')" ></asp:LinkButton>
                <br />
            </div>

            <asp:Button ID="btnUpdateList" runat="server" OnClick="btnUpdateList_Click" CssClass="hiddencol" />

            <center>
                <div id="autodivheight" class="divautoheight" style="height:500px;">

                    <asp:GridView ID="GrdInvoice" runat="server" 
                         AutoGenerateColumns="False" DataKeyNames="inv_invoice_id" 
                         OnRowCancelingEdit="GrdInvoice_RowCancelingEdit" 
                         OnRowDataBound="GrdInvoice_RowDataBound" 
                         OnRowEditing="GrdInvoice_RowEditing" 
                         OnRowUpdating="GrdInvoice_RowUpdating" ShowFooter="True" 
                         OnRowCommand="GrdInvoice_RowCommand" 
                         OnRowDeleting="GrdInvoice_RowDeleting" 
                         OnRowCreated="GrdInvoice_RowCreated"
                         AllowSorting="True" 
                         OnSorting="GridView_Sorting"
                         RowStyle-VerticalAlign="top"
                         AllowPaging="True"
                         OnPageIndexChanging="GrdInvoice_PageIndexChanging"
                         PageSize="11"
                         ClientIDMode="Predictable"
                         CssClass="table table-bordered table-striped table-grid table-grid-top-bottum-padding-normal auto_width block_center">
                         <PagerSettings Mode="NumericFirstLast" FirstPageText="First" PreviousPageText="Previous" NextPageText="Next" LastPageText="Last" />

                        <Columns> 

                            <asp:TemplateField HeaderText="Invoice #"  HeaderStyle-HorizontalAlign="Left" SortExpression="inv_invoice_id" HeaderStyle-CssClass="nowrap" ItemStyle-CssClass="nowrap" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:LinkButton ID="lnkId" runat="server" Text='<%# Bind("inv_invoice_id") %>' ></asp:LinkButton>
                                    <asp:LinkButton ID="btnReversedMessageHoverToolTip" runat="server" Font-Bold="True" Text="*R" ToolTip="This invoice has been reversed" OnClientClick="javascript:return false;" />
                                    <a id="A1" runat="server" href="#" onclick="javascript:return false;" title='<%#  Eval("added_by_reversed_by_row") %>' style="text-decoration: none"><b>&nbsp;*&nbsp;</b></a>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText=""  HeaderStyle-HorizontalAlign="Left" HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:LinkButton ID="lnkPrint" runat="server" Text="Print" OnCommand="lnkPrint_Command" CommandArgument='<%# Bind("inv_invoice_id") %>' ></asp:LinkButton>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText=""  HeaderStyle-HorizontalAlign="Left" HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:LinkButton ID="lnkEmail" runat="server" Text="Email" OnCommand="lnkEmail_Command" CommandArgument='<%# Bind("inv_invoice_id") %>' OnClientClick="clear_span_text('lblErrorMessage')" ></asp:LinkButton>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText=""  HeaderStyle-HorizontalAlign="Left" HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None" ItemStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:LinkButton ID="lnkAllFromBooking" runat="server" Text="All Fr Bk" CommandName="AllFromBooking" CommandArgument='<%# Bind("inv_invoice_id") %>' OnClientClick="clear_span_text('lblErrorMessage')" ></asp:LinkButton>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Reversed By"  HeaderStyle-HorizontalAlign="Left" SortExpression="reversed_by_col"  HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None" ItemStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblAddedReversedBy" runat="server" Text='<%# Eval("reversed_by_col") %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Un-Reverse"  HeaderStyle-HorizontalAlign="Left" HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:LinkButton ID="lnkUnReverse" runat="server" Text="Un-Reverse" CommandName="Un-Reverse" CommandArgument='<%# Bind("inv_invoice_id") %>' OnClientClick="clear_span_text('lblErrorMessage')" ></asp:LinkButton>
                                </ItemTemplate> 
                            </asp:TemplateField> 


                            <asp:TemplateField HeaderText="Inv Date"  HeaderStyle-HorizontalAlign="Left" SortExpression="inv_invoice_date_added"  HeaderStyle-CssClass="nowrap" FooterStyle-BorderStyle="None" ItemStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblDate" runat="server" Text='<%# Bind("inv_invoice_date_added", "{0:dd MMM yyyy}") %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Treat Date"  HeaderStyle-HorizontalAlign="Left" SortExpression="booking_date_start" HeaderStyle-CssClass="nowrap" ItemStyle-CssClass="nowrap" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblBookingStartDate" runat="server" Text='<%# Bind("booking_date_start", "{0:dd MMM yyyy}") %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Type" HeaderStyle-HorizontalAlign="Left" SortExpression="invoice_type_descr" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <EditItemTemplate> 
                                    <asp:DropDownList ID="ddlInvoiceType" runat="server" DataTextField="invoice_type_descr" DataValueField="invoice_type_id"> </asp:DropDownList> 
                                </EditItemTemplate> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblInvoiceType" runat="server" Text='<%# Eval("invoice_type_descr") %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Debtor"  HeaderStyle-HorizontalAlign="Left" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblPayerPagePopup" runat="server" Text="Patient Info"></asp:Label>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Claim Nbr" HeaderStyle-HorizontalAlign="Left" SortExpression="inv_healthcare_claim_number" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblClaimNumber" runat="server" Text='<%# Bind("inv_healthcare_claim_number") %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Inv Total" HeaderStyle-HorizontalAlign="Left" SortExpression="inv_total" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" FooterStyle-CssClass="nowrap"> 
                                <EditItemTemplate> 
                                    <asp:TextBox Width="60px" ID="txtTotal" runat="server" Text='<%# Bind("inv_total") %>'></asp:TextBox> 
                                    <asp:RequiredFieldValidator ID="txtValidateTotalRequired" runat="server" CssClass="failureNotification"  
                                        ControlToValidate="txtTotal" 
                                        ErrorMessage="Amount is required."
                                        Display="Dynamic"
                                        ValidationGroup="ValidationSummary">*</asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="txtValidateTotalRegex" runat="server" CssClass="failureNotification" 
                                        ControlToValidate="txtTotal"
                                        ValidationExpression="^\d+(\.\d{1,2})?$"
                                        ErrorMessage="Amount can only be numbers and option decimal place with 1 or 2 digits following."
                                        Display="Dynamic"
                                        ValidationGroup="ValidationSummary">*</asp:RegularExpressionValidator>
                                </EditItemTemplate> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblTotal" runat="server" Text='<%# Bind("inv_total") %>'></asp:Label> 
                                </ItemTemplate> 
                                <FooterTemplate> 
                                    <asp:Label ID="lblSum_Total" runat="server" Font-Bold="True"></asp:Label> 
                                    &nbsp;
                                </FooterTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="GST" HeaderStyle-HorizontalAlign="Left" SortExpression="inv_gst" FooterStyle-BorderStyle="None" FooterStyle-CssClass="nowrap"> 
                                <EditItemTemplate> 
                                    <asp:TextBox Width="60px" ID="txtGST" runat="server" Text='<%# Bind("inv_gst") %>'></asp:TextBox> 
                                    <asp:RequiredFieldValidator ID="txtValidateGSTRequired" runat="server" CssClass="failureNotification"  
                                        ControlToValidate="txtGST" 
                                        ErrorMessage="GST is required."
                                        Display="Dynamic"
                                        ValidationGroup="ValidationSummary">*</asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="txtValidateGSTRegex" runat="server" CssClass="failureNotification" 
                                        ControlToValidate="txtGST"
                                        ValidationExpression="^\d+(\.\d{1,2})?$"
                                        ErrorMessage="GST can only be numbers and option decimal place with 1 or 2 digits following."
                                        Display="Dynamic"
                                        ValidationGroup="ValidationSummary">*</asp:RegularExpressionValidator>
                                </EditItemTemplate> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblGST" runat="server" Text='<%# Bind("inv_gst") %>'></asp:Label> 
                                </ItemTemplate> 
                                <FooterTemplate> 
                                    <asp:Label ID="lblSum_GST" runat="server"></asp:Label> 
                                    &nbsp;
                                </FooterTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Receipts" HeaderStyle-HorizontalAlign="Left" SortExpression="inv_receipts_total" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" FooterStyle-CssClass="nowrap">
                                <ItemTemplate> 
                                    <asp:Label ID="lblReceipts" runat="server" Text='<%# Eval("inv_receipts_total") %>'></asp:Label> 
                                </ItemTemplate> 
                                <FooterTemplate> 
                                    <asp:Label ID="lblSum_Receipts" runat="server" Font-Bold="True"></asp:Label> 
                                    &nbsp;
                                </FooterTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Adj" HeaderStyle-HorizontalAlign="Left" SortExpression="inv_credit_notes_total" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" FooterStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblCreditNotes" runat="server" Text='<%# Eval("inv_credit_notes_total") %>'></asp:Label> 
                                </ItemTemplate> 
                                <FooterTemplate> 
                                    <asp:Label ID="lblSum_CreditNotes" runat="server" Font-Bold="True"></asp:Label> 
                                    &nbsp;
                                </FooterTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Due" HeaderStyle-HorizontalAlign="Left" SortExpression="total_due" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" FooterStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblDue" runat="server" Text='<%# Eval("total_due") %>'></asp:Label> 
                                </ItemTemplate> 
                                <FooterTemplate> 
                                    <asp:Label ID="lblSum_Due" runat="server" Font-Bold="True"></asp:Label> 
                                    &nbsp;
                                </FooterTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Paid" SortExpression="inv_is_paid" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblIsPaid" runat="server" Text='<%# Eval("inv_is_paid").ToString()=="True"?"Yes":"No" %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <%-- 
                            <asp:TemplateField HeaderText="Reversed" SortExpression="inv_reversed_by" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblIsReversed" runat="server" Text='<%# Eval("inv_reversed_by") != DBNull.Value ? "Yes" : "No" %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField>
                            --%>

                            <asp:TemplateField HeaderText="Refund" SortExpression="inv_is_refund" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblIsRefund" runat="server" Text='<%# Eval("inv_is_refund").ToString()=="True"?"Yes":"No" %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Batched" SortExpression="inv_is_batched" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblIsBatched" runat="server" Text='<%# Eval("inv_is_batched").ToString()=="True"?"Yes":"No" %>'></asp:Label> 
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Patient" HeaderStyle-HorizontalAlign="Left" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" ItemStyle-CssClass="text_left"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblPatientPagePopup" runat="server" Text="Patient Info"></asp:Label>
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Provider" SortExpression="provider_person_surname,provider_person_firstname" HeaderStyle-HorizontalAlign="Left" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" ItemStyle-CssClass="text_left"> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblProvider" runat="server" Text='<%# Eval("provider_person_firstname") + (Eval("provider_person_firstname").ToString().Length + Eval("provider_person_surname").ToString().Length == 0 ? "" : " ") + Eval("provider_person_surname") %>'></asp:Label>
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Rej. Code" SortExpression="inv_reject_letter_code" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <EditItemTemplate> 
                                    <asp:DropDownList  ID="ddlMedicareRejectionCode" runat="server"></asp:DropDownList>
                                </EditItemTemplate> 
                                <ItemTemplate> 
                                    <asp:Label ID="lblMedicareRejectionCode" runat="server" Text='<%# Eval("inv_reject_letter_code") %>'></asp:Label> 
                                    <br />
                                    <asp:LinkButton ID="btnReInvoice" runat="server" Text="ReInvoice" CommandName="ReInvoice" CommandArgument='<%# Bind("inv_invoice_id") %>'></asp:LinkButton>
                                </ItemTemplate> 
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="" ShowHeader="False" HeaderStyle-HorizontalAlign="Left" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None"> 
                                <EditItemTemplate> 
                                    <asp:LinkButton ID="lnkUpdate" runat="server" CausesValidation="True" CommandName="Update" Text="Update" ValidationGroup="ValidationSummary"></asp:LinkButton> 
                                    <asp:LinkButton ID="lnkCancel" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton> 
                                </EditItemTemplate> 
                                <ItemTemplate> 
                                   <asp:ImageButton ID="lnkEdit" runat="server" CommandName="Edit" ImageUrl="~/images/Inline-edit-icon-24.png"  AlternateText="Inline Edit" ToolTip="Inline Edit"/>
                                </ItemTemplate> 
                            </asp:TemplateField> 

                            <%-- 
                            <asp:CommandField HeaderText="" ShowDeleteButton="True" ShowHeader="True" ButtonType="Image"  DeleteImageUrl="~/images/Delete-icon-24.png" /> 
                            --%>

                            <asp:TemplateField HeaderText="Set Paid" ShowHeader="False" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" ItemStyle-VerticalAlign="Middle" ItemStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <table style="width:100%">
                                        <tr>
                                            <td>
                                                <asp:Button ID="btnSetPaid" runat="server" Text="Paid" CommandName="SetPaid" CommandArgument='<%# Bind("inv_invoice_id") %>' Visible='<%# Eval("inv_is_paid").ToString()=="True"? false : true  %>'  />
                                            </td>
                                            <td style="text-align:right;">
                                                <asp:Button ID="btnSetWiped" runat="server" Text="Wipe" CommandName="SetWiped" CommandArgument='<%# Bind("inv_invoice_id") %>' Visible='<%# Eval("inv_is_paid").ToString()=="True"? false : true  %>'  />
                                            </td>
                                        </tr>
                                    </table>
                                </ItemTemplate> 
                                <EditItemTemplate> 
                                </EditItemTemplate> 
                            </asp:TemplateField> 

                            <asp:TemplateField HeaderText="Add Rcpt/Cr.Note" ShowHeader="False" FooterStyle-VerticalAlign="Top" FooterStyle-BorderStyle="None" ItemStyle-VerticalAlign="Middle" ItemStyle-CssClass="nowrap"> 
                                <ItemTemplate> 
                                    <asp:Button ID="btnAddReceipt" runat="server" Text="Rcpt & Cr. Note" Visible='<%# Eval("inv_is_paid").ToString()=="True"? false : true  %>'  />
                                </ItemTemplate> 
                                <EditItemTemplate> 
                                </EditItemTemplate> 
                            </asp:TemplateField> 

                        </Columns> 
                    </asp:GridView>

                </div>
            </center>

        </div>
    </div>

</asp:Content>



