using MongoDB.Bson;
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
    public partial class AddExpense : Form
    {
        public readonly IMongoDatabase _db;
        public AddExpense()
        {
            InitializeComponent();
            var ConnectionString = "mongodb://localhost:27017";
            var client = new MongoClient(ConnectionString);
            _db = client.GetDatabase("SampleExpenseManagement");
            var allGroups = groups.Find(a => true).ToList();
            var loggedinUserGroups = from item in allGroups
                                     where item.Members.Contains(Program.loggedInUser.Username)
                                     select item.Name;
            foreach (var item in loggedinUserGroups)
            {
                cbGroupSelected.Items.Add(item.ToString());
            }
        }
        public IMongoCollection<Group> groups =>
            _db.GetCollection<Group>("UserGroups");
        public IMongoCollection<Expense> expenses =>
            _db.GetCollection<Expense>("Expenses");

        public IMongoCollection<ExpenseMapping> expenseMapping =>
            _db.GetCollection<ExpenseMapping>("ExpenseMapping");
        private void cbGroupSelected_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxMembers.Items.Clear();
            checkboxPaidBy.Items.Clear();
            var allGroups = groups.Find(a => true).ToList();
            var loggedinUserGroups = from item in allGroups
                                     where item.Name == cbGroupSelected.Text
                                     select item.Members;
            foreach (var item in loggedinUserGroups.First())
            {
                checkboxPaidBy.Items.Add(item.ToString());
                //if(item.ToString()!= Program.loggedInUser.Username)
                checkBoxMembers.Items.Add(item.ToString());
            }


        }

        private void btnAddExpense_Click(object sender, EventArgs e)
        {
            var allExpenses = groups.Find(a => true).ToList();
            int count = 1;
            var giventxt = txtExpenseDescription.Text;
            while (true) {
                var loggedinUserGroups1 = from item in allExpenses
                                         where item.Name == txtExpenseDescription.Text
                                         select item.Name;
                if (loggedinUserGroups1.Any())
                {
                    txtExpenseDescription.Text = giventxt + $" {count}";
                    count++;
                }
                else {
                    break;
                }
            }

            if (String.IsNullOrEmpty(cbGroupSelected.Text)) {
                MessageBox.Show("Please select a group!!");
                return;
            }

            if (checkBoxMembers.CheckedItems.Count<2)
            {
                MessageBox.Show("Please select at least 2 members to share the expense!!");
                return;
            }
            if (checkboxPaidBy.CheckedItems.Count != 1)
            {
                MessageBox.Show("Please select the one who paid for the expense!!");
                return;
            }

            if (String.IsNullOrEmpty(txtAmount.Text))
            {
                MessageBox.Show("Please select at least one member to share!!");
                return;
            }

            Decimal dummy;
            var a = Decimal.TryParse(txtAmount.Text, out dummy);
            if (a == false)
            {
                MessageBox.Show("Only integer or decmial allowed in Amount!!");
                return;
            }
            Expense newExpense = new Expense();
            newExpense._id = ObjectId.GenerateNewId().ToString();
            newExpense.Name = txtExpenseDescription.Text;
            var allGroups = groups.Find(a => true).ToList();
            var loggedinUserGroups = from item in allGroups
                                     where item.Name == cbGroupSelected.Text
                                     select item._id;
            newExpense.GroupId = loggedinUserGroups.First();
            newExpense.DateCreated = DateTime.Now;
            newExpense.CreatedBy = Program.loggedInUser.Username;
            newExpense.Amount = Convert.ToDecimal(txtAmount.Text);
            foreach (object item in checkboxPaidBy.CheckedItems)
            {
                newExpense.Payer = item.ToString();
            }
            String[] involved = new  String[checkBoxMembers.CheckedItems.Count];
            var b = 0;
            foreach (object item in checkBoxMembers.CheckedItems)
            {
                involved[b++] = item.ToString();
            }
            newExpense.Involved = involved;
            String[] confirmed = new String[checkBoxMembers.CheckedItems.Count];

            confirmed[0] = newExpense.Payer;
            newExpense.Confirmed = confirmed;

            expenses.InsertOne(newExpense);

            for (int i = 0; i < involved.Length; i++)
            {
                if (involved[i]!=newExpense.Payer)

                {
                    ExpenseMapping mapping = new ExpenseMapping();
                    mapping.ExpenseId = newExpense._id;
                    mapping.GroupId = newExpense.GroupId;
                    mapping.Payer = newExpense.Payer;
                    mapping.Payee = involved[i];
                    mapping.Amount =newExpense.Amount/involved.Length;
                    mapping.IsConfirmed = "pending";
                    expenseMapping.InsertOne(mapping);

                }
            }
            MessageBox.Show("Expense added successfully!");
            Home home = new Home();
            this.Close();
            home.Show();


        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Home home = new Home();
            home.Show();
        }
    }
}
