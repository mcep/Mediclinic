﻿<%@ control language="C#" autoeventwireup="true" inherits="Controls_HealthCardInfoControlV2, App_Web_2umnwf1k" %>

<asp:HiddenField ID="healthCardPatientID" runat="server" />
<asp:HiddenField ID="healthCardShowOnlyActiveCard" runat="server" />
<asp:HiddenField ID="healthCardShowLeftColumn" runat="server" />
<asp:HiddenField ID="healthCardShowCardInfoRow" runat="server" />
<asp:HiddenField ID="healthCardShowCardShortHeadingRow" runat="server" />
<asp:HiddenField ID="healthCardShowNoEpcMessageRow" runat="server" />
<asp:HiddenField ID="healthCardShowAddEditEpcLinks" runat="server" />


<table>
    <tr id="medicare_info_row" runat="server">
        <td id="r1c1" runat="server" style="min-width:120px;" class="nowrap"><b>Medicare Card:</b></td>
        <td id="r1c2" runat="server" style="min-width:8px;"></td>
        <td class="nowrap"><asp:Label ID="lblMedicareCard"  runat="server" Text="No Medicare Card"></asp:Label><br /></td>
        <td style="min-width:18px"></td>
        <td class="nowrap">
            <asp:LinkButton ID="lnkMedicareCardEdit" runat="server" Text="Edit" onclick="btnEditMedicareCard_Click"/>
            <asp:Label ID="lblMedicareAddBtnSeperator" runat="server">|</asp:Label>
            <asp:LinkButton ID="lnkMedicareCardAdd" runat="server" Text="Add" onclick="btnAddMedicareCard_Click" OnClientClick="javascript:if (!confirm('Are you sure you want to delete this card and associated '+(is_aged_care() ? 'referral' : 'EPC')+'?')) return false;" />
        </td>
        <td style="width:6px"></td>
        <td class="nowrap"><asp:Label ID="lblMedicareCardInfo" runat="server" /></td>
    </tr>
    <tr id="medicare_short_heading_row" runat="server">
        <td id="r2c1" runat="server" style="min-width:120px;" class="nowrap">Medicare Card:</td>
        <td id="r2c2" runat="server" style="min-width:8px;"></td>
        <td colspan="5" class="nowrap">Medicare Card:</td>
    </tr>
    <tr align="left" id="medicare_epc_info_row" runat="server">
        <td id="r3c1" runat="server" valign="top">
            Default: <asp:CheckBox ID="chkMedicareIsActive" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="r3c2" runat="server" ></td>
        <td colspan="5" class="nowrap" style="width:100%">
            <asp:Label ID="lblEPCText_Medicare" runat="server" Font-Bold="True">EPC</asp:Label>: [Signed: <asp:Label ID="lblEPCSignedDate" runat="server" Font-Bold="True" />] [<asp:Label ID="lblEPCExpireDateText" runat="server" Font-Bold="True" Text="Expires:" /> <asp:Label ID="lblEPCExpireDate" runat="server" Font-Bold="True" />] 

            <div id="spnShowAddEditEpcLinksMedicare" runat="server" style="width:100%; text-align:right;">
                <asp:HyperLink ID="lnkEditMedicareEPC" runat="server" Text="Edit" /> | <asp:HyperLink ID="lnkNewMedicareEPC" runat="server" Text="Add" />
            </div>

            <asp:Label ID="lblEPCsRemaining" runat="server"></asp:Label>
        </td>
    </tr>
    <tr id="medicare_epc_combined_remaining_row" runat="server">
        <td id="r4c1" runat="server" ></td>
        <td id="r4c2" runat="server" ></td>
        <td colspan="5" class="nowrap">
            <asp:Label ID="lblCombinedEPCRemainingThisYearText" runat="server" Text="Combined Remaining This Year:" /> <asp:Label ID="lblCombinedEPCRemainingThisYear" runat="server" Font-Bold="True" /> (Used <asp:Label ID="lblCombinedEPCUsedThisYear" runat="server" Font-Bold="True" />) 
            <span id="spn_combined_epc_remaining_next_year" runat="server">
            <br />
            Combined Remaining Next Year: <asp:Label ID="lblCombinedEPCRemainingNextYear" runat="server" Font-Bold="True" />
            </span>
        </td>
    </tr>
    <tr id="medicare_no_epc_message_row" runat="server">
        <td id="r5c1" runat="server">
            Default: <asp:CheckBox ID="chkMedicareIsActive2" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="r5c2" runat="server"></td>
        <td>
            <b>No <asp:Label ID="lblEPCText_NoMedicareCard" runat="server" Font-Bold="True">EPC</asp:Label> Card</b>
        </td>
        <td></td>
        <td><span id="spnShowAddEpcLinksMedicare" runat="server"><asp:HyperLink ID="lnkNewMedicareEPC2" runat="server" Text="Add" /></span></td>
    </tr>

    <tr id="space_row" runat="server" height="9">
        <td id="r6c1" runat="server"></td>
        <td id="r6c2" runat="server"></td>
        <td colspan="5"></td>
    </tr>

    <tr id="dva_info_row" runat="server">
        <td id="r7c1" runat="server" style="min-width:120px;" class="nowrap"><b>DVA Card:</b></td>
        <td id="r7c2" runat="server" style="min-width:8px;"></td>
        <td class="nowrap"><asp:Label ID="lblDVACard"  runat="server" Text="No DVA Card"></asp:Label><br /></td>
        <td></td>
        <td class="nowrap">
            <asp:LinkButton ID="lnkDVACardEdit" runat="server" Text="Edit" onclick="btnEditDVACard_Click"/>
            <asp:Label ID="lblDVAAddBtnSeperator" runat="server">|</asp:Label>
            <asp:LinkButton ID="lnkDVACardAdd" runat="server" Text="Add" onclick="btnAddDVACard_Click" OnClientClick="javascript:if (!confirm('Are you sure you want to delete this card and associated '+(is_aged_care() ? 'referral' : 'EPC')+'?')) return false;" />
        </td>
        <td style="width:6px"></td>
        <td class="nowrap"><asp:Label ID="lblDVACardInfo" runat="server" /></td>
    </tr>
    <tr id="dva_short_heading_row" runat="server">
        <td id="r8c1" runat="server" style="min-width:120px;" class="nowrap">DVA Card:</td>
        <td id="r8c2" runat="server" style="min-width:8px;"></td>
        <td colspan="5" class="nowrap">DVA Card:</td>
    </tr>
    <tr align="left" id="dva_epc_info_row" runat="server">
        <td id="r9c1" runat="server" valign="top">
            Defaut: <asp:CheckBox ID="chkDvaIsActive" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="r9c2" runat="server"></td>
        <td colspan="5" class="nowrap">
            <asp:Label ID="lblEPCText_DVA" runat="server" Font-Bold="True">EPC</asp:Label>: [Signed: <asp:Label ID="lblDVAEPCSignedDate" runat="server" Font-Bold="True" />] [<asp:Label ID="lblDVAEPCExpireDateText" runat="server" Font-Bold="True" Text="Expires:" /> <asp:Label ID="lblDVAEPCExpireDate" runat="server" Font-Bold="True" />] 

            <div id="spnShowAddEditEpcLinksDVA" runat="server" style="width:100%; text-align:right;">
                <asp:HyperLink ID="lnkEditDVAEPC" runat="server" Text="Edit" /> | <asp:HyperLink ID="lnkNewDVAEPC" runat="server" Text="Add" />
            </div>

        </td>
    </tr>
    <tr id="dva_no_epc_message_row" runat="server">
        <td id="r10c1" runat="server">
            Default: <asp:CheckBox ID="chkDvaIsActive2" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="r10c2" runat="server"></td>
        <td colspan="5">
            <b>No <asp:Label ID="lblEPCText_NoDVACard" runat="server" Font-Bold="True">EPC</asp:Label> Card</b> <span id="spnShowAddEpcLinksDVA" runat="server"> &nbsp;&nbsp; <asp:HyperLink ID="lnkNewDVAEPC2" runat="server" Text="Add" /></span>
        </td>
    </tr>



    <tr id="tac_info_row" runat="server">
        <td id="Td1" runat="server" style="min-width:120px;" class="nowrap"><b>TAC Card:</b></td>
        <td id="Td2" runat="server" style="min-width:8px;"></td>
        <td class="nowrap"><asp:Label ID="lblTACCard"  runat="server" Text="No TAC Card"></asp:Label><br /></td>
        <td></td>
        <td class="nowrap">
            <asp:LinkButton ID="lnkTACCardEdit" runat="server" Text="Edit" onclick="btnEditTACCard_Click"/>
            <asp:Label ID="lblTACAddBtnSeperator" runat="server">|</asp:Label>
            <asp:LinkButton ID="lnkTACCardAdd" runat="server" Text="Add" onclick="btnAddTACCard_Click" OnClientClick="javascript:if (!confirm('Are you sure you want to delete this card and associated '+(is_aged_care() ? 'referral' : 'EPC')+'?')) return false;" />
        </td>
        <td style="width:6px"></td>
        <td class="nowrap"><asp:Label ID="lblTACCardInfo" runat="server" /></td>
    </tr>
    <tr id="tac_short_heading_row" runat="server">
        <td id="Td3" runat="server" style="min-width:120px;" class="nowrap">TAC Card:</td>
        <td id="Td4" runat="server" style="min-width:8px;"></td>
        <td colspan="5" class="nowrap">TAC Card:</td>
    </tr>
    <tr align="left" id="tac_epc_info_row" runat="server">
        <td id="Td5" runat="server" valign="top">
            Defaut: <asp:CheckBox ID="chkTacIsActive" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="Td6" runat="server"></td>
        <td colspan="5" class="nowrap">
            <asp:Label ID="lblEPCText_TAC" runat="server" Font-Bold="True">EPC</asp:Label>: [Signed: <asp:Label ID="lblTACEPCSignedDate" runat="server" Font-Bold="True" />] [<asp:Label ID="lblTACEPCExpireDateText" runat="server" Font-Bold="True" Text="Expires:" /> <asp:Label ID="lblTACEPCExpireDate" runat="server" Font-Bold="True" />] 

            <div id="spnShowAddEditEpcLinksTAC" runat="server" style="width:100%; text-align:right;">
                <asp:HyperLink ID="lnkEditTACEPC" runat="server" Text="Edit" /> | <asp:HyperLink ID="lnkNewTACEPC" runat="server" Text="Add" />
            </div>

        </td>
    </tr>
    <tr id="tac_no_epc_message_row" runat="server">
        <td id="Td7" runat="server">
            Default: <asp:CheckBox ID="chkTacIsActive2" runat="server" OnCheckedChanged="chkIsActive_CheckedChanged" AutoPostBack="True" />
        </td>
        <td id="Td8" runat="server"></td>
        <td colspan="5">
            <b>No <asp:Label ID="lblEPCText_NoTACCard" runat="server" Font-Bold="True">EPC</asp:Label> Card</b> <span id="spnShowAddEpcLinksTAC" runat="server"> &nbsp;&nbsp; <asp:HyperLink ID="lnkNewTACEPC2" runat="server" Text="Add" /></span>
        </td>
    </tr>




    <tr id="no_epc_message_row" runat="server">
        <td id="r11c1" runat="server"></td>
        <td id="r11c2" runat="server"></td>
        <td colspan="5">
            <asp:Label ID="lblEPCText_InfoTitle" runat="server" Font-Bold="True">EPC</asp:Label> Info:<br />
            No Medicare Card Or DVA Card Entered
        </td>
    </tr>
</table>
