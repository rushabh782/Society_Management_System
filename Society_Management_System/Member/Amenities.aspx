<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="Amenities.aspx.cs" Inherits="Society_Management_System.Member.Amenities" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body {
            background: linear-gradient(135deg, #667eea, #764ba2);
            font-family: 'Poppins', sans-serif;
            color: #fff;
        }
        .glass-card {
            background: rgba(255, 255, 255, 0.15);
            border-radius: 20px;
            backdrop-filter: blur(12px);
            border: 1px solid rgba(255, 255, 255, 0.25);
            padding: 25px;
            margin-bottom: 25px;
            color: #fff;
        }
        .btn-glass {
            background: rgba(255, 255, 255, 0.3);
            border: none;
            border-radius: 10px;
            color: #fff;
            font-weight: 500;
            transition: 0.3s;
        }
        .btn-glass:hover {
            background: rgba(255, 255, 255, 0.6);
            color: #333;
        }
        th { color: #ffda79; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="container mt-5">
        <h2 class="text-center">🏢 Society Amenities</h2>

        <div class="glass-card">
            <h4>Amenities Available</h4>
            <asp:GridView ID="gvAmenities" runat="server" AutoGenerateColumns="False" CssClass="table table-borderless text-white">
                <Columns>
                    <asp:BoundField DataField="name" HeaderText="Amenity Name" />
                    <asp:TemplateField HeaderText="Booking Required">
                        <ItemTemplate>
                            <%# (Convert.ToBoolean(Eval("booking_required")) ? "Yes" : "No") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <%# Convert.ToBoolean(Eval("booking_required")) 
                                ? "<button class='btn btn-glass' onclick=\"window.location.href='BookAmenity.aspx?amenity_id=" + Eval("amenity_id") + "'\">Book</button>"
                                : "<span>Open Access</span>" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="glass-card">
            <h4>My Bookings</h4>
            <asp:GridView ID="gvBookings" runat="server" AutoGenerateColumns="False" CssClass="table table-borderless text-white">
                <Columns>
                    <asp:BoundField DataField="amenity_name" HeaderText="Amenity" />
                    <asp:BoundField DataField="start_time" HeaderText="Start Time" DataFormatString="{0:g}" />
                    <asp:BoundField DataField="end_time" HeaderText="End Time" DataFormatString="{0:g}" />
                    <asp:BoundField DataField="status" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>

