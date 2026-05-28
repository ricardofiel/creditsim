using System;
using System.Linq;
using System.Web.UI.WebControls;
using CreditSim.Data.Repositories;
using CreditSim.Web.Controllers;

namespace CreditSim.Web
{
    /// <summary>
    /// Code-behind for Default.aspx — the customer list page with server-side GridView paging.
    /// Customers are ordered by createdAt DESC (most recent first), page size 10.
    /// </summary>
    public partial class Default : System.Web.UI.Page
    {
        private ICustomerRepository Repository =>
            new CustomerRepository(ConnectionStringProvider.Get());

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindGrid();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private void BindGrid()
        {
            var customers = Repository.GetAllAsync().GetAwaiter().GetResult()
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            GridView1.DataSource = customers;
            GridView1.DataBind();
        }
    }
}

