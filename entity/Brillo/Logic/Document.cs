﻿using System;
using System.Linq;
using System.Windows;

namespace entity.Brillo.Logic
{
    public class Document
    {
        public void Document_PrintPaymentReceipt(payment payment)
        {
            DocumentViewr MainWindow = new DocumentViewr();
            MainWindow.loadPaymentRecieptReport(payment.id_payment);

            Window window = new Window
            {
                Title = "Report",
                Content = MainWindow
            };

            window.ShowDialog();
        }
    }
}
