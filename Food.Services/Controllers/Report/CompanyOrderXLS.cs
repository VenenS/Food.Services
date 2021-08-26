using ClosedXML.Excel;
using System;
using System.IO;

namespace ITWebNet.Food.Controllers
{
    public class CompanyOrderXls
    {
        public byte[] GetXlsBytes(ReportData data)
        {
            byte[] bytes = null;
            if (data != null && data.OrdersData.Count > 0)
            {
                XLWorkbook wb;
                using (var sr = new StreamReader(Path.Combine(Environment.CurrentDirectory,
                    @"templates\CompanyOrderReportTemplate.xlsx")))
                {
                    wb = new XLWorkbook(sr.BaseStream);
                }

                var workSheetTemplate = wb.Worksheets.Worksheet("template");
                var customerTemplate = wb.Worksheets.Worksheet("order");
                var positionTemplate = wb.Worksheets.Worksheet("position");

                customerTemplate.NamedRanges.Add("Статус", "F3:G3");

                var newWorkBook = new XLWorkbook();
                foreach (var order in data.OrdersData)
                {
                    //создаем лист по шаблону
                    var newWorkSheet =
                        workSheetTemplate.CopyTo(order.CompanyOrder.DeliveryDate?.ToString("dd.MM.yyyy") + " " + order.Company.Name??
                                                 order.CompanyOrder.CreateDate.Value.ToString("dd.MM.yyyy") + " " + order.Company.Name);
                    foreach (var co in order.Orders)
                    {
                        //заполняем данными
                        newWorkSheet.NamedRanges.NamedRange("Адрес").Ranges.Value =
                            order.Company.DeliveryAddress != null
                                ? string.Format("г. {0} ул. {1} д. {2} кв/оф {3}",
                                    order.Company.DeliveryAddress.CityName,
                                    order.Company.DeliveryAddress.StreetName, order.Company.DeliveryAddress.HouseNumber,
                                    order.Company.DeliveryAddress.OfficeNumber)
                                : string.Empty;
                        newWorkSheet.NamedRanges.NamedRange("Заказчик").Ranges.Value = order.Company.Name ?? co.Orders[0].User.Email;
                        newWorkSheet.NamedRanges.NamedRange("Дата").Ranges.Value = order.CompanyOrder.DeliveryDate;
                        newWorkSheet.NamedRanges.NamedRange("Сумма_заказа").Ranges.Value =
                            order.TotalPrice + " руб.";

                        newWorkSheet.Ranges("B5,J5").Value = data.Cafe.Address;
                        newWorkSheet.Ranges("B6,J6").Value = data.Cafe.Phone;

                        string address = "";
                        foreach (var userOrder in co.Orders)
                        {
                            // Создаем заказ по шаблону и заполняем данными                        
                            var userWorksheet = customerTemplate.CopyTo(Guid.NewGuid().ToString().Substring(0, 5));
                            if (address != null && address != userOrder.Order?.OrderInfo?.OrderAddress)
                            {
                                address = userOrder.Order?.OrderInfo?.OrderAddress;
                                userWorksheet.NamedRanges.NamedRange("Empty").Ranges.Value = userOrder.Order?.OrderInfo?.OrderAddress;
                            }
                            userWorksheet.NamedRanges.NamedRange("Клиент").Ranges.Value = userOrder.User.UserFullName;
                            userWorksheet.NamedRanges.NamedRange("Статус").Ranges.Value = userOrder.OrderStatusReport;
                            userWorksheet.NamedRanges.NamedRange("Итого").Ranges.Value = userOrder.TotalPrice;
                            var lastRow = newWorkSheet.LastRowUsed().RowNumber();
                            newWorkSheet.Row(lastRow).InsertRowsBelow(1);
                            var firstTableCell = userWorksheet.FirstCell();
                            var lastTableCell = userWorksheet.LastCell();
                            var rngData = userWorksheet.Range(firstTableCell.Address, lastTableCell.Address);
                            newWorkSheet.Cell(lastRow + 1, 1).Value = rngData;

                            switch (userOrder.OrderStatusReport)
                            {
                                case "Новый заказ": positionTemplate.Style.Fill.BackgroundColor = XLColor.FromArgb(220, 230, 241); break;
                                case "Заказ отменён": positionTemplate.Style.Fill.BackgroundColor = XLColor.FromArgb(253, 233, 217); break;
                                default: positionTemplate.Style.Fill.BackgroundColor = XLColor.FromArgb(235, 241, 222); break;
                            }

                            foreach (var position in userOrder.OrderDishes)
                            {
                                
                                var orderPosition = positionTemplate.CopyTo(position.Dish.Id.ToString());
                                orderPosition.NamedRanges.NamedRange("Наименование").Ranges.Value = position.Dish.Name;
                                orderPosition.NamedRanges.NamedRange("Количество").Ranges.Value = position.ItemCount;
                                orderPosition.NamedRanges.NamedRange("Цена").Ranges.Value = position.Dish.BasePrice;
                                orderPosition.NamedRanges.NamedRange("Скидка").Ranges.Value =
                                    position.ItemDiscount / 100d;
                                orderPosition.NamedRanges.NamedRange("Сумма").Ranges.Value = position.ItemTotalPrice;
                                var firstCell = orderPosition.FirstCell();
                                var lastCell = orderPosition.LastCell();
                                var dishData = orderPosition.Range(firstCell.Address, lastCell.Address);
                                lastRow = newWorkSheet.LastRowUsed().RowNumber();
                                newWorkSheet.Cell(lastRow + 1, 1).Value = dishData;
                            }
                        }
                    }
                    newWorkSheet.CopyTo(newWorkBook, order.CompanyOrder.DeliveryDate.Value.Date.ToString("dd.MM.yyyy") + " " + order.Company.Name);
                }
                using (var ms = new MemoryStream())
                {
                    newWorkBook.SaveAs(ms);
                    bytes = ms.ToArray();
                }
            }
            else
            {
                //формируем пустой документ если нет отчетов для выгрузки
                XLWorkbook wb;
                using (var sr = new StreamReader(Path.Combine(Environment.CurrentDirectory,
                    @"templates\CompanyOrderReportTemplate.xlsx")))
                {
                    wb = new XLWorkbook(sr.BaseStream);
                }

                var workSheetTemplate = wb.Worksheets.Worksheet("template");
                var customerTemplate = wb.Worksheets.Worksheet("order");
                var positionTemplate = wb.Worksheets.Worksheet("position");
                var newWorkBook = new XLWorkbook();
                var newWorkSheet =
                        workSheetTemplate.CopyTo(data.EndDate.ToShortDateString());
                newWorkSheet.Ranges("B5,J5").Value = "";
                newWorkSheet.Ranges("B6,J6").Value = "";
                newWorkSheet.CopyTo(newWorkBook, data.EndDate.ToShortDateString());
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