using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Food.Services.GenerateXLSX.Model;
using System;
using System.IO;
// ReSharper disable PossiblyMistakenUseOfParamsMethod
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace Food.Services.GenerateXLSX
{
    /// <summary>
    /// Генерация excel документа с заказами для сотрудников компании
    /// </summary>
    public class CompanyOrderReportForUsers
    {
        public byte[] CreatePackageAsBytes(ReportUsersOrders model)
        {
            using (var mstm = new MemoryStream())
            {
                using (var document = SpreadsheetDocument.Create(mstm, SpreadsheetDocumentType.Workbook))
                {
                    CreateParts(document, model);
                }
                mstm.Flush();
                mstm.Close();
                return mstm.ToArray();
            }
        }

        private void CreateParts(SpreadsheetDocument document, ReportUsersOrders model)
        {
            var workbookPart = document.AddWorkbookPart();
            var workbook = new Workbook();

            var sheets = new Sheets();
            var sheet = new Sheet() { Id = "rId1", SheetId = 1, Name = "Отчет" };
            sheets.Append(sheet);

            workbook.Append(sheets);

            workbookPart.Workbook = workbook;

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId1");
            var worksheet = new Worksheet();

            var columns = new Columns(
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true }
                );

            uint styleDefaultLrtbBorder = 1;
            uint styleTitleBold = 2;
            uint styleHeadFillEmployee = 3;
            uint styleHeadFillOrder = 4;
            uint styleTbBorder = 5;
            uint styleRtbBorder = 6;
            uint styleFillEmployeeTbBorder = 7;
            uint styleFillEmployeeRtbBorder = 8;
            //uint styleFillOrderTbBorder = 9;
            uint styleFillOrderRtbBorder = 10;

            var heightRow = 19;

            var mergeCells = new MergeCells();
            long iRow = 1; //Номер строки

            var sheetData = new SheetData();

            #region Шапка

            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:J{iRow++}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:J{iRow++}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:J{iRow++}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:J{iRow++}") });

            sheetData.Append(new Row(new Cell()
            {
                StyleIndex = styleTitleBold,
                CellValue = new CellValue("ОТЧЕТ ПО ЗАКАЗАМ СОТРУДНИКОВ ЗА ПЕРИОД"),
                DataType = CellValues.String
            },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleTbBorder },
            new Cell() { StyleIndex = styleRtbBorder })
            { Height = 25 });

            sheetData.Append(
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Период"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue(string.Format("С {0} по {1}", model.StartDate, model.EndDate)),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleRtbBorder }
                    )
                { Height = heightRow },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Заказчик"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue(model.Company.Name),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleRtbBorder }
                    )
                { Height = heightRow },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Сумма заказов за период"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue(model.TotalSumm + " руб."),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleRtbBorder }
                    ));

            foreach (var employeeOrder in model.Employee)
            {
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:J{iRow++}") });
                sheetData.Append(new Row());

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:F{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"G{iRow}:H{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:C{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"F{iRow}:G{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });

                var userName = (!string.IsNullOrEmpty(employeeOrder.User.UserFullName))? employeeOrder.User.UserFullName : employeeOrder.User.Email;

                sheetData.Append(new Row(
                    new Cell()
                    {
                        StyleIndex = styleHeadFillEmployee,
                        CellValue = new CellValue("Сотрудник"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleHeadFillEmployee,
                        CellValue = new CellValue(userName),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleFillEmployeeTbBorder },
                    new Cell() { StyleIndex = styleFillEmployeeTbBorder },
                    new Cell() { StyleIndex = styleFillEmployeeTbBorder },
                    new Cell() { StyleIndex = styleFillEmployeeRtbBorder },
                    new Cell()
                    {
                        StyleIndex = styleHeadFillEmployee,
                        CellValue = new CellValue("Итого за период:"),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleFillEmployeeRtbBorder },
                    new Cell()
                    {
                        StyleIndex = styleHeadFillEmployee,
                        CellValue = new CellValue(employeeOrder.TotalSumm.ToString()),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleFillEmployeeRtbBorder }
                    )
                { Height = heightRow });

                sheetData.Append(new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Наименование"),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleRtbBorder },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Ед. изм."),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Кол-во"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Цена, руб."),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleRtbBorder },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Скидка"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefaultLrtbBorder,
                        CellValue = new CellValue("Сумма, руб."),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleRtbBorder }
                    )
                { Height = heightRow });

                #endregion

                #region Заказы

                foreach (var order in employeeOrder.Orders)
                {
                    var orderStatusStr = string.Empty;
                    switch (order.Status)
                    {
                        case 1:
                            orderStatusStr = "Новый заказ";
                            break;
                        case 2:
                            orderStatusStr = "Заказ принят кафе";
                            break;
                        case 3:
                            orderStatusStr = "Заказ в процессе доставки";
                            break;
                        case 4:
                            orderStatusStr = "Заказ успешно доставлен";
                            break;
                        case 5:
                            orderStatusStr = "Заказ отменён";
                            break;
                        case 6:
                            orderStatusStr = "Unknown";
                            break;
                    }
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:C{iRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"D{iRow}:E{iRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"F{iRow}:G{iRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });
                    sheetData.Append(
                        new Row(
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue("Заказ №"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue(order.Id.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleFillOrderRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue("Дата заказа"),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleFillOrderRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue(order.Create.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleFillOrderRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue("Итого:"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleHeadFillOrder,
                                CellValue = new CellValue(order.TotalSum.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleFillOrderRtbBorder }
                        )
                        { Height = heightRow });

                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:J{iRow++}") });
                    sheetData.Append(
                        new Row(
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue("Адрес доставки"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(order.OrderInfo.OrderAddress),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleRtbBorder }
                        )
                        { Height = heightRow });

                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:F{iRow++}") });
                    sheetData.Append(
                        new Row(
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue("Исполнитель"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(String.Format("{0}", order.Cafe.FullName)),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue("Статус"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(String.Format("{0}", orderStatusStr)),
                                DataType = CellValues.String
                            },

                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleRtbBorder }
                        )
                        { Height = heightRow });

                    #region элементы заказа

                    foreach (var orderItem in order.OrderItems)
                    {
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:C{iRow}") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"F{iRow}:G{iRow}") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });
                        sheetData.Append(
                        new Row(
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(orderItem.DishName),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue("шт."),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(orderItem.DishCount.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(orderItem.DishBasePrice.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue((orderItem.Discount == 0)? String.Empty: orderItem.Discount + "%"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(orderItem.TotalPrice.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleRtbBorder }
                        )
                        { Height = heightRow });
                    }

                    #endregion

                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:E{iRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"F{iRow}:G{iRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });
                    sheetData.Append(
                        new Row(
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue("Доставка"),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleTbBorder },
                            new Cell() { StyleIndex = styleRtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(order.OrderInfo.DeliverySumm.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleRtbBorder },
                            new Cell() { StyleIndex = styleDefaultLrtbBorder },
                            new Cell()
                            {
                                StyleIndex = styleDefaultLrtbBorder,
                                CellValue = new CellValue(order.OrderInfo.DeliverySumm.ToString()),
                                DataType = CellValues.String
                            },
                            new Cell() { StyleIndex = styleRtbBorder }
                        )
                        { Height = heightRow });

                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:J{iRow++}") });
                    sheetData.Append(new Row());

                }

                #endregion
            }

            worksheet.Append(sheetData);
            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Append(mergeCells);
            worksheetPart.Worksheet.Save();

            #region Стили

            var workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("sId1");

            var stylesheet = new Stylesheet(
                new Fonts(
                    new Font() { FontSize = new FontSize() { Val = 13 }, FontName = new FontName() { Val = "Times New Roman" }, Bold = new Bold() },
                    new Font() { FontSize = new FontSize() { Val = 10 }, FontName = new FontName() { Val = "Times New Roman" } }),
                new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "fff2f2f2" } }) { PatternType = PatternValues.Solid }),
                    new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "ffcfe6f4" } }) { PatternType = PatternValues.Solid })
                    ),
                new Borders(
                    new Border(
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()
                        ),
                        new Border(
                        new LeftBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                        new RightBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                        new TopBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                        new BottomBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin }
                        ),
                        new Border(
                            new TopBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                            new BottomBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin }
                            ),
                        new Border(
                            new RightBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                            new TopBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin },
                            new BottomBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin }
                            )
                            ),
                new CellFormats(
                    new CellFormat() { FontId = 0, BorderId = 0, ApplyFont = true, ApplyBorder = true },
                    //styleDefault_LRTBBorder = 1
                    new CellFormat() { FontId = 1, BorderId = 1, ApplyFont = true, ApplyBorder = true },
                    //styleTitleBold = 2
                    new CellFormat() { FontId = 0, BorderId = 1, ApplyFont = true, ApplyBorder = true },
                    //styleHeadFillEmployee = 3
                    new CellFormat() { FontId = 1, FillId = 3, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true },
                    //styleHeadFillOrder = 4
                    new CellFormat() { FontId = 1, FillId = 2, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true },
                    //style_TBBorder = 5
                    new CellFormat() { BorderId = 2, ApplyBorder = true },
                    //style_RTBBorder = 6
                    new CellFormat() { BorderId = 3, ApplyBorder = true },
                    //styleFillEmployee_TBBorder = 7
                    new CellFormat() { BorderId = 2, FillId = 3, ApplyBorder = true },
                    //styleFillEmployee_RTBBorder = 8
                    new CellFormat() { BorderId = 3, FillId = 3, ApplyBorder = true },
                    //styleFillOrder_TBBorder = 9
                    new CellFormat() { BorderId = 2, FillId = 2, ApplyBorder = true },
                    //styleFillOrder_RTBBorder = 10
                    new CellFormat() { BorderId = 3, FillId = 2, ApplyBorder = true }
                    )
            );

            workbookStylesPart.Stylesheet = stylesheet;

            #endregion

            workbook.Save();
        }
    }
}