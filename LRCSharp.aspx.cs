using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Data;
using System.Linq;

//Author: Shivendra.Chauhan
//Date: 08-Dec-2017
//Algorithms involved for Linear regression :
// a) doolittle's method of LU decomposition
// b) Forward substitution to solve set of linear equations
// c) Backward substitution to solve set of linear equations

namespace PredictNumber.Pages
{
    public partial class MatrixOperations : System.Web.UI.Page
    {
        List<double> lstvars = new List<double>();
        double[] Betacoefficients;
  
        #region Read Text File

        protected void ReadNumbers(object sender, EventArgs e)
        {
            int count = 0;
            double val = 0;
            string str = txtArr.Text;
            if (str.Length == 0)
            {
                Label1.Text = "Please provide inputs ";
            }
            else
            { 
            string[] arr = str.Split(',');
            
            foreach (var item in arr)
            {
                lstvars.Add(Convert.ToDouble(item));
            }
            
            foreach (var item in (double[])Session["BetaValues"])
            {
                
                if (count > 0)
                {
                    val += lstvars[count - 1] * item;
                }
                else if (count == 0)
                {
                    val = item;
                }
                count++;
            }
            Label1.Text = "Predicted Value for inputs : ("+ str + ") :: " + val.ToString();
            }
        }
        protected void ImportMatrixFile(object sender, EventArgs e)
        {
            //Save the uploaded Excel file.

            if (FileUpload1.PostedFile.FileName.ToString().Length != 0)
            {
                string filePath = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload1.PostedFile.FileName);
                FileUpload1.SaveAs(filePath);
                double[][] rawdata = MatrixLoad(filePath, false, ' ');
                double[][] designedMat = Design(rawdata);
                Betacoefficients = Solve(designedMat);
                Session["BetaValues"] = Betacoefficients;
                //double num = RSquared(rawdata, Betacoefficients);
                string coeff = "";

                foreach (var item in Betacoefficients)
                {
                    coeff += item.ToString() + " ; ";
                }
                lblTranspose.Text = "Sample data has been read and model is ready.Please provide inputs to predict number!!";
            }
            else {
                lblTranspose.Text = "Please choose a file to proceed";
            }
            
        }
        #endregion

        #region Matrix Operations
        //Matrix Load
        static double[][] MatrixLoad(string file, bool header,char sep)
        {
            // load a matrix from a text file
            string line = "";
            string[] tokens = null;
            int ct = 0;
            int rows, cols;
            // determined # rows and cols
            System.IO.FileStream ifs = new System.IO.FileStream(file, System.IO.FileMode.Open);
            System.IO.StreamReader sr = new System.IO.StreamReader(ifs);
            while ((line = sr.ReadLine()) != null)
            {
                ++ct;
                tokens = line.Split(sep); // do validation here
            }
            sr.Close(); ifs.Close();
            if (header == true)
                rows = ct - 1;
            else
                rows = ct;
            cols = tokens.Length;
            double[][] result = MatrixCreate(rows, cols);

            // load
            int i = 0; // row index
            ifs = new System.IO.FileStream(file, System.IO.FileMode.Open);
            sr = new System.IO.StreamReader(ifs);

            if (header == true)
                line = sr.ReadLine();  // consume header
            while ((line = sr.ReadLine()) != null)
            {
                tokens = line.Split(sep);
                for (int j = 0; j < cols; ++j)
                    result[i][j] = double.Parse(tokens[j]);
                ++i; // next row
            }
            sr.Close(); ifs.Close();
            return result;
        }
        //Design Matrix
        static double[][] Design(double[][] data)
        {
            // add a leading col of 1.0 values
            int rows = data.Length;
            int cols = data[0].Length;
            double[][] result = MatrixCreate(rows, cols + 1);
            for (int i = 0; i < rows; ++i)
                result[i][0] = 1.0;

            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j + 1] = data[i][j];

