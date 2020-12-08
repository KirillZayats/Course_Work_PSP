using MatrixLibrary;
using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TransformationLibrary;

namespace MathLibrary
{
    public class DistributedSolution
    {
        public Client Client { get; set; }
        public Matrix Matrix { get; set; }
        private byte[] buffer;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="client"></param>
        public DistributedSolution(Client client)
        {
            this.Client = client;
        }


        
        /// <summary>
        /// Получение номера клиента и матрицы с сервера
        /// </summary>
        public void GetParametersFromServer()
        {
            buffer = new byte[12];
            Client.Receive(buffer);
            int[] matrixAsParameters = Converter.GetInt(buffer);

            Client.NumberClient = matrixAsParameters[0];
            int row = matrixAsParameters[1];
            int coll = matrixAsParameters[2];

            buffer = new byte[sizeof(double) * row * coll];
            Client.Receive(buffer);
            double[] matrixAsVector = Converter.GetDoubles(buffer);

            Matrix = new Matrix(matrixAsVector, row, coll);
        }

        /// <summary>
        /// Выполнение алгоритма клиентом
        /// </summary>
        public void Run()
        {
            int sizeMessage = 4;
            if (Client.NumberClient == 1)
                WorkFirstClient(sizeMessage);
            else
                WorkClient(sizeMessage);
            //Thread.Sleep()
            SendResult();
        }

        /// <summary>
        /// Выполнение алгоритма клиентом
        /// </summary>
        /// <param name="sizeMessage"></param>
        private void WorkClient(int sizeMessage)
        {
            buffer = new byte[sizeof(double) * sizeMessage];

            int checkClient = 0; //булевеская проверка последнего клиента
            int lastClient = 0; //булевеская проверка последнего клиента
            int startIndexRow = 0;
            int indexRow = 1; //начальный индекс строки

            while (checkClient < Client.NumberClient - 1)
            {
                int checkDataRow =  0;
                int rowData = startIndexRow + 1;
                int startColl = 0;
                int countColl = 0; //сколько столбцов было пройдено
                //коеф который прибавл в строке
                while (checkDataRow == 0)
                {
                    Client.Receive(buffer);
                    double[] getParameters = Converter.GetDoubles(buffer);
                    //Console.WriteLine("R " + getParameters[3]);
                    lastClient = (int)getParameters[2];

                    checkDataRow = (int)getParameters[1];
                    Matrix.CalculateNewMatrix(getParameters[3], rowData, startIndexRow, Matrix.Coll, startColl);

                    if (rowData == Matrix.Row - 1)
                    {
                        //startColl++;
                        startIndexRow++;
                        rowData = startIndexRow + 1;

                        countColl++;
                    }
                    else
                        rowData++;
                }
                checkClient++;
            }
            CheckClient(lastClient, startIndexRow);
        }

        /// <summary>
        /// Проверка позиции клиента и размера матрицы для последующего отправления данных 
        /// </summary>
        /// <param name="lastClient"></param>
        /// <param name="startIndexRow"></param>
        private void CheckClient(int lastClient, int startIndexRow)
        {
            if (lastClient == 0)
            {
                SendNextClients(startIndexRow, lastClient);
            }
            if (lastClient == 1 && Matrix.Coll != 1)
            {
                SendNextClients(startIndexRow, lastClient);
            }
        }

        /// <summary>
        /// Отправка данных следующим клиентам
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="lastClient"></param>
        private void SendNextClients(int startRow, int lastClient)
        {
            int startIndexRow = startRow;
            int indexRow = 1; //начальный индекс строки
            int indexAddition = startRow + 1; //индес, которые необходим для перехода на другую строку

            int indexColl = 0;
            while (indexColl < Matrix.Coll)
            {

                indexRow = indexAddition;
                while (indexRow < Matrix.Row)
                {

                    double koeff = -(Matrix.Values[indexRow, indexColl] / Matrix.Values[startIndexRow, indexColl]);
                    Client.Send(Converter.GetBytes(new double[4] { Client.NumberClient, GetBoolValueLastRow(indexRow, indexColl, lastClient), 0, koeff }));
                    Matrix.CalculateNewMatrix(koeff, indexRow, startIndexRow, Matrix.Coll, indexColl);
                    indexRow++;
                }
                startIndexRow++;
                indexAddition++;
                indexColl++;
            }
        }

        /// <summary>
        /// Проверка строки (является ли она последней)
        /// </summary>
        /// <param name="indexRow"></param>
        /// <param name="indexColl"></param>
        /// <returns></returns>
        private int GetBoolValueLastRow(int indexRow, int indexColl, int lastClient)
        {
            int checkRow = 0;
            if (indexRow == Matrix.Row - 1 && indexColl == Matrix.Coll - 1)
                checkRow = 1;
            if (lastClient == 1 && Matrix.Coll != 1)
            {
                if (indexRow == Matrix.Row - 1 && indexColl == Matrix.Coll - 2)
                    checkRow = 1;
            }
            else
            {
                if (indexRow == Matrix.Row - 1 && indexColl == Matrix.Coll - 1)
                    checkRow = 1;
            }
            return checkRow;
        }

        /// <summary>
        /// Отправка матрицы на сервер
        /// </summary>
        private void SendResult()
        {
            buffer = Converter.GetBytes(new int[1] { Matrix.Row * Matrix.Coll });
            Client.Send(buffer);

            buffer = Converter.GetBytes(Matrix.ConvertMatrixToArray());
            Client.Send(buffer);
        }

        /// <summary>
        /// Выполнение алгоритма первого клиента
        /// </summary>
        /// <param name="sizeMessage">размер вектора параметров, которые будут отправляться </param>
        private void WorkFirstClient(int sizeMessage)
        {
            int startIndexRow = 0;
            int indexRow; //начальный индекс строки
            int indexAddition = 1; //индес, которые необходим для перехода на другую строку

            byte[] sizeList = BitConverter.GetBytes(sizeMessage);
            Client.Send(sizeList);

            int collTest = 0;
            while (collTest < Matrix.Coll)
            {

                indexRow = indexAddition;
                while (indexRow < Matrix.Row)
                {
                    int testBool = 0;
                    if (indexRow == Matrix.Row - 1 && collTest == Matrix.Coll - 1)
                        testBool = 1;
                    double koeff = -(Matrix.Values[indexRow, collTest] / Matrix.Values[startIndexRow, collTest]);
                    double[] sendParameters = new double[4] { Client.NumberClient, testBool, 0, koeff };

                    buffer = Converter.GetBytes(sendParameters);
                    Client.Send(buffer);
                    Matrix.CalculateNewMatrix(koeff, indexRow, startIndexRow, Matrix.Coll, collTest);
                    indexRow++;
                }
                startIndexRow++;
                indexAddition++;
                collTest++;
            }
        }

    }
}
