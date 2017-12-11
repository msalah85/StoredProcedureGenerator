using System;
using System.Data;
using SP_Gen.Properties;

namespace SP_Gen.Classes
{
    class SQL_Generator
    {
        #region "Stored Procedure Generation Methods"
        public static string CreateDeleteRowSP(string spName, string TableName, DataRow[] Columns)
        {
            string SQL = string.Empty;

            SQL = String.Format(Resources.DropProcedure, TableName + "_Delete");
            SQL += "\n\n";

            SQL += "-- ==========================================================================================";
            SQL += "\n-- Entity Name:\t" + TableName + "_Delete";
            string AuthorName = Session.LoadFromSession("AuthorName").ToString();
            if (AuthorName != string.Empty)
            {
                SQL += "\n-- Author:\t" + AuthorName;
            }
            SQL += "\n-- Create date:\t" + DateTime.Now;
            SQL += "\n-- Description:\tThis stored procedure is intended for deleting a specific row from " +
                   TableName + " table";
            SQL +=
                "\n-- ==========================================================================================\n";

            #region "Header Definition"

            SQL += "Create Procedure " + TableName + "_Delete" + "\n";

            #endregion

            #region "Parameter Definition"
            bool firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIdentity"].ToString()) != 0 || int.Parse(row["IsIndex"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\t";
                    }
                    else
                    {
                        SQL += ",\n\t";
                    }

                    SQL += "@" + row["COLUMN_NAME"].ToString() + " ";

