﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;

namespace ScaleUP.Services.CommunicationAPI.Helper
{
    public static class ExcelGeneratorHelper
    {
        public static bool DataTable_To_Excel(DataTable pDatos, string sheetname, string pFilePath, bool? isLocked = false)
        {
            bool result = false;
            try
            {
                if (pDatos != null && pDatos.Rows.Count > 0)
                {
                    IWorkbook workbook = null;
                    ISheet worksheet = null;

                    using (FileStream stream = new FileStream(pFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        string Ext = System.IO.Path.GetExtension(pFilePath); //<-Extension del archivo
                        switch (Ext.ToLower())
                        {
                            case ".xls":
                                HSSFWorkbook workbookH = new HSSFWorkbook();
                                NPOI.HPSF.DocumentSummaryInformation dsi = NPOI.HPSF.PropertySetFactory.CreateDocumentSummaryInformation();
                                dsi.Company = "ShopKirana"; dsi.Manager = "IT Department";
                                workbookH.DocumentSummaryInformation = dsi;
                                workbook = workbookH;
                                break;

                            case ".xlsx":
                                workbook = new XSSFWorkbook();
                                var xssfWorkbook = workbook as NPOI.XSSF.UserModel.XSSFWorkbook;
                                var xmlProps = xssfWorkbook.GetProperties();
                                var coreProps = xmlProps.CoreProperties;
                                coreProps.Creator = "shopkirana";
                                workbook = xssfWorkbook;
                                break;
                        }

                        worksheet = workbook.CreateSheet(sheetname); //<-Usa el nombre de la tabla como nombre de la Hoja

                        //CREAR EN LA PRIMERA FILA LOS TITULOS DE LAS COLUMNAS
                        int iRow = 0;
                        if (pDatos.Columns.Count > 0)
                        {
                            int iCol = 0;
                            IRow fila = worksheet.CreateRow(iRow);
                            foreach (DataColumn columna in pDatos.Columns)
                            {
                                ICell cell = fila.CreateCell(iCol, CellType.String);
                                cell.SetCellValue(columna.ColumnName);
                                iCol++;
                            }
                            iRow++;
                        }

                        //FORMATOS PARA CIERTOS TIPOS DE DATOS
                        ICellStyle _doubleCellStyle = workbook.CreateCellStyle();
                        _doubleCellStyle.IsLocked = isLocked ?? false;
                        _doubleCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.00");

                        ICellStyle _intCellStyle = workbook.CreateCellStyle();
                        _intCellStyle.IsLocked = isLocked ?? false;
                        _intCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

                        ICellStyle _boolCellStyle = workbook.CreateCellStyle();
                        _boolCellStyle.IsLocked = isLocked ?? false;
                        _boolCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("BOOLEAN");

                        ICellStyle _dateCellStyle = workbook.CreateCellStyle();
                        _dateCellStyle.IsLocked = isLocked ?? false;
                        _dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy");

                        ICellStyle _dateTimeCellStyle = workbook.CreateCellStyle();
                        _dateTimeCellStyle.IsLocked = isLocked ?? false;
                        _dateTimeCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd-MM-yyyy HH:mm:ss");

                        //AHORA CREAR UNA FILA POR CADA REGISTRO DE LA TABLA
                        foreach (DataRow row in pDatos.Rows)
                        {
                            IRow fila = worksheet.CreateRow(iRow);
                            int iCol = 0;
                            foreach (DataColumn column in pDatos.Columns)
                            {
                                ICell cell = null; //<-Representa la celda actual                               
                                object cellValue = row[iCol]; //<- El valor actual de la celda

                                switch (column.DataType.ToString())
                                {
                                    case "System.Boolean":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Boolean);

                                            if (Convert.ToBoolean(cellValue)) { cell.SetCellFormula("TRUE()"); }
                                            else { cell.SetCellFormula("FALSE()"); }

                                            cell.CellStyle = _boolCellStyle;
                                        }
                                        break;

                                    case "System.String":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.String);
                                            cell.SetCellValue(Convert.ToString(cellValue));

                                            var cellStyleForLocked = workbook.CreateCellStyle(); //create the style in the beginning itself
                                            cellStyleForLocked.IsLocked = isLocked ?? false;
                                            cell.CellStyle = cellStyleForLocked;
                                        }
                                        break;

                                    case "System.Int32":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToInt32(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;
                                    case "System.Int64":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToInt64(cellValue));
                                            cell.CellStyle = _intCellStyle;
                                        }
                                        break;
                                    case "System.Decimal":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Math.Round(Convert.ToDouble(cellValue), 2));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;
                                    case "System.Double":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDouble(cellValue));
                                            cell.CellStyle = _doubleCellStyle;
                                        }
                                        break;

                                    case "System.DateTime":
                                        if (cellValue != DBNull.Value)
                                        {
                                            cell = fila.CreateCell(iCol, CellType.Numeric);
                                            cell.SetCellValue(Convert.ToDateTime(cellValue));

                                            //Si No tiene valor de Hora, usar formato dd-MM-yyyy
                                            DateTime cDate = Convert.ToDateTime(cellValue);
                                            if (cDate != null && cDate.Hour > 0) { cell.CellStyle = _dateTimeCellStyle; }
                                            else { cell.CellStyle = _dateCellStyle; }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                iCol++;
                            }
                            iRow++;
                        }

                        workbook.Write(stream);
                        stream.Close();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            return result;
        }

    }
}
