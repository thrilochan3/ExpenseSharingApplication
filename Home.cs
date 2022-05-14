using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExpenseSharingApplication
{
    public partial class Home : Form
    {
        public readonly IMongoDatabase _db;
        List<string> unconfirmedGridItems;
        List<string> unconfirmedGridIds;
        public Home()
        {
            InitializeComponent();
            label2.Text = Program.loggedInUser.FullName;
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
            unconfirmedGridItems=new List<string>();
            unconfirmedGridIds = new List<string>();
            loadFriends();
            dataGridView1.ClearSelection();
        }

        public IMongoCollection<Expense> expenses =>
            _db.GetCollection<Expense>("Expenses");
        public IMongoCollection<ExpenseMapping> expenseMapping =>
            _db.GetCollection<ExpenseMapping>("ExpenseMapping");
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            CreateGroup createGroup = new CreateGroup();
            createGroup.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            AddExpense addExpense = new AddExpense();
            addExpense.Show();
        }

        private void Home_Load(object sender, EventArgs e)
        {

        }

        public void loadFriends() {
            List<string> gridItems = new List<string>();
            var allExpenseMappings = expenseMapping.Find(a => true).ToList();
            var allExpenses  = expenses.Find(a => true).ToList();
            string loggedinUser = Program.loggedInUser.Username;
            var linkedExpenses = from item in allExpenses
                                 where item.Involved.Contains(loggedinUser)
                                select item.Involved;
            HashSet<string> linkedFriends = new HashSet<string>();
            foreach (var item in linkedExpenses) {
                for (int i = 0; i < item.Length; i++) { 
                    linkedFriends.Add(item[i]);
                }
            }
            var linkedUsers = from item in allExpenseMappings
                              where item.Payer == loggedinUser
                              || item.Payee == loggedinUser
                              select item;
            foreach (var user in linkedUsers) {
                linkedFriends.Add(user.Payee);
                linkedFriends.Add(user.Payer);
            }
            decimal totalSettlement = 0;
            foreach (var friend in linkedFriends) {
                if (friend != loggedinUser) {
                    var youOwe = from item in allExpenseMappings
                                 where item.Payee == loggedinUser && item.Payer == friend
                                 select item.Amount;
                    var totalOwe = youOwe.Sum();
                    var youGet = from item in allExpenseMappings
                                 where item.Payee == friend && item.Payer == loggedinUser
                                 select item.Amount;
                    var totalGet = youGet.Sum();
                    if (totalGet > totalOwe)
                    {
                        totalSettlement = totalSettlement + totalGet - totalOwe;
                        gridItems.Add($"You will get ${Decimal.Round(totalGet - totalOwe,2)} from {friend}");
                    }
                    else if (totalGet < totalOwe)
                    {
                        totalSettlement = totalSettlement - (totalOwe - totalGet);
                        gridItems.Add($"You Owe ${Decimal.Round(totalOwe - totalGet,2)} to {friend}");
                    }
                }
            }
            totalSettlement = Decimal.Round(totalSettlement,2);
            if (totalSettlement > 0) {
                label3.Text = $"You will get {totalSettlement}";
            }
            if (totalSettlement < 0) {
                label3.Text = $"You owe {totalSettlement*-1}";
            }
            if (totalSettlement == 0)
            {
                label3.Text = $"You are settled up here.";
            }
            dataGridView1.Rows.Clear();
            foreach (var item in gridItems) { 
                dataGridView1.Rows.Add(item);
            }  

            var linkedUnconfirmedExpenses = from item in allExpenses
                                 where item.Involved.Contains(loggedinUser)
                                 && !item.Confirmed.Contains(loggedinUser)
                                 select item;
            foreach (var item in linkedUnconfirmedExpenses) {
                string description = "";
                int involvedNo =  item.Involved.Length;
                if (involvedNo == 2)
                {
                    description = $"{item.Payer} Paid ${item.Amount} to you for the expense {item.Name}.";
                }
                else
                {
                    description = $"{item.Payer} Paid ${item.Amount} to you and other {involvedNo-2} for the expense {item.Name}.";
                }
                unconfirmedGridItems.Add(description);
                unconfirmedGridIds.Add(item._id.ToString());

            }
            dataGridView2.Rows.Clear();
            foreach (var item in unconfirmedGridItems)
            {
                dataGridView2.Rows.Add(item);
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          var index = dataGridView2.CurrentRow.Index;
          var expenseId =  unconfirmedGridIds[index];
            var allExpenses = expenses.Find(a => true).ToList();
            var selectedExpenseList =  from item in allExpenses
                                   where item._id == expenseId
                                   select item;
            var selectedExpense =  selectedExpenseList.FirstOrDefault();
            ConfrimExpense confrimExpense = new ConfrimExpense(selectedExpense);
            this.Hide();
            confrimExpense.Show();
        }

        private void linkLogout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();   
            Login login = new Login();
            login.Show();
        }

        private void linkPayFriend_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Pay_a_friend pay_A_Friend = new Pay_a_friend();
            this.Hide();
            pay_A_Friend.Show();

        }
    }
}
