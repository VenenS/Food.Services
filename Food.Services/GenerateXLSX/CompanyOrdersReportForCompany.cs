using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ITWebNet.Food.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ITWebNet.FoodService.Food.DbAccessor;

// ReSharper disable PossiblyMistakenUseOfParamsMethod

namespace Food.Services.GenerateXLSX
{
    /// <summary>
    /// Генерация excel документа с заказами сотрудников для всей компании
    /// </summary>
    public class CompanyOrdersReportForCompany
    {
        public byte[] CreatePackageAsBytes(ReportData model)
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

        private void CreateParts(SpreadsheetDocument document, ReportData model)
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
                new Column() { BestFit = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true },
                new Column() { BestFit = true, CustomWidth = true }
                );

            //uint styleDefault0 = 0;
            uint styleTitleBoldCenter = 6;
            uint styleTitleCenter2 = 1;
            uint styleDefault = 2;
            uint styleDefaultCenterLtrbBorder = 3;
            uint styleDefaultBold = 4;
            uint styleDefaultBoldCenterLtrbBorder = 5;
            uint styleDefaultBoldCenterTBorder = 7;
            uint styleDefaultCenter = 8;

            var heightRow = 30;

            var mergeCells = new MergeCells();
            long iRow = 1; //Номер строки
            int count=4; //счетчик
            int counterString = 4; //Счетчик строк. ВНИМАНИЕ! Костыль! Но без него все пойдет по 1 месту
            var sheetData = new SheetData();
            var addressList = new List<string>();


