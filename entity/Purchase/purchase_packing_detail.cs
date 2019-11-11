namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Linq;
    public partial class purchase_packing_detail : Audit, IDataErrorInfo
    {
        public purchase_packing_detail()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            purchase_packing_detail_relation = new List<purchase_packing_detail_relation>();
            child = new List<purchase_packing_detail>();
            timestamp = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_purchase_packing_detail { get; set; }

        public int id_purchase_packing { get; set; }
        public int? id_purchase_order_detail { get; set; }
        public int? id_location { get; set; }

        [Required]
        [CustomValidation(typeof(Class.EntityValidation), "CheckId")]
        public int id_item
        {
            get
            {
                return _id_item;
            }
            set
            {
                if (value > 0)
                {
                    _id_item = value;
                    RaisePropertyChanged("unit_price");
                    RaisePropertyChanged("unit_cost");
                }
            }
        }

        private int _id_item;

        [Required]
        [CustomValidation(typeof(Class.EntityValidation), "CheckIddecimal")]
        public decimal quantity
        {
            get { return _quantity; }
            set
            {
                if (Convert.ToDecimal(value) > 0)
                {
                    _quantity = value;
                }
            }
        }


        private decimal _quantity;

        public DateTime? expire_date { get; set; }
        public string batch_code { get; set; }

        public decimal? gross_weight { get; set; }
        public decimal? net_weight { get; set; }
        public decimal? volume { get; set; }

        public int? id_branch { get; set; }

        decimal _VerifiedQuantity;
        [NotMapped]
        public decimal VerifiedQuantity
        {
            get { return Convert.ToDecimal(child.Sum(x=>x.verified_quantity)); }
            set
            {
                _VerifiedQuantity = value;
            }
        }

        public decimal? verified_quantity
        {
            get
            {
                //if (_verified_quantity == null)
                //{
                //    _verified_quantity = quantity;
                //}
                return _verified_quantity;
            }
            set { _verified_quantity = value; RaisePropertyChanged("verified_quantity"); }
        }

        private decimal? _verified_quantity;

        public int? verified_by
        {
            get { return _verified_by; }
            set
            {
                _verified_by = value;
                RaisePropertyChanged("verified_by");
            }
        }

        private int? _verified_by;

        public virtual purchase_packing purchase_packing { get; set; }
        public virtual purchase_order_detail purchase_order_detail { get; set; }
        public virtual app_location app_location { get; set; }
        public virtual item item { get; set; }
        public virtual app_measurement measurement_weight { get; set; }
        public virtual app_measurement measurement_volume { get; set; }
        public virtual ICollection<item_movement> item_movement { get; set; }
        public virtual ICollection<item_movement_archive>  item_mov_archive { get; set; }
        public virtual ICollection<purchase_packing_dimension> purchase_packing_dimension { get; set; }
        public virtual ICollection<purchase_packing_detail_relation> purchase_packing_detail_relation { get; set; }

        public virtual ICollection<purchase_packing_detail> child { get; set; }

        public virtual purchase_packing_detail parent { get; set; }


        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

                // iterate over all of the properties
                // of this object - aggregating any validation errors
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    String propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }

        public string this[string columnName]
        {
            get
            {
                // apply property level validation rules
                if (columnName == "id_item")
                {
                    if (id_item == 0)
                        return Brillo.Localize.PleaseSelect;
                }
                if (columnName == "quantity")
                {
                    if (quantity == 0)
                    {
                        return "Quantity cannot be zero";
                    }
                    else if (purchase_order_detail != null)
                    {
                        if (purchase_order_detail.quantity < quantity)
                        {
                            return "Quantity exceded";
                        }
                    }
                }
                return "";
            }
        }
    }
}