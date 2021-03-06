﻿using System;

namespace entity.Reporting
{
    public class NonDirect_Payment
    {
        public string Contact { get; set; }
        public string Code { get; set; }
        public string GovernmentID { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string SalesRep { get; set; }
        public string PaymentType { get; set; }
        public string Number { get; set; }
        public string Account { get; set; }
        public string PaymentTypeNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime PaymentDetailDate { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public decimal BuyRate { get; set; }
        public string Comment { get; set; }
    }
}