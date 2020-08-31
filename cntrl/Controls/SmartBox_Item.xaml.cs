﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Data;
using entity.Brillo;

namespace cntrl.Controls
{
    public partial class SmartBox_Item : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string prop)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        //Settings that if Marked True will show Quantity Popup.
        public static readonly DependencyProperty QuantityIntegrationProperty = DependencyProperty.Register("QuantityIntegration", typeof(bool), typeof(SmartBox_Item));

        public bool QuantityIntegration
        {
            get { return (bool)GetValue(QuantityIntegrationProperty); }
            set { SetValue(QuantityIntegrationProperty, value); }
        }

        //Quantity of the Popup control if used.
        public decimal Quantity
        {
            get { return _Quantity; }
            set
            {
                _Quantity = value;
                RaisePropertyChanged("Quantity");
            }
        }

        private decimal _Quantity;

        public enum Types
        {
            InStock_Only,
            InStock_wServices,
            All
        }

        //Setting that if Marked true, will exclude Out Of Stock.
        private Types _Type = Types.All;
        public Types Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    _Type = value;
                    RaisePropertyChanged("Type");
                }
            }
        }

        private bool _ExactSearch;
        public bool ExactSearch
        {
            get { return _ExactSearch; }
            set
            {

                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                _ExactSearch = Sec.SpecialSecurity_ReturnsBoolean(entity.Privilage.Privilages.ItemBarcodeSearchOnly) ? value : false;
                RaisePropertyChanged("ExactSearch");
            }
        }

        public bool can_New
        {
            get { return _can_new; }
            set
            {
                _can_new = new entity.Brillo.Security(entity.App.Names.Items).create ? value : false;
                RaisePropertyChanged("can_New");
            }
        }

        private bool _can_new;

        public bool can_Edit
        {
            get { return _can_new; }
            set
            {
                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                _can_edit = new entity.Brillo.Security(entity.App.Names.Items).edit ? value : false;
                RaisePropertyChanged("can_Edit");
            }
        }

        private bool _can_edit;

        public decimal QuantityInStock { get; set; }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SmartBox_Item));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty LocationIDProperty = DependencyProperty.Register("LocationID", typeof(int), typeof(SmartBox_Item), new FrameworkPropertyMetadata(
            0,
            new PropertyChangedCallback(OnLocationChangeCallBack)));

        public int LocationID
        {
            get { return (int)GetValue(LocationIDProperty); }
            set { SetValue(LocationIDProperty, value); }
        }


        #region "INotifyPropertyChanged"

        private static void OnLocationChangeCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SmartBox_Item c = sender as SmartBox_Item;
            if (c != null)
            {
                c.OnLocationChange((int)e.NewValue);
            }
        }

        protected virtual void OnLocationChange(int newvalue)
        {
            var task = Task.Factory.StartNew(() => LoadData(newvalue));
        }


        #endregion "INotifyPropertyChanged"
        public event RoutedEventHandler Select;

        private void ItemGrid_MouseDoubleClick(object sender, EventArgs e)
        {
            if (itemViewSource != null)
            {
                if (itemViewSource.View != null)
                {
                    entity.Brillo.StockList Item = itemViewSource.View.CurrentItem as entity.Brillo.StockList;

                    if (Item != null)
                    {
                        ItemID = Item.ItemID;
                        if (Item.Quantity != null)
                        {
                            QuantityInStock = (decimal)Item.Quantity;
                        }

                        ItemPopUp.IsOpen = false;
                        //  Text = Item.Name;
                        if (Quantity <= 1)
                        {
                            Quantity = 1;
                        }
                    }
                    else
                    {
                        ItemID = 0;
                        if (Quantity <= 1)
                        {
                            Quantity = 1;
                        }
                        Text = tbxSearch.Text;
                    }

                }
            }

            Select?.Invoke(this, new RoutedEventArgs());
            Text = "";
            tbxSearch.Focus();
            tbxSearch.SelectAll();
        }

        public int ItemID { get; set; }

        public entity.item.item_type? item_types { get; set; }

        public List<entity.Brillo.StockList> Items = new List<entity.Brillo.StockList>();
        // public IQueryable<entity.BrilloQuery.Item> Items { get; set; }

        //private Task taskSearch;
        private CancellationTokenSource tokenSource;

        private CancellationToken token;

        private CollectionViewSource itemViewSource;

        public SmartBox_Item()
        {
            InitializeComponent();


            Security Sec = new Security(App.Names.Items);
            //InStock_Only = Sec.SpecialSecurity_ReturnsBoolean(entity.Privilage.Privilages.Include_OutOfStock) == true ? false : true;

            if (CurrentSession.Allow_BarCodeSearchOnly)
            {
                ExactSearch = true;
            }
            else
            {
                ExactSearch = false;
            }
            smartBoxItemSetting.Default.Save();

            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(LoginControl_IsVisibleChanged);
            itemViewSource = ((CollectionViewSource)(FindResource("itemViewSource")));
        }

        private void _SmartBox_Item_Loaded(object sender, RoutedEventArgs e)
        {
            int LocId = LocationID;
            LoadData(LocationID);

            //Basic Data like Salesman, Contracts, VAT, Currencies, etc to speed up Window Load.
            //Load_BasicData(null, null);
            //Load Basic Data into Timer.
            //System.Timers.Timer myTimer = new System.Timers.Timer();
            //myTimer.Elapsed += new ElapsedEventHandler(LoadData);
            //myTimer.Interval = 60000;
            //myTimer.Start();
        }

        private void LoadData(int LocId)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                progBar.Visibility = Visibility.Visible;
                tbxSearch.IsEnabled = false;
            }));

            
            LoadData_Thread(LocId, false);
            // var task = Task.Factory.StartNew(() => LoadData_Thread(LocId, IgnorStock));
        }

        private void forceLoadData(int LocId)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                progBar.Visibility = Visibility.Visible;
                tbxSearch.IsEnabled = false;
            }));

            LoadData_Thread(LocId, true);
            // var task = Task.Factory.StartNew(() => LoadData_Thread(LocId, IgnorStock));
        }

        private void LoadData_Thread(int LocID, bool forceData)
        {
            Stock Stock = new Stock();

            Items.Clear();
            using (db db = new db())
            {
                List<item> ItemList = db.items.Where(x => x.id_company == CurrentSession.Id_Company && x.is_active).ToList();

                foreach (item item in ItemList)
                {
                    StockList data = new StockList
                    {
                        ItemID = item.id_item,
                        CompanyID = Convert.ToInt32(item.id_company),
                        Name = item.name,
                        Code = item.code,
                        Type = Convert.ToInt32(item.id_item_type),
                        can_expire = false
                    };

                    Items.Add(data);
                }

            }

            var task = Task.Factory.StartNew(() => AddStock());


            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                tbxSearch.IsEnabled = true;
                progBar.Visibility = Visibility.Collapsed;
            }));

        }

        private void AddStock()
        {
            try
            {
                int BranchID = CurrentSession.Id_Branch;
                string strstock = @"
                                select Max(LocationID) as LocationID, 
                           Max(Location) as Location,
                           Max(BranchID) as BranchID,
                           Max(MovementID) as MovementID,
                           Max(MovementID) as MovementID,
						   ItemID,
                           Max(ProductID) as ProductID,
                           Max(can_expire) as can_expire,
                           Max(MovementRelID) as MovementRelID,
                           Max(Cost) as Cost,
                           sum(Quantity) as Quantity,
                           Max(ConversionQuantity) as ConversionQuantity,
                           Max(BatchCode) as BatchCode,
                           Max(ExpiryDate) as ExpiryDate,
                           Max(TransDate) as TransDate,
                           Max(BarCode) as BarCode
                            from(  select  
                                l.id_location as LocationID
                                , l.name as Location
                                , l.id_branch as BranchID
                                , im.id_movement as MovementID
                                , ip.id_item as ItemID
                                , ip.id_item_product as ProductID
                                , ip.can_expire
                                , im.id_movement_value_rel as MovementRelID
                                , (select sum(unit_value) from item_movement_value_detail where id_movement_value_rel = im.id_movement_value_rel) as Cost
                                , (im.credit - sum(IFNULL(child.debit,0))) as Quantity
                                , (im.credit - sum(IFNULL(child.debit,0))) * max(icf.value) * (select ROUND(EXP(SUM(LOG(`value`))),4) as value from item_movement_dimension where id_movement = im.id_movement) as ConversionQuantity
                                , im.code as BatchCode
                                , im.expire_date as ExpiryDate
                                ,im.trans_date as TransDate
                                ,im.barcode as BarCode

                                from item_movement as im
                                left join item_movement as child on im.id_movement = child.parent_id_movement
                                inner join item_product as ip on im.id_item_product = ip.id_item_product
                                left join item_conversion_factor as icf on ip.id_item_product = icf.id_item_product
                                inner join app_location as l on im.id_location = l.id_location
                                left join item_movement_value_rel as imvr on im.id_movement_value_rel = imvr.id_movement_value_rel
                                where im.id_company = {0} and l.id_branch = {1}
                                group by im.id_movement
                                HAVING (max(im.credit) - sum(IFNULL(child.debit,0))) > 0
                                 ) as i
                                group by ItemID
                                order by ExpiryDate";

                strstock = String.Format(strstock, CurrentSession.Id_Company, BranchID);

                DataTable dtstock = entity.CurrentItems.stock.exeDT(strstock);
                foreach (DataRow itemRow in dtstock.Rows)
                {
                    decimal Quantity = Convert.ToDecimal(itemRow["Quantity"]);
                    decimal ItemID = Convert.ToDecimal(itemRow["ItemID"]);

                    if (Items.Where(x => x.ItemID == ItemID).Count() > 0)
                    {
                        Items.Where(x => x.ItemID == ItemID).FirstOrDefault().Quantity = Quantity;
                    }
                }

                if (Type == Types.InStock_Only)
                {
                    //delete items without stock and services. should only show items in stock
                    List<StockList> DeleteList = Items.Where(x => ((x.Type == 1 || x.Type == 2 || x.Type == 6) && (x.Quantity == 0 || x.Quantity == null))
                    || (x.Type == 3 || x.Type == 4 || x.Type == 5 || x.Type == 7)).ToList();
                    foreach (StockList item in DeleteList)
                    {
                        Items.Remove(item);
                    }
                }
                else if (Type == Types.InStock_wServices)
                {
                    //delete items without stock. but keep services
                    List<StockList> DeleteList = Items.Where(x => (x.Type == 1 || x.Type == 2 || x.Type == 6) && (x.Quantity == 0 || x.Quantity == null)).ToList();
                    foreach (StockList item in DeleteList)
                    {
                        Items.Remove(item);
                    }
                }
                else
                {
                    //remove nothing. keep everything.
                }
            }
            catch
            { }
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

        private void Quantity_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (QuantityIntegration)
                {
                    //  Quantity = 1;
                    popQuantity.IsOpen = false;
                    ItemGrid_MouseDoubleClick(sender, e);
                    FocusManager.SetFocusedElement(tbxSearch.Parent, tbxSearch);
                }
            }
        }

        private void StartSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (QuantityIntegration)
                {
                    Quantity = 1;
                    ItemPopUp.IsOpen = false;
                    popQuantity.IsOpen = true;
                    tbxQuantity.Focus();
                }
                else
                {
                    Quantity = 1;
                    ItemGrid_MouseDoubleClick(sender, e);
                }
            }
            else if (e.Key == Key.Up)
            {
                if (itemViewSource != null)
                {
                    if (itemViewSource.View != null)
                    {
                        itemViewSource.View.MoveCurrentToPrevious();
                        itemViewSource.View.Refresh();
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                if (itemViewSource != null)
                {
                    if (itemViewSource.View != null)
                    {
                        itemViewSource.View.MoveCurrentToNext();
                        itemViewSource.View.Refresh();
                    }
                }
            }
            else
            {
                string SearchText = tbxSearch.Text;

                if (SearchText.Count() >= 1)
                {
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    Search_OnThread(SearchText);
                }
            }
        }

        private void Search_OnThread(string SearchText)
        {

            var predicate = PredicateBuilder.True<entity.Brillo.StockList>();
            entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);

            if (Sec.SpecialSecurity_ReturnsBoolean(Privilage.Privilages.ItemBarcodeSearchOnly))
            {

            }

            if (_ExactSearch)
            {
                predicate = (x => (x.CompanyID == CurrentSession.Id_Company) && (x.Code == SearchText));
            }
            else
            {
                predicate = (x => (x.CompanyID == CurrentSession.Id_Company) &&
                    (
                        x.Code.ToLower().Contains(SearchText.ToLower())
                        || x.Name.ToLower().Contains(SearchText.ToLower())
                    ));

                if (item_types != null)
                {
                    predicate = predicate.And(x => x.Type == (int)item_types);
                }
            }

            itemViewSource.Source = Items.AsQueryable().Where(predicate).OrderBy(x => x.Name);


            //if (Type == Types.InStock_wServices)
            //{
            //    itemViewSource.Source = CurrentItems.getItems_GroupBy(CurrentSession.Id_Branch, DateTime.Now, false, true)
            //    .AsQueryable()
            //    .Where(predicate)
            //    .OrderBy(x => x.Name);
            //}
            //else if (Type == Types.InStock_Only)
            //{
            //    itemViewSource.Source = CurrentItems.getProducts_InStock_GroupBy(CurrentSession.Id_Branch, DateTime.Now, false)
            //    .AsQueryable()
            //    .Where(predicate)
            //    .OrderBy(x => x.Name);
            //}
            //else
            //{
            //    itemViewSource.Source = CurrentItems.getItems_GroupBy(CurrentSession.Id_Branch, DateTime.Now, false, false)
            //    .AsQueryable()
            //    .Where(predicate)
            //    .OrderBy(x => x.Name);
            //}



            ItemPopUp.IsOpen = true;
        }

        private void Add_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
            if (Sec.create)
            {
                Curd.item item = new Curd.item();
                item.itemobject = new item();
                popCrud.IsOpen = true;
                popCrud.Visibility = Visibility.Visible;
                ContactPopUp.Children.Add(item);
            }
        }

        private void crudItem_btnCancel_Click(object sender)
        {
            popCrud.IsOpen = false;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            popupCustomize.IsOpen = false;
            popupCustomize.Visibility = Visibility.Collapsed;
        }

        private void Label_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.IsOpen = true;
            popupCustomize.Visibility = Visibility.Visible;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            smartBoxItemSetting.Default.Save();
        }

        private void Refresh_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            forceLoadData(LocationID);
        }

        public void SmartBoxItem_Focus()
        {
            tbxSearch.Focus();
        }
    }
}