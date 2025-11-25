using SmartRecordMatcher.Models;
using SmartRecordMatcher.Models.SmartRecordMatcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRecordMatcher.Services
{
    public class StorageService
    {
        public WeightConfig LoadWeightConfig()
        {
            //TODO: در آینده از فایل weights.json بخوان
            return new WeightConfig();
        }
    }

}
