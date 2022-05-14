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
    public partial class Pay_a_friend : Form
    {
        public readonly IMongoDatabase _db;

        public Pay_a_friend()
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
            loadFriends();
        }

        public IMongoCollection<Expense> expenses =>
            _db.GetCollection<Expense>("Expenses");
        public IMongoCollection<ExpenseMapping> expenseMapping =>
            _db.GetCollection<ExpenseMapping>("ExpenseMapping");
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtAmount.Text) || String.IsNullOrEmpty(cbFriend.Text))
            {
                MessageBox.Show("Please enter both the fields");
                return;
            }
            Decimal dummy;
            var a = Decimal.TryParse(txtAmount.Text, out dummy);
            if (a == false)
            {
                MessageBox.Show("Only integer or decmial allowed in Amount!!");
                return;
            }
            ExpenseMapping mapping = new ExpenseMapping();
            mapping.ExpenseId = null;
            mapping.GroupId = null;
            mapping.Payer = Program.loggedInUser.Username;
            mapping.Payee = cbFriend.Text;
            mapping.Amount = Convert.ToDecimal(txtAmount.Text);
            mapping.IsConfirmed = "pending";
            expenseMapping.InsertOne(mapping);
            MessageBox.Show("Payed to your friend");
            Home home = new Home();
            this.Hide();
            home.Show();
        }
        public void loadFriends()
        {
            var allExpenseMappings = expenseMapping.Find(a => true).ToList();
            var allExpenses = expenses.Find(a => true).ToList();
            string loggedinUser = Program.loggedInUser.Username;
            var linkedExpenses = from item in allExpenses
                                 where item.Involved.Contains(loggedinUser)
                                 select item.Involved;
            HashSet<string> linkedFriends = new HashSet<string>();
            foreach (var item in linkedExpenses)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[i] != Program.loggedInUser.Username)
                    {
                        linkedFriends.Add(item[i]);

                    }
                }
            }
            cbFriend.Items.Clear();
            foreach (var friend in linkedFriends)
            {
                if (!String.IsNullOrEmpty(friend))
                {
                    cbFriend.Items.Add(friend);

                }
            }
        }

        private void cbFriend_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblText.Text = "";
            decimal totalSettlement = 0;
            var allExpenseMappings = expenseMapping.Find(a => true).ToList();
            string loggedinUser = Program.loggedInUser.Username;
            var youOwe = from item in allExpenseMappings
                         where item.Payee == loggedinUser && item.Payer == cbFriend.Text
                         select item.Amount;
            var totalOwe = youOwe.Sum();
            var youGet = from item in allExpenseMappings
                         where item.Payee == cbFriend.Text && item.Payer == loggedinUser
                         select item.Amount;
            var totalGet = youGet.Sum();
            if (totalGet > totalOwe)
            {
                totalSettlement = totalSettlement + totalGet - totalOwe;
                lblText.Text = $"You will get ${totalSettlement} from {cbFriend.Text}, No need to pay!";
            }
            else if (totalGet < totalOwe)
            {
                totalSettlement = totalSettlement - (totalOwe - totalGet);
                lblText.Text = $"You owe ${totalSettlement * -1} to {cbFriend.Text}";
            }
            else {
                lblText.Text = $"You are settlep up with {cbFriend.Text}, NO need to pay!";
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Home home = new Home();
            this.Hide();
            home.Show();
        }
    }
}