            return result;
        }
        //Solve for co-efficients
        static double[] Solve(double[][] design)
        {
            // find linear regression coefficients
            // 1. Separate X matrix and Y vector from augmented design matrix
            int rows = design.Length;
            int cols = design[0].Length;
            double[][] X = MatrixCreate(rows, cols - 1);
            double[][] Y = MatrixCreate(rows, 1); // a column vector

            int j;
            for (int i = 0; i < rows; ++i)
            {
                for (j = 0; j < cols - 1; ++j)
                {
                    X[i][j] = design[i][j];
                }
                Y[i][0] = design[i][j]; // last column
            }

            // 2. (beta coefficients) B = inv(Xt * X) * Xt * y
            double[][] Xt = MatrixTranspose(X);
            double[][] XtX = MatrixProduct(Xt, X);
            double[][] inv = MatrixInverse(XtX);
            double[][] invXt = MatrixProduct(inv, Xt);

            double[][] mResult = MatrixProduct(invXt, Y);
            double[] result = MatrixToVector(mResult);
            return result;
        } // Solve

        #endregion

        #region Matrix Operations - Helper Methods

        static double[][] MatrixCreate(int rows, int cols)
        {
            // allocates/creates a matrix initialized to all 0.0
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        } // CreateMatrix
        static double[][] MatrixTranspose(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[][] result = MatrixCreate(cols, rows); // note indexing (rows to col and col to rows for transpose)
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result[j][i] = matrix[i][j];
                }
            }
            return result;
        } // TransposeMatrix

        static double[][] MatrixProduct(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            double[][] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            return result;
        } // Matrix product

        static double[][] MatrixInverse(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = MatrixDuplicate(matrix);
            //create a duplicate matrix using MatrixDuplicate
            //decompose matrix using MatrixDecompose method to get LU Decomposed matrix
            //for each row , create a vector named b and call HelperSolve method providing LU Matrix and b vector as inputs 
            //to get decomposed vectors
            int[] perm;
            int toggle;
            double[][] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = HelperSolve(lum, b); // use decomposition

                for (int j = 0; j < n; ++j)
                    result[j][i] = x[j];
            }
            return result;
        } // Matrix Inverse

        static double[][] MatrixDuplicate(double[][] matrix)
        {
            // allocates/creates a duplicate of a matrix
            double[][] result = MatrixCreate(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }   // Matrix duplicate

        static double[][] MatrixDecompose(double[][] matrix, out int[] perm,out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // returns: result is L (with 1s on diagonal) and U;but both will be combined in one matrix and will be returned
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (rows != cols)
                throw new Exception("Non-square mattrix");

            int n = rows; // convenience

            double[][] result = MatrixDuplicate(matrix); // 

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps

            for (int j = 0; j < n - 1; ++j) // each column(there will not be any element below diagonal element for last column hence j<n-1)
            {
                double colMax = Math.Abs(result[j][j]); //start from diagonal element of column
                int pRow = j;

                for (int i = j + 1; i < n; ++i) // other rows elements in same column
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // -------------------------------------------------------------
                // if there is a 0 on the diagonal, find a good row 
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j

                if (result[j][j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                //calculating upper triangular and lower triangular matrix(LU Decomposition)
                //both matrix in a single matrix
                //reference: http://nptel.ac.in/courses/108108079/pdf/Unit%201/Unit_1.7.pdf
                //https://www.youtube.com/watch?v=dza5JTvMpzk
                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j]; // for Lower matrix
                    for (int k = j + 1; k < n; ++k) // for Upper matrix
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }

            } // main j column loop

            return result;
        }   // MatrixDecompose

        static double[] HelperSolve(double[][] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);
            //FORWARD SUBSTITUTION (since x(0) = z(0) in L*z = X, we will start from i=1 and try to convert X into z)
            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j) //coz luMatrix(i,j)=1 if j=i in L matrix and if j>i, then  luMatrix(i,j)=0
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            if (luMatrix[n - 1][n - 1] != 0.0)  // divided to start backward substitution
            {
                x[n - 1] /= luMatrix[n - 1][n - 1];
            }           

            //x[n - 1] /= luMatrix[n - 1][n - 1]; // divided to start backward substitution
            //BACKWARD SUBSTITUTION
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j]; // x(j) is updated and will have value of z(j)
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }  // Helper solve

        static double[] MatrixToVector(double[][] matrix)  // Matrix to vector
        {
            // single column matrix to vector
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            if (cols != 1)
                throw new Exception("Bad matrix");
            double[] result = new double[rows];
            for (int i = 0; i < rows; ++i)
                result[i] = matrix[i][0];
            return result;
        }
        #endregion
    }
}