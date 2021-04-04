using System;
using System.Collections.Generic;
using System.Text;

namespace PTI.Microservices.Generators.ConsoleConsumer.Models
{
    public class GetInventoryResponseModel
    {
        public int sold { get; set; }
        public int _string { get; set; }
        public int status { get; set; }
        public int pending { get; set; }
        public int available { get; set; }
        public int active { get; set; }
    }

}
