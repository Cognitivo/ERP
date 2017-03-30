﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace cntrl.Controls
{
    public partial class SmartBox_Contact : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SmartBox_Contact));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public bool can_New
        {
            get { return _can_new; }
            set
            {
                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                if (Sec.create)
                {
                    _can_new = value;
                    RaisePropertyChanged("can_New");
                }
                else
                {
                    _can_new = false;
                    RaisePropertyChanged("can_New");
                }
            }
        }

        private bool _can_new;

        public bool can_Edit
        {
            get { return _can_new; }
            set
            {
                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                if (Sec.edit)
                {
                    _can_edit = value;
                    RaisePropertyChanged("can_Edit");
                }
                else
                {
                    _can_edit = false;
                    RaisePropertyChanged("can_Edit");
                }
            }
        }

        private bool _can_edit;

        public event RoutedEventHandler Select;

        private void ContactGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (contactViewSource.View != null)
            {
                entity.BrilloQuery.Contact Contact = contactViewSource.View.CurrentItem as entity.BrilloQuery.Contact;

                if (Contact != null)
                {
                    ContactID = Contact.ID;
                    Text = Contact.Name;

                    popContact.IsOpen = false;

                    Select?.Invoke(this, new RoutedEventArgs());
                }
            }
        }

        public int ContactID { get; set; }
        public IQueryable<entity.BrilloQuery.Contact> ContactList { get; set; }

        public bool Get_Customers { get; set; }
        public bool Get_Suppliers { get; set; }
        public bool Get_Employees { get; set; }
        public bool Get_Users { get; set; }
        public bool ExactSearch { get; set; }

        private Task taskSearch;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        private CollectionViewSource contactViewSource;

        public SmartBox_Contact()
        {
            InitializeComponent();

            ///Exists code if in design view.
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            contactViewSource = ((CollectionViewSource)(FindResource("contactViewSource")));

            LoadData();

            IsVisibleChanged += new DependencyPropertyChangedEventHandler(LoginControl_IsVisibleChanged);

            if (rbtnCode.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Code");
            }
            if (rbtnName.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Name");
            }
            if (rbtnGov_ID.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("GovID");
            }
            if (rbtnTel.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Tel");
            }
        }

        public void LoadData()
        {
            progBar.Visibility = Visibility.Visible;
            Task task = Task.Factory.StartNew(() => LoadData_Thread());
        }

        private void LoadData_Thread()
        {
            ContactList = null;
            using (entity.BrilloQuery.GetContacts Execute = new entity.BrilloQuery.GetContacts())
            {
                ContactList = Execute.List.AsQueryable();
            }

            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate () { progBar.Visibility = Visibility.Collapsed; }));
        }

        private void LoginControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    tbxSearch.Focus();
                }));
            }
        }

        private void StartSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ContactGrid_MouseDoubleClick(sender, e);
            }
            else if (e.Key == Key.Up)
            {
                if (contactViewSource != null)
                {
                    if (contactViewSource.View != null)
                    {
                        contactViewSource.View.MoveCurrentToPrevious();
                        contactViewSource.View.Refresh();
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                if (contactViewSource != null)
                {
                    if (contactViewSource.View != null)
                    {
                        contactViewSource.View.MoveCurrentToNext();
                        contactViewSource.View.Refresh();
                    }
                }
            }
            else
            {
                string SearchText = tbxSearch.Text;
                if (rbtnExactCode.IsChecked == true)
                {
                    ExactSearch = true;
                }
                else
                {
                    ExactSearch = false;
                }
                if (SearchText.Count() >= 1)
                {
                    if (taskSearch != null)
                    {
                        if (taskSearch.Status == TaskStatus.Running)
                        {
                            tokenSource.Cancel();
                        }
                    }

                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    taskSearch = Task.Factory.StartNew(() => Search_OnThread(SearchText), token);
                }
            }
        }

        private void Search_OnThread(string SearchText)
        {
            SearchText = SearchText.ToUpper();
            var predicate = PredicateBuilder.True<entity.BrilloQuery.Contact>();

            if (Get_Customers)
            {
                predicate = (x => x.IsCustomer == true && x.IsActive);
            }

            if (Get_Suppliers)
            {
                predicate = (x => x.IsSupplier == true && x.IsActive);
            }

            if (Get_Employees)
            {
                predicate = (x => x.IsEmployee == true && x.IsActive);
            }
            var predicateOR = PredicateBuilder.False<entity.BrilloQuery.Contact>();
            if (ExactSearch)
            {
                predicateOR = (x => x.Code.ToUpper().Equals(SearchText));
                predicate = predicate.And
              (
                  predicateOR
              );
            }
            else
            {
                var param = smartBoxContactSetting.Default.SearchFilter;

                predicateOR = (x => x.Name.ToUpper().Contains(SearchText));

                if (param.Contains("Code"))
                {
                    predicateOR = predicateOR.Or(x => x.Code.ToUpper().Contains(SearchText));
                }

                if (param.Contains("GovID"))
                {
                    predicateOR = predicateOR.Or(x => x.Gov_Code.ToUpper().Contains(SearchText));
                }

                if (param.Contains("Tel"))
                {
                    predicateOR = predicateOR.Or(x => x.Telephone.ToUpper().Contains(SearchText));
                }

                predicate = predicate.And
                (
                    predicateOR
                );
            }

            Dispatcher.InvokeAsync(new Action(() =>
            {
                if (popContact.IsOpen == false)
                {
                    popContact.IsOpen = true;
                }
                try
                {
                    contactViewSource.Source = ContactList.Where(predicate).ToList();
                }
                catch (Exception)
                {
                    throw;
                }
            }));
        }

        private void Label_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.IsOpen = true;
            popupCustomize.Visibility = Visibility.Visible;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            smartBoxContactSetting.Default.Save();
            popupCustomize.IsOpen = false;
            popupCustomize.Visibility = Visibility.Collapsed;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (smartBoxContactSetting.Default.SearchFilter != null)
            {
                smartBoxContactSetting.Default.SearchFilter.Clear();
            }

            if (rbtnCode.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Code");
            }
            if (rbtnName.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Name");
            }
            if (rbtnGov_ID.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("GovID");
            }
            if (rbtnTel.IsChecked == true)
            {
                smartBoxContactSetting.Default.SearchFilter.Add("Tel");
            }

            smartBoxContactSetting.Default.Save();
        }

        private void _SmartBox_Contact_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ContactID = 0;
        }

        private void crudContact_btnCancel_Click(object sender)
        {
            popCrud.IsOpen = false;
        }

        private void openContactCRUD(object sender, MouseButtonEventArgs e)
        {
            entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Contact);

            if (Sec.create)
            {
                cntrl.Curd.contact contactCURD = new Curd.contact();

                if (Get_Customers)
                {
                    contactCURD.IsCustomer = true;
                }
                else if (Get_Suppliers)
                {
                    contactCURD.IsSupplier = true;
                }
                else if (Get_Employees)
                {
                    contactCURD.IsEmployee = true;
                }

                contactCURD.btnSave_Click += popCrud_Closed;

                popCrud.IsOpen = true;
                stackCRUD.Children.Add(contactCURD);
            }
        }

        private void popCrud_Closed(object sender)
        {
            LoadData();
        }

        private void Refresh_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadData();
        }
    }
}