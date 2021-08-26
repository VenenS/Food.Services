using ClosedXML.Excel;
using Food.Services.GenerateXLSX.Model;
using System;
using System.IO;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;

namespace ITWebNet.Food.Controllers
{
    public class UserOrderDetailsXLS
    {
        public byte[] GetXlsBytes(ReportUserOrders data)
        {
            var cafe = Accessor.Instance.GetCafeById(data.Orders[0].CafeId);
            data.Orders[0].Cafe = new CafeModel()
            {
                Address = cafe.Address,
                Phone = cafe.Phone
            };
            byte[] bytes = null;
            if (data != null && data.Orders.Count > 0)
            {
                XLWorkbook wb;
                using (var sr = new StreamReader(Path.Combine(Environment.CurrentDirectory,
                    @"templates\UserOrderDetailsTmpl.xlsx")))
                {
                    wb = new XLWorkbook(sr.BaseStream);
                }
                var workSheetTemplate = wb.Worksheets.Worksheet("template");
                var customerTemplate = wb.Worksheets.Worksheet("order");
                var positionTemplate = wb.Worksheets.Worksheet("position");

                var newWorkBook = new XLWorkbook();
                var newWorkSheet =
                        workSheetTemplate.CopyTo(data.Orders[0].Create.ToShortDateString() ??
                                                 data.Orders[0].DeliverDate.Value.ToShortDateString());
                newWorkSheet.NamedRanges.NamedRange("Номер_заказа").Ranges.Value = "ЗАКАЗ № " + data.Orders[0].Id;
                newWorkSheet.NamedRanges.NamedRange("Заказчик").Ranges.Value = data.User.UserFullName;
                newWorkSheet.NamedRanges.NamedRange("Дата").Ranges.Value = data.Orders[0].Create.ToShortDateString() + " " + data.Orders[0].Create.ToShortTimeString();
                newWorkSheet.NamedRanges.NamedRange("Телефон").Ranges.Value = data.Orders[0].PhoneNumber;
                newWorkSheet.NamedRanges.NamedRange("Комментарии_заказ").Ranges.Value = data.Orders[0].Comment;
                newWorkSheet.NamedRanges.NamedRange("Комментарии_оплата").Ranges.Value = data.Orders[0].OddMoneyComment ?? "-";
                newWorkSheet.NamedRanges.NamedRange("Сумма_заказа").Ranges.Value =
                    data.TotalSumm + " руб.";
                newWorkSheet.NamedRanges.NamedRange("Адрес").Ranges.Value =
                            data.Orders[0].OrderInfo.OrderAddress != null
                                ? data.Orders[0].OrderInfo.OrderAddress.ToString()
                                : string.Empty;
                newWorkSheet.Ranges("B10,J10").Value = "адрес: " + data.Orders[0].Cafe.Address + ", телефон: " + data.Orders[0].Cafe.Phone;
                newWorkSheet.Ranges("B9,J9").Value = data.Orders[0].OrderInfo.DeliverySumm + " руб.";
                var userWorksheet = customerTemplate.CopyTo(Guid.NewGuid().ToString().Substring(0, 5));
                var lastRow = newWorkSheet.LastRowUsed().RowNumber();
                newWorkSheet.Row(lastRow).InsertRowsBelow(1);
                var firstTableCell = userWorksheet.FirstCell();
                var lastTableCell = userWorksheet.LastCell();
                var rngData = userWorksheet.Range(firstTableCell.Address, lastTableCell.Address);
                newWorkSheet.Cell(lastRow + 1, 1).Value = rngData;
                foreach (var position in data.Orders[0].OrderItems)
                {
                    var orderPosition = positionTemplate.CopyTo(position.Id.ToString());
                    orderPosition.NamedRanges.NamedRange("Наименование").Ranges.Value = position.DishName;
                    orderPosition.NamedRanges.NamedRange("Кол").Ranges.Value = position.DishCount;
                    orderPosition.NamedRanges.NamedRange("Цена").Ranges.Value = position.DishBasePrice;
                    orderPosition.NamedRanges.NamedRange("Скидка").Ranges.Value =
                        position.Discount / 100d;
                    orderPosition.NamedRanges.NamedRange("Сумма").Ranges.Value = position.TotalPrice;
                    var firstCell = orderPosition.FirstCell();
                    var lastCell = orderPosition.LastCell();
                    var dishData = orderPosition.Range(firstCell.Address, lastCell.Address);
                    lastRow = newWorkSheet.LastRowUsed().RowNumber();
                    newWorkSheet.Cell(lastRow + 1, 1).Value = dishData;
                }
                newWorkSheet.CopyTo(newWorkBook, data.Orders[0].Create.ToShortDateString());
                using (var ms = new MemoryStream())
                {
                    newWorkBook.SaveAs(ms);
                    bytes = ms.ToArray();
                }


            }
            return bytes;
        }
    }
}