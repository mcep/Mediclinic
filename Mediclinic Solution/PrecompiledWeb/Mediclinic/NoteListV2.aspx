﻿<%@ page title="Cost Centres" language="C#" masterpagefile="~/SiteV2.master" autoeventwireup="true" inherits="NoteListV2, App_Web_o4dos14t" validaterequest="false" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
    <script type="text/javascript">

        function check_selected_atleast_one() {

            var count = 0;
            var c = document.getElementsByTagName('input');
            for (var i = 0; i < c.length; i++)
                if (c[i].type == 'checkbox' && c[i].id.indexOf('chkPrint') !== -1 && c[i].checked)
                    count++;

            //alert(count);

            if (count == 0)
                alert("Please use checkboxes and select at least one note to print.");

            return count > 0;
        }

        function highlight_row(chkBox) {  // doesnt pass in a checkbox -- read first comment below
            // asp:CheckBox control doesn't have a onchange event, and onchange event will be rendered in a <span> tag and not the <input> tag. 
            // so get parent, then get the control

            var gvDrv = document.getElementById("<%= GrdNote.ClientID %>");
            for (i = 1; i < gvDrv.rows.length; i++) {

                // dont do all the processing if its not the row
                if (chkBox != null && gvDrv.rows[i] != chkBox.parentNode.parentNode)
                    continue;

                    // if it is the row, process than return out of the function
                else {

                    var cells = gvDrv.rows[i].cells;
                    for (j = 0; j < cells.length; j++) {
                        var HTML = cells[j].innerHTML;

                        if (chkBox != null && cells[j] != chkBox.parentNode)
                            continue; // alert("found");

                        if (HTML.indexOf("chkPrint") != -1) {
                            var lblID = cells[0].getElementsByTagName("*")[0];
                            var chkSelect = cells[j].getElementsByTagName("*")[1];  // first item is the onchange event rendered as a div, so get 2nd item

                            var cells2 = gvDrv.rows[i].cells;
                            for (j2 = 0; j2 < cells2.length; j2++)
                                cells2[j2].style.backgroundColor = chkSelect.checked ? '#FAFAD2' : '';  // LightGoldenrodYellow 
                        }
                    }

                    if (chkBox != null)
                        return;
                }
            }
        }

        function getCookie(c_name) {
            var c_value = document.cookie;
            var c_start = c_value.indexOf(" " + c_name + "=");
            if (c_start == -1) {
                c_start = c_value.indexOf(c_name + "=");
            }
            if (c_start == -1) {
                c_value = null;
            }
            else {
                c_start = c_value.indexOf("=", c_start) + 1;
                var c_end = c_value.indexOf(";", c_start);
                if (c_end == -1) {
                    c_end = c_value.length;
                }
                c_value = unescape(c_value.substring(c_start, c_end));
            }
            return c_value;
        }
        function setCookie(c_name, value, exmins) {
            var exdate = new Date();
            exdate.setMinutes(exdate.getMinutes() + exmins);
            var c_value = escape(value) + ((exmins == null) ? "" : "; expires=" + exdate.toUTCString());
            document.cookie = c_name + "=" + c_value;
        }
        function deleteCookie(c_name) {
            date = new Date();
            date.setDate(date.getDate() - 1);
            document.cookie = escape(c_name) + '=;expires=' + date.toUTCString();
        }


        function set_note(txtBox, user_id, entity_id) {
            setCookie(user_id + "_" + entity_id + "_" + "new_note", txtBox.value, 180);
        }
        function load_note(txtBox, user_id, entity_id) {
            var txt = getCookie(user_id + "_" + entity_id + "_" + "new_note");
            if (txt != null)
                txtBox.value = getCookie(user_id + "_" + entity_id + "_" + "new_note");
        }
        function clear_note(txtBox, user_id, entity_id) {
            setCookie(user_id + "_" + entity_id + "_" + "new_note", "", -1440);
        }
        function alert_note(txtBox, user_id, entity_id) {
            alert(getCookie(user_id + "_" + entity_id + "_" + "new_note"));
        }



        function send_email() {

            var openWindow = window.open('SendEmail.aspx', 'Email', 'width=675px,height=675px,left=100,top=100,center=yes,resizable=no,scrollbars=no');
            if (openWindow) {
                openWindow.onload = function () {
                    openWindow.document.getElementById("FreeTextBox1").value = document.getElementById('emailText').value;
                }
            }

            //var param = { 'body_text': document.getElementById('emailText').value };
            //OpenWindowWithPost("SendEmail.aspx",
            //  "width=675px,height=675px,left=100,top=100,center=yes,resizable=no,scrollbars=no",
            //  "Email", param);
        }
        function OpenWindowWithPost(url, windowoption, name, params) {
            var form = document.createElement("form");
            form.setAttribute("method", "post");
            form.setAttribute("action", url);
            form.setAttribute("target", name);

            for (var i in params) {
                if (params.hasOwnProperty(i)) {
                    var input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = i;
                    input.value = params[i];
                    form.appendChild(input);
                }
            }

            document.body.appendChild(form);

            //note I am using a post.htm page since I did not want to make double request to the page 
            //it might have some Page_Load call which might screw things up.
            window.open("post.htm", name, windowoption);

            form.submit();

            document.body.removeChild(form);
        }

        function select_body_part(v) {
            var editModeDDL = null;
            for (var i = 0; i < 100; i++)
                if (document.getElementById('MainContent_GrdNote_ddlBodyPart_' + i) != null)
                    editModeDDL = document.getElementById('MainContent_GrdNote_ddlBodyPart_' + i)

            if (editModeDDL == null)
                selectItemByValue(document.getElementById('MainContent_GrdNote_ddlNewBodyPart'), String(v))
            else
                selectItemByValue(editModeDDL, String(v))
        }
        function selectItemByValue(elmnt, v) {
            for (var i = 0; i < elmnt.options.length; i++) {
                if (elmnt.options[i].value === v) {
                    elmnt.selectedIndex = i;
                    break;
                }
            }
        }

    </script>
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">

    <div class="clearfix">
        <div class="page_title"><asp:Label ID="lblHeading" runat="server">Notes</asp:Label></div>
        <div class="main_content" style="padding:10px 5px;">

            <center>

                 <table>
                     <tr>
                         <td id="body_chart" runat="server" style="vertical-align:top;">

		                    <img src="images/body_chart.png" alt="" usemap="#logos" />
		                    <map name="logos">
			                    <area shape="circle" coords="73,39,6"    onclick="select_body_part(1);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="76,65,2"    onclick="select_body_part(2);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="75,73,3"    onclick="select_body_part(3);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="95,104,6"   onclick="select_body_part(4);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="57,104,6"   onclick="select_body_part(5);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="103,155,6"  onclick="select_body_part(6);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="50,155,6"   onclick="select_body_part(7);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="76,141,6"   onclick="select_body_part(8);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="76,175,6"   onclick="select_body_part(9);return false;"  href="#"alt="" />
			                    <area shape="circle" coords="76,195,6"   onclick="select_body_part(10);return false;" href="#"alt="" />
			                    <area shape="circle" coords="80,218,10"  onclick="select_body_part(11);return false;" href="#"alt="" />
			                    <area shape="circle" coords="57,249,6"   onclick="select_body_part(13);return false;" href="#"alt="" />
			                    <area shape="circle" coords="95,249,6"   onclick="select_body_part(12);return false;" href="#"alt="" />
			                    <area shape="circle" coords="63,331,6"   onclick="select_body_part(15);return false;" href="#"alt="" />
			                    <area shape="circle" coords="95,331,6"   onclick="select_body_part(14);return false;" href="#"alt="" />
			                    <area shape="circle" coords="28,133,6"   onclick="select_body_part(17);return false;" href="#"alt="" />
			                    <area shape="circle" coords="126,133,6"  onclick="select_body_part(16);return false;" href="#"alt="" />

			                    <area shape="circle" coords="232,21,6"   onclick="select_body_part(18);return false;" href="#"alt="" />
			                    <area shape="circle" coords="232,56,6"   onclick="select_body_part(19);return false;" href="#"alt="" />
			                    <area shape="circle" coords="232,106,6"  onclick="select_body_part(20);return false;" href="#"alt="" />
			                    <area shape="circle" coords="182,135,6"  onclick="select_body_part(16);return false;" href="#"alt="" />
			                    <area shape="circle" coords="281,135,6"  onclick="select_body_part(17);return false;" href="#"alt="" />
			                    <area shape="circle" coords="232,158,6"  onclick="select_body_part(21);return false;" href="#"alt="" />
			                    <area shape="circle" coords="232,184,6"  onclick="select_body_part(22);return false;" href="#"alt="" />
			                    <area shape="circle" coords="214,249,6"  onclick="select_body_part(23);return false;" href="#"alt="" />
			                    <area shape="circle" coords="250,249,6"  onclick="select_body_part(24);return false;" href="#"alt="" />
			                    <area shape="circle" coords="217,326,6"  onclick="select_body_part(25);return false;" href="#"alt="" />
			                    <area shape="circle" coords="244,326,6"  onclick="select_body_part(26);return false;" href="#"alt="" />
			                    <area shape="circle" coords="73,393,6"   onclick="select_body_part(28);return false;" href="#"alt="" />
			                    <area shape="circle" coords="91,393,6"   onclick="select_body_part(27);return false;" href="#"alt="" />
			                    <area shape="circle" coords="218,395,6"  onclick="select_body_part(27);return false;" href="#"alt="" />
			                    <area shape="circle" coords="244,395,6"  onclick="select_body_part(28);return false;" href="#"alt="" />

			                    <area shape="default" nohref="nohref" title="Default" alt="Default"/>
		                    </map>

                         </td>
                         <td id="body_chart_space" runat="server" style="min-width:2px;width:15px;"></td>
                         <td>

                            <center>
                                <asp:ValidationSummary ID="ValidationSummaryAdd" runat="server" CssClass="failureNotification" ValidationGroup="ValidationSummaryAdd"/>
                                <asp:ValidationSummary ID="ValidationSummaryEdit" runat="server" CssClass="failureNotification" ValidationGroup="ValidationSummaryEdit"/>
                                <asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" CssClass="failureNotification"></asp:Label>

                                <div style="height:4px;"></div>

                                <asp:HiddenField ID="userID" runat="server" />
                                <asp:HiddenField ID="entityID" runat="server" />
                                <asp:HiddenField ID="emailText" runat="server" />

                                
                                <a href="#" onclick="send_email(); return false;" style="display:none;">Email Notes</a>
                                <div style="height:12px;"></div>

                            </center>

                            <div id="autodivheight" class="divautoheight" style="min-height:235px; height:500px; width: auto; padding-right: 17px;">

                                <asp:GridView ID="GrdNote" runat="server" 
                                     AutoGenerateColumns="False" DataKeyNames="note_id" 
                                     OnRowCancelingEdit="GrdNote_RowCancelingEdit" 
                                     OnRowDataBound="GrdNote_RowDataBound" 
                                     OnRowEditing="GrdNote_RowEditing" 
                                     OnRowUpdating="GrdNote_RowUpdating" ShowFooter="True" 
                                     OnRowCommand="GrdNote_RowCommand" 
                                     OnRowDeleting="GrdNote_RowDeleting" 
                                     OnRowCreated="GrdNote_RowCreated"
                                     AllowSorting="True" 
                                     OnSorting="GrdNote_Sorting"
                                     RowStyle-VerticalAlign="top"
                                     ClientIDMode="Predictable"
                                     CssClass="table table-bordered table-striped table-grid table-grid-top-bottum-padding-normal auto_width block_center vertical_align_top">
                                    <Columns> 

                                        <asp:TemplateField HeaderText="ID"  HeaderStyle-HorizontalAlign="Left" SortExpression="note_id"> 
                                            <EditItemTemplate> 
                                                <asp:Label ID="lblId" runat="server" Text='<%# Bind("note_id") %>'></asp:Label>
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <asp:Label ID="lblId" runat="server" Text='<%# Bind("note_id") %>'></asp:Label> 
                                            </ItemTemplate> 
                                        </asp:TemplateField>
                
                                        <asp:TemplateField HeaderText="Entity"  HeaderStyle-HorizontalAlign="Left" SortExpression="entity_id">
                                            <EditItemTemplate> 
                                                <asp:Label ID="lblEntityID" runat="server" Text='<%# Bind("entity_id") %>'></asp:Label>
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <asp:Label ID="lblEntityID" runat="server" Text='<%# Bind("entity_id") %>'></asp:Label> 
                                            </ItemTemplate> 
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Date"  HeaderStyle-HorizontalAlign="Left" SortExpression="date_added" FooterStyle-VerticalAlign="Top" FooterStyle-Wrap="False" ItemStyle-Wrap="false">
                                            <EditItemTemplate> 
                                                <asp:DropDownList ID="ddlDate_Day" runat="server"/> <asp:DropDownList ID="ddlDate_Month" runat="server"/> <asp:DropDownList ID="ddlDate_Year" runat="server"/>
                                                <asp:CustomValidator ID="ddlDateValidateAllOrNoneSet" runat="server"  CssClass="failureNotification"  
                                                    ControlToValidate="ddlDate_Day"
                                                    OnServerValidate="DateAllOrNoneCheck"
                                                    ErrorMessage="Date must be valid with each of day/month/year selected"
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryEdit">*</asp:CustomValidator>
                                                <asp:Label ID="lblAddedBy" runat="server" Text='<%#  Eval("added_by_staff_id") == DBNull.Value ? "" : "<br /><a href=\"#\" onclick=\"return false;\" title=\"Added By\">A</a>: " + Eval("added_by_person_firstname") + " " + Eval("added_by_person_surname") %>' ></asp:Label> 
                                                <asp:Label ID="lblModifiedBy" runat="server" Visible='<%# Eval("last_modified_note_info_visible") %>' Text='<%#  Eval("modified_by_staff_id") == DBNull.Value ? "" : "<br /><a href=\"#\" onclick=\"return false;\" title=\"Last Modified By\">M</a>: " + Eval("modified_by_person_firstname") + " " + Eval("modified_by_person_surname") %>' ></asp:Label> 
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <asp:Label ID="lblDateAdded" runat="server" Text='<%# Eval("date_added") == DBNull.Value ? "" : Convert.ToDateTime(Eval("date_added")).ToString("dd / MM / yyyy") %>' ></asp:Label> 
                                                <asp:Label ID="lblAddedBy" runat="server" Text='<%#  Eval("added_by_staff_id") == DBNull.Value ? "" : "<br /><a href=\"#\" onclick=\"return false;\" title=\"Added By\">A</a>: " + Eval("added_by_person_firstname") + " " + Eval("added_by_person_surname") %>' ></asp:Label> 
                                                <asp:Label ID="lblModifiedBy" runat="server" Visible='<%# Eval("last_modified_note_info_visible") != DBNull.Value && Convert.ToBoolean(Eval("last_modified_note_info_visible")) %>' Text='<%#  Eval("modified_by_staff_id") == DBNull.Value ? "" : "<br /><a href=\"#\" onclick=\"return false;\" title=\"Last Modified By\">M</a>: " + Eval("modified_by_person_firstname") + " " + Eval("modified_by_person_surname") %>' ></asp:Label> 
                                            </ItemTemplate> 
                                            <FooterTemplate> 
                                                <asp:DropDownList ID="ddlNewDate_Day" runat="server"/> <asp:DropDownList ID="ddlNewDate_Month" runat="server"/> <asp:DropDownList ID="ddlNewDate_Year" runat="server"/>
                                                <asp:CustomValidator ID="ddlNewDateValidateAllOrNoneSet" runat="server"  CssClass="failureNotification"  
                                                    ControlToValidate="ddlNewDate_Day"
                                                    OnServerValidate="DateAllOrNoneCheck"
                                                    ErrorMessage="Date must be valid with each of day/month/year selected"
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryAdd">*</asp:CustomValidator>
                                            </FooterTemplate> 
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Type" HeaderStyle-HorizontalAlign="Left" SortExpression="note_type_descr" FooterStyle-VerticalAlign="Top"> 
                                            <EditItemTemplate> 
                                                <asp:DropDownList ID="ddlNoteType" runat="server" DataTextField="descr" DataValueField="note_type_id"> </asp:DropDownList> 
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <div style="text-align:left;">
                                                    <asp:Label ID="lblNoteType" runat="server" Text='<%# Eval("note_type_descr") %>'></asp:Label> 
                                                </div>
                                            </ItemTemplate> 
                                            <FooterTemplate> 
                                                <asp:DropDownList ID="ddlNewNoteType" runat="server" DataTextField="descr" DataValueField="note_type_id"> </asp:DropDownList>
                                            </FooterTemplate> 
                                        </asp:TemplateField> 

                                        <asp:TemplateField HeaderText="Body Part" HeaderStyle-HorizontalAlign="Left" SortExpression="body_part_descr" FooterStyle-VerticalAlign="Top"> 
                                            <EditItemTemplate> 
                                                <asp:DropDownList ID="ddlBodyPart" runat="server" DataTextField="descr" DataValueField="body_part_id"></asp:DropDownList>
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <asp:Label ID="lblBodyPart" runat="server" Text='<%# Eval("body_part_descr") %>'></asp:Label> 
                                            </ItemTemplate> 
                                            <FooterTemplate> 
                                                <asp:DropDownList ID="ddlNewBodyPart" runat="server" DataTextField="descr" DataValueField="body_part_id"></asp:DropDownList>
                                            </FooterTemplate> 
                                        </asp:TemplateField> 

                                        <asp:TemplateField HeaderText="Note" HeaderStyle-HorizontalAlign="Left" SortExpression="text" FooterStyle-VerticalAlign="Top"> 
                                            <EditItemTemplate> 
                                                <asp:TextBox ID="txtText" TextMode="multiline" rows="6" style="width:375px;" runat="server" Text='<%# Bind("text") %>'></asp:TextBox> 
                                                <asp:RequiredFieldValidator ID="txtValidateTextRequired" runat="server" CssClass="failureNotification"  
                                                    ControlToValidate="txtText" 
                                                    ErrorMessage="Text is required."
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryEdit">*</asp:RequiredFieldValidator>
                                                <asp:RegularExpressionValidator ID="txtValidatetxtTextRegex" runat="server" CssClass="failureNotification"
                                                    ControlToValidate="txtText"
                                                    ValidationExpression="^[^<>]+$"
                                                    ErrorMessage="The following letters are not permitted: '<', '>'"
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryEdit">*</asp:RegularExpressionValidator>
                                            </EditItemTemplate> 
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtNewText" TextMode="multiline" rows="6" style="width:375px" runat="server" ></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="txtValidateNewTextRequired" runat="server" CssClass="failureNotification"  
                                                    ControlToValidate="txtNewText" 
                                                    ErrorMessage="Note Text is required."
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryAdd">*</asp:RequiredFieldValidator>
                                                <asp:RegularExpressionValidator ID="txtValidatetxtNewTextRegex" runat="server" CssClass="failureNotification"
                                                    ControlToValidate="txtNewText"
                                                    ValidationExpression="^[^<>]+$"
                                                    ErrorMessage="The following letters are not permitted: '<', '>'"
                                                    Display="Dynamic"
                                                    ValidationGroup="ValidationSummaryAdd">*</asp:RegularExpressionValidator>
                                            </FooterTemplate> 
                                            <ItemTemplate>
                                                <div style="width:375px;max-width:375px;text-align:left;">
                                                    <asp:Label ID="lblText" runat="server" Text='<%# Eval("text") == DBNull.Value ? "" : ((string)Eval("text")).Replace("\n", "<br/>") %>'></asp:Label> 
                                                </div>
                                            </ItemTemplate> 
                                        </asp:TemplateField> 

                                        <%--
                                        <asp:TemplateField HeaderText="Site" HeaderStyle-HorizontalAlign="Left" SortExpression="site_name" FooterStyle-VerticalAlign="Top"> 
                                            <EditItemTemplate> 
                                                <asp:DropDownList ID="ddlSite" runat="server" DataTextField="name" DataValueField="site_id"> </asp:DropDownList> 
                                            </EditItemTemplate> 
                                            <ItemTemplate> 
                                                <asp:Label ID="lblSite" runat="server" Text='<%# Eval("site_name") %>'></asp:Label> 
                                            </ItemTemplate> 
                                            <FooterTemplate> 
                                                <asp:DropDownList ID="ddlNewSite" runat="server" DataTextField="name" DataValueField="site_id"> </asp:DropDownList>
                                            </FooterTemplate> 
                                        </asp:TemplateField> 
                                        --%>

                                        <asp:TemplateField HeaderText="." ShowHeader="False" HeaderStyle-HorizontalAlign="Left" FooterStyle-VerticalAlign="Top" HeaderStyle-Font-Bold="false" > 
                                            <EditItemTemplate> 
                                                <asp:LinkButton ID="lnkUpdate" runat="server" CausesValidation="True" CommandName="Update" Text="Update" ValidationGroup="ValidationSummaryEdit"></asp:LinkButton> 
                                                <asp:LinkButton ID="lnkCancel" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton> 
                                            </EditItemTemplate> 
                                            <FooterTemplate> 
                                                <asp:LinkButton ID="lnkAdd" runat="server" CausesValidation="True" CommandName="Insert" Text="Insert" ValidationGroup="ValidationSummaryAdd"></asp:LinkButton> 
                                            </FooterTemplate> 
                                            <ItemTemplate> 
                                               <asp:ImageButton ID="lnkEdit" runat="server" CommandName="Edit" ImageUrl="~/images/Inline-edit-icon-24.png" AlternateText="Inline Edit" ToolTip="Inline Edit"/>
                                            </ItemTemplate> 
                                        </asp:TemplateField> 

                                        <asp:TemplateField HeaderText=""  HeaderStyle-HorizontalAlign="Center">
                                            <HeaderTemplate>
                                                <asp:ImageButton ID="btnPrint" runat="server" OnClick="btnPrint_Click" ImageUrl="~/images/printer_green-24.png" AlternateText="Print" ToolTip="Print" OnClientClick="javascript: if (!check_selected_atleast_one()) return false;" />
                                                <asp:ImageButton ID="btnEmail" runat="server" OnClick="btnEmail_Click" ImageUrl="~/images/email-24.png" AlternateText="Email To Referrer" ToolTip="Email To Referrer" OnClientClick="javascript: if (!check_selected_atleast_one()) return false;" />
                                                <asp:HiddenField ID="hiddenRefEmail" runat="server" />
                                                <asp:HiddenField ID="hiddenRefName" runat="server" />
                                                <asp:HiddenField ID="hiddenBookingOrg" runat="server" />
                                                <asp:HiddenField ID="HiddenBookingPatientName" runat="server" />
                                            </HeaderTemplate>
                                            <ItemTemplate> 
                                                <asp:CheckBox ID="chkPrint" runat="server" onchange="highlight_row(this);" />
                                            </ItemTemplate> 
                                        </asp:TemplateField>

                                        <%--
                                        <asp:CommandField HeaderText="" ShowDeleteButton="True" ShowHeader="True" ButtonType="Image"  DeleteImageUrl="~/images/Delete-icon-24.png" /> 
                                        --%>

                                    </Columns> 
                                </asp:GridView>


                            </div>
                            
                             <center>
                                 <div id="body_chart2" runat="server">

                                     <div style="height:8px;"></div>

		                            <img src="images/body_chart.png" alt="" usemap="#logos" />
		                            <map name="logos">
			                            <area shape="circle" coords="73,39,6"    onclick="select_body_part(1);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="76,65,2"    onclick="select_body_part(2);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="75,73,3"    onclick="select_body_part(3);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="95,104,6"   onclick="select_body_part(4);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="57,104,6"   onclick="select_body_part(5);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="103,155,6"  onclick="select_body_part(6);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="50,155,6"   onclick="select_body_part(7);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="76,141,6"   onclick="select_body_part(8);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="76,175,6"   onclick="select_body_part(9);return false;"  href="#"alt="" />
			                            <area shape="circle" coords="76,195,6"   onclick="select_body_part(10);return false;" href="#"alt="" />
			                            <area shape="circle" coords="80,218,10"  onclick="select_body_part(11);return false;" href="#"alt="" />
			                            <area shape="circle" coords="57,249,6"   onclick="select_body_part(13);return false;" href="#"alt="" />
			                            <area shape="circle" coords="95,249,6"   onclick="select_body_part(12);return false;" href="#"alt="" />
			                            <area shape="circle" coords="63,331,6"   onclick="select_body_part(15);return false;" href="#"alt="" />
			                            <area shape="circle" coords="95,331,6"   onclick="select_body_part(14);return false;" href="#"alt="" />
			                            <area shape="circle" coords="28,133,6"   onclick="select_body_part(17);return false;" href="#"alt="" />
			                            <area shape="circle" coords="126,133,6"  onclick="select_body_part(16);return false;" href="#"alt="" />

			                            <area shape="circle" coords="232,21,6"   onclick="select_body_part(18);return false;" href="#"alt="" />
			                            <area shape="circle" coords="232,56,6"   onclick="select_body_part(19);return false;" href="#"alt="" />
			                            <area shape="circle" coords="232,106,6"  onclick="select_body_part(20);return false;" href="#"alt="" />
			                            <area shape="circle" coords="182,135,6"  onclick="select_body_part(16);return false;" href="#"alt="" />
			                            <area shape="circle" coords="281,135,6"  onclick="select_body_part(17);return false;" href="#"alt="" />
			                            <area shape="circle" coords="232,158,6"  onclick="select_body_part(21);return false;" href="#"alt="" />
			                            <area shape="circle" coords="232,184,6"  onclick="select_body_part(22);return false;" href="#"alt="" />
			                            <area shape="circle" coords="214,249,6"  onclick="select_body_part(23);return false;" href="#"alt="" />
			                            <area shape="circle" coords="250,249,6"  onclick="select_body_part(24);return false;" href="#"alt="" />
			                            <area shape="circle" coords="217,326,6"  onclick="select_body_part(25);return false;" href="#"alt="" />
			                            <area shape="circle" coords="244,326,6"  onclick="select_body_part(26);return false;" href="#"alt="" />
			                            <area shape="circle" coords="73,393,6"   onclick="select_body_part(28);return false;" href="#"alt="" />
			                            <area shape="circle" coords="91,393,6"   onclick="select_body_part(27);return false;" href="#"alt="" />
			                            <area shape="circle" coords="218,395,6"  onclick="select_body_part(27);return false;" href="#"alt="" />
			                            <area shape="circle" coords="244,395,6"  onclick="select_body_part(28);return false;" href="#"alt="" />

			                            <area shape="default" nohref="nohref" title="Default" alt="Default"/>
		                            </map>

                                 </div>

                             </center>


                            <br />
                            <center>
                                <asp:Button ID="btnClose" runat="server" Text="Close" OnClientClick="self.close();" />
                            </center>


                         </td>
                     </tr>

                 </table>


            </center>

        </div>
    </div>


</asp:Content>



