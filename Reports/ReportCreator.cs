using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IDZ_SERVER.DataBase.Entities;
using word = Microsoft.Office.Interop.Word;
using excel = Microsoft.Office.Interop.Excel;
using System.Data.Entity;

namespace IDZ_SERVER.Reports
{
    public class ReportCreator
    {
        word.Document wDoc = new word.Document();
        excel.Application excelApp = new excel.Application();
        word.Application wordApp = new word.Application();
        public ReportCreator()
        {

        }
        private void Replace(string find, string replace)
        {
            word.Range range = wDoc.StoryRanges[word.WdStoryType.wdMainTextStory];
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: find, ReplaceWith: replace);
        }
        private void Replace(string find, excel.ChartObject replace)
        {
            word.Range range = wDoc.StoryRanges[word.WdStoryType.wdMainTextStory];
            range.Find.ClearFormatting();
            range.Find.Execute(FindText: find, ReplaceWith: replace);
        }
        public void GeneratWordReport()
        {
            excelCreator();
            List<ARMOR> armorList;
            int entryCount;
            using (DataBaseContext db = new DataBaseContext())
            {
                armorList = db.ARMOR.Take(10).ToList();
            }
            entryCount = armorList.Count();
            string replaceString = "";
            foreach (ARMOR armor in armorList)
            {
                replaceString += armor.NAME + "^p";
            }
            object file = @"C:\Users\пр\Desktop\лабы\Архитектура ИС\IDZ\IDZ_SERVER\bin\Debug\template.docx";
            wDoc = wordApp.Documents.Add(ref file, false, word.WdNewDocumentType.wdNewBlankDocument, true);
            Replace("{N}", entryCount.ToString());
            Replace("{M}", replaceString);

            excel.Workbook excelbook = excelApp.Workbooks.Open(@"C:\Users\пр\Desktop\лабы\Архитектура ИС\IDZ\IDZ_SERVER\bin\Debug\outExcel.xlsx");
            excel.ChartObjects chartObjects = excelbook.Sheets["Лист1"].ChartObjects();
            excel.ChartObject chartObject1 = chartObjects.Item(1);
            excel.ChartObject chartObject2 = chartObjects.Item(2);
            word.Range range = wDoc.Content.Paragraphs.Last.Range;
            chartObject1.Copy();
            range.Paste();
            wDoc.Content.Paragraphs.Add();
            range = wDoc.Content.Paragraphs.Last.Range;
            chartObject2.Copy();
            range.Paste();

            try
            {
                wDoc.SaveAs(@"C:\Users\пр\Desktop\лабы\Архитектура ИС\IDZ\IDZ_SERVER\bin\Debug\report");
                wDoc.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            excelApp.Quit();
            wordApp.Quit(word.WdSaveOptions.wdPromptToSaveChanges);
        }
        private void excelCreator()
        {
            List<ARMOR_DEFENCE_VIEW> heavyArmorList;
            List<ARMOR_DEFENCE_VIEW> lightArmorList;
            using (DataBaseContext db = new DataBaseContext())
            {
                int heavyId = db.ARMOR_TYPE.Where(p => p.NAME == "Тяжёлая броня").First().ID;
                int lightId = db.ARMOR_TYPE.Where(p => p.NAME == "Лёгкая броня").First().ID;

                heavyArmorList = db.ARMOR_DEFENCE_VIEW.Where(p => p.ARMOR_TYPE_ID == heavyId).
                    OrderByDescending(p => p.DEFENCE).Take(10).ToList();
                lightArmorList = db.ARMOR_DEFENCE_VIEW.Where(p => p.ARMOR_TYPE_ID == lightId).
                    OrderByDescending(p => p.DEFENCE).Take(10).ToList();
            }
            excel.Workbook book = excelApp.Workbooks.Add();
            excel.Worksheet ws = book.ActiveSheet;

            for (int i = 0; i < heavyArmorList.Count(); i++)
            {
                ws.Cells[1, i + 1].Value = heavyArmorList[i].DEFENCE;
                ws.Cells[2, i + 1].Value = heavyArmorList[i].NAME;

                ws.Cells[3, i + 1].Value = lightArmorList[i].DEFENCE;
                ws.Cells[4, i + 1].Value = lightArmorList[i].NAME;
            }
            excel.Chart heavyChart = ((excel.ChartObjects)ws.ChartObjects(Type.Missing)).Add(50, 100, 450, 300).Chart;
            excel.Chart lightChart = ((excel.ChartObjects)ws.ChartObjects(Type.Missing)).Add(50, 500, 450, 300).Chart;

            heavyChart.ChartType = excel.XlChartType.xlColumnClustered;
            lightChart.ChartType = excel.XlChartType.xlColumnClustered;

            heavyChart.SetSourceData(ws.Range["A1:J1"]);
            lightChart.SetSourceData(ws.Range["A3:J3"]);

            excel.Series heavySeries = heavyChart.SeriesCollection(1);
            excel.Series lightSeries = lightChart.SeriesCollection(1);

            heavySeries.XValues = heavyArmorList.Select(p => p.NAME).ToArray();
            lightSeries.XValues = lightArmorList.Select(p => p.NAME).ToArray();

            heavyChart.HasTitle = true;
            lightChart.HasTitle = true;

            heavyChart.ChartTitle.Text = "Топ 10 тяжёлой брони";
            lightChart.ChartTitle.Text = "Топ 10 легкой брони";

            try
            {
                ws.SaveAs(@"C:\Users\пр\Desktop\лабы\Архитектура ИС\IDZ\IDZ_SERVER\bin\Debug\outExcel");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            book.Close();
            excelApp.Quit();
        }
    }
}
