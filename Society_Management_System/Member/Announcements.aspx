<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="Announcements.aspx.cs" Inherits="Society_Management_System.Member.Announcements" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .glass-card {
            backdrop-filter: blur(6px);
            background: rgba(255,255,255,0.06);
            border: 1px solid rgba(255,255,255,0.12);
            border-radius: 12px;
            padding: 18px;
            margin-bottom: 14px;
            box-shadow: 0 6px 18px rgba(0,0,0,0.08);
        }
        .ann-title { font-size: 1.12rem; font-weight:600; margin-bottom:6px; }
        .ann-meta { font-size:0.85rem; color:#cfcfcf; margin-bottom:10px; }
        .no-data { text-align:center; color:#bbb; padding:30px 0; }
    </style>

    <div class="container mt-4">
        <h2>Announcements</h2>

        <asp:Panel ID="pnlAnnouncements" runat="server" Visible="false">
            <asp:Repeater ID="rptAnnouncements" runat="server" OnItemDataBound="rptAnnouncements_ItemDataBound">
                <ItemTemplate>
                    <div class="glass-card">
                        <div class="ann-title"><%# Eval("title") %></div>
                        <div class="ann-meta">
                            Visible: <%# Eval("visible_from", "{0:dd MMM yyyy}") %>
                            <%# (Eval("visible_to") == DBNull.Value ? "" : " to " + String.Format("{0:dd MMM yyyy}", Eval("visible_to"))) %>
                        </div>
                        <div class="ann-content">
                            <asp:Literal ID="litContent" runat="server" />
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <asp:Panel ID="pnlEmpty" runat="server" CssClass="no-data" Visible="false">
            No announcements available.
        </asp:Panel>
    </div>
</asp:Content>