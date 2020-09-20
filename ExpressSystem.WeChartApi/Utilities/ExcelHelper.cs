using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ExpressSystem.WeChartApi.Utilities
{
    public class ExcelHelper
    {
        /// <summary>
        /// 导出的字段名称和描述
        /// </summary>
        public Dictionary<string, string> Fields { get; set; }

        private HSSFWorkbook _workbook = null;
        private ISheet _sheet = null;
        /// <summary>
        /// 创建实例，验证导出文件名
        /// </summary>
        /// <param name="FullName"></param>
        /// <param name="Fields"></param>
        public ExcelHelper(Dictionary<string, string> Fields)
        {
            this.Fields = Fields;
            _workbook = new HSSFWorkbook();
            _sheet = _workbook.CreateSheet("Sheet1");
        }

        /// <summary>
        /// 执行导出操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public byte[] Export<T>(List<T> list)
        {
            //写入表格头
            WriteHead();
            //写入数据
            ICellStyle cellStyle = _workbook.CreateCellStyle();
            cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("@");//为避免日期格式被Excel自动替换，所以设定 format 为 『@』 表示一率当成text來看
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.Alignment = HorizontalAlignment.Center;

            IFont cellFont = _workbook.CreateFont();
            cellFont.Boldweight = (short)FontBoldWeight.Normal;
            cellStyle.SetFont(cellFont);

            //建立行内容，从1开始
            int rowInex = 1;

            foreach (var rowItem in list)
            {
                //创建行
                IRow row = _sheet.CreateRow(rowInex);
                row.HeightInPoints = 25;

                int cellIndex = 0;
                foreach (var cellItem in this.Fields)
                {
                    //创建单元格
                    ICell cell = row.CreateCell(cellIndex);
                    //反射获取属性的值
                    PropertyInfo info = rowItem.GetType().GetProperty(cellItem.Key);
                    if (info == null)
                    {
                        cell.SetCellValue($"'{cellItem.Key}'属性不存在");
                    }
                    else
                    {
                        object value = info.GetValue(rowItem);
                        if (value != null && value.ToString()!= "0001/1/1 0:00:00")
                            cell.SetCellValue(value.ToString());
                    }
                    cell.CellStyle = cellStyle;
                    cellIndex++;
                }
                //进入下一次循环
                rowInex++;
            }

            byte[] resultByte = null;

            //保存
            using (MemoryStream ms = new MemoryStream())
            {
                _workbook.Write(ms);
                resultByte = ms.ToArray();
                ms.Dispose();
            }

            return resultByte;
        }
        /// <summary>
        /// 写入表头
        /// </summary>
        private void WriteHead()
        {
            //设置表头样式
            ICellStyle headStyle = _workbook.CreateCellStyle();
            headStyle.BorderBottom = BorderStyle.Thin;
            headStyle.BorderLeft = BorderStyle.Thin;
            headStyle.BorderRight = BorderStyle.Thin;
            headStyle.BorderRight = BorderStyle.Thin;
            headStyle.Alignment = HorizontalAlignment.Center;
            headStyle.FillForegroundColor = HSSFColor.Blue.Index;
            headStyle.VerticalAlignment = VerticalAlignment.Center;

            IFont headFont = _workbook.CreateFont();
            headFont.Boldweight = (short)FontBoldWeight.Bold;
            headStyle.SetFont(headFont);

            IRow row = _sheet.CreateRow(0);
            row.HeightInPoints = 30;

            int index = 0;
            foreach (var item in this.Fields)
            {
                ICell cell = row.CreateCell(index);
                cell.SetCellValue(item.Value);
                cell.CellStyle = headStyle;
                index++;
            }
        }

        /// <summary>
        /// 读取Excel指定Sheet数据
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="sheetName">Sheet名</param>
        /// <returns></returns>
        public static DataTable ReadExcelToDataTable(Stream fileStream, string sheetName)
        {
            //获取文件信息
            IWorkbook workbook = WorkbookFactory.Create(fileStream);
            //获取sheet信息
            if (!string.IsNullOrEmpty(sheetName))
            {
                ISheet sheet = workbook.GetSheet(sheetName);
                if (sheet == null)
                {
                    throw new Exception($"未找到sheet:{sheetName}");
                }
                return ReadExcelFunc(workbook, sheet);
            }
            return null;
        }

        /// <summary>
        /// 读取Excel信息
        /// </summary>
        /// <param name="workbook">工作区</param>
        /// <param name="sheet">sheet</param>
        /// <returns></returns>
        private static DataTable ReadExcelFunc(IWorkbook workbook, ISheet sheet)
        {
            DataTable dt = new DataTable();
            //获取列信息
            IRow cells = sheet.GetRow(sheet.FirstRowNum);
            int cellsCount = cells.PhysicalNumberOfCells;
            int emptyCount = 0;
            int cellIndex = sheet.FirstRowNum;
            List<string> listColumns = new List<string>();
            bool isFindColumn = false;
            while (!isFindColumn)
            {
                emptyCount = 0;
                listColumns.Clear();
                for (int i = 0; i < cellsCount; i++)
                {
                    if (string.IsNullOrEmpty(cells.GetCell(i).StringCellValue))
                    {
                        emptyCount++;
                    }
                    listColumns.Add(cells.GetCell(i).StringCellValue);
                }
                //这里根据逻辑需要，空列超过多少判断
                if (emptyCount == 0)
                {
                    isFindColumn = true;
                }
                cellIndex++;
                cells = sheet.GetRow(cellIndex);
            }

            foreach (string columnName in listColumns)
            {
                //if (dt.Columns.Contains(columnName))
                //{
                //    //如果允许有重复列名，自己做处理
                //    continue;
                //}
                dt.Columns.Add(columnName, typeof(string));
            }
            //开始获取数据
            int rowsCount = sheet.PhysicalNumberOfRows;
            //cellIndex += 1;
            DataRow dr = null;
            for (int i = cellIndex; i < rowsCount; i++)
            {
                cells = sheet.GetRow(i);
                dr = dt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (cells.GetCell(j) == null)
                    {
                        dr[j] = "";
                        continue;
                    }
                    //这里可以判断数据类型
                    switch (cells.GetCell(j).CellType)
                    {
                        case CellType.String:
                            dr[j] = cells.GetCell(j).StringCellValue;
                            break;
                        case CellType.Numeric:
                            dr[j] = cells.GetCell(j).NumericCellValue.ToString();
                            break;
                        case CellType.Unknown:
                            dr[j] = cells.GetCell(j).StringCellValue;
                            break;
                    }
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

    }
}
