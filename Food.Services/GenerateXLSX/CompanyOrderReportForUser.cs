using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Food.Services.GenerateXLSX.Model;
using System;
using System.Globalization;
using System.IO;
// ReSharper disable PossiblyMistakenUseOfParamsMethod

namespace Food.Services.GenerateXLSX
{
    /// <summary>
    /// Генерация excel документа с заказами для сотрудника
    /// </summary>
    public class CompanyOrderReportForUser
    {
        public byte[] CreatePackageAsBytes(ReportUserOrders model)
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

        private void CreateParts(SpreadsheetDocument document, ReportUserOrders model)
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

            //uint styleDefaultNone = 0;
            uint styleDefault = 1;
            uint styleHeadOrder = 2;
            uint styleDefaultTitleBoldLtrbBorder = 3;
            uint styleTbBorder = 4;
            uint styleTrbBorder = 5;

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
                StyleIndex = styleDefaultTitleBoldLtrbBorder,
                CellValue = new CellValue("Отчет по заказам сотрудника за период"),
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
            new Cell() { StyleIndex = styleTrbBorder })
            { Height = 25 });

            sheetData.Append(
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefault,
                        CellValue = new CellValue("Период"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefault,
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
                    new Cell() { StyleIndex = styleTrbBorder }
                    )
                { Height = heightRow },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefault,
                        CellValue = new CellValue("Заказчик"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefault,
                        CellValue = new CellValue(model.User.UserFullName),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTrbBorder }
                    )
                { Height = heightRow },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleDefault,
                        CellValue = new CellValue("Сумма заказов за период"),
                        DataType = CellValues.String
                    },
                    new Cell()
                    {
                        StyleIndex = styleDefault,
                        CellValue = new CellValue(model.TotalSumm.ToString(CultureInfo.InvariantCulture)),
                        DataType = CellValues.String
                    },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTrbBorder }
                    ));


            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:J{iRow++}") });
            sheetData.Append(new Row());

            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:C{iRow}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"F{iRow}:G{iRow}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"I{iRow}:J{iRow++}") });
            sheetData.Append(new Row(
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Наименование"),
                    DataType = CellValues.String
                },
                new Cell() { StyleIndex = styleTbBorder },
                new Cell() { StyleIndex = styleTbBorder },
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Ед. изм."),
                    DataType = CellValues.String
                },
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Кол-во"),
                    DataType = CellValues.String
                },
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Цена, руб."),
                    DataType = CellValues.String
                },
                new Cell() { StyleIndex = styleTbBorder },
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Скидка"),
                    DataType = CellValues.String
                },
                new Cell()
                {
                    StyleIndex = styleDefault,
                    CellValue = new CellValue("Сумма, руб."),
                    DataType = CellValues.String
                }, new Cell() { StyleIndex = styleTrbBorder }
                )
            { Height = heightRow });

            #endregion

            #region Заказы

            foreach (var order in model.Orders)
            {
                var orderStatusStr = string.Empty;
                switch (order.Status) {
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
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue("Заказ №"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue(order.Id.ToString()),
                            DataType = CellValues.Number
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue("Дата заказа"),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue(order.Create.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue("Итого:"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleHeadOrder,
                            CellValue = new CellValue(order.TotalSum.ToString()),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTrbBorder }
                    )
                    { Height = heightRow });

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:J{iRow++}") });
                sheetData.Append(
                    new Row(
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue("Адрес доставки"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
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
                    new Cell() { StyleIndex = styleTrbBorder }
                    )
                    { Height = heightRow });

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:F{iRow++}") });
                //mergeCells.Append(new MergeCell() { Reference = new StringValue($"H{iRow}:J{iRow++}") });
                sheetData.Append(
                    new Row(
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue("Исполнитель"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(String.Format("{0}", order.Cafe.FullName)),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue("Статус"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(String.Format("{0}", orderStatusStr)),
                            DataType = CellValues.String
                        },
                    new Cell() { StyleIndex = styleTbBorder },
                    new Cell() { StyleIndex = styleTrbBorder }
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
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(orderItem.DishName),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue("шт."),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(orderItem.DishCount.ToString()),
                            DataType = CellValues.Number
                        },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(orderItem.DishBasePrice.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.Number
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue((orderItem.Discount == 0) ? String.Empty : orderItem.Discount + "%"),
                            DataType = CellValues.String
                        },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(orderItem.TotalPrice.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTrbBorder }
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
                            StyleIndex = styleDefault,
                            CellValue = new CellValue("Доставка"),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(order.OrderInfo.DeliverySumm.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTbBorder },
                        new Cell() { StyleIndex = styleDefault },
                        new Cell()
                        {
                            StyleIndex = styleDefault,
                            CellValue = new CellValue(order.OrderInfo.DeliverySumm.ToString(CultureInfo.InvariantCulture)),
                            DataType = CellValues.String
                        },
                        new Cell() { StyleIndex = styleTrbBorder }
                    )
                    { Height = heightRow });

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:J{iRow++}") });
                sheetData.Append(new Row());

            }


            #endregion

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
                    new Fill(new PatternFill(new ForegroundColor { Rgb = new HexBinaryValue() { Value = "fff2f2f2" } })
                    { PatternType = PatternValues.Solid })
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
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyFont = true, ApplyBorder = true },
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 1, ApplyFont = true, ApplyBorder = true },
                    new CellFormat() { FontId = 1, FillId = 2, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true },
                    //3 styleDefaultTitleBold_LTRBBorder
                    new CellFormat() { FontId = 0, BorderId = 1, ApplyFont = true, ApplyBorder = true },
                    //4 style_TBBorder
                    new CellFormat() { BorderId = 2, ApplyBorder = true },
                    //5 style_TRBBorder
                    new CellFormat() { BorderId = 3, ApplyBorder = true }
                    )
            );

            workbookStylesPart.Stylesheet = stylesheet;

            #endregion

            workbook.Save();
        }
    }
}