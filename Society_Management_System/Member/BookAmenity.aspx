<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="BookAmenity.aspx.cs" Inherits="Society_Management_System.Member.BookAmenity" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="glass-card">
        <h2>Book Amenity</h2>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert"></asp:Panel>

        <div class="form-group">
            <label>Amenity Name</label>
            <asp:Label ID="lblAmenityName" runat="server" CssClass="form-control-static"></asp:Label>
        </div>

        <div class="form-group">
            <label>Start Time</label>
            <asp:TextBox ID="txtStartTime" runat="server" TextMode="DateTimeLocal" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="form-group">
            <label>End Time</label>
            <asp:TextBox ID="txtEndTime" runat="server" TextMode="DateTimeLocal" CssClass="form-control"></asp:TextBox>
        </div>

        <asp:Button ID="btnBook" runat="server" Text="Book Amenity" CssClass="btn-book" OnClick="btnBook_Click" />
        <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Member/MemberAmenities.aspx" CssClass="btn-back">← Back to Amenities</asp:HyperLink>
    </div>

    <style>
        .glass-card {
            margin: 60px auto;
            width: 500px;
            padding: 30px;
            border-radius: 20px;
            background: rgba(255, 255, 255, 0.15);
            box-shadow: 0 8px 32px rgba(31, 38, 135, 0.37);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            color: #fff;
            text-align: left;
        }
        h2 {
            text-align: center;
            font-weight: 600;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-control, .form-control-static {
            width: 100%;
            padding: 10px;
            border-radius: 10px;
            border: none;
            background: rgba(255, 255, 255, 0.25);
            color: #fff;
        }
        .btn-book {
            background-color: #FFD43B;
            border: none;
            color: #333;
            padding: 10px 20px;
            border-radius: 10px;
            font-weight: bold;
            cursor: pointer;
        }
        .btn-book:hover {
            background-color: #FFC107;
        }
        .btn-back {
            display: inline-block;
            margin-left: 15px;
            color: #FFD43B;
            text-decoration: none;
        }
        .alert {
            padding: 10px;
            border-radius: 10px;
            margin-bottom: 20px;
            text-align: center;
        }
        .alert-success { background: rgba(46, 204, 113, 0.3); }
        .alert-error { background: rgba(231, 76, 60, 0.3); }
    </style>
</asp:Content>
