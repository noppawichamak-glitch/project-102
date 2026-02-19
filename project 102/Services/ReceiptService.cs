using System;
using System.Collections.Generic;
using project_102.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace project_102.Services
{
    public class ReceiptService
    {
        public void GenerateReceiptPDF(string receiptNo, List<Product> cartItems, decimal totalAmount)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().AlignCenter().Text($"RECEIPT: {receiptNo}")
                        .SemiBold().FontSize(14);

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Item").Bold();
                            header.Cell().Text("Qty").Bold();
                            header.Cell().Text("Price").Bold();
                        });

                        foreach (var item in cartItems)
                        {
                            table.Cell().Text(item.Name);
                            table.Cell().Text(item.Stock.ToString());
                            table.Cell().Text($"{item.Price:N2}");
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1);
                        col.Item().AlignRight().Text($"Total: {totalAmount:N2} THB").Bold().FontSize(12);
                        col.Item().AlignCenter().Text($"Thank you!").FontSize(8);
                    });
                });
            }).GeneratePdf($"receipt_{receiptNo}.pdf");
        }
    }
}
