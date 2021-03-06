﻿using entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace cntrl.PanelAdv
{
    public partial class pnlPacking : UserControl
    {
        private CollectionViewSource sales_packingViewSource;

        private List<sales_packing> _selected_sales_packing = null;
        public List<sales_packing> selected_sales_packing { get { return _selected_sales_packing; } set { _selected_sales_packing = value; } }

        public contact _contact { get; set; }
        public db _entity { get; set; }
        public sales_invoice _sales_invoice;

        public pnlPacking()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Do not load your data at design time.
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                //Load your data here and assign the result to the CollectionViewSource.

                if (_contact != null)
                {
                    sales_packingViewSource = Resources["sales_packingViewSource"] as CollectionViewSource;
                    sales_packingViewSource.Source = _entity.sales_packing.Where(x => x.id_contact == _contact.id_contact && x.status == Status.Documents_General.Approved).ToList();
                }
            }
        }

        public event btnSave_ClickedEventHandler Link_Click;

        public delegate void btnSave_ClickedEventHandler(object sender);

        public void btnSave_MouseUp(object sender, EventArgs e)
        {
            selected_sales_packing = sales_packingDataGrid.ItemsSource.OfType<sales_packing>().Where(x => x.selected == true).ToList();
            Link_Click?.Invoke(sender);
        }

        private void sbxContact_Select(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sbxContact.ContactID > 0)
                {
                    contact contact = _entity.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();

                    sales_packingViewSource = (CollectionViewSource)Resources["sales_packingViewSource"];
                    sales_packingViewSource.Source = _entity.sales_packing.Where(x => x.id_contact == contact.id_contact).ToList();
                }
            }
            catch
            {
                //  toolBar.msgError(ex);
            }
        }
    }
}