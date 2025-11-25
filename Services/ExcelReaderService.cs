using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using SmartRecordMatcher.Models;

namespace SmartRecordMatcher.Services
{
    public static class ExcelReaderService
    {
        public static List<RowRecord> ReadSimpleAddressFile(string path)
        {
            var records = new List<RowRecord>();

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            using var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheet(1); // اولین شیت

            // فرض می‌کنیم ستون اول: Id، ستون دوم: Address
            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows)
            {
                string id = row.Cell(1).GetString().Trim();
                string address = row.Cell(2).GetString().Trim();

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(address))
                {
                    records.Add(new RowRecord
                    {
                        Id = id,
                        OriginalAddress = address
                    });
                }
            }

            return records;
        }
    }
}
