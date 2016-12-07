using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TransactionService.Domain
{
    public class Transaction : IExtensibleDataObject
    {
        //Forward upgrade compatibility
        private ExtensionDataObject theData;
        public virtual ExtensionDataObject ExtensionData
        {
            get { return theData; }
            set { theData = value; }
        }
        //Current properties
        public int Suid { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Listing { get; set; }
        public decimal Price { get; set; }
        public int ShareAmount { get; set; }
    }
}