                    if (row["DATA_TYPE"].ToString().ToLower().Contains("char"))
                    {
                        string Length = (row["CHARACTER_MAXIMUM_LENGTH"].ToString().Equals("-1") ? "MAX" : row["CHARACTER_MAXIMUM_LENGTH"].ToString());
                        SQL += row["DATA_TYPE"].ToString() + "(" + Length + ")";
                    }
                    else if (row["DATA_TYPE"].ToString().ToLower().Contains("numeric"))
                    {
                        SQL += string.Format("numeric({0:G},{1:G})", row["NUMERIC_PRECISION"].ToString(), row["NUMERIC_SCALE"].ToString());
                    }
                    else
                    {
                        SQL += row["DATA_TYPE"].ToString();
                    }
                }
            }

            #endregion

            #region "Delete Command / Header Definition"
            SQL += "\nAs\nBEGIN\n";
            SQL += "\tDELETE " + TableName + " ";
            #endregion

            #region "Primary Key Column Detection"
            string pkColumn = string.Empty;

            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0)
                {
                    pkColumn = row["COLUMN_NAME"].ToString();
                    break;
                }
            }
            #endregion

            #region "Delete Command / Where Clause Definition"
            firstParam = true;
            SQL += " WHERE ";
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIdentity"].ToString()) != 0 || int.Parse(row["IsIndex"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += " ";
                    }
                    else
                    {
                        SQL += "\n\t\tAND ";
                    }
                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = " + "@" + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n\tRETURN @@ROWCOUNT;";

            #endregion

            SQL += "\nEND\n\nGO\n";

            return SQL;
        }

        public static string CreateSelectProperties(string spName, string TableName, DataRow[] Columns)
        {
            string SQL = string.Empty;

            SQL = String.Format(Resources.DropProcedure, TableName + "_Properties");
            SQL += "\n\n";

            SQL += "-- ==========================================================================================";
            SQL += "\n-- Entity Name:\t" + TableName + "_Properties";
            string AuthorName = Session.LoadFromSession("AuthorName").ToString();
            if (AuthorName != string.Empty)
            {
                SQL += "\n-- Author:\t" + AuthorName;
            }
            SQL += "\n-- Create date:\t" + DateTime.Now.ToString();
            SQL += "\n-- Description:\tThis stored procedure is intended for selecting all rows from " + TableName +
                   " table";
            SQL +=
                "\n-- ==========================================================================================\n";

            #region "Header Definition"
            SQL += "Create Procedure " + TableName + "_Properties" + "\n";
            #endregion

            SQL += "As\nBegin\n";
            SQL += "\tSELECT ORDINAL_POSITION, COLUMN_NAME,DATA_TYPE,IS_NULLABLE,COLUMN_DEFAULT,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,100 AS tbl_name";
            SQL += " FROM INFORMATION_SCHEMA.Columns";
            SQL += " WHERE Table_Name = '" + TableName + "'; " + "\n";
            SQL += "\tSELECT column_name,101 AS tbl_name FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE";
            SQL += " WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1";
            SQL += " AND table_name = '" + TableName + "' ORDER BY ORDINAL_POSITION ";

            SQL += "\nEND\n\nGO\n";

            return SQL;
        }

        public static string CreateSelectListSP(string spName, string TableName, DataRow[] Columns)
        {
            string SQL = string.Empty;

            SQL = String.Format(Resources.DropProcedure, TableName + "_List");
            SQL += "\n\n";

            SQL += "-- ==========================================================================================";
            SQL += "\n-- Entity Name:\t" + TableName + "_List";
            string AuthorName = Session.LoadFromSession("AuthorName").ToString();
            if (AuthorName != string.Empty)
            {
                SQL += "\n-- Author:\t" + AuthorName;
            }
            SQL += "\n-- Create date:\t" + DateTime.Now.ToString();
            SQL += "\n-- Description:\tThis stored procedure is intended for selecting a specific row from " +
                   TableName + " table";
            SQL +=
                "\n-- ==========================================================================================\n";

            #region "Header Definition"

            SQL += "Create Procedure " + TableName + "_List" + "\n";

            #endregion



            #region "Select Command / Header Definition"
            SQL += "\t@DisplayStart int=0,\n";
            SQL += "\t@DisplayLength int=10,\n";
            SQL += "\t@SearchParam nvarchar(50) = NULL,\n";
            SQL += "\t@SortColumn nvarchar(3) = '0',\n";
            SQL += "\t@SortDirection nvarchar(4) = 'desc'";
            SQL += "\nAs\nBegin\n";
            SQL += "\tSELECT * FROM (SELECT ";

            #endregion

            #region "Primary Key Column Detection"

            string pkColumn = string.Empty;

            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0)
                {
                    pkColumn = row["COLUMN_NAME"].ToString();
                    break;
                }
            }

            #endregion

            #region "Select Command / Columns List Definition"

            bool firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (firstParam == true)
                {
                    firstParam = false;
                }
                else
                {
                    SQL += ",";
                }
                SQL += QualifyFieldName(row["COLUMN_NAME"].ToString());
            }

            SQL += "\n\t,(ROW_NUMBER() OVER(ORDER BY\n";
            firstParam = true;
            int i = 0;
            foreach (DataRow row in Columns)
            {
                if (firstParam == true)
                {
                    firstParam = false;
                }
                else
                {
                    SQL += ",\n";
                }
                SQL += "\t\t CASE WHEN @SortColumn = '" + i + "' AND @SortDirection = 'asc' THEN " + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " END ASC,\n";
                SQL += "\t\t CASE WHEN @SortColumn = '" + i + "' AND @SortDirection = 'desc' THEN " + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " END desc";
                i++;
            }

            #endregion
            SQL += ")) AS RowNo\n\tFROM " + TableName + " Where @SearchParam IS NULL \n";

            foreach (DataRow row in Columns)
            {
                SQL += "\t\t OR " + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " LIKE '%'+ @SearchParam + '%'\n";
            }
            SQL += "\t) " + TableName + " Where RowNo > @DisplayStart AND RowNo <= (@DisplayStart + @DisplayLength)\n";
            SQL += "\n\tSELECT Count(*) AS TableCount FROM " + TableName + " Where @SearchParam IS NULL\n";

            foreach (DataRow row in Columns)
            {
                SQL += "\t\t OR " + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " LIKE '%'+ @SearchParam + '%'\n";
            }

            SQL += "END\n\nGO\n";

            return SQL;
        }

        public static string CreateUpdateSP(string spName, string TableName, DataRow[] Columns)
        {
            string SQL = string.Empty;

            SQL = String.Format(Resources.DropProcedure, TableName + "_Save");
            SQL += "\n\n";

            SQL += "-- ==========================================================================================";
            SQL += "\n-- Entity Name:\t" + TableName + "_Save";
            string AuthorName = Session.LoadFromSession("AuthorName").ToString();
            if (AuthorName != string.Empty)
            {
                SQL += "\n-- Author:\t" + AuthorName;
            }
            SQL += "\n-- Create date:\t" + DateTime.Now.ToString();
            SQL += "\n-- Description:\tThis stored procedure is intended for updating " + TableName + " table";
            SQL +=
                "\n-- ==========================================================================================\n";

            #region "Header Definition"

            SQL += "Create Procedure " + TableName + "_Save" + "\n";

            #endregion

            #region "Parameter Definition"

            bool firstParam = true;

            foreach (DataRow row in Columns)
            {
                if (firstParam == true)
                {
                    firstParam = false;
                    SQL += "\t";
                }
                else
                {
                    SQL += ",\n\t";
                }

                SQL += "@" + row["COLUMN_NAME"].ToString() + " ";

                if (row["DATA_TYPE"].ToString().ToLower().Contains("char"))
                {
                    string Length = (row["CHARACTER_MAXIMUM_LENGTH"].ToString().Equals("-1") ? "MAX" : row["CHARACTER_MAXIMUM_LENGTH"].ToString());
                    SQL += row["DATA_TYPE"].ToString() + "(" + Length + ")";
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("numeric"))
                {
                    SQL += string.Format("numeric({0:G},{1:G})", row["NUMERIC_PRECISION"].ToString(), row["NUMERIC_SCALE"].ToString());
                }
                else
                {
                    SQL += row["DATA_TYPE"].ToString();
                }
            }
            #endregion

            #region "Update Command / Header Definition"
            SQL += "\nAs\nBegin";
            SQL += "\nDECLARE @RefID int;\n";
            SQL += "IF EXISTS (SELECT NULL FROM " + TableName;
            SQL += " WHERE ";


            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += " ";
                    }
                    else
                    {
                        SQL += " and ";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = @" + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += ")";
            SQL += "\nBegin\n";


            SQL += "\tUpdate " + TableName + "\n\tSet\n\t\t";

            #endregion

            #region "Primary Key Column Detection"

            string pkColumn = string.Empty;

            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0)
                {
                    pkColumn = row["COLUMN_NAME"].ToString();
                    break;
                }
            }

            #endregion

            #region "Update Command / Setting Values Definition"

            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 && int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        SQL += ",\n\t\t";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = @" + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n\tWhere\t\t";

            #endregion

            #region "Update Command / Where Clause Definition"

            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tAND ";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = @" + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += ";";

            #endregion


            SQL += "\n\tSELECT @RefID = 1;";
            SQL += "\nEnd";
            SQL += "\nELSE";
            SQL += "\nBegin\n";
            SQL += "\tInsert Into " + TableName + "\n\t\t(";


            #region "Insert Command / Target Columns Definition"

            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        SQL += ",";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString());
                }
            }
            SQL += ")\n\tValues\n\t\t(";

            #endregion

            #region "Insert Command / Supplying Values Definition"

            firstParam = true;

            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        SQL += ",";
                    }
                    SQL += "@" + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += ");\n";

            #endregion

            SQL += "\tSELECT @RefID = @@IDENTITY;";
            SQL += "\nEnd";
            SQL += "\n\tRETURN @RefID;";
            SQL += "\nEnd\n\nGO\n";

            return SQL;
        }
        public static string CreateSaveXmlSP(string TableNameFooter, string TableName, DataRow[] Columns, DataRow[] ColumnsFooter)
        {
            string SQL = string.Empty;

            SQL = String.Format(Resources.DropProcedure, TableName + "_Save");
            SQL += "\n\n";

            SQL += "-- ==========================================================================================";
            SQL += "\n-- Entity Name:\t" + TableName + "_Save";
            string AuthorName = Session.LoadFromSession("AuthorName").ToString();
            if (AuthorName != string.Empty)
            {
                SQL += "\n-- Author:\t" + AuthorName;
            }
            SQL += "\n-- Create date:\t" + DateTime.Now.ToString();
            SQL += "\n-- Description:\tThis stored procedure is intended for updating " + TableName + " table";
            SQL +=
                "\n-- ==========================================================================================\n";

            #region "Header Definition"

            SQL += "Create Procedure " + TableName + "_Save" + "";

            #endregion



            #region "Update Command / Header Definition"
            SQL += "\n\t@doc xml";
            SQL += "\nAs\nBegin\n";
            SQL += "\rBEGIN TRAN SaveMasterDetails\n";
            SQL += "\rIF EXISTS (SELECT NULL FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[#tbl]') AND OBJECTPROPERTY(id, N'IsTable') = 1) DROP Table [dbo].[#tbl] \n";
            SQL += "\rIF EXISTS (SELECT NULL FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[#tbl2]') AND OBJECTPROPERTY(id, N'IsTable') = 1) DROP Table [dbo].[#tbl2] \n";
            SQL += "SELECT \n";
            #region "Parameter Definition"

            bool firstParam = true;

            foreach (DataRow row in Columns)
            {
                if (firstParam == true)
                {
                    firstParam = false;
                    SQL += "\t";
                }
                else
                {
                    SQL += ",\n\t";
                }

                SQL += "" + row["COLUMN_NAME"].ToString() + "=XTbl.XCol.value('@" + row["COLUMN_NAME"].ToString() + "[1]','";

                if (row["DATA_TYPE"].ToString().ToLower().Contains("char"))
                {
                    string Length = (row["CHARACTER_MAXIMUM_LENGTH"].ToString().Equals("-1")
                                         ? "MAX"
                                         : row["CHARACTER_MAXIMUM_LENGTH"].ToString());
                    SQL += row["DATA_TYPE"].ToString() + "(" + Length + ")";
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("numeric"))
                {
                    SQL += string.Format("numeric({0:G},{1:G})",
                        row["NUMERIC_PRECISION"].ToString(),
                        row["NUMERIC_SCALE"].ToString());
                }
                else
                {
                    SQL += row["DATA_TYPE"].ToString();
                }
                SQL += "') ";
            }
            #endregion

            SQL += "\n INTO #tbl FROM  @doc.nodes('//Master') AS XTbl(XCol) ";


            SQL += "\n Update " + TableName + "\n\tSet\n\t\t";

            #endregion

            #region "Primary Key Column Detection"

            string pkColumn = string.Empty;

            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    pkColumn = row["COLUMN_NAME"].ToString();
                    break;
                }
            }

            #endregion

            #region "Update Command / Setting Values Definition"

            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 && int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        SQL += ",\n\t\t";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = #tbl." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n FROM #tbl INNER JOIN " + TableName + " ON\t\t";

            #endregion

            #region "Update Command / Where Clause Definition"

            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += TableName + "." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = #tbl." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n";

            #endregion


            SQL += " INSERT INTO " + TableName + "(";
            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 || int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\t, ";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString());
                }
            }
            SQL += " )\n SELECT\n";
            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 || int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\t, ";
                    }

                    SQL += "temp." + QualifyFieldName(row["COLUMN_NAME"].ToString());
                }
            }

            SQL += "\nFROM  #tbl temp LEFT OUTER JOIN DBO." + TableName + " AS c ON";
            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += "c." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = temp." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n where (";
            firstParam = true;
            foreach (DataRow row in Columns)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += "c." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + "IS NULL ";
                }
            }
            SQL += "\n\t\t\t\t)\n";
            SQL += "SELECT \n";
            #region "Parameter Definition"

            firstParam = true;

            foreach (DataRow row in ColumnsFooter)
            {
                if (firstParam == true)
                {
                    firstParam = false;
                    SQL += "\t";
                }
                else
                {
                    SQL += ",\n\t";
                }

                SQL += "" + row["COLUMN_NAME"].ToString() + "=XTbl.XCol.value('@" + row["COLUMN_NAME"].ToString() + "[1]','";

                if (row["DATA_TYPE"].ToString().ToLower().Contains("char"))
                {
                    string Length = (row["CHARACTER_MAXIMUM_LENGTH"].ToString().Equals("-1")
                                         ? "MAX"
                                         : row["CHARACTER_MAXIMUM_LENGTH"].ToString());
                    SQL += row["DATA_TYPE"].ToString() + "(" + Length + ")";
                }
                else if (row["DATA_TYPE"].ToString().ToLower().Contains("numeric"))
                {
                    SQL += string.Format("numeric({0:G},{1:G})",
                        row["NUMERIC_PRECISION"].ToString(),
                        row["NUMERIC_SCALE"].ToString());
                }
                else
                {
                    SQL += row["DATA_TYPE"].ToString();
                }
                SQL += "') ";
            }
            #endregion

            SQL += "\n INTO #tbl2 FROM  @doc.nodes('//Details') AS XTbl(XCol) \n\n";
            SQL += "\nDELETE FROM " + TableNameFooter + " FROM #tbl2 RIGHT OUTER JOIN " + TableNameFooter + " ON ";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += TableNameFooter + "." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = #tbl2." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n where (";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += "#tbl2." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + "IS NULL ";
                }
            }
            SQL += "\n\t\t\t\t)\n";


            SQL += "Update " + TableNameFooter + "\n\tSet\n\t\t";


            #region "Primary Key Column Detection"

            pkColumn = string.Empty;

            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    pkColumn = row["COLUMN_NAME"].ToString();
                    break;
                }
            }

            #endregion

            #region "Update Command / Setting Values Definition"

            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 && int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        SQL += ",\n\t\t";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = #tbl2." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n FROM #tbl2 INNER JOIN " + TableNameFooter + " ON\t\t";

            #endregion

            #region "Update Command / Where Clause Definition"

            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += TableNameFooter + "." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = #tbl2." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n";

            #endregion
            SQL += " INSERT INTO " + TableNameFooter + "(";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 || int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\t, ";
                    }

                    SQL += QualifyFieldName(row["COLUMN_NAME"].ToString());
                }
            }
            SQL += " )\n SELECT\n";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) == 0 || int.Parse(row["IsIdentity"].ToString()) == 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\t, ";
                    }

                    SQL += "temp." + QualifyFieldName(row["COLUMN_NAME"].ToString());
                }
            }

            SQL += "\nFROM  #tbl2 temp LEFT OUTER JOIN DBO." + TableNameFooter + " AS D ON";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += "D." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + " = temp." + row["COLUMN_NAME"].ToString();
                }
            }
            SQL += "\n where (";
            firstParam = true;
            foreach (DataRow row in ColumnsFooter)
            {
                if (int.Parse(row["IsIndex"].ToString()) != 0 || int.Parse(row["IsIdentity"].ToString()) != 0)
                {
                    if (firstParam == true)
                    {
                        firstParam = false;
                        SQL += "\n\t\t";
                    }
                    else
                    {
                        SQL += "\n\t\tand ";
                    }

                    SQL += "D." + QualifyFieldName(row["COLUMN_NAME"].ToString()) + "IS NULL ";
                }
            }
            SQL += "\t\t\t\t)\n";

            SQL += "Drop table #tbl;";
            SQL += "\nDrop table #tbl2;";
            SQL += "\nCOMMIT TRAN";
            SQL += "\nIF @@ERROR <> 0 ";
            SQL += "\nRETURN 0";
            SQL += "\nELSE	-- Return 1 to the calling program to indicate success. 	";
            SQL += "\nRETURN ";
            SQL += "\nEND";

            return SQL;
        }

        private static string QualifyFieldName(string FieldName)
        {
            return "[" + FieldName + "]";
        }
        #endregion
    }
}
