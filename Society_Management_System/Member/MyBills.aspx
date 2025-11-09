<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="MyBills.aspx.cs" Inherits="Society_Management_System.Member.MyBills" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>My Maintenance Bills</title>
    <script src="https://checkout.razorpay.com/v1/checkout.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="mb-4 text-2xl font-bold text-white">My Maintenance Bills</h2>
    <asp:Label ID="LblMessage" runat="server" CssClass="text-red-500 mb-2 block" />

    <table class="table table-striped table-dark table-hover w-full rounded-lg">
        <thead>
            <tr>
                <th>Bill ID</th>
                <th>Society</th>
                <th>Unit</th>
                <th>Bill Month</th>
                <th>Due Date</th>
                <th>Total Amount</th>
                <th>Status</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody id="tblBody"></tbody>
    </table>

    <script type="text/javascript">
        $(function () {
            loadBills();

            window.initiatePayment = function (billId, amount) {
                $.ajax({
                    type: "POST",
                    url: "MyBills.aspx/StartPayment",
                    data: JSON.stringify({ billId: billId, amount: amount }),
                    contentType: "application/json; charset=utf-8",
                    success: function (res) {
                        if (res.d.success) {
                            var options = {
                                key: "rzp_test_Kl7588Yie2yJTV",
                                amount: amount * 100,
                                currency: "INR",
                                order_id: res.d.orderId,
                                name: "Society Hub",
                                description: "Maintenance Bill Payment",
                                handler: function (response) {
                                    $.ajax({
                                        url: "MyBills.aspx/VerifyPayment",
                                        type: "POST",
                                        data: JSON.stringify({ paymentId: response.razorpay_payment_id, billId: billId }),
                                        contentType: "application/json; charset=utf-8",
                                        success: function (res) {
                                            if (res.d.success) {
                                                alert("Payment successful!");
                                                loadBills();
                                            } else {
                                                alert("Payment verification failed.");
                                            }
                                        }
                                    });
                                }
                            };
                            var rzp = new Razorpay(options);
                            rzp.open();
                        } else {
                            alert("Failed to initiate payment.");
                        }
                    }
                });
            };

            window.downloadPdf = function (billId) {
                window.location.href = 'MyBills.aspx?download=true&billId=' + billId;
            };

            function loadBills() {
                $.ajax({
                    url: 'MyBills.aspx/GetBills',
                    type: 'POST',
                    contentType: 'application/json; charset=utf-8',
                    success: function (res) {
                        var bills = res.d;
                        var tbody = $('#tblBody');
                        tbody.empty();
                        bills.forEach(function (bill) {
                            var actionBtn = bill.Status === 'Unpaid' ?
                                <button class="btn btn-sm btn-primary" onclick="initiatePayment(${bill.BillID},${bill.TotalAmount})">Pay</button> :
                                <button class="btn btn-sm btn-secondary" onclick="downloadPdf(${bill.BillID})">Download PDF</button>;

                            var row = `<tr>
                                <td>${bill.BillID}</td>
                                <td>${bill.Society}</td>
                                <td>${bill.Unit}</td>
                                <td>${bill.BillMonth}</td>
                                <td>${bill.DueDate}</td>
                                <td>₹${bill.TotalAmount.toFixed(2)}</td>
                                <td>${bill.Status}</td>
                                <td>${actionBtn}</td>
                            </tr>`;
                            tbody.append(row);
                        });
                    },
                    error: function () {
                        alert("Failed to load bills.");
                    }
                });
            }
        });
    </script>
</asp:Content>