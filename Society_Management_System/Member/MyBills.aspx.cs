using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;

namespace Society_Management_System.Member
{
    public partial class MyBills : Page
    {
        private static readonly string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["SocietyDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Redirect if session missing
                if (Session["MemberId"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Set user label in master
                var master = this.Master as Society_Management_System.Member.Member;
                if (master != null)
                {
                    var lblUser = master.FindControl("lblUserName") as System.Web.UI.WebControls.Label;
                    if (lblUser != null)
                    {
                        lblUser.Text = "Member"; // Display role/user
                    }
                }

                // PDF download action
                if (Request.QueryString["download"] == "true" && long.TryParse(Request.QueryString["billId"], out long billId))
                {
                    GenerateAndSendPdf(billId);
                    Response.End();
                }
            }
        }

        [WebMethod]
        public static List<BillInfo> GetBills()
        {
            var list = new List<BillInfo>();
            var memberIdObj = System.Web.HttpContext.Current.Session["MemberId"];
            if (memberIdObj == null) return list;

            long memberId = Convert.ToInt64(memberIdObj);

            using (var con = new SqlConnection(ConnStr))
            {
                string query = @"
                    SELECT MB.bill_id AS BillID, S.name AS Society, U.unit_no AS Unit,
                           MB.bill_month AS BillMonth, MB.due_date AS DueDate,
                           MB.total_amount AS TotalAmount, MB.status AS Status
                    FROM maintenance_bills MB
                    INNER JOIN societies S ON MB.society_id = S.society_id
                    INNER JOIN units U ON MB.unit_id = U.unit_id
                    INNER JOIN member_units MU ON U.unit_id = MU.unit_id
                    WHERE MU.member_id = @memberId
                    ORDER BY MB.bill_id DESC";

                var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new BillInfo
                    {
                        BillID = reader.GetInt64(0),
                        Society = reader.GetString(1),
                        Unit = reader.GetString(2),
                        BillMonth = reader.GetDateTime(3).ToString("yyyy-MM"),
                        DueDate = reader.GetDateTime(4).ToString("yyyy-MM-dd"),
                        TotalAmount = reader.GetDecimal(5),
                        Status = reader.GetString(6)
                    });
                }
            }
            return list;
        }

        [WebMethod]
        public static object StartPayment(long billId, decimal amount)
        {
            // In production, create a Razorpay order using their API and API key/secret
            string razorpayOrderId = "order_" + Guid.NewGuid();
            return new { success = true, orderId = razorpayOrderId };
        }

        [WebMethod]
        public static object VerifyPayment(string paymentId, long billId)
        {
            bool success = false;

            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE maintenance_bills SET status='Paid' WHERE bill_id = @billId", con);
                cmd.Parameters.AddWithValue("@billId", billId);
                success = cmd.ExecuteNonQuery() > 0;
            }
            return new { success };
        }

        private void GenerateAndSendPdf(long billId)
        {
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", $"attachment; filename=Bill_{billId}.pdf");
            Response.Write("PDF generation not yet implemented."); // Replace with actual PDF logic
            Response.Flush();
        }

        public class BillInfo
        {
            public long BillID { get; set; }
            public string Society { get; set; }
            public string Unit { get; set; }
            public string BillMonth { get; set; }
            public string DueDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
        }
    }
}