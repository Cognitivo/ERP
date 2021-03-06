﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace entity
{
    public partial class production_order_dimension : Audit, IDataErrorInfo
    {
        public production_order_dimension()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_order_dimension { get; set; }

        public int id_order_detail { get; set; }

        [Required]
        public int id_dimension { get; set; }

        public int id_measurement { get; set; }

        [Required]
        public decimal value { get; set; }

        public virtual production_order_detail production_order_detail { get; set; }
        public virtual app_dimension app_dimension { get; set; }
        public virtual app_measurement app_measurement { get; set; }

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    string propertyError = this[prop.Name];
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
                //apply property level validation rules
                if (columnName == "id_dimension")
                {
                    if (id_dimension <= 0)
                        return "id_dimension cannot be zero";
                }
                return "";
            }
        }
    }
}