            #region Шапка

            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow++}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow++}") });
            mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow++}") });

            sheetData.Append(
                new Row(new Cell()
                {
                    StyleIndex = styleTitleBoldCenter,
                    CellValue = new CellValue("Отчет"),
                    DataType = CellValues.String
                })
                { Height = 25 },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleTitleCenter2,
                        CellValue = new CellValue($"c {model.StartDate.ToShortDateString()} по {model.EndDate.ToShortDateString()}"),
                        DataType = CellValues.String
                    }
                    )
                { Height = heightRow },
                new Row(
                    new Cell()
                    {
                        StyleIndex = styleTitleCenter2,
                        CellValue = new CellValue($"на общую сумму {model.TotalSumm } рублей"),
                        DataType = CellValues.String
                    })
                { Height = heightRow }
                );

            #endregion

            #region Заказы

            foreach (var order in model.OrdersData)
            {
                
                #region элементы заказа

                foreach (var itemOrder in order.Orders)
                {
                    foreach (var orderData in itemOrder.Orders)
                    {
                        if (String.IsNullOrEmpty(orderData.Order.OrderInfo.OrderAddress))
                            orderData.Order.OrderInfo.OrderAddress = "Без адреса";
                        addressList.Add(orderData.Order.OrderInfo.OrderAddress);
                    }
                }
                var address = addressList.Distinct();
                
                if (counterString != 4)
                {
                    iRow = counterString;
                }

                mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow}") });

                sheetData.Append(
                    new Row(new Cell()
                        {
                            StyleIndex = styleDefaultBold,
                            CellValue = new CellValue($"Заказ № {order.CompanyOrder.Id}"),
                            DataType = CellValues.String
                        }, new Cell(), new Cell(), new Cell(), new Cell(), new Cell())
                        {Height = heightRow});
                iRow++;
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:C{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"D{iRow}:E{iRow}") });
                sheetData.Append(
                    new Row(
                            new Cell()
                            {
                                StyleIndex = styleDefaultBoldCenterTBorder,
                                CellValue = new CellValue("Дата заказа"),
                                DataType = CellValues.String
                            },
                            new Cell()
                            {
                                StyleIndex = styleDefaultBoldCenterTBorder,
                                CellValue = new CellValue("Кафе"),
                                DataType = CellValues.String
                            }, new Cell() {StyleIndex = styleDefaultBoldCenterTBorder},
                            new Cell()
                            {
                                StyleIndex = styleDefaultBoldCenterTBorder,
                                CellValue = new CellValue("Стоимость заказа"),
                                DataType = CellValues.String
                            },
                            new Cell() {StyleIndex = styleDefaultBoldCenterTBorder},
                            new Cell()
                            {
                                StyleIndex = styleDefaultBoldCenterTBorder,
                                CellValue = new CellValue("Статус"),
                                DataType = CellValues.String
                            }
                        )
                        {Height = heightRow});
                iRow++;
                counterString += 2;
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"B{iRow}:C{iRow}") });
                mergeCells.Append(new MergeCell() { Reference = new StringValue($"D{iRow}:E{iRow}") });

                sheetData.Append(
                        new Row(
                                new Cell()
                                {
                                    StyleIndex = styleDefaultCenter,
                                    CellValue = new CellValue(order.CompanyOrder.DeliveryDate.Value.ToShortDateString() + " " +
                                                              order.CompanyOrder.DeliveryDate.Value.ToShortTimeString()),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultCenter,
                                    CellValue = new CellValue(order.CompanyOrder.Cafe.FullName),
                                    DataType = CellValues.String
                                }, new Cell(),
                                new Cell()
                                {
                                    StyleIndex = styleDefaultCenter,
                                    CellValue = new CellValue(order.TotalPrice.ToString(CultureInfo.InvariantCulture)),
                                    DataType = CellValues.String
                                }, new Cell(),
                                new Cell()
                                {
                                    StyleIndex = styleDefaultCenter,
                                    CellValue = new CellValue(order.OrderStatusReport),
                                    DataType = CellValues.String
                                }
                            )
                        { Height = heightRow }
                    );
                iRow++;
                counterString++;
                foreach (var deliveryAddress in address)
                {
                    iRow = counterString;
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow++}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{iRow}:F{iRow}") });
                    

                    sheetData.Append(
                        new Row(new Cell()
                        {
                            StyleIndex = styleDefaultBold,
                            CellValue = new CellValue($"Адрес: {deliveryAddress}"),
                            DataType = CellValues.String
                        }, new Cell(), new Cell(), new Cell(), new Cell(), new Cell())
                        { Height = heightRow });
                    sheetData.Append(
                        new Row(
                                new Cell()
                                {
                                    StyleIndex = styleDefault,
                                    CellValue = new CellValue("Заказы сотрудников:"),
                                    DataType = CellValues.String
                                }, new Cell(), new Cell(), new Cell(), new Cell(), new Cell())
                        { Height = heightRow });
                    sheetData.Append(
                        new Row(
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("№ заказа"),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("Время поступления"),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("Имя клиента"),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("Телефон"),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("Стоимость заказа"),
                                    DataType = CellValues.String
                                },
                                new Cell()
                                {
                                    StyleIndex = styleDefaultBoldCenterLtrbBorder,
                                    CellValue = new CellValue("Статус"),
                                    DataType = CellValues.String
                                }
                            )
                        { Height = heightRow }
                        
                    );
                    
                    counterString += 3;
                    
                    foreach (var orderItems in order.Orders)
                    {
                        foreach (var orderItem in orderItems.Orders)
                        {
                            if (orderItem.Order.OrderInfo.OrderAddress == deliveryAddress)
                            {
                                sheetData.Append(
                                    new Row(
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(orderItem.Order.Id.ToString()),
                                                DataType = CellValues.Number
                                            },
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(
                                                    orderItem.Order.Create.ToShortDateString() + " " +
                                                    orderItem.Order.Create.ToShortTimeString()),
                                                DataType = CellValues.String
                                            },
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(
                                                    orderItem.User.UserFullName ?? String.Empty),
                                                DataType = CellValues.String
                                            },
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(orderItem.Order.PhoneNumber),
                                                DataType = CellValues.String
                                            },
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(
                                                    orderItem.TotalPrice.ToString(CultureInfo.InvariantCulture)),
                                                DataType = CellValues.String
                                            },
                                            new Cell()
                                            {
                                                StyleIndex = styleDefaultCenterLtrbBorder,
                                                CellValue = new CellValue(orderItem.OrderStatusReport),
                                                DataType = CellValues.String
                                            }
                                        )
                                    { Height = heightRow });
                                counterString++;
                            }
                        }
                    }
                    

                }

                addressList.Clear();

                #endregion

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
                    new Font() { FontSize = new FontSize() { Val = 15 }, FontName = new FontName() { Val = "Times New Roman" }, Bold = new Bold() },
                    new Font() { FontSize = new FontSize() { Val = 10 }, FontName = new FontName() { Val = "Times New Roman" } },
                    new Font() { FontSize = new FontSize() { Val = 10 }, FontName = new FontName() { Val = "Times New Roman" }, Bold = new Bold() }),
                new Fills(
                    new Fill(new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(new PatternFill(new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "fff2f2f2" } }) { PatternType = PatternValues.Solid })),
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
                        new TopBorder(new Color() { Rgb = new HexBinaryValue() { Value = "ff000000" } }) { Style = BorderStyleValues.Thin }
                        )),
                new CellFormats(
                    //0
                    new CellFormat() { FontId = 0, ApplyFont = true },
                    //styleTitleCenter2
                    new CellFormat() { FontId = 1, ApplyFont = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true },
                    //styleDefault
                    new CellFormat() { FontId = 1, ApplyFont = true },
                    //styleDefaultCenter_LTRBBorder
                    new CellFormat() { FontId = 1, ApplyFont = true, BorderId = 1, ApplyBorder = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true },
                    //styleDefaultBold
                    new CellFormat() { FontId = 2, ApplyFont = true },
                    //styleDefaultBoldCenter_LTRBBorder  styleDefaultBoldCenter_TBorder
                    new CellFormat() { FontId = 2, ApplyFont = true, BorderId = 1, ApplyBorder = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true },
                    //styleTitleBoldCenter 
                    new CellFormat() { FontId = 0, ApplyFont = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true },
                    //styleDefaultBoldCenter_TBorder
                    new CellFormat() { FontId = 2, ApplyFont = true, BorderId = 2, ApplyBorder = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true },
                    //styleDefaultCenter
                    new CellFormat() { FontId = 1, ApplyFont = true, Alignment = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }, ApplyAlignment = true }
                    )
            );

            workbookStylesPart.Stylesheet = stylesheet;

            #endregion

            workbook.Save();
        }
    }
}