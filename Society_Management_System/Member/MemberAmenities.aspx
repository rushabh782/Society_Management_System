<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="MemberAmenities.aspx.cs" Inherits="Society_Management_System.Member.MemberAmenities" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="glass-container">
        <h2 class="text-center">Available Amenities</h2>

        <asp:Repeater ID="rptAmenities" runat="server">
            <HeaderTemplate>
                <table class="glass-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Amenity Name</th>
                            <th>Booking Required</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>

            <ItemTemplate>
    <tr>
        <td><%# Eval("amenity_id") %></td>
        <td><%# Eval("name") %></td>
        <td><%# (Convert.ToBoolean(Eval("booking_required")) ? "Yes" : "No") %></td>
        <td>
            <%# Convert.ToBoolean(Eval("booking_required")) 
                ? "<a href='BookAmenity.aspx?amenity_id=" + Eval("amenity_id") + "' class=\"book-btn\">Book Now</a>"
                : "<span class='not-required'>No Booking Needed</span>" %>
        </td>
    </tr>
</ItemTemplate>


            <FooterTemplate>
                    </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </div>

    <style>
        .glass-container {
            backdrop-filter: blur(15px);
            background: rgba(255, 255, 255, 0.1);
            border-radius: 20px;
            padding: 30px;
            margin: 30px;
            color: #fff;
            box-shadow: 0 8px 32px rgba(0,0,0,0.37);
        }

        h2 {
            text-align: center;
            font-size: 24px;
            color: #fff;
            margin-bottom: 20px;
        }

        .glass-table {
            width: 100%;
            border-collapse: collapse;
            text-align: left;
        }

        .glass-table th, .glass-table td {
            padding: 12px 15px;
            border-bottom: 1px solid rgba(255,255,255,0.2);
        }

        .glass-table th {
            color: #ffc107;
            font-weight: 600;
        }

        .book-btn {
            background-color: #00bcd4;
            color: white;
            padding: 6px 12px;
            border-radius: 10px;
            text-decoration: none;
            transition: 0.3s;
        }

        .book-btn:hover {
            background-color: #0097a7;
        }

        .not-required {
            color: #ddd;
            font-style: italic;
        }
    </style>
</asp:Content>
